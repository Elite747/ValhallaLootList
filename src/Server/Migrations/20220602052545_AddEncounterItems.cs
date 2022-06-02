// Copyright (C) 2021 Donovan Sullivan
// GNU General Public License v3.0+ (see LICENSE or https://www.gnu.org/licenses/gpl-3.0.txt)

using Microsoft.EntityFrameworkCore.Migrations;

namespace ValhallaLootList.Server.Migrations;

public partial class AddEncounterItems : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.CreateTable(
            name: "EncounterItems",
            columns: table => new
            {
                EncounterId = table.Column<string>(type: "nvarchar(20)", nullable: false),
                ItemId = table.Column<long>(type: "bigint", nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_EncounterItems", x => new { x.EncounterId, x.ItemId });
                table.ForeignKey(
                    name: "FK_EncounterItems_Encounters_EncounterId",
                    column: x => x.EncounterId,
                    principalTable: "Encounters",
                    principalColumn: "Id",
                    onDelete: ReferentialAction.Cascade);
                table.ForeignKey(
                    name: "FK_EncounterItems_Items_ItemId",
                    column: x => x.ItemId,
                    principalTable: "Items",
                    principalColumn: "Id",
                    onDelete: ReferentialAction.Cascade);
            });

        migrationBuilder.CreateIndex(
            name: "IX_EncounterItems_ItemId",
            table: "EncounterItems",
            column: "ItemId");
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropTable(
            name: "EncounterItems");
    }
}
