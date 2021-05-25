// Copyright (C) 2021 Donovan Sullivan
// GNU General Public License v3.0+ (see LICENSE or https://www.gnu.org/licenses/gpl-3.0.txt)

using System;
using System.Linq;
using System.Linq.Expressions;
using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;
using IdentityServer4.EntityFramework.Entities;
using IdentityServer4.EntityFramework.Extensions;
using IdentityServer4.EntityFramework.Interfaces;
using IdentityServer4.EntityFramework.Options;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

namespace ValhallaLootList.Server.Data
{
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
        public virtual DbSet<EncounterKill> EncounterKills { get; set; } = null!;
        public virtual DbSet<CharacterEncounterKill> CharacterEncounterKills { get; set; } = null!;
        public virtual DbSet<Character> Characters { get; set; } = null!;
        public virtual DbSet<CharacterLootList> CharacterLootLists { get; set; } = null!;
        public virtual DbSet<LootListEntry> LootListEntries { get; set; } = null!;
        public virtual DbSet<LootListTeamSubmission> LootListTeamSubmissions { get; set; } = null!;
        public virtual DbSet<Drop> Drops { get; set; } = null!;
        public virtual DbSet<DropPass> DropPasses { get; set; } = null!;
        public virtual DbSet<Instance> Instances { get; set; } = null!;
        public virtual DbSet<Item> Items { get; set; } = null!;
        public virtual DbSet<ItemRestriction> ItemRestrictions { get; set; } = null!;
        public virtual DbSet<Raid> Raids { get; set; } = null!;
        public virtual DbSet<RaidAttendee> RaidAttendees { get; set; } = null!;
        public virtual DbSet<RaidTeam> RaidTeams { get; set; } = null!;
        public virtual DbSet<RaidTeamLeader> RaidTeamLeaders { get; set; } = null!;
        public virtual DbSet<RaidTeamSchedule> RaidTeamSchedules { get; set; } = null!;
        public virtual DbSet<TeamRemoval> TeamRemovals { get; set; } = null!;
        public virtual DbSet<Bracket> Brackets { get; set; } = null!;
        public virtual DbSet<PhaseDetails> PhaseDetails { get; set; } = null!;
        public virtual DbSet<PriorityScope> PriorityScopes { get; set; } = null!;

        public Task<PriorityScope> GetCurrentPriorityScopeAsync(CancellationToken cancellationToken = default)
        {
            return PriorityScopes.OrderByDescending(ps => ps.StartsAt).FirstAsync(cancellationToken);
        }

        public async Task<DonationMatrix> GetDonationMatrixAsync(Expression<Func<Donation, bool>> predicate, PriorityScope scope, CancellationToken cancellationToken = default)
        {
            var results = await Donations
                .AsNoTracking()
                .Where(d => d.RemovalId == null)
                .Where(predicate)
                .Select(d => new { d.DonatedAt.Year, d.DonatedAt.Month, d.CharacterId, d.CopperAmount })
                .GroupBy(d => new { d.Year, d.Month, d.CharacterId })
                .Select(g => new MonthDonations
                {
                    CharacterId = g.Key.CharacterId,
                    Donated = g.Sum(d => d.CopperAmount),
                    Month = g.Key.Month,
                    Year = g.Key.Year,
                })
                .OrderBy(md => md.CharacterId)
                .ThenBy(md => md.Year)
                .ThenBy(md => md.Month)
                .ToListAsync(cancellationToken);

            return new(results, scope);
        }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            builder.ConfigurePersistedGrantContext(_operationalStoreOptions);

            builder.Entity<Character>(e =>
            {
                e.HasIndex(character => character.Name).IsUnique();
                e.Property(character => character.Id).ValueGeneratedNever();
                e.HasOne(character => character.Team).WithMany(team => team!.Roster).OnDelete(DeleteBehavior.SetNull);
                e.HasOne(character => character.Owner).WithMany().OnDelete(DeleteBehavior.SetNull);
            });

            builder.Entity<CharacterEncounterKill>(e =>
            {
                e.ToTable("CharacterEncounterKill");
                e.HasKey(cek => new { cek.EncounterKillRaidId, cek.EncounterKillEncounterId, cek.CharacterId });
                e.HasOne(cek => cek.Character).WithMany(c => c.EncounterKills).OnDelete(DeleteBehavior.Cascade);
                e.HasOne(cek => cek.EncounterKill).WithMany(ek => ek.Characters).OnDelete(DeleteBehavior.Cascade);
            });

            builder.Entity<CharacterLootList>(e =>
            {
                e.HasKey(ll => new { ll.CharacterId, ll.Phase });
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
                e.HasOne(donation => donation.Removal).WithMany().OnDelete(DeleteBehavior.SetNull);
            });

            builder.Entity<DropPass>(e =>
            {
                e.HasKey(e => new { e.DropId, e.CharacterId });
                e.HasOne(dp => dp.Drop).WithMany(d => d.Passes).OnDelete(DeleteBehavior.Cascade);
                e.HasOne(dp => dp.LootListEntry).WithMany(lle => lle!.Passes).OnDelete(DeleteBehavior.Restrict);
                e.HasOne(dp => dp.Removal).WithMany().OnDelete(DeleteBehavior.SetNull);
            });

            builder.Entity<Encounter>(e =>
            {
                e.Property(encounter => encounter.Id).ValueGeneratedNever();
                e.HasOne(encounter => encounter.Instance).WithMany(i => i.Encounters).OnDelete(DeleteBehavior.Restrict);
            });

            builder.Entity<EncounterKill>(e =>
            {
                e.HasKey(kill => new { kill.EncounterId, kill.RaidId });
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
                    new(phase: 1, index: 0, minRank: 15, maxRank: 18, maxItems: 1, allowOffspec: false, allowTypeDuplicates: false),
                    new(phase: 1, index: 1, minRank: 11, maxRank: 14, maxItems: 1, allowOffspec: false, allowTypeDuplicates: false),
                    new(phase: 1, index: 2, minRank: 7, maxRank: 10, maxItems: 2, allowOffspec: false, allowTypeDuplicates: false),
                    new(phase: 1, index: 3, minRank: 1, maxRank: 6, maxItems: 2, allowOffspec: true, allowTypeDuplicates: true)
                    );
            });

            builder.Entity<PhaseDetails>(e =>
            {
                e.Property(phase => phase.Id).ValueGeneratedNever();
                e.HasData(
                    new PhaseDetails(id: 1, startsAt: default)
                    );
            });

            builder.Entity<AppUser>().Property(e => e.Id).ValueGeneratedNever();

            builder.Entity<PriorityScope>(e =>
            {
                e.Property(ps => ps.Id).ValueGeneratedNever();
                e.HasData(
                    new PriorityScope { Id = 1, StartsAt = default, AttendancesPerPoint = 4, FullTrialPenalty = -18, HalfTrialPenalty = -9, ObservedAttendances = 8, RequiredDonationCopper = 50_00_00 }
                    );
            });
        }

        Task<int> IPersistedGrantDbContext.SaveChangesAsync() => base.SaveChangesAsync();
    }
}
