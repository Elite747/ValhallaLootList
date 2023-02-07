// Copyright (C) 2021 Donovan Sullivan
// GNU General Public License v3.0+ (see LICENSE or https://www.gnu.org/licenses/gpl-3.0.txt)

using System.Diagnostics;
using System.Linq.Expressions;
using IdentityServer4.EntityFramework.Entities;
using IdentityServer4.EntityFramework.Extensions;
using IdentityServer4.EntityFramework.Interfaces;
using IdentityServer4.EntityFramework.Options;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using ValhallaLootList.DataTransfer;

namespace ValhallaLootList.Server.Data;

public class ApplicationDbContext : IdentityDbContext<AppUser, IdentityRole<long>, long>, IPersistedGrantDbContext
{
    private readonly OperationalStoreOptions _operationalStoreOptions;

    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options, IOptions<OperationalStoreOptions> operationalStoreOptions) : base(options)
    {
        _operationalStoreOptions = operationalStoreOptions.Value;
    }

    public ApplicationDbContext(string connectionString)
        : base(new DbContextOptionsBuilder<ApplicationDbContext>().UseSqlServer(connectionString).Options)
    {
        _operationalStoreOptions = new OperationalStoreOptions();
    }

    public virtual DbSet<PersistedGrant> PersistedGrants { get; set; } = null!;
    public virtual DbSet<DeviceFlowCodes> DeviceFlowCodes { get; set; } = null!;
    public virtual DbSet<Donation> Donations { get; set; } = null!;
    public virtual DbSet<Encounter> Encounters { get; set; } = null!;
    public virtual DbSet<EncounterItem> EncounterItems { get; set; } = null!;
    public virtual DbSet<EncounterKill> EncounterKills { get; set; } = null!;
    public virtual DbSet<CharacterEncounterKill> CharacterEncounterKills { get; set; } = null!;
    public virtual DbSet<Character> Characters { get; set; } = null!;
    public virtual DbSet<CharacterLootList> CharacterLootLists { get; set; } = null!;
    public virtual DbSet<LootListEntry> LootListEntries { get; set; } = null!;
    public virtual DbSet<LootListTeamSubmission> LootListTeamSubmissions { get; set; } = null!;
    public virtual DbSet<Drop> Drops { get; set; } = null!;
    [Obsolete("Passes counted from drops instead.")]
    public virtual DbSet<DropPass> DropPasses { get; set; } = null!;
    public virtual DbSet<Instance> Instances { get; set; } = null!;
    public virtual DbSet<Item> Items { get; set; } = null!;
    public virtual DbSet<ItemRestriction> ItemRestrictions { get; set; } = null!;
    public virtual DbSet<Raid> Raids { get; set; } = null!;
    public virtual DbSet<RaidAttendee> RaidAttendees { get; set; } = null!;
    public virtual DbSet<RaidTeam> RaidTeams { get; set; } = null!;
    public virtual DbSet<TeamMember> TeamMembers { get; set; } = null!;
    public virtual DbSet<RaidTeamLeader> RaidTeamLeaders { get; set; } = null!;
    public virtual DbSet<RaidTeamSchedule> RaidTeamSchedules { get; set; } = null!;
    public virtual DbSet<TeamRemoval> TeamRemovals { get; set; } = null!;
    public virtual DbSet<Bracket> Brackets { get; set; } = null!;
    public virtual DbSet<PhaseDetails> PhaseDetails { get; set; } = null!;

    public async Task<List<DateTime>> GetObservedRaidDatesAsync(long teamId, int observedAttendances)
    {
        return await Raids
            .AsNoTracking()
            .Where(r => r.RaidTeamId == teamId)
            .Select(r => r.StartedAt.Date)
            .Distinct()
            .OrderByDescending(date => date)
            .Take(observedAttendances)
            .ToListAsync();
    }

    public async Task<Dictionary<long, List<PriorityBonusDto>>> GetBonusTableAsync(long teamId, DateTimeOffset date, long? characterId = null)
    {
        if (date.Hour < 3)
        {
            // as a correction for when raids run past midnight, treat dates passed in here within 2 hours of midnight as the previous day.
            // This way bonuses don't change at midnight when a raid is still likely to be running.
            date = date.AddHours(-date.Hour - 1);
        }

        var team = await RaidTeams.FindAsync(teamId);

        Debug.Assert(team is not null);

        var currentPhaseStart = await PhaseDetails.Where(p => p.StartsAt <= date).OrderByDescending(p => p.StartsAt).Select(p => p.StartsAt).FirstAsync();

        var attendanceRecords = await RaidAttendees.AsNoTracking()
            .Where(a => a.RemovalId == null && !a.IgnoreAttendance && a.Raid.RaidTeamId == teamId && (characterId == null || a.CharacterId == characterId))
            .Select(a => new { a.CharacterId, a.Raid.StartedAt })
            .ToListAsync();

        var raidsThisPhase = await Raids.AsNoTracking()
            .Where(r => r.RaidTeamId == teamId && r.StartedAt >= currentPhaseStart)
            .Select(r => r.StartedAt)
            .ToListAsync();

        var results = new Dictionary<long, List<PriorityBonusDto>>();

        await foreach (var member in TeamMembers.AsNoTracking()
            .Where(tm => tm.TeamId == teamId && (characterId == null || tm.CharacterId == characterId))
            .Select(tm => new
            {
                tm.CharacterId,
                tm.JoinedAt,
                tm.Prepared,
                tm.Enchanted,
                tm.Bench,
                tm.OverrideStatus,
                Donations = tm.Character!.Donations.Count(d => d.TargetMonth == date.Month && d.TargetYear == date.Year)
            })
            .AsAsyncEnumerable())
        {
            var bonuses = results[member.CharacterId] = new();

            int attendancesThisPhase = 0, attendancesTotal = 0;

            foreach (var attendanceRecord in attendanceRecords)
            {
                if (attendanceRecord.CharacterId == member.CharacterId && attendanceRecord.StartedAt >= member.JoinedAt && attendanceRecord.StartedAt.Date < date.Date)
                {
                    attendancesTotal++;
                    if (attendanceRecord.StartedAt >= currentPhaseStart)
                    {
                        attendancesThisPhase++;
                    }
                }
            }

            bonuses.AddRange(PrioCalculator.GetListBonuses(
                absences: raidsThisPhase.Count(raidDate => raidDate >= member.JoinedAt && raidDate.Date < date.Date) - attendancesThisPhase,
                attendances: attendancesTotal,
                donationTickets: member.Donations,
                enchanted: member.Enchanted,
                prepared: member.Prepared,
                bench: member.Bench,
                teamSize: team.TeamSize,
                overrideStatus: member.OverrideStatus));
        }

        return results;
    }

    public async Task<bool> PhaseActiveAsync(byte phase)
    {
        var phaseDetails = await PhaseDetails.FindAsync(phase);
        return phaseDetails is not null && phaseDetails.StartsAt <= DateTimeOffset.UtcNow;
    }

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        builder.ConfigurePersistedGrantContext(_operationalStoreOptions);

        builder.Entity<Character>(e =>
        {
            e.HasIndex(character => character.Name).IsUnique();
            e.Property(character => character.Id).ValueGeneratedNever();
            e.HasOne(character => character.Owner).WithMany().OnDelete(DeleteBehavior.SetNull);
        });

        builder.Entity<TeamMember>(e =>
        {
            e.HasKey(tm => new { tm.TeamId, tm.CharacterId });
            e.HasOne(tm => tm.Team).WithMany(team => team.Roster).OnDelete(DeleteBehavior.Cascade);
            e.HasOne(tm => tm.Character).WithMany(character => character.Teams).OnDelete(DeleteBehavior.Cascade);
        });

        builder.Entity<CharacterEncounterKill>(e =>
        {
            e.ToTable("CharacterEncounterKill");
            e.HasKey(cek => new { cek.EncounterKillRaidId, cek.EncounterKillEncounterId, cek.EncounterKillTrashIndex, cek.CharacterId });
            e.HasOne(cek => cek.Character).WithMany(c => c.EncounterKills).OnDelete(DeleteBehavior.Cascade);
            e.HasOne(cek => cek.EncounterKill).WithMany(ek => ek.Characters).OnDelete(DeleteBehavior.Cascade);
        });

        builder.Entity<CharacterLootList>(e =>
        {
            e.HasKey(ll => new { ll.CharacterId, ll.Phase, ll.Size });
            e.HasOne(ll => ll.Character).WithMany(c => c.CharacterLootLists).OnDelete(DeleteBehavior.Cascade);
        });

        builder.Entity<Drop>(e =>
        {
            e.Property(drop => drop.Id).ValueGeneratedNever();
            e.HasOne(drop => drop.EncounterKill).WithMany(ek => ek.Drops).OnDelete(DeleteBehavior.Cascade);
            e.HasOne(drop => drop.Item).WithMany(item => item.Drops).OnDelete(DeleteBehavior.Restrict);
            e.HasOne(drop => drop.Winner).WithMany().OnDelete(DeleteBehavior.Restrict);
            e.HasOne(drop => drop.WinningEntry).WithOne(entry => entry!.Drop!).HasForeignKey<LootListEntry>(entry => entry.DropId).OnDelete(DeleteBehavior.Restrict);
        });

        builder.Entity<Donation>(e =>
        {
            e.Property(donation => donation.Id).ValueGeneratedNever();
            e.HasOne(donation => donation.Character).WithMany(c => c.Donations).OnDelete(DeleteBehavior.Cascade);
        });

#pragma warning disable CS0618 // Type or member is obsolete
        builder.Entity<DropPass>(e =>
        {
            e.HasKey(e => new { e.DropId, e.CharacterId });
            e.HasOne(dp => dp.Drop).WithMany(d => d.Passes).OnDelete(DeleteBehavior.Cascade);
            e.HasOne(dp => dp.LootListEntry).WithMany(lle => lle!.Passes).OnDelete(DeleteBehavior.Restrict);
            e.HasOne(dp => dp.Removal).WithMany().OnDelete(DeleteBehavior.SetNull);
        });
#pragma warning restore CS0618 // Type or member is obsolete

        builder.Entity<Encounter>(e =>
        {
            e.Property(encounter => encounter.Id).ValueGeneratedNever();
            e.HasOne(encounter => encounter.Instance).WithMany(i => i.Encounters).OnDelete(DeleteBehavior.Restrict);
        });

        builder.Entity<EncounterItem>(e =>
        {
            e.HasKey(ei => new { ei.EncounterId, ei.ItemId, ei.Heroic, ei.Is25 });
            e.HasOne(ei => ei.Encounter).WithMany(encounter => encounter.Items).OnDelete(DeleteBehavior.Cascade);
            e.HasOne(ei => ei.Item).WithMany(item => item.Encounters).OnDelete(DeleteBehavior.Cascade);
        });

        builder.Entity<EncounterKill>(e =>
        {
            e.HasKey(kill => new { kill.EncounterId, kill.RaidId, kill.TrashIndex });
            e.HasOne(ek => ek.Encounter).WithMany().OnDelete(DeleteBehavior.Restrict);
            e.HasOne(ek => ek.Raid).WithMany(r => r.Kills).OnDelete(DeleteBehavior.Cascade);
        });

        builder.Entity<Instance>().Property(instance => instance.Id).ValueGeneratedNever();

        builder.Entity<Item>().Property(item => item.Id).ValueGeneratedNever();

        builder.Entity<LootListTeamSubmission>(e =>
        {
            e.HasKey(sub => new { sub.LootListCharacterId, sub.LootListPhase, sub.TeamId });
            e.HasOne(sub => sub.LootList).WithMany(ll => ll.Submissions).OnDelete(DeleteBehavior.Cascade);
            e.HasOne(sub => sub.Team).WithMany(team => team.Submissions).OnDelete(DeleteBehavior.Cascade);
        });

        builder.Entity<RaidAttendee>(e =>
        {
            e.HasKey(attendee => new { attendee.CharacterId, attendee.RaidId });
            e.HasOne(a => a.Character).WithMany(c => c.Attendances).OnDelete(DeleteBehavior.Restrict);
            e.HasOne(a => a.Raid).WithMany(r => r.Attendees).OnDelete(DeleteBehavior.Cascade);
            e.HasOne(a => a.Removal).WithMany().OnDelete(DeleteBehavior.SetNull);
        });

        builder.Entity<Raid>(e =>
        {
            e.Property(raid => raid.Id).ValueGeneratedNever();
            e.HasOne(r => r.RaidTeam).WithMany(t => t.Raids).OnDelete(DeleteBehavior.Restrict);
        });

        builder.Entity<RaidTeam>(e =>
        {
            e.HasIndex(raid => raid.Name).IsUnique();
            e.Property(team => team.Id).ValueGeneratedNever();
        });

        builder.Entity<RaidTeamSchedule>(e =>
        {
            e.Property(schedule => schedule.Id).ValueGeneratedNever();
            e.HasOne(schedule => schedule.RaidTeam).WithMany(team => team.Schedules).OnDelete(DeleteBehavior.Cascade);
        });

        builder.Entity<RaidTeamLeader>(e =>
        {
            e.HasKey(rtl => new { rtl.RaidTeamId, rtl.UserId });
            e.HasOne(rtl => rtl.RaidTeam).WithMany(team => team.Leaders).OnDelete(DeleteBehavior.Cascade);
            e.HasOne(rtl => rtl.User).WithMany().OnDelete(DeleteBehavior.Cascade);
        });

        builder.Entity<ItemRestriction>(e =>
        {
            e.Property(restriction => restriction.Id).ValueGeneratedNever();
            e.HasIndex(restriction => new { restriction.RestrictionLevel, restriction.Specializations });
            e.HasOne(restriction => restriction.Item).WithMany(item => item.Restrictions).OnDelete(DeleteBehavior.Cascade);
        });

        builder.Entity<LootListEntry>(e =>
        {
            e.Property(entry => entry.Id).ValueGeneratedNever();
            e.HasOne(entry => entry.LootList).WithMany(ll => ll.Entries).OnDelete(DeleteBehavior.Cascade);
        });

        builder.Entity<TeamRemoval>(e =>
        {
            e.Property(removal => removal.Id).ValueGeneratedNever();
            e.HasOne(removal => removal.Team).WithMany(team => team.Removals).OnDelete(DeleteBehavior.Restrict);
            e.HasOne(removal => removal.Character).WithMany(character => character.Removals).OnDelete(DeleteBehavior.Restrict);
        });

        builder.Entity<Bracket>(e =>
        {
            e.HasKey(bracket => new { bracket.Phase, bracket.Index });
            e.HasData(
                // Phase 1 brackets
                new Bracket(phase: 1, index: 0, minRank: 21, maxRank: 24, normalItems: 1, heroicItems: 0, allowOffspec: false, allowTypeDuplicates: false),
                new Bracket(phase: 1, index: 1, minRank: 17, maxRank: 20, normalItems: 2, heroicItems: 0, allowOffspec: false, allowTypeDuplicates: false),
                new Bracket(phase: 1, index: 2, minRank: 13, maxRank: 16, normalItems: 2, heroicItems: 0, allowOffspec: false, allowTypeDuplicates: false),
                new Bracket(phase: 1, index: 3, minRank: 01, maxRank: 12, normalItems: 2, heroicItems: 0, allowOffspec: true, allowTypeDuplicates: true)
                // Phase 2 brackets
                //new Bracket(phase: 2, index: 0, minRank: 21, maxRank: 24, normalItems: 1, heroicItems: 1, allowOffspec: false, allowTypeDuplicates: false),
                //new Bracket(phase: 2, index: 1, minRank: 17, maxRank: 20, normalItems: 1, heroicItems: 1, allowOffspec: false, allowTypeDuplicates: false),
                //new Bracket(phase: 2, index: 2, minRank: 13, maxRank: 16, normalItems: 1, heroicItems: 1, allowOffspec: false, allowTypeDuplicates: false),
                //new Bracket(phase: 2, index: 3, minRank: 01, maxRank: 12, normalItems: 1, heroicItems: 1, allowOffspec: true, allowTypeDuplicates: true)
                );
        });

        builder.Entity<PhaseDetails>(e =>
        {
            e.Property(phase => phase.Id).ValueGeneratedNever();
            e.HasData(
                new PhaseDetails(id: 1, startsAt: default)
                //new PhaseDetails(id: 2, startsAt: DateTime.MaxValue)
                );
        });

        builder.Entity<AppUser>().Property(e => e.Id).ValueGeneratedNever();
    }

    Task<int> IPersistedGrantDbContext.SaveChangesAsync() => base.SaveChangesAsync();
}
