﻿// <auto-generated />
using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using ValhallaLootList.Server.Data;

namespace ValhallaLootList.Server.Migrations
{
    [DbContext(typeof(ApplicationDbContext))]
    [Migration("20210127023422_InitialCreate")]
    partial class InitialCreate
    {
        protected override void BuildTargetModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder
                .HasAnnotation("Relational:MaxIdentifierLength", 64)
                .HasAnnotation("ProductVersion", "5.0.1");

            modelBuilder.Entity("IdentityServer4.EntityFramework.Entities.DeviceFlowCodes", b =>
                {
                    b.Property<string>("UserCode")
                        .HasMaxLength(200)
                        .HasColumnType("varchar(200) CHARACTER SET utf8mb4");

                    b.Property<string>("ClientId")
                        .IsRequired()
                        .HasMaxLength(200)
                        .HasColumnType("varchar(200) CHARACTER SET utf8mb4");

                    b.Property<DateTime>("CreationTime")
                        .HasColumnType("datetime(6)");

                    b.Property<string>("Data")
                        .IsRequired()
                        .HasMaxLength(50000)
                        .HasColumnType("longtext CHARACTER SET utf8mb4");

                    b.Property<string>("Description")
                        .HasMaxLength(200)
                        .HasColumnType("varchar(200) CHARACTER SET utf8mb4");

                    b.Property<string>("DeviceCode")
                        .IsRequired()
                        .HasMaxLength(200)
                        .HasColumnType("varchar(200) CHARACTER SET utf8mb4");

                    b.Property<DateTime?>("Expiration")
                        .IsRequired()
                        .HasColumnType("datetime(6)");

                    b.Property<string>("SessionId")
                        .HasMaxLength(100)
                        .HasColumnType("varchar(100) CHARACTER SET utf8mb4");

                    b.Property<string>("SubjectId")
                        .HasMaxLength(200)
                        .HasColumnType("varchar(200) CHARACTER SET utf8mb4");

                    b.HasKey("UserCode");

                    b.HasIndex("DeviceCode")
                        .IsUnique();

                    b.HasIndex("Expiration");

                    b.ToTable("DeviceCodes");
                });

            modelBuilder.Entity("IdentityServer4.EntityFramework.Entities.PersistedGrant", b =>
                {
                    b.Property<string>("Key")
                        .HasMaxLength(200)
                        .HasColumnType("varchar(200) CHARACTER SET utf8mb4");

                    b.Property<string>("ClientId")
                        .IsRequired()
                        .HasMaxLength(200)
                        .HasColumnType("varchar(200) CHARACTER SET utf8mb4");

                    b.Property<DateTime?>("ConsumedTime")
                        .HasColumnType("datetime(6)");

                    b.Property<DateTime>("CreationTime")
                        .HasColumnType("datetime(6)");

                    b.Property<string>("Data")
                        .IsRequired()
                        .HasMaxLength(50000)
                        .HasColumnType("longtext CHARACTER SET utf8mb4");

                    b.Property<string>("Description")
                        .HasMaxLength(200)
                        .HasColumnType("varchar(200) CHARACTER SET utf8mb4");

                    b.Property<DateTime?>("Expiration")
                        .HasColumnType("datetime(6)");

                    b.Property<string>("SessionId")
                        .HasMaxLength(100)
                        .HasColumnType("varchar(100) CHARACTER SET utf8mb4");

                    b.Property<string>("SubjectId")
                        .HasMaxLength(200)
                        .HasColumnType("varchar(200) CHARACTER SET utf8mb4");

                    b.Property<string>("Type")
                        .IsRequired()
                        .HasMaxLength(50)
                        .HasColumnType("varchar(50) CHARACTER SET utf8mb4");

                    b.HasKey("Key");

                    b.HasIndex("Expiration");

                    b.HasIndex("SubjectId", "ClientId", "Type");

                    b.HasIndex("SubjectId", "SessionId", "Type");

                    b.ToTable("PersistedGrants");
                });

            modelBuilder.Entity("Microsoft.AspNetCore.Identity.IdentityRole", b =>
                {
                    b.Property<string>("Id")
                        .HasColumnType("varchar(255) CHARACTER SET utf8mb4");

                    b.Property<string>("ConcurrencyStamp")
                        .IsConcurrencyToken()
                        .HasColumnType("longtext CHARACTER SET utf8mb4");

                    b.Property<string>("Name")
                        .HasMaxLength(256)
                        .HasColumnType("varchar(256) CHARACTER SET utf8mb4");

                    b.Property<string>("NormalizedName")
                        .HasMaxLength(256)
                        .HasColumnType("varchar(256) CHARACTER SET utf8mb4");

                    b.HasKey("Id");

                    b.HasIndex("NormalizedName")
                        .IsUnique()
                        .HasDatabaseName("RoleNameIndex");

                    b.ToTable("AspNetRoles");
                });

            modelBuilder.Entity("Microsoft.AspNetCore.Identity.IdentityRoleClaim<string>", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int");

                    b.Property<string>("ClaimType")
                        .HasColumnType("longtext CHARACTER SET utf8mb4");

                    b.Property<string>("ClaimValue")
                        .HasColumnType("longtext CHARACTER SET utf8mb4");

                    b.Property<string>("RoleId")
                        .IsRequired()
                        .HasColumnType("varchar(255) CHARACTER SET utf8mb4");

                    b.HasKey("Id");

                    b.HasIndex("RoleId");

                    b.ToTable("AspNetRoleClaims");
                });

            modelBuilder.Entity("Microsoft.AspNetCore.Identity.IdentityUserClaim<string>", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int");

                    b.Property<string>("ClaimType")
                        .HasColumnType("longtext CHARACTER SET utf8mb4");

                    b.Property<string>("ClaimValue")
                        .HasColumnType("longtext CHARACTER SET utf8mb4");

                    b.Property<string>("UserId")
                        .IsRequired()
                        .HasColumnType("varchar(255) CHARACTER SET utf8mb4");

                    b.HasKey("Id");

                    b.HasIndex("UserId");

                    b.ToTable("AspNetUserClaims");
                });

            modelBuilder.Entity("Microsoft.AspNetCore.Identity.IdentityUserLogin<string>", b =>
                {
                    b.Property<string>("LoginProvider")
                        .HasMaxLength(128)
                        .HasColumnType("varchar(128) CHARACTER SET utf8mb4");

                    b.Property<string>("ProviderKey")
                        .HasMaxLength(128)
                        .HasColumnType("varchar(128) CHARACTER SET utf8mb4");

                    b.Property<string>("ProviderDisplayName")
                        .HasColumnType("longtext CHARACTER SET utf8mb4");

                    b.Property<string>("UserId")
                        .IsRequired()
                        .HasColumnType("varchar(255) CHARACTER SET utf8mb4");

                    b.HasKey("LoginProvider", "ProviderKey");

                    b.HasIndex("UserId");

                    b.ToTable("AspNetUserLogins");
                });

            modelBuilder.Entity("Microsoft.AspNetCore.Identity.IdentityUserRole<string>", b =>
                {
                    b.Property<string>("UserId")
                        .HasColumnType("varchar(255) CHARACTER SET utf8mb4");

                    b.Property<string>("RoleId")
                        .HasColumnType("varchar(255) CHARACTER SET utf8mb4");

                    b.HasKey("UserId", "RoleId");

                    b.HasIndex("RoleId");

                    b.ToTable("AspNetUserRoles");
                });

            modelBuilder.Entity("Microsoft.AspNetCore.Identity.IdentityUserToken<string>", b =>
                {
                    b.Property<string>("UserId")
                        .HasColumnType("varchar(255) CHARACTER SET utf8mb4");

                    b.Property<string>("LoginProvider")
                        .HasMaxLength(128)
                        .HasColumnType("varchar(128) CHARACTER SET utf8mb4");

                    b.Property<string>("Name")
                        .HasMaxLength(128)
                        .HasColumnType("varchar(128) CHARACTER SET utf8mb4");

                    b.Property<string>("Value")
                        .HasColumnType("longtext CHARACTER SET utf8mb4");

                    b.HasKey("UserId", "LoginProvider", "Name");

                    b.ToTable("AspNetUserTokens");
                });

            modelBuilder.Entity("ValhallaLootList.Server.Data.AppUser", b =>
                {
                    b.Property<string>("Id")
                        .HasColumnType("varchar(255) CHARACTER SET utf8mb4");

                    b.Property<int>("AccessFailedCount")
                        .HasColumnType("int");

                    b.Property<string>("ConcurrencyStamp")
                        .IsConcurrencyToken()
                        .HasColumnType("longtext CHARACTER SET utf8mb4");

                    b.Property<string>("Email")
                        .HasMaxLength(256)
                        .HasColumnType("varchar(256) CHARACTER SET utf8mb4");

                    b.Property<bool>("EmailConfirmed")
                        .HasColumnType("tinyint(1)");

                    b.Property<bool>("LockoutEnabled")
                        .HasColumnType("tinyint(1)");

                    b.Property<DateTimeOffset?>("LockoutEnd")
                        .HasColumnType("datetime(6)");

                    b.Property<string>("NormalizedEmail")
                        .HasMaxLength(256)
                        .HasColumnType("varchar(256) CHARACTER SET utf8mb4");

                    b.Property<string>("NormalizedUserName")
                        .HasMaxLength(256)
                        .HasColumnType("varchar(256) CHARACTER SET utf8mb4");

                    b.Property<string>("PasswordHash")
                        .HasColumnType("longtext CHARACTER SET utf8mb4");

                    b.Property<string>("PhoneNumber")
                        .HasColumnType("longtext CHARACTER SET utf8mb4");

                    b.Property<bool>("PhoneNumberConfirmed")
                        .HasColumnType("tinyint(1)");

                    b.Property<string>("SecurityStamp")
                        .HasColumnType("longtext CHARACTER SET utf8mb4");

                    b.Property<bool>("TwoFactorEnabled")
                        .HasColumnType("tinyint(1)");

                    b.Property<string>("UserName")
                        .HasMaxLength(256)
                        .HasColumnType("varchar(256) CHARACTER SET utf8mb4");

                    b.HasKey("Id");

                    b.HasIndex("NormalizedEmail")
                        .HasDatabaseName("EmailIndex");

                    b.HasIndex("NormalizedUserName")
                        .IsUnique()
                        .HasDatabaseName("UserNameIndex");

                    b.ToTable("AspNetUsers");
                });

            modelBuilder.Entity("ValhallaLootList.Server.Data.BossKill", b =>
                {
                    b.Property<string>("BossId")
                        .HasColumnType("varchar(255) CHARACTER SET utf8mb4");

                    b.Property<string>("RaidId")
                        .HasColumnType("varchar(255) CHARACTER SET utf8mb4");

                    b.HasKey("BossId", "RaidId");

                    b.HasIndex("RaidId");

                    b.ToTable("BossKills");
                });

            modelBuilder.Entity("ValhallaLootList.Server.Data.Character", b =>
                {
                    b.Property<string>("Id")
                        .HasColumnType("varchar(255) CHARACTER SET utf8mb4");

                    b.Property<int>("Class")
                        .HasColumnType("int");

                    b.Property<bool>("IsLeader")
                        .HasColumnType("tinyint(1)");

                    b.Property<int>("MemberStatus")
                        .HasColumnType("int");

                    b.Property<string>("Name")
                        .IsRequired()
                        .HasMaxLength(16)
                        .HasColumnType("varchar(16) CHARACTER SET utf8mb4");

                    b.Property<int>("Race")
                        .HasColumnType("int");

                    b.Property<string>("TeamId")
                        .HasColumnType("varchar(255) CHARACTER SET utf8mb4");

                    b.HasKey("Id");

                    b.HasIndex("Name")
                        .IsUnique();

                    b.HasIndex("TeamId");

                    b.ToTable("Characters");
                });

            modelBuilder.Entity("ValhallaLootList.Server.Data.CharacterLootList", b =>
                {
                    b.Property<string>("CharacterId")
                        .HasColumnType("varchar(255) CHARACTER SET utf8mb4");

                    b.Property<byte>("Phase")
                        .HasColumnType("tinyint unsigned");

                    b.Property<string>("ApprovedBy")
                        .HasColumnType("longtext CHARACTER SET utf8mb4");

                    b.Property<bool>("Locked")
                        .HasColumnType("tinyint(1)");

                    b.Property<int>("MainSpec")
                        .HasColumnType("int");

                    b.Property<int>("OffSpec")
                        .HasColumnType("int");

                    b.HasKey("CharacterId", "Phase");

                    b.ToTable("CharacterLootLists");
                });

            modelBuilder.Entity("ValhallaLootList.Server.Data.Drop", b =>
                {
                    b.Property<string>("BossKillId")
                        .HasColumnType("varchar(255) CHARACTER SET utf8mb4");

                    b.Property<uint>("ItemId")
                        .HasColumnType("int unsigned");

                    b.Property<DateTime>("AwardedAtUtc")
                        .HasColumnType("datetime(6)");

                    b.Property<string>("AwardedBy")
                        .HasColumnType("longtext CHARACTER SET utf8mb4");

                    b.Property<string>("BossKillBossId")
                        .IsRequired()
                        .HasColumnType("varchar(255) CHARACTER SET utf8mb4");

                    b.Property<string>("BossKillRaidId")
                        .IsRequired()
                        .HasColumnType("varchar(255) CHARACTER SET utf8mb4");

                    b.Property<string>("WinnerId")
                        .HasColumnType("varchar(255) CHARACTER SET utf8mb4");

                    b.HasKey("BossKillId", "ItemId");

                    b.HasIndex("ItemId");

                    b.HasIndex("WinnerId");

                    b.HasIndex("BossKillBossId", "BossKillRaidId");

                    b.ToTable("Drops");
                });

            modelBuilder.Entity("ValhallaLootList.Server.Data.DropPass", b =>
                {
                    b.Property<string>("CharacterId")
                        .HasColumnType("varchar(255) CHARACTER SET utf8mb4");

                    b.Property<string>("DropId")
                        .HasColumnType("varchar(255) CHARACTER SET utf8mb4");

                    b.Property<string>("DropBossKillId")
                        .IsRequired()
                        .HasColumnType("varchar(255) CHARACTER SET utf8mb4");

                    b.Property<uint>("DropItemId")
                        .HasColumnType("int unsigned");

                    b.Property<int>("RelativePriority")
                        .HasColumnType("int");

                    b.HasKey("CharacterId", "DropId");

                    b.HasIndex("DropBossKillId", "DropItemId");

                    b.ToTable("DropPasses");
                });

            modelBuilder.Entity("ValhallaLootList.Server.Data.Encounter", b =>
                {
                    b.Property<string>("Id")
                        .HasColumnType("varchar(255) CHARACTER SET utf8mb4");

                    b.Property<string>("InstanceId")
                        .IsRequired()
                        .HasColumnType("varchar(255) CHARACTER SET utf8mb4");

                    b.Property<string>("Name")
                        .IsRequired()
                        .HasColumnType("varchar(255) CHARACTER SET utf8mb4");

                    b.HasKey("Id");

                    b.HasIndex("InstanceId");

                    b.HasIndex("Name")
                        .IsUnique();

                    b.ToTable("Encounters");
                });

            modelBuilder.Entity("ValhallaLootList.Server.Data.Instance", b =>
                {
                    b.Property<string>("Id")
                        .HasColumnType("varchar(255) CHARACTER SET utf8mb4");

                    b.Property<string>("Name")
                        .IsRequired()
                        .HasColumnType("varchar(255) CHARACTER SET utf8mb4");

                    b.Property<byte>("Phase")
                        .HasColumnType("tinyint unsigned");

                    b.HasKey("Id");

                    b.HasIndex("Name")
                        .IsUnique();

                    b.ToTable("Instances");
                });

            modelBuilder.Entity("ValhallaLootList.Server.Data.Item", b =>
                {
                    b.Property<uint>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int unsigned");

                    b.Property<int>("Agility")
                        .HasColumnType("int");

                    b.Property<int>("Armor")
                        .HasColumnType("int");

                    b.Property<int>("ArmorPenetration")
                        .HasColumnType("int");

                    b.Property<int>("BlockRating")
                        .HasColumnType("int");

                    b.Property<int>("BlockValue")
                        .HasColumnType("int");

                    b.Property<double>("DPS")
                        .HasColumnType("double");

                    b.Property<int>("Defense")
                        .HasColumnType("int");

                    b.Property<int>("Dodge")
                        .HasColumnType("int");

                    b.Property<string>("EncounterId")
                        .IsRequired()
                        .HasColumnType("varchar(255) CHARACTER SET utf8mb4");

                    b.Property<int>("Expertise")
                        .HasColumnType("int");

                    b.Property<bool>("HasOnUse")
                        .HasColumnType("tinyint(1)");

                    b.Property<bool>("HasProc")
                        .HasColumnType("tinyint(1)");

                    b.Property<bool>("HasSpecial")
                        .HasColumnType("tinyint(1)");

                    b.Property<int>("Haste")
                        .HasColumnType("int");

                    b.Property<int>("HealingPower")
                        .HasColumnType("int");

                    b.Property<int>("HealthPer5")
                        .HasColumnType("int");

                    b.Property<int>("Intellect")
                        .HasColumnType("int");

                    b.Property<int>("ItemLevel")
                        .HasColumnType("int");

                    b.Property<int>("ManaPer5")
                        .HasColumnType("int");

                    b.Property<int>("MeleeAttackPower")
                        .HasColumnType("int");

                    b.Property<int>("MeleeCrit")
                        .HasColumnType("int");

                    b.Property<string>("Name")
                        .IsRequired()
                        .HasColumnType("longtext CHARACTER SET utf8mb4");

                    b.Property<int>("Parry")
                        .HasColumnType("int");

                    b.Property<int>("PhysicalHit")
                        .HasColumnType("int");

                    b.Property<int>("RangedAttackPower")
                        .HasColumnType("int");

                    b.Property<int>("RangedCrit")
                        .HasColumnType("int");

                    b.Property<int>("Resilience")
                        .HasColumnType("int");

                    b.Property<uint?>("RewardFromId")
                        .HasColumnType("int unsigned");

                    b.Property<int>("Slot")
                        .HasColumnType("int");

                    b.Property<int>("Sockets")
                        .HasColumnType("int");

                    b.Property<double>("Speed")
                        .HasColumnType("double");

                    b.Property<int>("SpellCrit")
                        .HasColumnType("int");

                    b.Property<int>("SpellHaste")
                        .HasColumnType("int");

                    b.Property<int>("SpellHit")
                        .HasColumnType("int");

                    b.Property<int>("SpellPenetration")
                        .HasColumnType("int");

                    b.Property<int>("SpellPower")
                        .HasColumnType("int");

                    b.Property<int>("Spirit")
                        .HasColumnType("int");

                    b.Property<int>("Stamina")
                        .HasColumnType("int");

                    b.Property<int>("Strength")
                        .HasColumnType("int");

                    b.Property<int>("TopEndDamage")
                        .HasColumnType("int");

                    b.Property<int>("Type")
                        .HasColumnType("int");

                    b.Property<int?>("UsableClasses")
                        .HasColumnType("int");

                    b.HasKey("Id");

                    b.HasIndex("EncounterId");

                    b.HasIndex("RewardFromId");

                    b.ToTable("Items");
                });

            modelBuilder.Entity("ValhallaLootList.Server.Data.ItemRestriction", b =>
                {
                    b.Property<string>("Id")
                        .HasColumnType("varchar(255) CHARACTER SET utf8mb4");

                    b.Property<bool>("Automated")
                        .HasColumnType("tinyint(1)");

                    b.Property<uint>("ItemId")
                        .HasColumnType("int unsigned");

                    b.Property<string>("Reason")
                        .IsRequired()
                        .HasMaxLength(256)
                        .HasColumnType("varchar(256) CHARACTER SET utf8mb4");

                    b.Property<int>("RestrictionLevel")
                        .HasColumnType("int");

                    b.Property<int>("Specializations")
                        .HasColumnType("int");

                    b.HasKey("Id");

                    b.HasIndex("ItemId");

                    b.ToTable("ItemRestrictions");
                });

            modelBuilder.Entity("ValhallaLootList.Server.Data.LootListEntry", b =>
                {
                    b.Property<string>("Id")
                        .HasColumnType("varchar(255) CHARACTER SET utf8mb4");

                    b.Property<uint?>("ItemId")
                        .HasColumnType("int unsigned");

                    b.Property<string>("LootListCharacterId")
                        .IsRequired()
                        .HasColumnType("varchar(255) CHARACTER SET utf8mb4");

                    b.Property<byte>("LootListPhase")
                        .HasColumnType("tinyint unsigned");

                    b.Property<short>("PassCount")
                        .HasColumnType("smallint");

                    b.Property<byte>("Rank")
                        .HasColumnType("tinyint unsigned");

                    b.Property<bool>("Won")
                        .HasColumnType("tinyint(1)");

                    b.HasKey("Id");

                    b.HasIndex("ItemId");

                    b.HasIndex("LootListCharacterId", "LootListPhase");

                    b.ToTable("LootListEntries");
                });

            modelBuilder.Entity("ValhallaLootList.Server.Data.Raid", b =>
                {
                    b.Property<string>("Id")
                        .HasColumnType("varchar(255) CHARACTER SET utf8mb4");

                    b.Property<string>("ScheduleId")
                        .IsRequired()
                        .HasColumnType("varchar(255) CHARACTER SET utf8mb4");

                    b.Property<DateTime>("StartedAtUtc")
                        .HasColumnType("datetime(6)");

                    b.HasKey("Id");

                    b.HasIndex("ScheduleId");

                    b.ToTable("Raids");
                });

            modelBuilder.Entity("ValhallaLootList.Server.Data.RaidAttendee", b =>
                {
                    b.Property<string>("CharacterId")
                        .HasColumnType("varchar(255) CHARACTER SET utf8mb4");

                    b.Property<string>("RaidId")
                        .HasColumnType("varchar(255) CHARACTER SET utf8mb4");

                    b.Property<bool>("IgnoreAttendance")
                        .HasColumnType("tinyint(1)");

                    b.Property<string>("IgnoreReason")
                        .HasColumnType("longtext CHARACTER SET utf8mb4");

                    b.Property<bool>("UsingOffspec")
                        .HasColumnType("tinyint(1)");

                    b.HasKey("CharacterId", "RaidId");

                    b.HasIndex("RaidId");

                    b.ToTable("RaidAttendees");
                });

            modelBuilder.Entity("ValhallaLootList.Server.Data.RaidTeam", b =>
                {
                    b.Property<string>("Id")
                        .HasColumnType("varchar(255) CHARACTER SET utf8mb4");

                    b.Property<string>("Name")
                        .IsRequired()
                        .HasColumnType("varchar(255) CHARACTER SET utf8mb4");

                    b.HasKey("Id");

                    b.HasIndex("Name")
                        .IsUnique();

                    b.ToTable("RaidTeams");
                });

            modelBuilder.Entity("ValhallaLootList.Server.Data.RaidTeamSchedule", b =>
                {
                    b.Property<string>("Id")
                        .HasColumnType("varchar(255) CHARACTER SET utf8mb4");

                    b.Property<int>("Day")
                        .HasColumnType("int");

                    b.Property<TimeSpan>("Duration")
                        .HasColumnType("time(6)");

                    b.Property<string>("RaidTeamId")
                        .IsRequired()
                        .HasColumnType("varchar(255) CHARACTER SET utf8mb4");

                    b.Property<TimeSpan>("RealmTimeStart")
                        .HasColumnType("time(6)");

                    b.HasKey("Id");

                    b.HasIndex("RaidTeamId");

                    b.ToTable("RaidTeamSchedules");
                });

            modelBuilder.Entity("Microsoft.AspNetCore.Identity.IdentityRoleClaim<string>", b =>
                {
                    b.HasOne("Microsoft.AspNetCore.Identity.IdentityRole", null)
                        .WithMany()
                        .HasForeignKey("RoleId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();
                });

            modelBuilder.Entity("Microsoft.AspNetCore.Identity.IdentityUserClaim<string>", b =>
                {
                    b.HasOne("ValhallaLootList.Server.Data.AppUser", null)
                        .WithMany()
                        .HasForeignKey("UserId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();
                });

            modelBuilder.Entity("Microsoft.AspNetCore.Identity.IdentityUserLogin<string>", b =>
                {
                    b.HasOne("ValhallaLootList.Server.Data.AppUser", null)
                        .WithMany()
                        .HasForeignKey("UserId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();
                });

            modelBuilder.Entity("Microsoft.AspNetCore.Identity.IdentityUserRole<string>", b =>
                {
                    b.HasOne("Microsoft.AspNetCore.Identity.IdentityRole", null)
                        .WithMany()
                        .HasForeignKey("RoleId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.HasOne("ValhallaLootList.Server.Data.AppUser", null)
                        .WithMany()
                        .HasForeignKey("UserId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();
                });

            modelBuilder.Entity("Microsoft.AspNetCore.Identity.IdentityUserToken<string>", b =>
                {
                    b.HasOne("ValhallaLootList.Server.Data.AppUser", null)
                        .WithMany()
                        .HasForeignKey("UserId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();
                });

            modelBuilder.Entity("ValhallaLootList.Server.Data.BossKill", b =>
                {
                    b.HasOne("ValhallaLootList.Server.Data.Encounter", "Boss")
                        .WithMany()
                        .HasForeignKey("BossId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.HasOne("ValhallaLootList.Server.Data.Raid", "Raid")
                        .WithMany()
                        .HasForeignKey("RaidId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("Boss");

                    b.Navigation("Raid");
                });

            modelBuilder.Entity("ValhallaLootList.Server.Data.Character", b =>
                {
                    b.HasOne("ValhallaLootList.Server.Data.RaidTeam", "Team")
                        .WithMany("Roster")
                        .HasForeignKey("TeamId");

                    b.Navigation("Team");
                });

            modelBuilder.Entity("ValhallaLootList.Server.Data.CharacterLootList", b =>
                {
                    b.HasOne("ValhallaLootList.Server.Data.Character", "Character")
                        .WithMany("CharacterLootLists")
                        .HasForeignKey("CharacterId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("Character");
                });

            modelBuilder.Entity("ValhallaLootList.Server.Data.Drop", b =>
                {
                    b.HasOne("ValhallaLootList.Server.Data.Item", "Item")
                        .WithMany("Drops")
                        .HasForeignKey("ItemId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.HasOne("ValhallaLootList.Server.Data.Character", "Winner")
                        .WithMany("WonDrops")
                        .HasForeignKey("WinnerId");

                    b.HasOne("ValhallaLootList.Server.Data.BossKill", "BossKill")
                        .WithMany("Drops")
                        .HasForeignKey("BossKillBossId", "BossKillRaidId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("BossKill");

                    b.Navigation("Item");

                    b.Navigation("Winner");
                });

            modelBuilder.Entity("ValhallaLootList.Server.Data.DropPass", b =>
                {
                    b.HasOne("ValhallaLootList.Server.Data.Character", "Character")
                        .WithMany("Passes")
                        .HasForeignKey("CharacterId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.HasOne("ValhallaLootList.Server.Data.Drop", "Drop")
                        .WithMany("Passes")
                        .HasForeignKey("DropBossKillId", "DropItemId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("Character");

                    b.Navigation("Drop");
                });

            modelBuilder.Entity("ValhallaLootList.Server.Data.Encounter", b =>
                {
                    b.HasOne("ValhallaLootList.Server.Data.Instance", "Instance")
                        .WithMany("Encounters")
                        .HasForeignKey("InstanceId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("Instance");
                });

            modelBuilder.Entity("ValhallaLootList.Server.Data.Item", b =>
                {
                    b.HasOne("ValhallaLootList.Server.Data.Encounter", "Encounter")
                        .WithMany("Items")
                        .HasForeignKey("EncounterId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.HasOne("ValhallaLootList.Server.Data.Item", "RewardFrom")
                        .WithMany()
                        .HasForeignKey("RewardFromId");

                    b.Navigation("Encounter");

                    b.Navigation("RewardFrom");
                });

            modelBuilder.Entity("ValhallaLootList.Server.Data.ItemRestriction", b =>
                {
                    b.HasOne("ValhallaLootList.Server.Data.Item", "Item")
                        .WithMany("Restrictions")
                        .HasForeignKey("ItemId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("Item");
                });

            modelBuilder.Entity("ValhallaLootList.Server.Data.LootListEntry", b =>
                {
                    b.HasOne("ValhallaLootList.Server.Data.Item", "Item")
                        .WithMany()
                        .HasForeignKey("ItemId");

                    b.HasOne("ValhallaLootList.Server.Data.CharacterLootList", "LootList")
                        .WithMany("Entries")
                        .HasForeignKey("LootListCharacterId", "LootListPhase")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("Item");

                    b.Navigation("LootList");
                });

            modelBuilder.Entity("ValhallaLootList.Server.Data.Raid", b =>
                {
                    b.HasOne("ValhallaLootList.Server.Data.RaidTeamSchedule", "Schedule")
                        .WithMany("Raids")
                        .HasForeignKey("ScheduleId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("Schedule");
                });

            modelBuilder.Entity("ValhallaLootList.Server.Data.RaidAttendee", b =>
                {
                    b.HasOne("ValhallaLootList.Server.Data.Character", "Character")
                        .WithMany("Attendances")
                        .HasForeignKey("CharacterId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.HasOne("ValhallaLootList.Server.Data.Raid", "Raid")
                        .WithMany("Attendees")
                        .HasForeignKey("RaidId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("Character");

                    b.Navigation("Raid");
                });

            modelBuilder.Entity("ValhallaLootList.Server.Data.RaidTeamSchedule", b =>
                {
                    b.HasOne("ValhallaLootList.Server.Data.RaidTeam", "RaidTeam")
                        .WithMany("Schedules")
                        .HasForeignKey("RaidTeamId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("RaidTeam");
                });

            modelBuilder.Entity("ValhallaLootList.Server.Data.BossKill", b =>
                {
                    b.Navigation("Drops");
                });

            modelBuilder.Entity("ValhallaLootList.Server.Data.Character", b =>
                {
                    b.Navigation("Attendances");

                    b.Navigation("CharacterLootLists");

                    b.Navigation("Passes");

                    b.Navigation("WonDrops");
                });

            modelBuilder.Entity("ValhallaLootList.Server.Data.CharacterLootList", b =>
                {
                    b.Navigation("Entries");
                });

            modelBuilder.Entity("ValhallaLootList.Server.Data.Drop", b =>
                {
                    b.Navigation("Passes");
                });

            modelBuilder.Entity("ValhallaLootList.Server.Data.Encounter", b =>
                {
                    b.Navigation("Items");
                });

            modelBuilder.Entity("ValhallaLootList.Server.Data.Instance", b =>
                {
                    b.Navigation("Encounters");
                });

            modelBuilder.Entity("ValhallaLootList.Server.Data.Item", b =>
                {
                    b.Navigation("Drops");

                    b.Navigation("Restrictions");
                });

            modelBuilder.Entity("ValhallaLootList.Server.Data.Raid", b =>
                {
                    b.Navigation("Attendees");
                });

            modelBuilder.Entity("ValhallaLootList.Server.Data.RaidTeam", b =>
                {
                    b.Navigation("Roster");

                    b.Navigation("Schedules");
                });

            modelBuilder.Entity("ValhallaLootList.Server.Data.RaidTeamSchedule", b =>
                {
                    b.Navigation("Raids");
                });
#pragma warning restore 612, 618
        }
    }
}
