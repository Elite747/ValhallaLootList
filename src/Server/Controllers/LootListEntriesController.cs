﻿// Copyright (C) 2021 Donovan Sullivan
// GNU General Public License v3.0+ (see LICENSE or https://www.gnu.org/licenses/gpl-3.0.txt)

using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.ApplicationInsights;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ValhallaLootList.DataTransfer;
using ValhallaLootList.Server.Data;

namespace ValhallaLootList.Server.Controllers
{
    public class LootListEntriesController : ApiControllerV1
    {
        private readonly ApplicationDbContext _context;
        private readonly IAuthorizationService _authorizationService;
        private readonly TelemetryClient _telemetry;

        public LootListEntriesController(ApplicationDbContext context, IAuthorizationService authorizationService, TelemetryClient telemetry)
        {
            _context = context;
            _authorizationService = authorizationService;
            _telemetry = telemetry;
        }

        [HttpPut("{entryId:long}")]
        public async Task<ActionResult<LootListEntryUpdateDto>> PutEntry(long entryId, [FromBody] LootListEntrySubmissionDto dto)
        {
            var entry = await _context.LootListEntries.FindAsync(entryId);

            if (entry is null)
            {
                return NotFound();
            }

            await _context.Entry(entry).Reference(e => e.LootList).LoadAsync();

            var authorizationResult = await _authorizationService.AuthorizeAsync(User, entry.LootList.CharacterId, AppPolicies.CharacterOwnerOrAdmin);
            if (!authorizationResult.Succeeded)
            {
                return Unauthorized();
            }

            if (entry.LootList.Status != LootListStatus.Editing)
            {
                return Problem("Loot List cannot be edited.");
            }

            if (dto.EntryId == dto.SwapEntryId)
            {
                return Problem("Swap entry is the same entry as the target.");
            }

            if (entry.DropId.HasValue)
            {
                return Problem("Loot List entry has already been won and may not be changed.");
            }

            if (entry.ItemId == dto.ItemId && entry.Justification == dto.Justification)
            {
                return Problem("Entry is already set to the specified item.");
            }

            var returnDto = new LootListEntryUpdateDto { EntryId = entryId, ItemId = dto.ItemId, EntryJustification = dto.Justification, SwapEntryId = dto.SwapEntryId };
            bool allowed;
            string? reason;

            if (dto.SwapEntryId.HasValue)
            {
                var swapEntry = await _context.LootListEntries.FindAsync(dto.SwapEntryId);

                if (swapEntry is null)
                {
                    return NotFound();
                }

                await _context.Entry(swapEntry).Reference(e => e.LootList).LoadAsync();

                if ((entry.LootList.CharacterId, entry.LootList.Phase) != (swapEntry.LootList.CharacterId, swapEntry.LootList.Phase))
                {
                    return Problem("Swap entry is not part of the same list as the target entry.");
                }

                if (swapEntry.DropId.HasValue)
                {
                    return Problem("Loot List entry has already been won and may not be changed.");
                }

                var oldItemId = entry.ItemId;
                var oldJustification = entry.Justification;
                entry.ItemId = dto.ItemId;
                entry.Justification = dto.Justification;
                swapEntry.ItemId = oldItemId;
                swapEntry.Justification = oldJustification;

                (allowed, reason) = await CheckAllowedAsync(entry, swapEntry, dto.RemoveIfInvalid);

                if (!allowed)
                {
                    return Problem(reason);
                }

                returnDto.SwapItemId = swapEntry.ItemId;
                returnDto.SwapEntryJustification = swapEntry.Justification;
            }
            else
            {
                entry.ItemId = dto.ItemId;
                entry.Justification = dto.Justification;

                (allowed, reason) = await CheckAllowedAsync(entry, null, dto.RemoveIfInvalid);

                if (!allowed)
                {
                    return Problem(reason);
                }
            }

            (allowed, reason) = await CheckValidAsync(entry, dto);

            if (!allowed)
            {
                return Problem(reason);
            }

            await _context.SaveChangesAsync();

            _telemetry.TrackEvent("LootListEntryUpdated", User, props =>
            {
                props["EntryId"] = entry.Id.ToString();
                props["CharacterId"] = entry.LootList.CharacterId.ToString();
                props["Phase"] = entry.LootList.Phase.ToString();
            });

            return returnDto;
        }

        private async Task<(bool allowed, string? reason)> CheckValidAsync(LootListEntry entry, LootListEntrySubmissionDto dto)
        {
            if (entry.ItemId.HasValue)
            {
                var item = await _context.Items
                    .AsNoTracking()
                    .Where(i => i.Id == entry.ItemId.Value)
                    .Select(i => new
                    {
                        i.Id,
                        i.Name,
                        i.Phase,
                        i.RewardFromId,
                        QuestId = (uint?)i.RewardFrom!.QuestId,
                        MaxCount = (!i.IsUnique && (i.Slot == InventorySlot.Trinket || i.Slot == InventorySlot.Finger || i.Slot == InventorySlot.OneHand)) ? 2 : 1
                    })
                    .FirstOrDefaultAsync();

                if (item is null)
                {
                    return (false, $"Item #{entry.ItemId} does not exist.");
                }

                if (item.Phase != entry.LootList.Phase)
                {
                    return (false, item.Name + " is not part of the same phase as the loot list.");
                }

                var spec = entry.LootList.MainSpec | entry.LootList.OffSpec;

                var restriction = await _context.ItemRestrictions
                    .AsNoTracking()
                    .Where(r => r.ItemId == item.Id && (r.Specializations & spec) != 0 && r.RestrictionLevel == ItemRestrictionLevel.Unequippable)
                    .FirstOrDefaultAsync();

                if (restriction is not null)
                {
                    return (false, restriction.Reason);
                }

                long excludeId = dto.SwapEntryId ?? dto.EntryId;

                var existingItemMutexQuery = _context.LootListEntries
                    .AsNoTracking()
                    .Where(e => e.Id != excludeId && e.LootList.Phase == entry.LootList.Phase && e.LootList.CharacterId == entry.LootList.CharacterId);

                if (item.QuestId > 0)
                {
                    var firstConflict = await existingItemMutexQuery
                        .Where(e => e.ItemId == entry.ItemId || e.Item!.RewardFrom!.QuestId == item.QuestId.Value)
                        .Select(e => new { e.Item!.Id, e.Item!.Name })
                        .FirstOrDefaultAsync();

                    if (firstConflict is not null)
                    {
                        if (firstConflict.Id == item.Id)
                        {
                            return (false, item.Name + " is already on this loot list.");
                        }
                        else
                        {
                            return (false, $"{item.Name} comes from a quest reward that also gives {firstConflict.Name}. Only one of these items may be on your list.");
                        }
                    }
                }
                else if (await existingItemMutexQuery.Where(e => e.ItemId == dto.ItemId).CountAsync() >= item.MaxCount)
                {
                    if (item.MaxCount == 1)
                    {
                        return (false, item.Name + " is already on this loot list.");
                    }
                    return (false, $"{item.Name} has already been added {item.MaxCount:N0} times.");
                }
            }

            return (true, null);
        }

        private async Task<(bool allowed, string? reason)> CheckAllowedAsync(LootListEntry entry, LootListEntry? swapEntry, bool removeIfInvalid)
        {
            var bracket = await _context.Brackets
                .AsNoTracking()
                .Where(b => b.Phase == entry.LootList.Phase && entry.Rank >= b.MinRank && entry.Rank <= b.MaxRank)
                .FirstOrDefaultAsync();

            if (bracket is null)
            {
                return (false, "Entry rank is not valid.");
            }

            if (entry.ItemId > 0)
            {
                if (swapEntry is not null)
                {
                    var swapBracket = await _context.Brackets
                        .AsNoTracking()
                        .Where(b => b.Phase == swapEntry.LootList.Phase && swapEntry.Rank >= b.MinRank && swapEntry.Rank <= b.MaxRank)
                        .FirstOrDefaultAsync();

                    if (swapBracket.Index == bracket.Index)
                    {
                        return (true, null);
                    }

                    if (!swapBracket.AllowTypeDuplicates && swapEntry.ItemId.HasValue && await BracketHasTypeAsync(swapBracket, swapEntry))
                    {
                        if (removeIfInvalid)
                        {
                            swapEntry.ItemId = null;
                            swapEntry.Justification = null;
                        }
                        else
                        {
                            return (false, "Bracket already has an item of that type.");
                        }
                    }
                }

                if (!bracket.AllowTypeDuplicates && await BracketHasTypeAsync(bracket, entry))
                {
                    return (false, "Bracket already has an item of that type.");
                }

                return (true, null);
            }
            else if (swapEntry?.ItemId > 0)
            {
                var swapBracket = await _context.Brackets
                    .AsNoTracking()
                    .Where(b => b.Phase == entry.LootList.Phase && entry.Rank >= b.MinRank && entry.Rank <= b.MaxRank)
                    .FirstOrDefaultAsync();

                if (swapBracket.Index != bracket.Index && !swapBracket.AllowTypeDuplicates && await BracketHasTypeAsync(swapBracket, swapEntry))
                {
                    swapEntry.ItemId = null;
                }
            }

            return (true, null);
        }

        private async Task<bool> BracketHasTypeAsync(Bracket bracket, LootListEntry entry)
        {
            Debug.Assert(entry.ItemId.HasValue);
            var item = await _context.Items.FindAsync(entry.ItemId.Value);
            Debug.Assert(item is not null);
            var itemGroup = new ItemGroup(item.Type, item.Slot);

            var bracketItems = await _context.LootListEntries
                .AsNoTracking()
                .Where(e => e.LootList == entry.LootList && e.Rank >= bracket.MinRank && e.Rank <= bracket.MaxRank && e.Id != entry.Id)
                .Select(e => e.Item)
                .ToListAsync();

            await foreach (var bracketItem in _context.LootListEntries
                .AsNoTracking()
                .Where(e => e.LootList == entry.LootList && e.Rank >= bracket.MinRank && e.Rank <= bracket.MaxRank && e.Id != entry.Id && e.ItemId.HasValue)
                .Select(e => new { e.Item!.Type, e.Item!.Slot })
                .AsAsyncEnumerable())
            {
                if (itemGroup == new ItemGroup(bracketItem.Type, bracketItem.Slot))
                {
                    return true;
                }
            }

            return false;
        }
    }
}
