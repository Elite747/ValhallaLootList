// Copyright (C) 2021 Donovan Sullivan
// GNU General Public License v3.0+ (see LICENSE or https://www.gnu.org/licenses/gpl-3.0.txt)

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.ApplicationInsights;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ValhallaLootList.DataTransfer;
using ValhallaLootList.Helpers;
using ValhallaLootList.Server.Data;

namespace ValhallaLootList.Server.Controllers
{
    public class DropsController : ApiControllerV1
    {
        private readonly ApplicationDbContext _context;
        private readonly PrioCalculator _prioCalculator;
        private readonly TelemetryClient _telemetry;

        public DropsController(ApplicationDbContext context, PrioCalculator prioCalculator, TelemetryClient telemetry)
        {
            _context = context;
            _prioCalculator = prioCalculator;
            _telemetry = telemetry;
        }

        [HttpGet]
        public IAsyncEnumerable<WonDropDto> Get(long characterId)
        {
            return _context.Drops
                .AsNoTracking()
                .Where(drop => drop.WinnerId == characterId || drop.WinningEntry!.LootList.CharacterId == characterId)
                .OrderByDescending(drop => drop.AwardedAt)
                .Select(drop => new WonDropDto
                {
                    CharacterId = drop.WinnerId ?? characterId,
                    ItemId = drop.ItemId,
                    AwardedAt = drop.AwardedAt,
                    RaidId = drop.EncounterKillRaidId
                })
                .AsAsyncEnumerable();
        }

        [HttpPut("{id:long}"), Authorize(AppRoles.LootMaster)]
        public async Task<ActionResult<EncounterDropDto>> PutAssign(long id, [FromBody] AwardDropSubmissionDto dto, [FromServices] TimeZoneInfo realmTimeZoneInfo)
        {
            var now = realmTimeZoneInfo.TimeZoneNow();
            var drop = await _context.Drops.FindAsync(id);

            if (drop is null)
            {
                return NotFound();
            }

            var teamId = await _context.Raids
                .AsNoTracking()
                .Where(r => r.Id == drop.EncounterKillRaidId)
                .Select(r => (long?)r.RaidTeamId)
                .FirstOrDefaultAsync();

            if (!teamId.HasValue)
            {
                return NotFound();
            }

            if (!await _context.IsLeaderOf(User, teamId.Value))
            {
                return Unauthorized();
            }

            if (dto.WinnerId.HasValue && drop.WinnerId.HasValue)
            {
                ModelState.AddModelError(nameof(dto.WinnerId), "Existing winner must be cleared before setting a new winner.");
                return ValidationProblem();
            }

            drop.AwardedAt = now;
            drop.AwardedBy = User.GetDiscordId();

            var killers = await _context.CharacterEncounterKills
                .AsTracking()
                .Where(c => c.EncounterKillEncounterId == drop.EncounterKillEncounterId && c.EncounterKillRaidId == drop.EncounterKillRaidId)
                .Select(c => c.Character)
                .ToListAsync();

            await _context.Entry(drop).Collection(drop => drop.Passes).LoadAsync();
            drop.Passes.Clear();

            if (dto.WinnerId.HasValue)
            {
                var winner = killers.Find(k => k.Id == dto.WinnerId);

                if (winner is null)
                {
                    ModelState.AddModelError(nameof(dto.WinnerId), "Character was not present for the kill.");
                    return ValidationProblem();
                }

                var (winnerPrio, _, _, _) = await _prioCalculator.CalculatePrioAsync(winner.Id, drop.ItemId);

                killers.Remove(winner);

                drop.Winner = winner;
                drop.WinnerId = winner.Id;

                var lootListEntry = _context.LootListEntries
                    .AsTracking()
                    .Where(lle => (lle.ItemId == drop.ItemId || lle.Item!.RewardFromId == drop.ItemId) && lle.LootList.CharacterId == winner.Id && lle.DropId == null)
                    .OrderByDescending(lle => lle.Rank)
                    .FirstOrDefault();

                if (lootListEntry is not null)
                {
                    drop.WinningEntry = lootListEntry;
                    lootListEntry.Drop = drop;
                    lootListEntry.DropId = drop.Id;
                }

                foreach (var killer in killers)
                {
                    var (prio, locked, _, _) = await _prioCalculator.CalculatePrioAsync(killer.Id, drop.ItemId);

                    if (locked && prio.HasValue)
                    {
                        drop.Passes.Add(new DropPass
                        {
                            Character = killer,
                            CharacterId = killer.Id,
                            Drop = drop,
                            RelativePriority = prio.Value - (winnerPrio ?? 0)
                        });
                    }
                }
            }
            else
            {
                drop.Winner = null;
                drop.WinnerId = null;

                var lootListEntry = _context.LootListEntries
                    .AsTracking()
                    .Where(lle => lle.DropId == drop.Id)
                    .FirstOrDefault();

                if (lootListEntry is not null)
                {
                    lootListEntry.Drop = null;
                    lootListEntry.DropId = null;
                }
            }

            await _context.SaveChangesAsync();

            _telemetry.TrackEvent("DropAssigned", User, props =>
            {
                props["ItemId"] = drop.ItemId.ToString();
                props["DropId"] = drop.Id.ToString();

                if (drop.Winner is null)
                {
                    props["Unassigned"] = bool.TrueString;
                }
                else
                {
                    props["WinnerId"] = drop.Winner.Id.ToString();
                    props["Winner"] = drop.Winner.Name;
                }
            });

            return new EncounterDropDto
            {
                Id = drop.Id,
                AwardedAt = drop.AwardedAt,
                AwardedBy = drop.AwardedBy,
                ItemId = drop.ItemId,
                WinnerId = drop.WinnerId,
                WinnerName = drop.Winner?.Name
            };
        }

        [HttpGet("{id:long}/Ranks"), Authorize(AppRoles.LootMaster)]
        public async Task<ActionResult<List<ItemPrioDto>>> GetRanks(long id)
        {
            var drop = await _context.Drops.FindAsync(id);

            if (drop is null)
            {
                return NotFound();
            }

            var unequippableSpecs = Specializations.None;

            await foreach (var specs in _context.ItemRestrictions
                .AsNoTracking()
                .Where(r => r.ItemId == drop.ItemId && r.RestrictionLevel == ItemRestrictionLevel.Unequippable)
                .Select(r => r.Specializations)
                .AsAsyncEnumerable())
            {
                unequippableSpecs |= specs;
            }

            var teamId = await _context.Raids
                .AsNoTracking()
                .Where(r => r.Id == drop.EncounterKillRaidId)
                .Select(r => r.RaidTeamId)
                .FirstOrDefaultAsync();

            if (!await _context.IsLeaderOf(User, teamId))
            {
                return Unauthorized();
            }

            var killerIds = await _context.CharacterEncounterKills
                .AsNoTracking()
                .Where(c => c.EncounterKillEncounterId == drop.EncounterKillEncounterId && c.EncounterKillRaidId == drop.EncounterKillRaidId)
                .Select(c => new { Id = c.CharacterId, c.Character.Name, c.Character.Class })
                .ToListAsync();

            var dto = new List<ItemPrioDto>();

            foreach (var character in killerIds)
            {
                var prio = new ItemPrioDto
                {
                    CharacterId = character.Id,
                    CharacterName = character.Name
                };

                if ((unequippableSpecs & character.Class.ToSpecializations()) != 0)
                {
                    prio.Priority = null;
                    prio.IsError = true;
                    prio.Details = "Cannot equip this item!";
                }
                else
                {
                    (prio.Priority, _, prio.Details, prio.IsError) = await _prioCalculator.CalculatePrioAsync(character.Id, drop.ItemId);
                }

                dto.Add(prio);
            }

            return dto;
        }
    }
}
