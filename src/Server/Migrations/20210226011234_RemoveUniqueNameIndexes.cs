﻿//<auto-generated>
using Microsoft.EntityFrameworkCore.Migrations;

namespace ValhallaLootList.Server.Migrations
{
    public partial class RemoveUniqueNameIndexes : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Instances_Name",
                table: "Instances");

            migrationBuilder.DropIndex(
                name: "IX_Encounters_Name",
                table: "Encounters");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateIndex(
                name: "IX_Instances_Name",
                table: "Instances",
                column: "Name",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Encounters_Name",
                table: "Encounters",
                column: "Name",
                unique: true);
        }
    }
}