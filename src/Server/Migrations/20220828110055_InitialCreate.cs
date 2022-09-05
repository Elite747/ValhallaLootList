﻿using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ValhallaLootList.Server.Migrations
{
    public partial class InitialCreate : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "AspNetRoles",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    NormalizedName = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    ConcurrencyStamp = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AspNetRoles", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "AspNetUsers",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false),
                    UserName = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    NormalizedUserName = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    Email = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    NormalizedEmail = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    EmailConfirmed = table.Column<bool>(type: "bit", nullable: false),
                    PasswordHash = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    SecurityStamp = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ConcurrencyStamp = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    PhoneNumber = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    PhoneNumberConfirmed = table.Column<bool>(type: "bit", nullable: false),
                    TwoFactorEnabled = table.Column<bool>(type: "bit", nullable: false),
                    LockoutEnd = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    LockoutEnabled = table.Column<bool>(type: "bit", nullable: false),
                    AccessFailedCount = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AspNetUsers", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Brackets",
                columns: table => new
                {
                    Phase = table.Column<byte>(type: "tinyint", nullable: false),
                    Index = table.Column<byte>(type: "tinyint", nullable: false),
                    MinRank = table.Column<byte>(type: "tinyint", nullable: false),
                    MaxRank = table.Column<byte>(type: "tinyint", nullable: false),
                    NormalItems = table.Column<byte>(type: "tinyint", nullable: false),
                    HeroicItems = table.Column<byte>(type: "tinyint", nullable: false),
                    AllowOffspec = table.Column<bool>(type: "bit", nullable: false),
                    AllowTypeDuplicates = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Brackets", x => new { x.Phase, x.Index });
                });

            migrationBuilder.CreateTable(
                name: "DeviceCodes",
                columns: table => new
                {
                    UserCode = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    DeviceCode = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    SubjectId = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    SessionId = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    ClientId = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    CreationTime = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Expiration = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Data = table.Column<string>(type: "nvarchar(max)", maxLength: 50000, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DeviceCodes", x => x.UserCode);
                });

            migrationBuilder.CreateTable(
                name: "Instances",
                columns: table => new
                {
                    Id = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    Name = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    Phase = table.Column<byte>(type: "tinyint", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Instances", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "PersistedGrants",
                columns: table => new
                {
                    Key = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    Type = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    SubjectId = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    SessionId = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    ClientId = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    CreationTime = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Expiration = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ConsumedTime = table.Column<DateTime>(type: "datetime2", nullable: true),
                    Data = table.Column<string>(type: "nvarchar(max)", maxLength: 50000, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PersistedGrants", x => x.Key);
                });

            migrationBuilder.CreateTable(
                name: "PhaseDetails",
                columns: table => new
                {
                    Id = table.Column<byte>(type: "tinyint", nullable: false),
                    StartsAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PhaseDetails", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "RaidTeams",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(24)", maxLength: 24, nullable: false),
                    Inactive = table.Column<bool>(type: "bit", nullable: false),
                    TeamSize = table.Column<byte>(type: "tinyint", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RaidTeams", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "AspNetRoleClaims",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    RoleId = table.Column<long>(type: "bigint", nullable: false),
                    ClaimType = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ClaimValue = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AspNetRoleClaims", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AspNetRoleClaims_AspNetRoles_RoleId",
                        column: x => x.RoleId,
                        principalTable: "AspNetRoles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "AspNetUserClaims",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UserId = table.Column<long>(type: "bigint", nullable: false),
                    ClaimType = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ClaimValue = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AspNetUserClaims", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AspNetUserClaims_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "AspNetUserLogins",
                columns: table => new
                {
                    LoginProvider = table.Column<string>(type: "nvarchar(128)", maxLength: 128, nullable: false),
                    ProviderKey = table.Column<string>(type: "nvarchar(128)", maxLength: 128, nullable: false),
                    ProviderDisplayName = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    UserId = table.Column<long>(type: "bigint", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AspNetUserLogins", x => new { x.LoginProvider, x.ProviderKey });
                    table.ForeignKey(
                        name: "FK_AspNetUserLogins_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "AspNetUserRoles",
                columns: table => new
                {
                    UserId = table.Column<long>(type: "bigint", nullable: false),
                    RoleId = table.Column<long>(type: "bigint", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AspNetUserRoles", x => new { x.UserId, x.RoleId });
                    table.ForeignKey(
                        name: "FK_AspNetUserRoles_AspNetRoles_RoleId",
                        column: x => x.RoleId,
                        principalTable: "AspNetRoles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_AspNetUserRoles_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "AspNetUserTokens",
                columns: table => new
                {
                    UserId = table.Column<long>(type: "bigint", nullable: false),
                    LoginProvider = table.Column<string>(type: "nvarchar(128)", maxLength: 128, nullable: false),
                    Name = table.Column<string>(type: "nvarchar(128)", maxLength: 128, nullable: false),
                    Value = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AspNetUserTokens", x => new { x.UserId, x.LoginProvider, x.Name });
                    table.ForeignKey(
                        name: "FK_AspNetUserTokens_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Characters",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(12)", maxLength: 12, nullable: false),
                    Race = table.Column<int>(type: "int", nullable: false),
                    Class = table.Column<int>(type: "int", nullable: false),
                    IsFemale = table.Column<bool>(type: "bit", nullable: false),
                    Deactivated = table.Column<bool>(type: "bit", nullable: false),
                    VerifiedById = table.Column<long>(type: "bigint", nullable: true),
                    OwnerId = table.Column<long>(type: "bigint", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Characters", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Characters_AspNetUsers_OwnerId",
                        column: x => x.OwnerId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateTable(
                name: "Encounters",
                columns: table => new
                {
                    Id = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    Name = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    InstanceId = table.Column<string>(type: "nvarchar(20)", nullable: false),
                    Index = table.Column<short>(type: "smallint", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Encounters", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Encounters_Instances_InstanceId",
                        column: x => x.InstanceId,
                        principalTable: "Instances",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Raids",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false),
                    StartedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    LocksAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    Phase = table.Column<byte>(type: "tinyint", nullable: false),
                    RaidTeamId = table.Column<long>(type: "bigint", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Raids", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Raids_RaidTeams_RaidTeamId",
                        column: x => x.RaidTeamId,
                        principalTable: "RaidTeams",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "RaidTeamLeaders",
                columns: table => new
                {
                    RaidTeamId = table.Column<long>(type: "bigint", nullable: false),
                    UserId = table.Column<long>(type: "bigint", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RaidTeamLeaders", x => new { x.RaidTeamId, x.UserId });
                    table.ForeignKey(
                        name: "FK_RaidTeamLeaders_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_RaidTeamLeaders_RaidTeams_RaidTeamId",
                        column: x => x.RaidTeamId,
                        principalTable: "RaidTeams",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "RaidTeamSchedules",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false),
                    RaidTeamId = table.Column<long>(type: "bigint", nullable: false),
                    Day = table.Column<int>(type: "int", nullable: false),
                    RealmTimeStart = table.Column<TimeSpan>(type: "time", nullable: false),
                    Duration = table.Column<TimeSpan>(type: "time", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RaidTeamSchedules", x => x.Id);
                    table.ForeignKey(
                        name: "FK_RaidTeamSchedules_RaidTeams_RaidTeamId",
                        column: x => x.RaidTeamId,
                        principalTable: "RaidTeams",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "CharacterLootLists",
                columns: table => new
                {
                    CharacterId = table.Column<long>(type: "bigint", nullable: false),
                    Phase = table.Column<byte>(type: "tinyint", nullable: false),
                    Size = table.Column<byte>(type: "tinyint", nullable: false),
                    MainSpec = table.Column<long>(type: "bigint", nullable: false),
                    OffSpec = table.Column<long>(type: "bigint", nullable: false),
                    Status = table.Column<int>(type: "int", nullable: false),
                    ApprovedBy = table.Column<long>(type: "bigint", nullable: true),
                    Timestamp = table.Column<byte[]>(type: "rowversion", rowVersion: true, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CharacterLootLists", x => new { x.CharacterId, x.Phase, x.Size });
                    table.ForeignKey(
                        name: "FK_CharacterLootLists_Characters_CharacterId",
                        column: x => x.CharacterId,
                        principalTable: "Characters",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Donations",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false),
                    DonatedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    TargetMonth = table.Column<byte>(type: "tinyint", nullable: false),
                    TargetYear = table.Column<short>(type: "smallint", nullable: false),
                    EnteredById = table.Column<long>(type: "bigint", nullable: false),
                    Amount = table.Column<int>(type: "int", nullable: false),
                    Unit = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    CharacterId = table.Column<long>(type: "bigint", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Donations", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Donations_Characters_CharacterId",
                        column: x => x.CharacterId,
                        principalTable: "Characters",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "TeamMembers",
                columns: table => new
                {
                    CharacterId = table.Column<long>(type: "bigint", nullable: false),
                    TeamId = table.Column<long>(type: "bigint", nullable: false),
                    JoinedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    MemberStatus = table.Column<int>(type: "int", nullable: false),
                    Enchanted = table.Column<bool>(type: "bit", nullable: false),
                    Prepared = table.Column<bool>(type: "bit", nullable: false),
                    Disenchanter = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TeamMembers", x => new { x.TeamId, x.CharacterId });
                    table.ForeignKey(
                        name: "FK_TeamMembers_Characters_CharacterId",
                        column: x => x.CharacterId,
                        principalTable: "Characters",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_TeamMembers_RaidTeams_TeamId",
                        column: x => x.TeamId,
                        principalTable: "RaidTeams",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "TeamRemovals",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false),
                    JoinedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    RemovedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    TeamId = table.Column<long>(type: "bigint", nullable: false),
                    CharacterId = table.Column<long>(type: "bigint", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TeamRemovals", x => x.Id);
                    table.ForeignKey(
                        name: "FK_TeamRemovals_Characters_CharacterId",
                        column: x => x.CharacterId,
                        principalTable: "Characters",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_TeamRemovals_RaidTeams_TeamId",
                        column: x => x.TeamId,
                        principalTable: "RaidTeams",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Items",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(56)", maxLength: 56, nullable: false),
                    RewardFromId = table.Column<long>(type: "bigint", nullable: true),
                    Slot = table.Column<int>(type: "int", nullable: false),
                    Type = table.Column<int>(type: "int", nullable: false),
                    ItemLevel = table.Column<int>(type: "int", nullable: false),
                    Strength = table.Column<int>(type: "int", nullable: false),
                    Agility = table.Column<int>(type: "int", nullable: false),
                    Stamina = table.Column<int>(type: "int", nullable: false),
                    Intellect = table.Column<int>(type: "int", nullable: false),
                    Spirit = table.Column<int>(type: "int", nullable: false),
                    Hit = table.Column<int>(type: "int", nullable: false),
                    Crit = table.Column<int>(type: "int", nullable: false),
                    Haste = table.Column<int>(type: "int", nullable: false),
                    Defense = table.Column<int>(type: "int", nullable: false),
                    Dodge = table.Column<int>(type: "int", nullable: false),
                    BlockRating = table.Column<int>(type: "int", nullable: false),
                    BlockValue = table.Column<int>(type: "int", nullable: false),
                    Parry = table.Column<int>(type: "int", nullable: false),
                    SpellPower = table.Column<int>(type: "int", nullable: false),
                    ManaPer5 = table.Column<int>(type: "int", nullable: false),
                    AttackPower = table.Column<int>(type: "int", nullable: false),
                    Phase = table.Column<byte>(type: "tinyint", nullable: false),
                    Expertise = table.Column<int>(type: "int", nullable: false),
                    ArmorPenetration = table.Column<int>(type: "int", nullable: false),
                    SpellPenetration = table.Column<int>(type: "int", nullable: false),
                    HasProc = table.Column<bool>(type: "bit", nullable: false),
                    HasOnUse = table.Column<bool>(type: "bit", nullable: false),
                    HasSpecial = table.Column<bool>(type: "bit", nullable: false),
                    IsUnique = table.Column<bool>(type: "bit", nullable: false),
                    QuestId = table.Column<long>(type: "bigint", nullable: false),
                    UsableClasses = table.Column<int>(type: "int", nullable: true),
                    EncounterId = table.Column<string>(type: "nvarchar(20)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Items", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Items_Encounters_EncounterId",
                        column: x => x.EncounterId,
                        principalTable: "Encounters",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_Items_Items_RewardFromId",
                        column: x => x.RewardFromId,
                        principalTable: "Items",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "EncounterKills",
                columns: table => new
                {
                    RaidId = table.Column<long>(type: "bigint", nullable: false),
                    EncounterId = table.Column<string>(type: "nvarchar(20)", nullable: false),
                    TrashIndex = table.Column<byte>(type: "tinyint", nullable: false),
                    KilledAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    DiscordMessageId = table.Column<long>(type: "bigint", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_EncounterKills", x => new { x.EncounterId, x.RaidId, x.TrashIndex });
                    table.ForeignKey(
                        name: "FK_EncounterKills_Encounters_EncounterId",
                        column: x => x.EncounterId,
                        principalTable: "Encounters",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_EncounterKills_Raids_RaidId",
                        column: x => x.RaidId,
                        principalTable: "Raids",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "LootListTeamSubmissions",
                columns: table => new
                {
                    LootListCharacterId = table.Column<long>(type: "bigint", nullable: false),
                    LootListPhase = table.Column<byte>(type: "tinyint", nullable: false),
                    TeamId = table.Column<long>(type: "bigint", nullable: false),
                    LootListSize = table.Column<byte>(type: "tinyint", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_LootListTeamSubmissions", x => new { x.LootListCharacterId, x.LootListPhase, x.TeamId });
                    table.ForeignKey(
                        name: "FK_LootListTeamSubmissions_CharacterLootLists_LootListCharacterId_LootListPhase_LootListSize",
                        columns: x => new { x.LootListCharacterId, x.LootListPhase, x.LootListSize },
                        principalTable: "CharacterLootLists",
                        principalColumns: new[] { "CharacterId", "Phase", "Size" },
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_LootListTeamSubmissions_RaidTeams_TeamId",
                        column: x => x.TeamId,
                        principalTable: "RaidTeams",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "RaidAttendees",
                columns: table => new
                {
                    RaidId = table.Column<long>(type: "bigint", nullable: false),
                    CharacterId = table.Column<long>(type: "bigint", nullable: false),
                    Standby = table.Column<bool>(type: "bit", nullable: false),
                    RemovalId = table.Column<long>(type: "bigint", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RaidAttendees", x => new { x.CharacterId, x.RaidId });
                    table.ForeignKey(
                        name: "FK_RaidAttendees_Characters_CharacterId",
                        column: x => x.CharacterId,
                        principalTable: "Characters",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_RaidAttendees_Raids_RaidId",
                        column: x => x.RaidId,
                        principalTable: "Raids",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_RaidAttendees_TeamRemovals_RemovalId",
                        column: x => x.RemovalId,
                        principalTable: "TeamRemovals",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateTable(
                name: "EncounterItems",
                columns: table => new
                {
                    EncounterId = table.Column<string>(type: "nvarchar(20)", nullable: false),
                    ItemId = table.Column<long>(type: "bigint", nullable: false),
                    Heroic = table.Column<bool>(type: "bit", nullable: false),
                    Is25 = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_EncounterItems", x => new { x.EncounterId, x.ItemId, x.Heroic, x.Is25 });
                    table.ForeignKey(
                        name: "FK_EncounterItems_Encounters_EncounterId",
                        column: x => x.EncounterId,
                        principalTable: "Encounters",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_EncounterItems_Items_ItemId",
                        column: x => x.ItemId,
                        principalTable: "Items",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ItemRestrictions",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false),
                    ItemId = table.Column<long>(type: "bigint", nullable: false),
                    Specializations = table.Column<long>(type: "bigint", nullable: false),
                    RestrictionLevel = table.Column<int>(type: "int", nullable: false),
                    Reason = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: false),
                    Automated = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ItemRestrictions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ItemRestrictions_Items_ItemId",
                        column: x => x.ItemId,
                        principalTable: "Items",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "CharacterEncounterKill",
                columns: table => new
                {
                    CharacterId = table.Column<long>(type: "bigint", nullable: false),
                    EncounterKillRaidId = table.Column<long>(type: "bigint", nullable: false),
                    EncounterKillEncounterId = table.Column<string>(type: "nvarchar(20)", nullable: false),
                    EncounterKillTrashIndex = table.Column<byte>(type: "tinyint", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CharacterEncounterKill", x => new { x.EncounterKillRaidId, x.EncounterKillEncounterId, x.EncounterKillTrashIndex, x.CharacterId });
                    table.ForeignKey(
                        name: "FK_CharacterEncounterKill_Characters_CharacterId",
                        column: x => x.CharacterId,
                        principalTable: "Characters",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_CharacterEncounterKill_EncounterKills_EncounterKillEncounterId_EncounterKillRaidId_EncounterKillTrashIndex",
                        columns: x => new { x.EncounterKillEncounterId, x.EncounterKillRaidId, x.EncounterKillTrashIndex },
                        principalTable: "EncounterKills",
                        principalColumns: new[] { "EncounterId", "RaidId", "TrashIndex" },
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Drops",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false),
                    AwardedBy = table.Column<long>(type: "bigint", nullable: true),
                    AwardedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    Disenchanted = table.Column<bool>(type: "bit", nullable: false),
                    ItemId = table.Column<long>(type: "bigint", nullable: false),
                    WinnerId = table.Column<long>(type: "bigint", nullable: true),
                    EncounterKillRaidId = table.Column<long>(type: "bigint", nullable: false),
                    EncounterKillEncounterId = table.Column<string>(type: "nvarchar(20)", nullable: false),
                    EncounterKillTrashIndex = table.Column<byte>(type: "tinyint", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Drops", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Drops_Characters_WinnerId",
                        column: x => x.WinnerId,
                        principalTable: "Characters",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Drops_EncounterKills_EncounterKillEncounterId_EncounterKillRaidId_EncounterKillTrashIndex",
                        columns: x => new { x.EncounterKillEncounterId, x.EncounterKillRaidId, x.EncounterKillTrashIndex },
                        principalTable: "EncounterKills",
                        principalColumns: new[] { "EncounterId", "RaidId", "TrashIndex" },
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Drops_Items_ItemId",
                        column: x => x.ItemId,
                        principalTable: "Items",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "LootListEntries",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false),
                    LootListCharacterId = table.Column<long>(type: "bigint", nullable: false),
                    LootListPhase = table.Column<byte>(type: "tinyint", nullable: false),
                    LootListSize = table.Column<byte>(type: "tinyint", nullable: false),
                    Rank = table.Column<byte>(type: "tinyint", nullable: false),
                    ItemId = table.Column<long>(type: "bigint", nullable: true),
                    DropId = table.Column<long>(type: "bigint", nullable: true),
                    Justification = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    AutoPass = table.Column<bool>(type: "bit", nullable: false),
                    Heroic = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_LootListEntries", x => x.Id);
                    table.ForeignKey(
                        name: "FK_LootListEntries_CharacterLootLists_LootListCharacterId_LootListPhase_LootListSize",
                        columns: x => new { x.LootListCharacterId, x.LootListPhase, x.LootListSize },
                        principalTable: "CharacterLootLists",
                        principalColumns: new[] { "CharacterId", "Phase", "Size" },
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_LootListEntries_Drops_DropId",
                        column: x => x.DropId,
                        principalTable: "Drops",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_LootListEntries_Items_ItemId",
                        column: x => x.ItemId,
                        principalTable: "Items",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "DropPasses",
                columns: table => new
                {
                    CharacterId = table.Column<long>(type: "bigint", nullable: false),
                    DropId = table.Column<long>(type: "bigint", nullable: false),
                    LootListEntryId = table.Column<long>(type: "bigint", nullable: true),
                    RelativePriority = table.Column<int>(type: "int", nullable: false),
                    WonEntryId = table.Column<long>(type: "bigint", nullable: true),
                    RemovalId = table.Column<long>(type: "bigint", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DropPasses", x => new { x.DropId, x.CharacterId });
                    table.ForeignKey(
                        name: "FK_DropPasses_Characters_CharacterId",
                        column: x => x.CharacterId,
                        principalTable: "Characters",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_DropPasses_Drops_DropId",
                        column: x => x.DropId,
                        principalTable: "Drops",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_DropPasses_LootListEntries_LootListEntryId",
                        column: x => x.LootListEntryId,
                        principalTable: "LootListEntries",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_DropPasses_TeamRemovals_RemovalId",
                        column: x => x.RemovalId,
                        principalTable: "TeamRemovals",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.InsertData(
                table: "Brackets",
                columns: new[] { "Index", "Phase", "AllowOffspec", "AllowTypeDuplicates", "HeroicItems", "MaxRank", "MinRank", "NormalItems" },
                values: new object[,]
                {
                    { (byte)0, (byte)1, false, false, (byte)0, (byte)24, (byte)21, (byte)1 },
                    { (byte)1, (byte)1, false, false, (byte)0, (byte)20, (byte)17, (byte)1 },
                    { (byte)2, (byte)1, false, false, (byte)0, (byte)16, (byte)13, (byte)1 },
                    { (byte)3, (byte)1, true, true, (byte)0, (byte)12, (byte)1, (byte)1 },
                    { (byte)0, (byte)2, false, false, (byte)1, (byte)24, (byte)21, (byte)1 },
                    { (byte)1, (byte)2, false, false, (byte)1, (byte)20, (byte)17, (byte)1 },
                    { (byte)2, (byte)2, false, false, (byte)1, (byte)16, (byte)13, (byte)1 },
                    { (byte)3, (byte)2, true, true, (byte)1, (byte)12, (byte)1, (byte)1 }
                });

            migrationBuilder.InsertData(
                table: "PhaseDetails",
                columns: new[] { "Id", "StartsAt" },
                values: new object[,]
                {
                    { (byte)1, new DateTimeOffset(new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)) },
                    { (byte)2, new DateTimeOffset(new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)) }
                });

            migrationBuilder.CreateIndex(
                name: "IX_AspNetRoleClaims_RoleId",
                table: "AspNetRoleClaims",
                column: "RoleId");

            migrationBuilder.CreateIndex(
                name: "RoleNameIndex",
                table: "AspNetRoles",
                column: "NormalizedName",
                unique: true,
                filter: "[NormalizedName] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_AspNetUserClaims_UserId",
                table: "AspNetUserClaims",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_AspNetUserLogins_UserId",
                table: "AspNetUserLogins",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_AspNetUserRoles_RoleId",
                table: "AspNetUserRoles",
                column: "RoleId");

            migrationBuilder.CreateIndex(
                name: "EmailIndex",
                table: "AspNetUsers",
                column: "NormalizedEmail");

            migrationBuilder.CreateIndex(
                name: "UserNameIndex",
                table: "AspNetUsers",
                column: "NormalizedUserName",
                unique: true,
                filter: "[NormalizedUserName] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_CharacterEncounterKill_CharacterId",
                table: "CharacterEncounterKill",
                column: "CharacterId");

            migrationBuilder.CreateIndex(
                name: "IX_CharacterEncounterKill_EncounterKillEncounterId_EncounterKillRaidId_EncounterKillTrashIndex",
                table: "CharacterEncounterKill",
                columns: new[] { "EncounterKillEncounterId", "EncounterKillRaidId", "EncounterKillTrashIndex" });

            migrationBuilder.CreateIndex(
                name: "IX_Characters_Name",
                table: "Characters",
                column: "Name",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Characters_OwnerId",
                table: "Characters",
                column: "OwnerId");

            migrationBuilder.CreateIndex(
                name: "IX_DeviceCodes_DeviceCode",
                table: "DeviceCodes",
                column: "DeviceCode",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_DeviceCodes_Expiration",
                table: "DeviceCodes",
                column: "Expiration");

            migrationBuilder.CreateIndex(
                name: "IX_Donations_CharacterId",
                table: "Donations",
                column: "CharacterId");

            migrationBuilder.CreateIndex(
                name: "IX_DropPasses_CharacterId",
                table: "DropPasses",
                column: "CharacterId");

            migrationBuilder.CreateIndex(
                name: "IX_DropPasses_LootListEntryId",
                table: "DropPasses",
                column: "LootListEntryId");

            migrationBuilder.CreateIndex(
                name: "IX_DropPasses_RemovalId",
                table: "DropPasses",
                column: "RemovalId");

            migrationBuilder.CreateIndex(
                name: "IX_Drops_EncounterKillEncounterId_EncounterKillRaidId_EncounterKillTrashIndex",
                table: "Drops",
                columns: new[] { "EncounterKillEncounterId", "EncounterKillRaidId", "EncounterKillTrashIndex" });

            migrationBuilder.CreateIndex(
                name: "IX_Drops_ItemId",
                table: "Drops",
                column: "ItemId");

            migrationBuilder.CreateIndex(
                name: "IX_Drops_WinnerId",
                table: "Drops",
                column: "WinnerId");

            migrationBuilder.CreateIndex(
                name: "IX_EncounterItems_ItemId",
                table: "EncounterItems",
                column: "ItemId");

            migrationBuilder.CreateIndex(
                name: "IX_EncounterKills_RaidId",
                table: "EncounterKills",
                column: "RaidId");

            migrationBuilder.CreateIndex(
                name: "IX_Encounters_InstanceId",
                table: "Encounters",
                column: "InstanceId");

            migrationBuilder.CreateIndex(
                name: "IX_ItemRestrictions_ItemId",
                table: "ItemRestrictions",
                column: "ItemId");

            migrationBuilder.CreateIndex(
                name: "IX_ItemRestrictions_RestrictionLevel_Specializations",
                table: "ItemRestrictions",
                columns: new[] { "RestrictionLevel", "Specializations" });

            migrationBuilder.CreateIndex(
                name: "IX_Items_EncounterId",
                table: "Items",
                column: "EncounterId");

            migrationBuilder.CreateIndex(
                name: "IX_Items_RewardFromId",
                table: "Items",
                column: "RewardFromId");

            migrationBuilder.CreateIndex(
                name: "IX_LootListEntries_DropId",
                table: "LootListEntries",
                column: "DropId",
                unique: true,
                filter: "[DropId] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_LootListEntries_ItemId",
                table: "LootListEntries",
                column: "ItemId");

            migrationBuilder.CreateIndex(
                name: "IX_LootListEntries_LootListCharacterId_LootListPhase_LootListSize",
                table: "LootListEntries",
                columns: new[] { "LootListCharacterId", "LootListPhase", "LootListSize" });

            migrationBuilder.CreateIndex(
                name: "IX_LootListTeamSubmissions_LootListCharacterId_LootListPhase_LootListSize",
                table: "LootListTeamSubmissions",
                columns: new[] { "LootListCharacterId", "LootListPhase", "LootListSize" });

            migrationBuilder.CreateIndex(
                name: "IX_LootListTeamSubmissions_TeamId",
                table: "LootListTeamSubmissions",
                column: "TeamId");

            migrationBuilder.CreateIndex(
                name: "IX_PersistedGrants_Expiration",
                table: "PersistedGrants",
                column: "Expiration");

            migrationBuilder.CreateIndex(
                name: "IX_PersistedGrants_SubjectId_ClientId_Type",
                table: "PersistedGrants",
                columns: new[] { "SubjectId", "ClientId", "Type" });

            migrationBuilder.CreateIndex(
                name: "IX_PersistedGrants_SubjectId_SessionId_Type",
                table: "PersistedGrants",
                columns: new[] { "SubjectId", "SessionId", "Type" });

            migrationBuilder.CreateIndex(
                name: "IX_RaidAttendees_RaidId",
                table: "RaidAttendees",
                column: "RaidId");

            migrationBuilder.CreateIndex(
                name: "IX_RaidAttendees_RemovalId",
                table: "RaidAttendees",
                column: "RemovalId");

            migrationBuilder.CreateIndex(
                name: "IX_Raids_RaidTeamId",
                table: "Raids",
                column: "RaidTeamId");

            migrationBuilder.CreateIndex(
                name: "IX_RaidTeamLeaders_UserId",
                table: "RaidTeamLeaders",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_RaidTeams_Name",
                table: "RaidTeams",
                column: "Name",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_RaidTeamSchedules_RaidTeamId",
                table: "RaidTeamSchedules",
                column: "RaidTeamId");

            migrationBuilder.CreateIndex(
                name: "IX_TeamMembers_CharacterId",
                table: "TeamMembers",
                column: "CharacterId");

            migrationBuilder.CreateIndex(
                name: "IX_TeamRemovals_CharacterId",
                table: "TeamRemovals",
                column: "CharacterId");

            migrationBuilder.CreateIndex(
                name: "IX_TeamRemovals_TeamId",
                table: "TeamRemovals",
                column: "TeamId");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "AspNetRoleClaims");

            migrationBuilder.DropTable(
                name: "AspNetUserClaims");

            migrationBuilder.DropTable(
                name: "AspNetUserLogins");

            migrationBuilder.DropTable(
                name: "AspNetUserRoles");

            migrationBuilder.DropTable(
                name: "AspNetUserTokens");

            migrationBuilder.DropTable(
                name: "Brackets");

            migrationBuilder.DropTable(
                name: "CharacterEncounterKill");

            migrationBuilder.DropTable(
                name: "DeviceCodes");

            migrationBuilder.DropTable(
                name: "Donations");

            migrationBuilder.DropTable(
                name: "DropPasses");

            migrationBuilder.DropTable(
                name: "EncounterItems");

            migrationBuilder.DropTable(
                name: "ItemRestrictions");

            migrationBuilder.DropTable(
                name: "LootListTeamSubmissions");

            migrationBuilder.DropTable(
                name: "PersistedGrants");

            migrationBuilder.DropTable(
                name: "PhaseDetails");

            migrationBuilder.DropTable(
                name: "RaidAttendees");

            migrationBuilder.DropTable(
                name: "RaidTeamLeaders");

            migrationBuilder.DropTable(
                name: "RaidTeamSchedules");

            migrationBuilder.DropTable(
                name: "TeamMembers");

            migrationBuilder.DropTable(
                name: "AspNetRoles");

            migrationBuilder.DropTable(
                name: "LootListEntries");

            migrationBuilder.DropTable(
                name: "TeamRemovals");

            migrationBuilder.DropTable(
                name: "CharacterLootLists");

            migrationBuilder.DropTable(
                name: "Drops");

            migrationBuilder.DropTable(
                name: "Characters");

            migrationBuilder.DropTable(
                name: "EncounterKills");

            migrationBuilder.DropTable(
                name: "Items");

            migrationBuilder.DropTable(
                name: "AspNetUsers");

            migrationBuilder.DropTable(
                name: "Raids");

            migrationBuilder.DropTable(
                name: "Encounters");

            migrationBuilder.DropTable(
                name: "RaidTeams");

            migrationBuilder.DropTable(
                name: "Instances");
        }
    }
}