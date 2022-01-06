// Copyright (C) 2021 Donovan Sullivan
// GNU General Public License v3.0+ (see LICENSE or https://www.gnu.org/licenses/gpl-3.0.txt)

using Microsoft.EntityFrameworkCore.Migrations;

namespace ValhallaLootList.Server.Migrations;

public partial class AddDirectOwnershipLinks : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.AddColumn<long>(
            name: "OwnerId",
            table: "Characters",
            type: "bigint",
            nullable: true);

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

        migrationBuilder.CreateIndex(
            name: "IX_Characters_OwnerId",
            table: "Characters",
            column: "OwnerId");

        migrationBuilder.CreateIndex(
            name: "IX_RaidTeamLeaders_UserId",
            table: "RaidTeamLeaders",
            column: "UserId");

        migrationBuilder.AddForeignKey(
            name: "FK_Characters_AspNetUsers_OwnerId",
            table: "Characters",
            column: "OwnerId",
            principalTable: "AspNetUsers",
            principalColumn: "Id",
            onDelete: ReferentialAction.SetNull);

        migrationBuilder.Sql(@"UPDATE c
SET c.OwnerId = topEnt.UserId
FROM Characters c
INNER JOIN (
	SELECT claim.UserId, CONVERT(BIGINT, claim.ClaimValue) AS CharacterId
	FROM AspNetUserClaims claim
	WHERE claim.ClaimType = 'Character'
) topEnt ON topEnt.CharacterId = c.Id");

        migrationBuilder.Sql(@"INSERT INTO RaidTeamLeaders (RaidTeamId, UserId)
SELECT CONVERT(BIGINT, claim.ClaimValue) AS RaidTeamId, claim.UserId
FROM AspNetUserClaims claim
WHERE claim.ClaimType = 'RaidLeader'");
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropForeignKey(
            name: "FK_Characters_AspNetUsers_OwnerId",
            table: "Characters");

        migrationBuilder.DropTable(
            name: "RaidTeamLeaders");

        migrationBuilder.DropIndex(
            name: "IX_Characters_OwnerId",
            table: "Characters");

        migrationBuilder.DropColumn(
            name: "OwnerId",
            table: "Characters");
    }
}
