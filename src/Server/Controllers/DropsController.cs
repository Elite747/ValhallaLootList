// Copyright (C) 2021 Donovan Sullivan
// GNU General Public License v3.0+ (see LICENSE or https://www.gnu.org/licenses/gpl-3.0.txt)

using System.Diagnostics;
using System.Linq.Expressions;
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
        var teamId = raid.RaidTeamId;

        var bonusTable = await _context.GetBonusTableAsync(teamId, now);

        var presentCharacters = await _context.CharacterEncounterKills
            .AsNoTracking()
            .Where(cek => cek.EncounterKillEncounterId == drop.EncounterKillEncounterId && cek.EncounterKillRaidId == drop.EncounterKillRaidId && cek.EncounterKillTrashIndex == drop.EncounterKillTrashIndex)
            .Select(cek => new { cek.Character.Id, cek.Character.Name })
            .ToListAsync();

        var presentMembers = await _context.TeamMembers
            .AsNoTracking()
            .Where(tm => tm.TeamId == raid.RaidTeamId)
            .Select(ConvertToDropInfo(drop.ItemId))
            .ToListAsync();

        await _context.Entry(drop).Collection(drop => drop.Passes).LoadAsync();
        drop.Passes.Clear();

        if (dto.WinnerId.HasValue)
        {
            var winner = presentCharacters.Find(e => e.Id == dto.WinnerId);

            if (winner is null)
            {
                ModelState.AddModelError(nameof(dto.WinnerId), "Character was not present for the kill.");
                return ValidationProblem();
            }

            var member = presentMembers.Find(m => m.Id == dto.WinnerId);
            int? winnerPrio = null;

            if (member?.Entry is not null)
            {
                winnerPrio = member.Entry.Rank;

                var passes = await _context.DropPasses
                    .AsTracking()
                    .Where(p => p.LootListEntryId == member.Entry.Id && p.RemovalId == null)
                    .ToListAsync();

                Debug.Assert(passes.Count == member.Entry.Passes);

                if (bonusTable.TryGetValue(member.Id, out var bonuses))
                {
                    winnerPrio = winnerPrio.Value + bonuses.Sum(b => b.Value);
                }

                winnerPrio = winnerPrio.Value + PrioCalculator.GetItemBonuses(passes.Count).Sum(b => b.Value);

                drop.WinningEntry = await _context.LootListEntries.FindAsync(member.Entry.Id);
                Debug.Assert(drop.WinningEntry is not null);
                drop.WinningEntry.Drop = drop;
                drop.WinningEntry.DropId = drop.Id;

                foreach (var pass in passes)
                {
                    pass.WonEntryId = member.Entry.Id;
                }
            }

            drop.WinnerId = winner.Id;
            drop.Disenchanted = dto.Disenchant;

            foreach (var killer in presentMembers)
            {
                if (killer.Entry is not null && killer.TeamId == teamId && killer != member)
                {
                    var thisPrio = killer.Entry.Rank;

                    if (bonusTable.TryGetValue(killer.Id, out var bonuses))
                    {
                        thisPrio += bonuses.Sum(b => b.Value);
                    }

                    thisPrio += PrioCalculator.GetItemBonuses(killer.Entry.Passes).Sum(b => b.Value);

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
            drop.Disenchanted = false;

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
            .Select(ConvertToDropInfo(drop.ItemId))
            .ToListAsync();

        var dto = new List<ItemPrioDto>();

        foreach (var killer in presentMembers.Where(c => c.Entry?.AutoPass == false))
        {
            Debug.Assert(killer.Entry is not null);
            var prio = new ItemPrioDto
            {
                CharacterId = killer.Id,
                CharacterName = killer.Name,
                Status = killer.Entry.Status,
                Rank = killer.Entry.Rank,
            };

            if (bonusTable.TryGetValue(killer.Id, out var bonuses))
            {
                prio.Bonuses.AddRange(bonuses);
            }

            prio.Bonuses.AddRange(PrioCalculator.GetItemBonuses(killer.Entry.Passes));

            dto.Add(prio);
        }

        return dto;
    }

    private class CharacterDropInfo
    {
        public long Id { get; init; }
        public string Name { get; init; } = string.Empty;
        public long? TeamId { get; init; }
        public RaidMemberStatus MemberStatus { get; init; }
        public bool Enchanted { get; init; }
        public bool Prepared { get; init; }
        public TargetEntry? Entry { get; init; }
    }

    private class TargetEntry
    {
        public long Id { get; init; }
        public int Rank { get; init; }
        public LootListStatus Status { get; init; }
        public int Passes { get; init; }
        public bool AutoPass { get; init; }
    }

    private Expression<Func<TeamMember, CharacterDropInfo>> ConvertToDropInfo(uint itemId) => member => new()
    {
        Id = member.CharacterId,
        Name = member.Character!.Name,
        TeamId = member.TeamId,
        MemberStatus = member.MemberStatus,
        Enchanted = member.Enchanted,
        Prepared = member.Prepared,
        Entry = _context.LootListEntries.Where(e => !e.DropId.HasValue && e.LootList.CharacterId == member.Character.Id && (e.ItemId == itemId || e.Item!.RewardFromId == itemId))
            .OrderByDescending(e => e.Rank)
            .Select(e => new TargetEntry
            {
                Id = e.Id,
                Rank = e.Rank,
                Status = e.LootList.Status,
                Passes = e.Passes.Count(p => p.RemovalId == null),
                AutoPass = e.AutoPass
            })
            .FirstOrDefault()
    };
}
