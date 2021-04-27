// Copyright (C) 2021 Donovan Sullivan
// GNU General Public License v3.0+ (see LICENSE or https://www.gnu.org/licenses/gpl-3.0.txt)

using Microsoft.EntityFrameworkCore.Migrations;

namespace ValhallaLootList.Server.Migrations
{
    public partial class ImprovedCharacterDeletion : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_LootListEntries_CharacterLootLists_LootListCharacterId_LootListPhase",
                table: "LootListEntries");

            migrationBuilder.AddColumn<bool>(
                name: "Deactivated",
                table: "Characters",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddForeignKey(
                name: "FK_LootListEntries_CharacterLootLists_LootListCharacterId_LootListPhase",
                table: "LootListEntries",
                columns: new[] { "LootListCharacterId", "LootListPhase" },
                principalTable: "CharacterLootLists",
                principalColumns: new[] { "CharacterId", "Phase" },
                onDelete: ReferentialAction.Cascade);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_LootListEntries_CharacterLootLists_LootListCharacterId_LootListPhase",
                table: "LootListEntries");

            migrationBuilder.DropColumn(
                name: "Deactivated",
                table: "Characters");

            migrationBuilder.AddForeignKey(
                name: "FK_LootListEntries_CharacterLootLists_LootListCharacterId_LootListPhase",
                table: "LootListEntries",
                columns: new[] { "LootListCharacterId", "LootListPhase" },
                principalTable: "CharacterLootLists",
                principalColumns: new[] { "CharacterId", "Phase" },
                onDelete: ReferentialAction.Restrict);
        }
    }
}
