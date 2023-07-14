// Copyright (C) 2021 Donovan Sullivan
// GNU General Public License v3.0+ (see LICENSE or https://www.gnu.org/licenses/gpl-3.0.txt)

using Microsoft.ApplicationInsights;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ValhallaLootList.DataTransfer;
using ValhallaLootList.Helpers;
using ValhallaLootList.Server.Data;
using ValhallaLootList.Server.Discord;

namespace ValhallaLootList.Server.Controllers;

public class DropsController : ApiControllerV1
{
    private readonly ApplicationDbContext _context;
    private readonly TelemetryClient _telemetry;
    private readonly TimeZoneInfo _realmTimeZone;
    private readonly IAuthorizationService _authorizationService;

    public DropsController(ApplicationDbContext context, TelemetryClient telemetry, TimeZoneInfo realmTimeZone, IAuthorizationService authorizationService)
    {
        _context = context;
        _telemetry = telemetry;
        _realmTimeZone = realmTimeZone;
        _authorizationService = authorizationService;
    }

    [HttpGet]
    public IAsyncEnumerable<WonDropDto> Get(long characterId)
    {
        return _context.Drops
            .AsNoTracking()
            .Where(drop => !drop.Disenchanted && (drop.WinnerId == characterId || drop.WinningEntry!.LootList.CharacterId == characterId))
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

    [HttpGet("audit"), Authorize(AppPolicies.Administrator)]
    public IAsyncEnumerable<AuditDropDto> Audit()
    {
        return _context.Drops
            .AsNoTracking()
            .Where(drop => drop.WinnerId == null && drop.EncounterKill.Raid.LocksAt < DateTimeOffset.UtcNow)
            .OrderByDescending(drop => drop.Id)
            .Select(drop => new AuditDropDto
            {
                DropId = drop.Id,
                ItemId = drop.ItemId,
                ItemName = drop.Item.Name,
                RaidId = drop.EncounterKillRaidId,
                RaidDate = drop.EncounterKill.Raid.StartedAt,
                TeamName = drop.EncounterKill.Raid.RaidTeam.Name
            })
            .AsAsyncEnumerable();
    }

    [HttpPut("{id:long}"), Authorize(AppPolicies.LootMaster)]
    public async Task<ActionResult<EncounterDropDto>> PutAssign(long id, [FromBody] AwardDropSubmissionDto dto, [FromServices] MessageSender messageSender)
    {
        var now = _realmTimeZone.TimeZoneNow();
        var drop = await _context.Drops.FindAsync(id);

        if (drop is null)
        {
            return NotFound();
        }

        var raid = await _context.Raids
            .AsNoTracking()
            .Where(r => r.Id == drop.EncounterKillRaidId)
            .Select(r => new { r.RaidTeamId, r.LocksAt })
            .FirstOrDefaultAsync();

        if (raid is null)
        {
            return NotFound();
        }

        var authResult = await _authorizationService.AuthorizeAsync(User, raid.RaidTeamId, AppPolicies.LootMaster);

        if (!authResult.Succeeded)
        {
            return Unauthorized();
        }

        if (DateTimeOffset.UtcNow > raid.LocksAt)
        {
            return Problem("Can't alter a locked raid.");
        }

        if (dto.WinnerId.HasValue && drop.WinnerId.HasValue)
        {
            ModelState.AddModelError(nameof(dto.WinnerId), "Existing winner must be cleared before setting a new winner.");
            return ValidationProblem();
        }

        drop.AwardedAt = now;
        drop.AwardedBy = User.GetDiscordId();

        if (dto.WinnerId.HasValue)
        {
            var winner = await _context.CharacterEncounterKills
                .AsNoTracking()
                .Where(cek => cek.CharacterId == dto.WinnerId.Value && cek.EncounterKillEncounterId == drop.EncounterKillEncounterId && cek.EncounterKillRaidId == drop.EncounterKillRaidId && cek.EncounterKillTrashIndex == drop.EncounterKillTrashIndex)
                .Select(cek => new { cek.Character.Id, cek.Character.Name })
                .FirstOrDefaultAsync();

            if (winner is null)
            {
                ModelState.AddModelError(nameof(dto.WinnerId), "Character was not present for the kill.");
                return ValidationProblem();
            }

            var topEntry = await _context.LootListEntries
                .AsTracking()
                .Where(e => e.LootList.CharacterId == winner.Id && !e.DropId.HasValue && (e.ItemId == drop.ItemId || e.Item!.RewardFromId == drop.ItemId))
                .OrderByDescending(e => e.Rank)
                .ThenBy(e => e.Id)
                .FirstOrDefaultAsync();

            if (topEntry is not null)
            {
                drop.WinningEntry = topEntry;
                topEntry.Drop = drop;
                topEntry.DropId = drop.Id;
            }

            drop.WinnerId = winner.Id;
            drop.Disenchanted = dto.Disenchant;
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
            drop.Disenchanted = false;

            if (oldWinningEntry is not null)
            {
                oldWinningEntry.Drop = null;
                oldWinningEntry.DropId = null;
            }
        }

        await _context.SaveChangesAsync();

        await messageSender.SendKillMessageAsync(drop.EncounterKillRaidId, drop.EncounterKillEncounterId, drop.EncounterKillTrashIndex);

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
            Disenchanted = drop.Disenchanted
        };
    }

    [HttpGet("{id:long}/Ranks"), Authorize(AppPolicies.LootMaster)]
    public async Task<ActionResult<List<ItemPrioDto>>> GetRanks(long id)
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

        var authResult = await _authorizationService.AuthorizeAsync(User, drop.TeamId, AppPolicies.LootMaster);

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

        var bonusTable = await _context.GetBonusTableAsync(drop.TeamId, _realmTimeZone.TimeZoneNow());

        var presentMembers = await _context.TeamMembers
            .AsNoTracking()
            .Where(tm => tm.TeamId == drop.TeamId)
            .Select(tm => new
            {
                Id = tm.CharacterId,
                tm.Character!.Name,
                tm.Enchanted,
                tm.Prepared,
                tm.JoinedAt
            })
            .ToListAsync();

        var allEntries = await _context.LootListEntries
            .AsNoTracking()
            .Where(e => !e.DropId.HasValue && !e.AutoPass && (e.ItemId == drop.ItemId || e.Item!.RewardFromId == drop.ItemId) && e.LootList.Character.Teams.Any(t => t.TeamId == drop.TeamId))
            .Select(e => new
            {
                e.Id,
                e.Rank,
                e.LootList.Status,
                e.LootList.CharacterId
            })
            .ToListAsync();

        var allPasses = await _context.Drops
            .AsNoTracking()
            .Where(d => (d.ItemId == drop.ItemId || d.Item!.RewardFromId == drop.ItemId) && d.EncounterKill.Raid.RaidTeamId == drop.TeamId)
            .Select(d => new
            {
                d.EncounterKill.KilledAt,
                Characters = d.EncounterKill.Characters.Select(c => c.CharacterId).ToList()
            })
            .ToListAsync();

        var dto = new List<ItemPrioDto>();
        foreach (var killer in presentMembers)
        {
            var topEntry = allEntries.Where(e => e.CharacterId == killer.Id).OrderByDescending(e => e.Rank).FirstOrDefault();

            if (topEntry is not null)
            {
                var prio = new ItemPrioDto
                {
                    CharacterId = killer.Id,
                    CharacterName = killer.Name,
                    Status = topEntry.Status,
                    Rank = topEntry.Rank,
                };

                if (bonusTable.TryGetValue(killer.Id, out var bonuses))
                {
                    prio.Bonuses.AddRange(bonuses);
                }

                prio.Bonuses.AddRange(PrioCalculator.GetItemBonuses(allPasses.Count(p => p.KilledAt >= killer.JoinedAt && p.Characters.Contains(killer.Id))));

                dto.Add(prio);
            }
        }

        return dto;
    }
}
