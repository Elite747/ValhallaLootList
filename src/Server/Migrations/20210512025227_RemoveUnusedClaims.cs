// Copyright (C) 2021 Donovan Sullivan
// GNU General Public License v3.0+ (see LICENSE or https://www.gnu.org/licenses/gpl-3.0.txt)

using Microsoft.EntityFrameworkCore.Migrations;

namespace ValhallaLootList.Server.Migrations;

public partial class RemoveUnusedClaims : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.Sql("DELETE FROM AspNetUserClaims WHERE ClaimType NOT IN ('Character', 'RaidLeader')");
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
    }
}
