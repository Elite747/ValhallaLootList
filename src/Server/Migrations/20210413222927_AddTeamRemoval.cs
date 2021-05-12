// Copyright (C) 2021 Donovan Sullivan
// GNU General Public License v3.0+ (see LICENSE or https://www.gnu.org/licenses/gpl-3.0.txt)

using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace ValhallaLootList.Server.Migrations
{
    public partial class AddTeamRemoval : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<long>(
                name: "RemovalId",
                table: "RaidAttendees",
                type: "bigint",
                nullable: true);

            migrationBuilder.AddColumn<long>(
                name: "RemovalId",
                table: "DropPasses",
                type: "bigint",
                nullable: true);

            migrationBuilder.AddColumn<long>(
                name: "RemovalId",
                table: "Donations",
                type: "bigint",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "TeamRemovals",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false),
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

            migrationBuilder.CreateIndex(
                name: "IX_RaidAttendees_RemovalId",
                table: "RaidAttendees",
                column: "RemovalId");

            migrationBuilder.CreateIndex(
                name: "IX_DropPasses_RemovalId",
                table: "DropPasses",
                column: "RemovalId");

            migrationBuilder.CreateIndex(
                name: "IX_Donations_RemovalId",
                table: "Donations",
                column: "RemovalId");

            migrationBuilder.CreateIndex(
                name: "IX_TeamRemovals_CharacterId",
                table: "TeamRemovals",
                column: "CharacterId");

            migrationBuilder.CreateIndex(
                name: "IX_TeamRemovals_TeamId",
                table: "TeamRemovals",
                column: "TeamId");

            migrationBuilder.AddForeignKey(
                name: "FK_Donations_TeamRemovals_RemovalId",
                table: "Donations",
                column: "RemovalId",
                principalTable: "TeamRemovals",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);

            migrationBuilder.AddForeignKey(
                name: "FK_DropPasses_TeamRemovals_RemovalId",
                table: "DropPasses",
                column: "RemovalId",
                principalTable: "TeamRemovals",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);

            migrationBuilder.AddForeignKey(
                name: "FK_RaidAttendees_TeamRemovals_RemovalId",
                table: "RaidAttendees",
                column: "RemovalId",
                principalTable: "TeamRemovals",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Donations_TeamRemovals_RemovalId",
                table: "Donations");

            migrationBuilder.DropForeignKey(
                name: "FK_DropPasses_TeamRemovals_RemovalId",
                table: "DropPasses");

            migrationBuilder.DropForeignKey(
                name: "FK_RaidAttendees_TeamRemovals_RemovalId",
                table: "RaidAttendees");

            migrationBuilder.DropTable(
                name: "TeamRemovals");

            migrationBuilder.DropIndex(
                name: "IX_RaidAttendees_RemovalId",
                table: "RaidAttendees");

            migrationBuilder.DropIndex(
                name: "IX_DropPasses_RemovalId",
                table: "DropPasses");

            migrationBuilder.DropIndex(
                name: "IX_Donations_RemovalId",
                table: "Donations");

            migrationBuilder.DropColumn(
                name: "RemovalId",
                table: "RaidAttendees");

            migrationBuilder.DropColumn(
                name: "RemovalId",
                table: "DropPasses");

            migrationBuilder.DropColumn(
                name: "RemovalId",
                table: "Donations");
        }
    }
}
