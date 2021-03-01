﻿//<auto-generated>
using Microsoft.EntityFrameworkCore.Migrations;

namespace ValhallaLootList.Server.Migrations
{
    public partial class AddEncounterIndex : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Items_Encounters_EncounterId",
                table: "Items");

            migrationBuilder.AlterColumn<string>(
                name: "EncounterId",
                table: "Items",
                type: "varchar(255) CHARACTER SET utf8mb4",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "varchar(255) CHARACTER SET utf8mb4");

            migrationBuilder.AddColumn<sbyte>(
                name: "Index",
                table: "Encounters",
                type: "tinyint",
                nullable: false,
                defaultValue: (sbyte)0);

            migrationBuilder.AddForeignKey(
                name: "FK_Items_Encounters_EncounterId",
                table: "Items",
                column: "EncounterId",
                principalTable: "Encounters",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Items_Encounters_EncounterId",
                table: "Items");

            migrationBuilder.DropColumn(
                name: "Index",
                table: "Encounters");

            migrationBuilder.AlterColumn<string>(
                name: "EncounterId",
                table: "Items",
                type: "varchar(255) CHARACTER SET utf8mb4",
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "varchar(255) CHARACTER SET utf8mb4",
                oldNullable: true);

            migrationBuilder.AddForeignKey(
                name: "FK_Items_Encounters_EncounterId",
                table: "Items",
                column: "EncounterId",
                principalTable: "Encounters",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}