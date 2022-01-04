// Copyright (C) 2021 Donovan Sullivan
// GNU General Public License v3.0+ (see LICENSE or https://www.gnu.org/licenses/gpl-3.0.txt)

using Microsoft.EntityFrameworkCore.Migrations;

namespace ValhallaLootList.Server.Migrations;

public partial class LimitStringLengths : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.AlterColumn<string>(
            name: "Name",
            table: "RaidTeams",
            type: "nvarchar(24)",
            maxLength: 24,
            nullable: false,
            oldClrType: typeof(string),
            oldType: "nvarchar(450)");

        migrationBuilder.AlterColumn<string>(
            name: "IgnoreReason",
            table: "RaidAttendees",
            type: "nvarchar(256)",
            maxLength: 256,
            nullable: true,
            oldClrType: typeof(string),
            oldType: "nvarchar(max)",
            oldNullable: true);

        migrationBuilder.AlterColumn<string>(
            name: "Name",
            table: "Items",
            type: "nvarchar(56)",
            maxLength: 56,
            nullable: false,
            oldClrType: typeof(string),
            oldType: "nvarchar(max)");

        migrationBuilder.AlterColumn<string>(
            name: "Name",
            table: "Characters",
            type: "nvarchar(12)",
            maxLength: 12,
            nullable: false,
            oldClrType: typeof(string),
            oldType: "nvarchar(16)",
            oldMaxLength: 16);
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.AlterColumn<string>(
            name: "Name",
            table: "RaidTeams",
            type: "nvarchar(450)",
            nullable: false,
            oldClrType: typeof(string),
            oldType: "nvarchar(24)",
            oldMaxLength: 24);

        migrationBuilder.AlterColumn<string>(
            name: "IgnoreReason",
            table: "RaidAttendees",
            type: "nvarchar(max)",
            nullable: true,
            oldClrType: typeof(string),
            oldType: "nvarchar(256)",
            oldMaxLength: 256,
            oldNullable: true);

        migrationBuilder.AlterColumn<string>(
            name: "Name",
            table: "Items",
            type: "nvarchar(max)",
            nullable: false,
            oldClrType: typeof(string),
            oldType: "nvarchar(56)",
            oldMaxLength: 56);

        migrationBuilder.AlterColumn<string>(
            name: "Name",
            table: "Characters",
            type: "nvarchar(16)",
            maxLength: 16,
            nullable: false,
            oldClrType: typeof(string),
            oldType: "nvarchar(12)",
            oldMaxLength: 12);
    }
}
