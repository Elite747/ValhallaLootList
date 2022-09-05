﻿// Copyright (C) 2021 Donovan Sullivan
// GNU General Public License v3.0+ (see LICENSE or https://www.gnu.org/licenses/gpl-3.0.txt)
// <auto-generated />

using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ValhallaLootList.Server.Migrations
{
    public partial class RemoveDebugArtifacts : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Items_Encounters_EncounterId",
                table: "Items");

            migrationBuilder.DropIndex(
                name: "IX_Items_EncounterId",
                table: "Items");

            migrationBuilder.DeleteData(
                table: "Brackets",
                keyColumns: new[] { "Index", "Phase" },
                keyValues: new object[] { (byte)0, (byte)2 });

            migrationBuilder.DeleteData(
                table: "Brackets",
                keyColumns: new[] { "Index", "Phase" },
                keyValues: new object[] { (byte)1, (byte)2 });

            migrationBuilder.DeleteData(
                table: "Brackets",
                keyColumns: new[] { "Index", "Phase" },
                keyValues: new object[] { (byte)2, (byte)2 });

            migrationBuilder.DeleteData(
                table: "Brackets",
                keyColumns: new[] { "Index", "Phase" },
                keyValues: new object[] { (byte)3, (byte)2 });

            migrationBuilder.DeleteData(
                table: "PhaseDetails",
                keyColumn: "Id",
                keyValue: (byte)2);

            migrationBuilder.DropColumn(
                name: "EncounterId",
                table: "Items");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "EncounterId",
                table: "Items",
                type: "nvarchar(20)",
                nullable: true);

            migrationBuilder.InsertData(
                table: "Brackets",
                columns: new[] { "Index", "Phase", "AllowOffspec", "AllowTypeDuplicates", "HeroicItems", "MaxRank", "MinRank", "NormalItems" },
                values: new object[,]
                {
                    { (byte)0, (byte)2, false, false, (byte)1, (byte)24, (byte)21, (byte)1 },
                    { (byte)1, (byte)2, false, false, (byte)1, (byte)20, (byte)17, (byte)1 },
                    { (byte)2, (byte)2, false, false, (byte)1, (byte)16, (byte)13, (byte)1 },
                    { (byte)3, (byte)2, true, true, (byte)1, (byte)12, (byte)1, (byte)1 }
                });

            migrationBuilder.InsertData(
                table: "PhaseDetails",
                columns: new[] { "Id", "StartsAt" },
                values: new object[] { (byte)2, new DateTimeOffset(new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)) });

            migrationBuilder.CreateIndex(
                name: "IX_Items_EncounterId",
                table: "Items",
                column: "EncounterId");

            migrationBuilder.AddForeignKey(
                name: "FK_Items_Encounters_EncounterId",
                table: "Items",
                column: "EncounterId",
                principalTable: "Encounters",
                principalColumn: "Id");
        }
    }
}