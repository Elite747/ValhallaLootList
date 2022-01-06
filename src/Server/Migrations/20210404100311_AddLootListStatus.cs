// Copyright (C) 2021 Donovan Sullivan
// GNU General Public License v3.0+ (see LICENSE or https://www.gnu.org/licenses/gpl-3.0.txt)

using Microsoft.EntityFrameworkCore.Migrations;

namespace ValhallaLootList.Server.Migrations;

public partial class AddLootListStatus : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.AddColumn<int>(
            name: "Status",
            table: "CharacterLootLists",
            type: "int",
            nullable: false,
            defaultValue: 0);

        migrationBuilder.AddColumn<long>(
            name: "SubmittedToId",
            table: "CharacterLootLists",
            type: "bigint",
            nullable: true);

        migrationBuilder.CreateIndex(
            name: "IX_CharacterLootLists_SubmittedToId",
            table: "CharacterLootLists",
            column: "SubmittedToId");

        migrationBuilder.AddForeignKey(
            name: "FK_CharacterLootLists_RaidTeams_SubmittedToId",
            table: "CharacterLootLists",
            column: "SubmittedToId",
            principalTable: "RaidTeams",
            principalColumn: "Id",
            onDelete: ReferentialAction.Restrict);

        /* Editing = 0,
         * Submitted = 1,
         * Approved = 2,
         * Locked = 3
         */
        migrationBuilder.Sql("UPDATE CharacterLootLists SET Status = 2 WHERE ApprovedBy IS NOT NULL");
        migrationBuilder.Sql("UPDATE CharacterLootLists SET Status = 3 WHERE Locked = CAST(1 AS bit)");
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropForeignKey(
            name: "FK_CharacterLootLists_RaidTeams_SubmittedToId",
            table: "CharacterLootLists");

        migrationBuilder.DropIndex(
            name: "IX_CharacterLootLists_SubmittedToId",
            table: "CharacterLootLists");

        migrationBuilder.DropColumn(
            name: "Status",
            table: "CharacterLootLists");

        migrationBuilder.DropColumn(
            name: "SubmittedToId",
            table: "CharacterLootLists");
    }
}
