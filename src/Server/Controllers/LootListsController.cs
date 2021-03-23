// Copyright (C) 2021 Donovan Sullivan
// GNU General Public License v3.0+ (see LICENSE or https://www.gnu.org/licenses/gpl-3.0.txt)

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ValhallaLootList.DataTransfer;
using ValhallaLootList.Helpers;
using ValhallaLootList.Server.Data;

namespace ValhallaLootList.Server.Controllers
{
    public class LootListsController : ApiControllerV1
    {
        private readonly ApplicationDbContext _context;
        private readonly TimeZoneInfo _serverTimeZoneInfo;

        public LootListsController(ApplicationDbContext context, TimeZoneInfo serverTimeZoneInfo)
        {
            _context = context;
            _serverTimeZoneInfo = serverTimeZoneInfo;
        }

        [HttpGet]
        public async Task<ActionResult<IList<LootListDto>>> Get(string? characterId = null, string? teamId = null, byte? phase = null)
        {
            try
            {
                var lootLists = await CreateDtosAsync(characterId, teamId, phase);

                if (lootLists is null)
                {
                    return NotFound();
                }

                return Ok(lootLists);
            }
            catch (ArgumentException ex)
            {
                return Problem(ex.Message, statusCode: 400);
            }
        }

        [HttpPost("Phase{phase:int}/{characterId}")]
        public async Task<ActionResult<LootListDto>> PostLootList(string characterId, byte phase, [FromBody] LootListSubmissionDto dto)
        {
            var bracketTemplates = await _context.Brackets.AsNoTracking().Where(b => b.Phase == phase).OrderBy(b => b.Index).ToListAsync();

            if (bracketTemplates.Count == 0)
            {
                return NotFound();
            }

            if (!ModelState.IsValid)
            {
                return ValidationProblem();
            }

            Debug.Assert(dto.MainSpec.HasValue);
            Debug.Assert(dto.Items is not null);

            var character = await _context.Characters.FindAsync(characterId);

            if (character is null)
            {
                return NotFound();
            }

            if (!User.IsAdmin() && character.OwnerId != User.GetDiscordId())
            {
                return Unauthorized();
            }

            if (await _context.CharacterLootLists.AsNoTracking().AnyAsync(ll => ll.CharacterId == character.Id && ll.Phase == phase))
            {
                return Problem(statusCode: 400, title: "Bad Request", detail: "A loot list for that character and phase already exists.");
            }

            if (!dto.MainSpec.Value.IsClass(character.Class))
            {
                ModelState.AddModelError(nameof(dto.MainSpec), "Selected specialization does not fit the player's class.");
                return ValidationProblem();
            }

            if (dto.OffSpec.HasValue && !dto.OffSpec.Value.IsClass(character.Class))
            {
                ModelState.AddModelError(nameof(dto.OffSpec), "Selected specialization does not fit the player's class.");
                return ValidationProblem();
            }

            var list = new CharacterLootList
            {
                CharacterId = character.Id,
                Character = character,
                Entries = new List<LootListEntry>(28),
                Locked = false,
                MainSpec = dto.MainSpec.Value,
                OffSpec = dto.OffSpec ?? dto.MainSpec.Value,
                Phase = phase
            };

            var allItemIds = new HashSet<uint>();

            foreach (var (rank, itemIds) in dto.Items)
            {
                var bracketTemplate = bracketTemplates.Find(bt => rank >= bt.MinRank && rank <= bt.MaxRank);

                if (bracketTemplate is null)
                {
                    ModelState.AddModelError($"Items[{rank}]", $"Rank {rank} is not allowed.");
                }
                else if (itemIds?.Length > 0)
                {
                    if (itemIds.Length > bracketTemplate.MaxItems)
                    {
                        ModelState.AddModelError($"Items[{rank}]", $"Rank {rank} can only have up to {bracketTemplate.MaxItems} items.");
                    }
                    else
                    {
                        for (int col = 0; col < itemIds.Length; col++)
                        {
                            uint itemId = itemIds[col];
                            if (itemId > 0 && !allItemIds.Add(itemId))
                            {
                                ModelState.AddModelError($"Items[{rank}][{col}]", "Duplicate items are not allowed.");
                            }
                        }
                    }
                }
            }

            if (!ModelState.IsValid)
            {
                return ValidationProblem();
            }

            await foreach (var wonItemId in _context.Drops.AsNoTracking()
                .Where(drop => drop.WinnerId == character.Id || drop.WinningEntry.LootList.CharacterId == character.Id)
                .Select(drop => drop.ItemId)
                .AsAsyncEnumerable())
            {
                if (allItemIds.Contains(wonItemId))
                {
                    foreach (var (rank, itemIds) in dto.Items)
                    {
                        var col = Array.IndexOf(itemIds, wonItemId);
                        if (col >= 0)
                        {
                            ModelState.AddModelError($"Items[{rank}][{col}]", "You have already won this item and may not put it on a new loot list.");
                        }
                    }

                    Debug.Assert(!ModelState.IsValid);

                    return ValidationProblem();
                }
            }

            var items = await _context.Items
                .AsNoTracking()
                .Where(item => allItemIds.Contains(item.Id))
                .Select(item => new { item.Id, item.Name, item.Slot, item.Type, Phase = (item.RewardFromId != null ? item.RewardFrom!.Encounter!.Instance.Phase : item.Encounter!.Instance!.Phase) }) // TODO: simplify phase query
                .ToDictionaryAsync(item => item.Id);

            var bothSpecs = dto.OffSpec.HasValue ? (dto.MainSpec.Value | dto.OffSpec.Value) : dto.MainSpec.Value;

            var restrictions = (await _context.ItemRestrictions
                .AsNoTracking()
                .Where(r => allItemIds.Contains(r.ItemId) && r.RestrictionLevel == ItemRestrictionLevel.Unequippable && (r.Specializations & bothSpecs) != 0)
                .ToListAsync())
                .ToLookup(r => r.ItemId);

            var bracketItemGroups = new HashSet<ItemGroup>();

            foreach (var (rank, itemIds) in dto.Items)
            {
                if (itemIds?.Length > 0)
                {
                    var bracketTemplate = bracketTemplates.First(bt => rank >= bt.MinRank && rank <= bt.MaxRank);
                    var bracketSpec = bracketTemplate.AllowOffspec ? bothSpecs : dto.MainSpec.Value;

                    for (int col = 0; col < itemIds.Length; col++)
                    {
                        uint itemId = itemIds[col];
                        if (itemId > 0)
                        {
                            if (items.TryGetValue(itemId, out var item))
                            {
                                if (phase != item.Phase)
                                {
                                    ModelState.AddModelError($"Items[{rank}][{col}]", "Item is not in the same content phase as the specified loot list phase.");
                                }

                                if (!bracketTemplate.AllowTypeDuplicates && !bracketItemGroups.Add(new ItemGroup(item.Type, item.Slot)))
                                {
                                    ModelState.AddModelError($"Items[{rank}][{col}]", $"Cannot have multiple items of the same type in Bracket {bracketTemplates.IndexOf(bracketTemplate) + 1}.");
                                }

                                foreach (var restriction in restrictions[itemId].Where(r => (r.Specializations & bracketSpec) != 0))
                                {
                                    ModelState.AddModelError($"Items[{rank}][{col}]", restriction.Reason);
                                }

                                list.Entries.Add(new LootListEntry
                                {
                                    ItemId = itemId,
                                    LootList = list,
                                    Rank = (byte)rank
                                });
                            }
                            else
                            {
                                ModelState.AddModelError($"Items[{rank}][{col}]", "Item does not exist.");
                            }
                        }
                    }
                }

                bracketItemGroups.Clear();
            }

            if (!ModelState.IsValid)
            {
                return ValidationProblem();
            }

            _context.Add(list);

            await _context.SaveChangesAsync();

            var dtos = await CreateDtosAsync(character.Id, null, phase);
            Debug.Assert(dtos?.Count == 1);

            return CreatedAtAction(nameof(Get), new { characterId = character.Id, phase }, dtos[0]);
        }

        [HttpPut("Phase{phase:int}/{characterId}")]
        public async Task<ActionResult<LootListDto>> PutLootList(string characterId, byte phase, [FromBody] LootListSubmissionDto dto)
        {
            var list = await _context.CharacterLootLists.FindAsync(characterId, phase);

            if (list is null)
            {
                return NotFound();
            }

            var bracketTemplates = await _context.Brackets.AsNoTracking().Where(b => b.Phase == phase).OrderBy(b => b.Index).ToListAsync();

            if (bracketTemplates.Count == 0)
            {
                return NotFound();
            }

            Debug.Assert(dto.MainSpec.HasValue);
            Debug.Assert(dto.Items is not null);

            var character = await _context.Characters.FindAsync(characterId);

            if (character is null)
            {
                return NotFound();
            }

            if (!User.IsAdmin() && character.OwnerId != User.GetDiscordId())
            {
                return Unauthorized();
            }

            if (list.Locked)
            {
                return Problem("The loot list is locked and may not be edited.", statusCode: 400);
            }

            if (!dto.MainSpec.Value.IsClass(character.Class))
            {
                ModelState.AddModelError(nameof(dto.MainSpec), "Selected specialization does not fit the player's class.");
                return ValidationProblem();
            }

            if (dto.OffSpec.HasValue && !dto.OffSpec.Value.IsClass(character.Class))
            {
                ModelState.AddModelError(nameof(dto.OffSpec), "Selected specialization does not fit the player's class.");
                return ValidationProblem();
            }

            list.MainSpec = dto.MainSpec.Value;
            list.OffSpec = dto.OffSpec ?? dto.MainSpec.Value;

            await _context.Entry(list).Collection(ll => ll.Entries).LoadAsync();

            foreach (var existingEntry in list.Entries.ToList())
            {
                if (string.IsNullOrEmpty(existingEntry.DropId))
                {
                    list.Entries.Remove(existingEntry);
                    _context.LootListEntries.Remove(existingEntry);
                }
            }

            var allItemIds = new HashSet<uint>();

            foreach (var (rank, itemIds) in dto.Items)
            {
                var bracketTemplate = bracketTemplates.Find(bt => rank >= bt.MinRank && rank <= bt.MaxRank);

                if (bracketTemplate is null)
                {
                    ModelState.AddModelError($"Items[{rank}]", $"Rank {rank} is not allowed.");
                }
                else if (itemIds?.Length > 0)
                {
                    var adjustedMaxItems = bracketTemplate.MaxItems - list.Entries.Count(entry => entry.Rank == rank);
                    if (itemIds.Length > adjustedMaxItems)
                    {
                        ModelState.AddModelError($"Items[{rank}]", $"Rank {rank} can only have up to {adjustedMaxItems} items.");
                    }
                    else
                    {
                        for (int col = 0; col < itemIds.Length; col++)
                        {
                            uint itemId = itemIds[col];
                            if (itemId > 0 && !allItemIds.Add(itemId))
                            {
                                ModelState.AddModelError($"Items[{rank}][{col}]", "Duplicate items are not allowed.");
                            }
                        }
                    }
                }
            }

            if (!ModelState.IsValid)
            {
                return ValidationProblem();
            }

            await foreach (var wonItemId in _context.Drops.AsNoTracking()
                .Where(drop => drop.WinnerId == character.Id || drop.WinningEntry.LootList.CharacterId == character.Id)
                .Select(drop => drop.ItemId)
                .AsAsyncEnumerable())
            {
                if (allItemIds.Contains(wonItemId))
                {
                    foreach (var (rank, itemIds) in dto.Items)
                    {
                        var col = Array.IndexOf(itemIds, wonItemId);
                        if (col >= 0)
                        {
                            ModelState.AddModelError($"Items[{rank}][{col}]", "You have already won this item and may not put it on a new loot list.");
                        }
                    }

                    Debug.Assert(!ModelState.IsValid);

                    return ValidationProblem();
                }
            }

            var items = await _context.Items
                .AsNoTracking()
                .Where(item => allItemIds.Contains(item.Id))
                .Select(item => new { item.Id, item.Name, item.Slot, item.Type, Phase = (item.RewardFromId != null ? item.RewardFrom!.Encounter!.Instance.Phase : item.Encounter!.Instance!.Phase) }) // TODO: simplify phase query
                .ToDictionaryAsync(item => item.Id);

            var bothSpecs = dto.OffSpec.HasValue ? (dto.MainSpec.Value | dto.OffSpec.Value) : dto.MainSpec.Value;

            var restrictions = (await _context.ItemRestrictions
                .AsNoTracking()
                .Where(r => allItemIds.Contains(r.ItemId) && r.RestrictionLevel == ItemRestrictionLevel.Unequippable && (r.Specializations & bothSpecs) != 0)
                .ToListAsync())
                .ToLookup(r => r.ItemId);

            var bracketItemGroups = new HashSet<ItemGroup>();

            foreach (var (rank, itemIds) in dto.Items)
            {
                if (itemIds?.Length > 0)
                {
                    var bracketTemplate = bracketTemplates.First(bt => rank >= bt.MinRank && rank <= bt.MaxRank);
                    var bracketSpec = bracketTemplate.AllowOffspec ? bothSpecs : dto.MainSpec.Value;

                    for (int col = 0; col < itemIds.Length; col++)
                    {
                        uint itemId = itemIds[col];
                        if (itemId > 0)
                        {
                            if (items.TryGetValue(itemId, out var item))
                            {
                                if (phase != item.Phase)
                                {
                                    ModelState.AddModelError($"Items[{rank}][{col}]", "Item is not in the same content phase as the specified loot list phase.");
                                }

                                if (!bracketTemplate.AllowTypeDuplicates && !bracketItemGroups.Add(new ItemGroup(item.Type, item.Slot)))
                                {
                                    ModelState.AddModelError($"Items[{rank}][{col}]", $"Cannot have multiple items of the same type in Bracket {bracketTemplates.IndexOf(bracketTemplate) + 1}.");
                                }

                                foreach (var restriction in restrictions[itemId].Where(r => (r.Specializations & bracketSpec) != 0))
                                {
                                    ModelState.AddModelError($"Items[{rank}][{col}]", restriction.Reason);
                                }

                                list.Entries.Add(new LootListEntry
                                {
                                    ItemId = itemId,
                                    LootList = list,
                                    Rank = (byte)rank
                                });
                            }
                            else
                            {
                                ModelState.AddModelError($"Items[{rank}][{col}]", "Item does not exist.");
                            }
                        }
                    }
                }

                bracketItemGroups.Clear();
            }

            if (!ModelState.IsValid)
            {
                return ValidationProblem();
            }

            await _context.SaveChangesAsync();

            var dtos = await CreateDtosAsync(character.Id, null, phase);
            Debug.Assert(dtos?.Count == 1);

            return Ok(dtos[0]);
        }

        [HttpPost("Phase{phase:int}/{characterId}/Lock"), Authorize(AppRoles.RaidLeader)]
        public async Task<ActionResult> PostLock(string characterId, byte phase)
        {
            var list = await _context.CharacterLootLists.FindAsync(characterId, phase);

            if (list is null)
            {
                return NotFound();
            }

            if (!User.IsAdmin())
            {
                var teamId = await _context.Characters.Where(c => c.Id == characterId).Select(c => c.TeamId).FirstAsync();

                if (string.IsNullOrEmpty(teamId) || !User.HasClaim(AppClaimTypes.RaidLeader, teamId))
                {
                    return Unauthorized();
                }
            }

            if (list.Locked)
            {
                return Problem("Loot list is already locked.", statusCode: 400);
            }

            list.Locked = true;

            await _context.SaveChangesAsync();

            return Ok();
        }

        [HttpPost("Phase{phase:int}/{characterId}/Unlock"), Authorize(AppRoles.Administrator)]
        public async Task<ActionResult> PostUnlock(string characterId, byte phase)
        {
            var list = await _context.CharacterLootLists.FindAsync(characterId, phase);

            if (list is null)
            {
                return NotFound();
            }

            if (!list.Locked)
            {
                return Problem("Loot list is already unlocked.", statusCode: 400);
            }

            list.Locked = false;

            await _context.SaveChangesAsync();

            return Ok();
        }

        private async Task<IList<LootListDto>?> CreateDtosAsync(string? characterId, string? teamId, byte? phase)
        {
            var lootListQuery = _context.CharacterLootLists.AsNoTracking();
            var passQuery = _context.DropPasses.AsNoTracking();
            var entryQuery = _context.LootListEntries.AsNoTracking();
            var attendanceQuery = _context.RaidAttendees.AsNoTracking().AsSingleQuery().Where(x => !x.IgnoreAttendance);

            if (characterId?.Length > 0)
            {
                if (teamId?.Length > 0)
                {
                    throw new ArgumentException("Either characterId or teamId must be set, but not both.");
                }

                var character = await _context.Characters.AsNoTracking().Where(c => c.Id == characterId).Select(c => new { c.Id, c.TeamId }).FirstOrDefaultAsync();

                if (character is null)
                {
                    return null;
                }

                lootListQuery = lootListQuery.Where(ll => ll.CharacterId == characterId);
                passQuery = passQuery.Where(p => p.CharacterId == characterId);
                entryQuery = entryQuery.Where(e => e.LootList.CharacterId == characterId);
                attendanceQuery = attendanceQuery.Where(a => a.CharacterId == characterId);
                teamId = character.TeamId;
            }
            else if (teamId?.Length > 0)
            {
                if (await _context.RaidTeams.CountAsync(team => team.Id == teamId) == 0)
                {
                    return null;
                }

                lootListQuery = lootListQuery.Where(ll => ll.Character.TeamId == teamId);
                passQuery = passQuery.Where(p => p.Character.TeamId == teamId);
                entryQuery = entryQuery.Where(e => e.LootList.Character.TeamId == teamId);
                attendanceQuery = attendanceQuery.Where(a => a.Raid.RaidTeamId == teamId && a.Character.TeamId == teamId);
            }
            else
            {
                throw new ArgumentException("Either characterId or teamId must be set.");
            }

            if (phase.HasValue)
            {
                lootListQuery = lootListQuery.Where(ll => ll.Phase == phase.Value);
                entryQuery = entryQuery.Where(e => e.LootList.Phase == phase.Value);
            }

            var currentUserId = User.GetDiscordId();
            bool isAdmin = User.IsAdmin();

            var dtos = await lootListQuery
                .Select(ll => new LootListDto
                {
                    ApprovedBy = ll.ApprovedBy,
                    CharacterId = ll.CharacterId,
                    CharacterName = ll.Character.Name,
                    CharacterMemberStatus = ll.Character.MemberStatus,
                    Owned = isAdmin || ll.Character.OwnerId == currentUserId,
                    TeamId = ll.Character.TeamId,
                    TeamName = ll.Character.Team!.Name,
                    Locked = ll.Locked,
                    MainSpec = ll.MainSpec,
                    OffSpec = ll.OffSpec,
                    Phase = ll.Phase
                })
                .ToListAsync();

            var passes = await passQuery
                .Select(pass => new { pass.CharacterId, pass.Drop.ItemId, pass.RelativePriority })
                .ToListAsync();

            var offset = TimeSpanHelpers.GetTimeZoneOffsetString(_serverTimeZoneInfo.BaseUtcOffset);
            var attendances = await attendanceQuery
                .Select(a => new { a.CharacterId, MySqlTranslations.ConvertTz(a.Raid.StartedAtUtc, "+00:00", offset).Date })
                .GroupBy(a => a.CharacterId)
                .Select(g => new { CharacterId = g.Key, Count = g.Select(a => a.Date).Distinct().Count() })
                .ToDictionaryAsync(g => g.CharacterId, g => g.Count);

            await foreach (var entry in entryQuery
                .OrderByDescending(e => e.Rank)
                .Select(e => new
                {
                    e.Id,
                    e.ItemId,
                    e.Item!.RewardFromId,
                    ItemName = (string?)e.Item!.Name,
                    Won = e.DropId != null,
                    e.Rank,
                    e.LootList.Phase,
                    e.LootList.CharacterId
                })
                .AsAsyncEnumerable())
            {
                var dto = dtos.Find(x => x.CharacterId == entry.CharacterId && x.Phase == entry.Phase);
                if (dto is not null)
                {
                    int? prio = null;

                    if (entry.ItemId.HasValue)
                    {
                        var rewardFromId = entry.RewardFromId ?? entry.ItemId.Value;

                        if (!entry.Won && dto.Locked)
                        {
                            int loss = 0, underPrio = 0;

                            foreach (var pass in passes.Where(p => p.ItemId == entry.ItemId && p.CharacterId == entry.CharacterId))
                            {
                                if (pass.RelativePriority >= 0)
                                {
                                    loss++;
                                }
                                else
                                {
                                    underPrio++;
                                }
                            }

                            attendances.TryGetValue(entry.CharacterId, out var characterAttendances);

                            prio = PrioCalculator.CalculatePrio(entry.Rank, characterAttendances, dto.CharacterMemberStatus, loss, underPrio);
                        }
                    }

                    bool showRanks = dto.Locked || dto.Owned;

                    dto.Entries.Add(new LootListEntryDto
                    {
                        Id = entry.Id,
                        ItemId = entry.ItemId,
                        ItemName = entry.ItemName,
                        Prio = showRanks ? prio : null,
                        Rank = showRanks ? entry.Rank : 0,
                        Won = entry.Won
                    });
                }
            }

            return dtos;
        }
    }
}
