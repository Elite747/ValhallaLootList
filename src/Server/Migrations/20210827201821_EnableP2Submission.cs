// Copyright (C) 2021 Donovan Sullivan
// GNU General Public License v3.0+ (see LICENSE or https://www.gnu.org/licenses/gpl-3.0.txt)

using Microsoft.EntityFrameworkCore.Migrations;

namespace ValhallaLootList.Server.Migrations;

public partial class EnableP2Submission : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.UpdateData(
            table: "PhaseDetails",
            keyColumn: "Id",
            keyValue: (byte)2,
            column: "StartsAt",
            value: new DateTimeOffset(new DateTime(2021, 8, 27, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, -4, 0, 0, 0)));
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.UpdateData(
            table: "PhaseDetails",
            keyColumn: "Id",
            keyValue: (byte)2,
            column: "StartsAt",
            value: new DateTimeOffset(new DateTime(2021, 8, 31, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, -4, 0, 0, 0)));
    }
}
