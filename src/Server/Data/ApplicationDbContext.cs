// Copyright (C) 2021 Donovan Sullivan
// GNU General Public License v3.0+ (see LICENSE or https://www.gnu.org/licenses/gpl-3.0.txt)

using System.Threading.Tasks;
using IdentityServer4.EntityFramework.Entities;
using IdentityServer4.EntityFramework.Extensions;
using IdentityServer4.EntityFramework.Interfaces;
using IdentityServer4.EntityFramework.Options;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
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

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            builder.ConfigurePersistedGrantContext(_operationalStoreOptions);

            builder.Entity<Character>().HasIndex(e => e.Name).IsUnique();

            builder.Entity<CharacterEncounterKill>().HasKey(e => new { e.EncounterKillRaidId, e.EncounterKillEncounterId, e.CharacterId });

            builder.Entity<CharacterLootList>().HasKey(e => new { e.CharacterId, e.Phase });

            builder.Entity<Drop>().HasKey(e => new { e.EncounterKillRaidId, e.EncounterKillEncounterId, e.ItemId });

            builder.Entity<DropPass>().HasKey(e => new { e.DropEncounterKillRaidId, e.DropEncounterKillEncounterId, e.DropItemId, e.CharacterId });

            builder.Entity<Encounter>().HasIndex(e => e.Name).IsUnique();

            builder.Entity<EncounterKill>().HasKey(e => new { e.EncounterId, e.RaidId });

            builder.Entity<Instance>().HasIndex(e => e.Name).IsUnique();

            builder.Entity<RaidAttendee>().HasKey(e => new { e.CharacterId, e.RaidId });

            builder.Entity<RaidTeam>().HasIndex(e => e.Name).IsUnique();
        }

        Task<int> IPersistedGrantDbContext.SaveChangesAsync() => base.SaveChangesAsync();
    }
}
