using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace ValhallaLootList.Server.Migrations
{
    public partial class AddPrioScopes : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "PriorityScopes",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false),
                    StartsAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    ObservedAttendances = table.Column<int>(type: "int", nullable: false),
                    AttendancesPerPoint = table.Column<int>(type: "int", nullable: false),
                    FullTrialPenalty = table.Column<int>(type: "int", nullable: false),
                    HalfTrialPenalty = table.Column<int>(type: "int", nullable: false),
                    RequiredDonationCopper = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PriorityScopes", x => x.Id);
                });

            migrationBuilder.InsertData(
                table: "PriorityScopes",
                columns: new[] { "Id", "AttendancesPerPoint", "FullTrialPenalty", "HalfTrialPenalty", "ObservedAttendances", "RequiredDonationCopper", "StartsAt" },
                values: new object[] { 1L, 4, -18, -9, 8, 500000, new DateTimeOffset(new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)) });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "PriorityScopes");
        }
    }
}
