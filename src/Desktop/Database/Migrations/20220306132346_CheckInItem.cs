using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Xunkong.Desktop.Migrations
{
    public partial class CheckInItem : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_TravelRecord_AwardItems_Month",
                table: "TravelRecord_AwardItems");

            migrationBuilder.DropIndex(
                name: "IX_TravelRecord_AwardItems_Uid",
                table: "TravelRecord_AwardItems");

            migrationBuilder.DropIndex(
                name: "IX_TravelRecord_AwardItems_Year",
                table: "TravelRecord_AwardItems");

            migrationBuilder.CreateTable(
                name: "DailyCheckInItems",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Uid = table.Column<int>(type: "INTEGER", nullable: false),
                    Date = table.Column<string>(type: "TEXT", nullable: false),
                    Time = table.Column<DateTimeOffset>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DailyCheckInItems", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_TravelRecord_AwardItems_Uid_Year_Month",
                table: "TravelRecord_AwardItems",
                columns: new[] { "Uid", "Year", "Month" });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "DailyCheckInItems");

            migrationBuilder.DropIndex(
                name: "IX_TravelRecord_AwardItems_Uid_Year_Month",
                table: "TravelRecord_AwardItems");

            migrationBuilder.CreateIndex(
                name: "IX_TravelRecord_AwardItems_Month",
                table: "TravelRecord_AwardItems",
                column: "Month");

            migrationBuilder.CreateIndex(
                name: "IX_TravelRecord_AwardItems_Uid",
                table: "TravelRecord_AwardItems",
                column: "Uid");

            migrationBuilder.CreateIndex(
                name: "IX_TravelRecord_AwardItems_Year",
                table: "TravelRecord_AwardItems",
                column: "Year");
        }
    }
}
