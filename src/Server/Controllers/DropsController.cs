// Copyright (C) 2021 Donovan Sullivan
// GNU General Public License v3.0+ (see LICENSE or https://www.gnu.org/licenses/gpl-3.0.txt)

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ValhallaLootList.DataTransfer;
using ValhallaLootList.Server.Data;

namespace ValhallaLootList.Server.Controllers
{
    public class DropsController : ApiControllerV1
    {
        private readonly ApplicationDbContext _context;
        private readonly PrioCalculator _prioCalculator;

        public DropsController(ApplicationDbContext context, PrioCalculator prioCalculator)
        {
            _context = context;
            _prioCalculator = prioCalculator;
        }

        [HttpGet]
        public IAsyncEnumerable<WonDropDto> Get(string characterId)
        {
            return _context.Drops
                .AsNoTracking()
                .Where(drop => drop.WinnerId == characterId || drop.WinningEntry!.LootList.CharacterId == characterId)
                .OrderByDescending(drop => drop.AwardedAtUtc)
                .Select(drop => new WonDropDto
                {
                    CharacterId = drop.WinnerId ?? characterId,
                    ItemId = drop.ItemId,
                    AwardedAt = new DateTimeOffset(drop.AwardedAtUtc, TimeSpan.Zero),
                    RaidId = drop.EncounterKillRaidId
                })
                .AsAsyncEnumerable();
        }

        [HttpPut("{id}"), Authorize(AppRoles.LootMaster)]
        public async Task<ActionResult<EncounterDropDto>> PutAssign(string id, [FromBody] AwardDropSubmissionDto dto)
        {
            var now = DateTime.UtcNow;
            var drop = await _context.Drops.FindAsync(id);

            if (drop is null)
            {
                return NotFound();
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

            if (dto.WinnerId?.Length > 0 && drop.WinnerId?.Length > 0)
            {
                ModelState.AddModelError(nameof(dto.WinnerId), "Existing winner must be cleared before setting a new winner.");
                return ValidationProblem();
            }

            drop.AwardedAtUtc = now;
            drop.AwardedBy = User.GetAppUserId();

            var killers = await _context.CharacterEncounterKills
                .AsTracking()
                .Where(c => c.EncounterKillEncounterId == drop.EncounterKillEncounterId && c.EncounterKillRaidId == drop.EncounterKillRaidId)
                .Select(c => c.Character)
                .ToListAsync();

            await _context.Entry(drop).Collection(drop => drop.Passes).LoadAsync();
            drop.Passes.Clear();

            if (dto.WinnerId?.Length > 0)
            {
                var winner = killers.Find(k => k.Id == dto.WinnerId);

                if (winner is null)
                {
                    ModelState.AddModelError(nameof(dto.WinnerId), "Character was not present for the kill.");
                    return ValidationProblem();
                }

                var (winnerPrio, _, _) = await _prioCalculator.CalculatePrioAsync(winner.Id, drop.ItemId);

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
                    var (prio, locked, _) = await _prioCalculator.CalculatePrioAsync(killer.Id, drop.ItemId);

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

            return new EncounterDropDto
            {
                Id = drop.Id,
                AwardedAt = new DateTimeOffset(drop.AwardedAtUtc, TimeSpan.Zero),
                AwardedBy = drop.AwardedBy,
                ItemId = drop.ItemId,
                WinnerId = drop.WinnerId,
                WinnerName = drop.Winner?.Name
            };
        }

        [HttpGet("{id}/Ranks"), Authorize(AppRoles.LootMaster)]
        public async Task<ActionResult<List<ItemPrioDto>>> GetRanks(string id)
        {
            var drop = await _context.Drops.FindAsync(id);

            if (drop is null)
            {
                return NotFound();
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
                .Select(c => c.CharacterId)
                .ToListAsync();

            var dto = new List<ItemPrioDto>();

            foreach (var characterId in killerIds)
            {
                var (prio, _, details) = await _prioCalculator.CalculatePrioAsync(characterId, drop.ItemId);

                dto.Add(new ItemPrioDto
                {
                    CharacterId = characterId,
                    Priority = prio,
                    Details = details
                });
            }

            return dto;
        }
    }
}
