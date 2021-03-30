// Copyright (C) 2021 Donovan Sullivan
// GNU General Public License v3.0+ (see LICENSE or https://www.gnu.org/licenses/gpl-3.0.txt)

using System;
using System.Linq;
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
    public class ApplicationDbContext : IdentityDbContext<AppUser, IdentityRole<long>,  long>, IPersistedGrantDbContext
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
        public virtual DbSet<Drop> Drops { get; set; } = null!;
        public virtual DbSet<DropPass> DropPasses { get; set; } = null!;
        public virtual DbSet<Instance> Instances { get; set; } = null!;
        public virtual DbSet<Item> Items { get; set; } = null!;
        public virtual DbSet<ItemRestriction> ItemRestrictions { get; set; } = null!;
        public virtual DbSet<Raid> Raids { get; set; } = null!;
        public virtual DbSet<RaidAttendee> RaidAttendees { get; set; } = null!;
        public virtual DbSet<RaidTeam> RaidTeams { get; set; } = null!;
        public virtual DbSet<RaidTeamSchedule> RaidTeamSchedules { get; set; } = null!;
        public virtual DbSet<Bracket> Brackets { get; set; } = null!;
        public virtual DbSet<PhaseDetails> PhaseDetails { get; set; } = null!;

        public Task<byte> GetCurrentPhaseAsync(CancellationToken cancellationToken = default)
        {
            var now = DateTimeOffset.UtcNow;
            return PhaseDetails.AsNoTracking().Where(pd => pd.StartsAt <= now).OrderByDescending(pd => pd.Id).Select(pd => pd.Id).FirstAsync(cancellationToken);
        }

        public async Task<bool> IsLeaderOf(ClaimsPrincipal user, long teamId)
        {
            var userId = user.GetDiscordId();
            var teamIdString = teamId.ToString();
            return await UserClaims.CountAsync(claim => claim.UserId == userId && claim.ClaimType == AppClaimTypes.RaidLeader && claim.ClaimValue == teamIdString) > 0;
        }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            builder.ConfigurePersistedGrantContext(_operationalStoreOptions);

            builder.Entity<Character>(e =>
            {
                e.HasIndex(character => character.Name).IsUnique();
                e.Property(character => character.Id).ValueGeneratedNever();
            });

            builder.Entity<CharacterEncounterKill>()
                .ToTable("CharacterEncounterKill")
                .HasKey(e => new { e.EncounterKillRaidId, e.EncounterKillEncounterId, e.CharacterId });

            builder.Entity<CharacterLootList>().HasKey(e => new { e.CharacterId, e.Phase });

            builder.Entity<Drop>().Property(drop => drop.Id).ValueGeneratedNever();

            builder.Entity<Donation>().Property(donation => donation.Id).ValueGeneratedNever();

            builder.Entity<DropPass>().HasKey(e => new { e.DropId, e.CharacterId });

            builder.Entity<Encounter>().Property(encounter => encounter.Id).ValueGeneratedNever();

            builder.Entity<EncounterKill>().HasKey(kill => new { kill.EncounterId, kill.RaidId });

            builder.Entity<Instance>().Property(instance => instance.Id).ValueGeneratedNever();

            builder.Entity<Item>().Property(item => item.Id).ValueGeneratedNever();

            builder.Entity<RaidAttendee>().HasKey(attendee => new { attendee.CharacterId, attendee.RaidId });

            builder.Entity<Raid>().Property(raid => raid.Id).ValueGeneratedNever();

            builder.Entity<RaidTeam>(e =>
            {
                e.HasIndex(raid => raid.Name).IsUnique();
                e.Property(team => team.Id).ValueGeneratedNever();
            });

            builder.Entity<RaidTeamSchedule>().Property(schedule => schedule.Id).ValueGeneratedNever();

            builder.Entity<ItemRestriction>(e =>
            {
                e.Property(restriction => restriction.Id).ValueGeneratedNever();
                e.HasIndex(restriction => new { restriction.RestrictionLevel, restriction.Specializations });
            });

            builder.Entity<LootListEntry>().Property(entry => entry.Id).ValueGeneratedNever();

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
        }

        Task<int> IPersistedGrantDbContext.SaveChangesAsync() => base.SaveChangesAsync();
    }
}
