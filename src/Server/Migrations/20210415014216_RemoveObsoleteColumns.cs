// Copyright (C) 2021 Donovan Sullivan
// GNU General Public License v3.0+ (see LICENSE or https://www.gnu.org/licenses/gpl-3.0.txt)

using Microsoft.EntityFrameworkCore.Migrations;

namespace ValhallaLootList.Server.Migrations;

public partial class RemoveObsoleteColumns : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropForeignKey(
            name: "FK_CharacterLootLists_RaidTeams_SubmittedToId",
            table: "CharacterLootLists");

        migrationBuilder.DropIndex(
            name: "IX_CharacterLootLists_SubmittedToId",
            table: "CharacterLootLists");

        migrationBuilder.DropColumn(
            name: "Month",
            table: "Donations");

        migrationBuilder.DropColumn(
            name: "Year",
            table: "Donations");

        migrationBuilder.DropColumn(
            name: "Locked",
            table: "CharacterLootLists");

        migrationBuilder.DropColumn(
            name: "SubmittedToId",
            table: "CharacterLootLists");
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.AddColumn<byte>(
            name: "Month",
            table: "Donations",
            type: "tinyint",
            nullable: false,
            defaultValue: (byte)0);

        migrationBuilder.AddColumn<short>(
            name: "Year",
            table: "Donations",
            type: "smallint",
            nullable: false,
            defaultValue: (short)0);

        migrationBuilder.AddColumn<bool>(
            name: "Locked",
            table: "CharacterLootLists",
            type: "bit",
            nullable: false,
            defaultValue: false);

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
    }
}
