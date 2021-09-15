// Copyright (C) 2021 Donovan Sullivan
// GNU General Public License v3.0+ (see LICENSE or https://www.gnu.org/licenses/gpl-3.0.txt)

using Microsoft.EntityFrameworkCore.Migrations;

namespace ValhallaLootList.Server.Migrations
{
    public partial class AddMissingTrashIndexKey : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropPrimaryKey(
                name: "PK_CharacterEncounterKill",
                table: "CharacterEncounterKill");

            migrationBuilder.AddPrimaryKey(
                name: "PK_CharacterEncounterKill",
                table: "CharacterEncounterKill",
                columns: new[] { "EncounterKillRaidId", "EncounterKillEncounterId", "EncounterKillTrashIndex", "CharacterId" });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropPrimaryKey(
                name: "PK_CharacterEncounterKill",
                table: "CharacterEncounterKill");

            migrationBuilder.AddPrimaryKey(
                name: "PK_CharacterEncounterKill",
                table: "CharacterEncounterKill",
                columns: new[] { "EncounterKillRaidId", "EncounterKillEncounterId", "CharacterId" });
        }
    }
}
