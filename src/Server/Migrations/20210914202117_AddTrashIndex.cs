// Copyright (C) 2021 Donovan Sullivan
// GNU General Public License v3.0+ (see LICENSE or https://www.gnu.org/licenses/gpl-3.0.txt)

using Microsoft.EntityFrameworkCore.Migrations;

namespace ValhallaLootList.Server.Migrations;

public partial class AddTrashIndex : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropForeignKey(
            name: "FK_CharacterEncounterKill_EncounterKills_EncounterKillEncounterId_EncounterKillRaidId",
            table: "CharacterEncounterKill");

        migrationBuilder.DropForeignKey(
            name: "FK_Drops_EncounterKills_EncounterKillEncounterId_EncounterKillRaidId",
            table: "Drops");

        migrationBuilder.DropPrimaryKey(
            name: "PK_EncounterKills",
            table: "EncounterKills");

        migrationBuilder.DropIndex(
            name: "IX_Drops_EncounterKillEncounterId_EncounterKillRaidId",
            table: "Drops");

        migrationBuilder.DropIndex(
            name: "IX_CharacterEncounterKill_EncounterKillEncounterId_EncounterKillRaidId",
            table: "CharacterEncounterKill");

        migrationBuilder.AddColumn<byte>(
            name: "TrashIndex",
            table: "EncounterKills",
            type: "tinyint",
            nullable: false,
            defaultValue: (byte)0);

        migrationBuilder.AddColumn<byte>(
            name: "EncounterKillTrashIndex",
            table: "Drops",
            type: "tinyint",
            nullable: false,
            defaultValue: (byte)0);

        migrationBuilder.AddColumn<byte>(
            name: "EncounterKillTrashIndex",
            table: "CharacterEncounterKill",
            type: "tinyint",
            nullable: false,
            defaultValue: (byte)0);

        migrationBuilder.AddPrimaryKey(
            name: "PK_EncounterKills",
            table: "EncounterKills",
            columns: new[] { "EncounterId", "RaidId", "TrashIndex" });

        migrationBuilder.CreateIndex(
            name: "IX_Drops_EncounterKillEncounterId_EncounterKillRaidId_EncounterKillTrashIndex",
            table: "Drops",
            columns: new[] { "EncounterKillEncounterId", "EncounterKillRaidId", "EncounterKillTrashIndex" });

        migrationBuilder.CreateIndex(
            name: "IX_CharacterEncounterKill_EncounterKillEncounterId_EncounterKillRaidId_EncounterKillTrashIndex",
            table: "CharacterEncounterKill",
            columns: new[] { "EncounterKillEncounterId", "EncounterKillRaidId", "EncounterKillTrashIndex" });

        migrationBuilder.AddForeignKey(
            name: "FK_CharacterEncounterKill_EncounterKills_EncounterKillEncounterId_EncounterKillRaidId_EncounterKillTrashIndex",
            table: "CharacterEncounterKill",
            columns: new[] { "EncounterKillEncounterId", "EncounterKillRaidId", "EncounterKillTrashIndex" },
            principalTable: "EncounterKills",
            principalColumns: new[] { "EncounterId", "RaidId", "TrashIndex" },
            onDelete: ReferentialAction.Cascade);

        migrationBuilder.AddForeignKey(
            name: "FK_Drops_EncounterKills_EncounterKillEncounterId_EncounterKillRaidId_EncounterKillTrashIndex",
            table: "Drops",
            columns: new[] { "EncounterKillEncounterId", "EncounterKillRaidId", "EncounterKillTrashIndex" },
            principalTable: "EncounterKills",
            principalColumns: new[] { "EncounterId", "RaidId", "TrashIndex" },
            onDelete: ReferentialAction.Cascade);
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropForeignKey(
            name: "FK_CharacterEncounterKill_EncounterKills_EncounterKillEncounterId_EncounterKillRaidId_EncounterKillTrashIndex",
            table: "CharacterEncounterKill");

        migrationBuilder.DropForeignKey(
            name: "FK_Drops_EncounterKills_EncounterKillEncounterId_EncounterKillRaidId_EncounterKillTrashIndex",
            table: "Drops");

        migrationBuilder.DropPrimaryKey(
            name: "PK_EncounterKills",
            table: "EncounterKills");

        migrationBuilder.DropIndex(
            name: "IX_Drops_EncounterKillEncounterId_EncounterKillRaidId_EncounterKillTrashIndex",
            table: "Drops");

        migrationBuilder.DropIndex(
            name: "IX_CharacterEncounterKill_EncounterKillEncounterId_EncounterKillRaidId_EncounterKillTrashIndex",
            table: "CharacterEncounterKill");

        migrationBuilder.DropColumn(
            name: "TrashIndex",
            table: "EncounterKills");

        migrationBuilder.DropColumn(
            name: "EncounterKillTrashIndex",
            table: "Drops");

        migrationBuilder.DropColumn(
            name: "EncounterKillTrashIndex",
            table: "CharacterEncounterKill");

        migrationBuilder.AddPrimaryKey(
            name: "PK_EncounterKills",
            table: "EncounterKills",
            columns: new[] { "EncounterId", "RaidId" });

        migrationBuilder.CreateIndex(
            name: "IX_Drops_EncounterKillEncounterId_EncounterKillRaidId",
            table: "Drops",
            columns: new[] { "EncounterKillEncounterId", "EncounterKillRaidId" });

        migrationBuilder.CreateIndex(
            name: "IX_CharacterEncounterKill_EncounterKillEncounterId_EncounterKillRaidId",
            table: "CharacterEncounterKill",
            columns: new[] { "EncounterKillEncounterId", "EncounterKillRaidId" });

        migrationBuilder.AddForeignKey(
            name: "FK_CharacterEncounterKill_EncounterKills_EncounterKillEncounterId_EncounterKillRaidId",
            table: "CharacterEncounterKill",
            columns: new[] { "EncounterKillEncounterId", "EncounterKillRaidId" },
            principalTable: "EncounterKills",
            principalColumns: new[] { "EncounterId", "RaidId" },
            onDelete: ReferentialAction.Cascade);

        migrationBuilder.AddForeignKey(
            name: "FK_Drops_EncounterKills_EncounterKillEncounterId_EncounterKillRaidId",
            table: "Drops",
            columns: new[] { "EncounterKillEncounterId", "EncounterKillRaidId" },
            principalTable: "EncounterKills",
            principalColumns: new[] { "EncounterId", "RaidId" },
            onDelete: ReferentialAction.Cascade);
    }
}
