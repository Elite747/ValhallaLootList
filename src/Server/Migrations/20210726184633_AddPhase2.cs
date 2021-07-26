using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace ValhallaLootList.Server.Migrations
{
    public partial class AddPhase2 : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.InsertData(
                table: "Brackets",
                columns: new[] { "Index", "Phase", "AllowOffspec", "AllowTypeDuplicates", "MaxItems", "MaxRank", "MinRank" },
                values: new object[,]
                {
                    { (byte)0, (byte)2, false, false, (byte)1, (byte)18, (byte)15 },
                    { (byte)1, (byte)2, false, false, (byte)1, (byte)14, (byte)11 },
                    { (byte)2, (byte)2, false, false, (byte)2, (byte)10, (byte)7 },
                    { (byte)3, (byte)2, true, true, (byte)2, (byte)6, (byte)1 }
                });

            migrationBuilder.InsertData(
                table: "PhaseDetails",
                columns: new[] { "Id", "StartsAt" },
                values: new object[] { (byte)2, new DateTimeOffset(new DateTime(2021, 8, 31, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, -4, 0, 0, 0)) });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
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
        }
    }
}
