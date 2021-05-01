// Copyright (C) 2021 Donovan Sullivan
// GNU General Public License v3.0+ (see LICENSE or https://www.gnu.org/licenses/gpl-3.0.txt)

using System;
using System.Collections.Generic;
using System.Diagnostics;
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
        private readonly TelemetryClient _telemetry;

        public DropsController(ApplicationDbContext context, TelemetryClient telemetry)
        {
            _context = context;
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

        [HttpPut("{id:long}"), Authorize(AppPolicies.LootMaster)]
        public async Task<ActionResult<EncounterDropDto>> PutAssign(long id, [FromBody] AwardDropSubmissionDto dto, [FromServices] TimeZoneInfo realmTimeZoneInfo, [FromServices] IAuthorizationService auth)
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

            var authResult = await auth.AuthorizeAsync(User, teamId, AppPolicies.LootMaster);

            if (!authResult.Succeeded)
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
            var scope = await _context.GetCurrentPriorityScopeAsync();

            var presentTeamRaiders = await _context.CharacterEncounterKills
                .AsTracking()
                .Where(cek => cek.EncounterKillEncounterId == drop.EncounterKillEncounterId && cek.EncounterKillRaidId == drop.EncounterKillRaidId)
                .Select(c => new
                {
                    Id = c.CharacterId,
                    c.Character.TeamId,
                    c.Character.MemberStatus,
                    Attended = c.Character.Attendances.Where(x => !x.IgnoreAttendance && x.Raid.RaidTeamId == teamId && x.RemovalId == null)
                        .Select(x => x.Raid.StartedAt.Date)
                        .Distinct()
                        .OrderByDescending(x => x)
                        .Take(scope.ObservedAttendances)
                        .Count(),
                    Entry = _context.LootListEntries.Where(e => !e.DropId.HasValue && e.LootList.CharacterId == c.CharacterId && (e.ItemId == drop.ItemId || e.Item!.RewardFromId == drop.ItemId))
                        .OrderByDescending(e => e.Rank)
                        .Select(e => new
                        {
                            e.Id,
                            e.Rank,
                            e.LootList.Status,
                            Passes = e.Passes.Count(p => p.RemovalId == null)
                        })
                        .FirstOrDefault()
                })
                .ToListAsync();

            var donationMatrix = await _context.GetDonationMatrixAsync(d => d.Character.Attendances.Any(a => a.RaidId == drop.EncounterKillRaidId), scope);

            await _context.Entry(drop).Collection(drop => drop.Passes).LoadAsync();
            drop.Passes.Clear();

            if (dto.WinnerId.HasValue)
            {
                var winner = presentTeamRaiders.Find(e => e.Id == dto.WinnerId);

                if (winner is null)
                {
                    ModelState.AddModelError(nameof(dto.WinnerId), "Character was not present for the kill.");
                    return ValidationProblem();
                }

                int? winnerPrio = null;

                if (winner.Entry is not null)
                {
                    winnerPrio = winner.Entry.Rank;

                    var passes = await _context.DropPasses
                        .AsTracking()
                        .Where(p => p.LootListEntryId == winner.Entry.Id && p.RemovalId == null)
                        .ToListAsync();

                    Debug.Assert(passes.Count == winner.Entry.Passes);

                    var donated = donationMatrix.GetCreditForMonth(winner.Id, now);

                    foreach (var bonus in PrioCalculator.GetAllBonuses(scope, winner.Attended, winner.MemberStatus, donated, passes.Count))
                    {
                        winnerPrio = winnerPrio.Value + bonus.Value;
                    }

                    drop.WinningEntry = await _context.LootListEntries.FindAsync(winner.Entry.Id);
                    drop.WinningEntry.Drop = drop;
                    drop.WinningEntry.DropId = drop.Id;

                    foreach (var pass in passes)
                    {
                        pass.WonEntryId = winner.Entry.Id;
                    }
                }

                drop.WinnerId = winner.Id;

                foreach (var killer in presentTeamRaiders)
                {
                    if (killer.Entry is not null && killer.TeamId == teamId && killer != winner)
                    {
                        var thisPrio = (int)killer.Entry.Rank;

                        var donated = donationMatrix.GetCreditForMonth(killer.Id, now);

                        foreach (var bonus in PrioCalculator.GetAllBonuses(scope, killer.Attended, killer.MemberStatus, donated, killer.Entry.Passes))
                        {
                            thisPrio += bonus.Value;
                        }

                        _context.DropPasses.Add(new DropPass
                        {
                            Drop = drop,
                            DropId = drop.Id,
                            CharacterId = killer.Id,
                            RelativePriority = thisPrio - (winnerPrio ?? 0),
                            LootListEntryId = killer.Entry.Id
                        });
                    }
                }
            }
            else
            {
                var oldWinningEntry = await _context.LootListEntries
                    .AsTracking()
                    .Where(e => e.DropId == drop.Id)
                    .SingleOrDefaultAsync();

                drop.Winner = null;
                drop.WinnerId = null;
                drop.WinningEntry = null;

                if (oldWinningEntry is not null)
                {
                    oldWinningEntry.Drop = null;
                    oldWinningEntry.DropId = null;

                    await foreach (var pass in _context.DropPasses
                        .AsTracking()
                        .Where(pass => pass.WonEntryId == oldWinningEntry.Id)
                        .AsAsyncEnumerable())
                    {
                        pass.WonEntryId = null;
                    }
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
                WinnerId = drop.WinnerId
            };
        }

        [HttpGet("{id:long}/Ranks"), Authorize(AppPolicies.LootMaster)]
        public async Task<ActionResult<List<ItemPrioDto>>> GetRanks(long id, [FromServices] IAuthorizationService auth, [FromServices] TimeZoneInfo serverTimeZone)
        {
            var drop = await _context.Drops
                .AsNoTracking()
                .Where(d => d.Id == id)
                .Select(d => new
                {
                    TeamId = d.EncounterKill.Raid.RaidTeamId,
                    d.EncounterKillRaidId,
                    d.EncounterKillEncounterId,
                    d.ItemId
                })
                .FirstOrDefaultAsync();

            if (drop is null)
            {
                return NotFound();
            }

            var authResult = await auth.AuthorizeAsync(User, drop.TeamId, AppPolicies.LootMaster);

            if (!authResult.Succeeded)
            {
                return Unauthorized();
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

            var now = serverTimeZone.TimeZoneNow();
            var scope = await _context.GetCurrentPriorityScopeAsync();

            var presentTeamRaiders = await _context.RaidAttendees
                .AsNoTracking()
                .Where(a => a.RaidId == drop.EncounterKillRaidId && a.Character.TeamId == drop.TeamId)
                .Select(c => new
                {
                    Id = c.CharacterId,
                    c.Character.Name,
                    c.Character.TeamId,
                    c.Character.MemberStatus,
                    Attended = c.Character.Attendances.Where(x => !x.IgnoreAttendance && x.Raid.RaidTeamId == drop.TeamId && x.RemovalId == null)
                        .Select(x => x.Raid.StartedAt.Date)
                        .Distinct()
                        .OrderByDescending(x => x)
                        .Take(scope.ObservedAttendances)
                        .Count(),
                    Entry = _context.LootListEntries.Where(e => !e.DropId.HasValue && e.LootList.CharacterId == c.CharacterId && (e.ItemId == drop.ItemId || e.Item!.RewardFromId == drop.ItemId))
                        .OrderByDescending(e => e.Rank)
                        .Select(e => new
                        {
                            e.Id,
                            e.Rank,
                            e.LootList.Status,
                            Passes = e.Passes.Count(p => p.RemovalId == null)
                        })
                        .FirstOrDefault()
                })
                .ToListAsync();

            var donationMatrix = await _context.GetDonationMatrixAsync(d => d.Character.Attendances.Any(a => a.RaidId == drop.EncounterKillRaidId), scope);

            var dto = new List<ItemPrioDto>();

            foreach (var killer in presentTeamRaiders.Where(c => c.Entry is not null))
            {
                Debug.Assert(killer.Entry is not null);
                var prio = new ItemPrioDto
                {
                    CharacterId = killer.Id,
                    CharacterName = killer.Name,
                    Status = killer.Entry.Status,
                    Rank = killer.Entry.Rank,
                };

                var donated = donationMatrix.GetCreditForMonth(killer.Id, now);

                prio.Bonuses.AddRange(PrioCalculator.GetAllBonuses(scope, killer.Attended, killer.MemberStatus, donated, killer.Entry.Passes));

                dto.Add(prio);
            }

            return dto;
        }
    }
}
