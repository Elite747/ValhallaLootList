﻿// <auto-generated />
using Microsoft.EntityFrameworkCore.Migrations;

namespace ValhallaLootList.Server.Migrations
{
    public partial class AddSubmissionsTable : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "LootListTeamSubmissions",
                columns: table => new
                {
                    LootListCharacterId = table.Column<long>(type: "bigint", nullable: false),
                    LootListPhase = table.Column<byte>(type: "tinyint", nullable: false),
                    TeamId = table.Column<long>(type: "bigint", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_LootListTeamSubmissions", x => new { x.LootListCharacterId, x.LootListPhase, x.TeamId });
                    table.ForeignKey(
                        name: "FK_LootListTeamSubmissions_CharacterLootLists_LootListCharacterId_LootListPhase",
                        columns: x => new { x.LootListCharacterId, x.LootListPhase },
                        principalTable: "CharacterLootLists",
                        principalColumns: new[] { "CharacterId", "Phase" },
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_LootListTeamSubmissions_RaidTeams_TeamId",
                        column: x => x.TeamId,
                        principalTable: "RaidTeams",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_LootListTeamSubmissions_TeamId",
                table: "LootListTeamSubmissions",
                column: "TeamId");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "LootListTeamSubmissions");
        }
    }
}
