// Copyright (C) 2021 Donovan Sullivan
// GNU General Public License v3.0+ (see LICENSE or https://www.gnu.org/licenses/gpl-3.0.txt)

using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using IdentityServer4.EntityFramework.Entities;
using IdentityServer4.EntityFramework.Extensions;
using IdentityServer4.EntityFramework.Interfaces;
using IdentityServer4.EntityFramework.Options;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Query.SqlExpressions;
using Microsoft.Extensions.Options;

namespace ValhallaLootList.Server.Data
{
    public class ApplicationDbContext : IdentityDbContext<AppUser>, IPersistedGrantDbContext
    {
        private readonly OperationalStoreOptions _operationalStoreOptions;

        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options, IOptions<OperationalStoreOptions> operationalStoreOptions) : base(options)
        {
            _operationalStoreOptions = operationalStoreOptions.Value;
        }

        public ApplicationDbContext(string connectionString)
            : base(new DbContextOptionsBuilder<ApplicationDbContext>().UseMySql(connectionString, MySqlServerVersion.LatestSupportedServerVersion).Options)
        {
            _operationalStoreOptions = new OperationalStoreOptions();
        }

        public virtual DbSet<PersistedGrant> PersistedGrants { get; set; } = null!;
        public virtual DbSet<DeviceFlowCodes> DeviceFlowCodes { get; set; } = null!;
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
            var now = DateTime.UtcNow;
            return PhaseDetails.Where(pd => pd.StartsAtUtc <= now).OrderByDescending(pd => pd.Id).Select(pd => pd.Id).FirstAsync(cancellationToken);
        }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            builder.ConfigurePersistedGrantContext(_operationalStoreOptions);

            builder.Entity<Character>().HasIndex(e => e.Name).IsUnique();

            builder.Entity<CharacterEncounterKill>()
                .ToTable("CharacterEncounterKill")
                .HasKey(e => new { e.EncounterKillRaidId, e.EncounterKillEncounterId, e.CharacterId });

            builder.Entity<CharacterLootList>().HasKey(e => new { e.CharacterId, e.Phase });

            builder.Entity<Drop>().HasKey(e => new { e.EncounterKillRaidId, e.EncounterKillEncounterId, e.ItemId });

            builder.Entity<DropPass>().HasKey(e => new { e.DropEncounterKillRaidId, e.DropEncounterKillEncounterId, e.DropItemId, e.CharacterId });

            builder.Entity<EncounterKill>().HasKey(e => new { e.EncounterId, e.RaidId });

            builder.Entity<RaidAttendee>().HasKey(e => new { e.CharacterId, e.RaidId });

            builder.Entity<RaidTeam>().HasIndex(e => e.Name).IsUnique();

            builder.Entity<Bracket>().HasKey(e => new { e.Phase, e.Index });
            builder.Entity<Bracket>().HasData(
                // Phase 1 brackets
                new(phase: 1, index: 0, minRank: 15, maxRank: 18, maxItems: 1, allowOffspec: false, allowTypeDuplicates: false),
                new(phase: 1, index: 1, minRank: 11, maxRank: 14, maxItems: 1, allowOffspec: false, allowTypeDuplicates: false),
                new(phase: 1, index: 2, minRank: 7, maxRank: 10, maxItems: 2, allowOffspec: false, allowTypeDuplicates: false),
                new(phase: 1, index: 3, minRank: 1, maxRank: 6, maxItems: 2, allowOffspec: true, allowTypeDuplicates: true)
                );

            builder.Entity<PhaseDetails>().HasData(
                new PhaseDetails(id: 1, startsAtUtc: default)
                );

            builder.HasDbFunction(typeof(MySqlTranslations).GetMethod(nameof(MySqlTranslations.ConvertTz)))
                .HasTranslation(args => new SqlFunctionExpression("CONVERT_TZ", args, nullable: true, args.Select(_ => false), typeof(DateTime), null));
        }

        Task<int> IPersistedGrantDbContext.SaveChangesAsync() => base.SaveChangesAsync();
    }
}
