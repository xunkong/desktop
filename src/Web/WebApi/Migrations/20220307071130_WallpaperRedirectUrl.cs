using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Xunkong.Web.Api.Migrations
{
    public partial class WallpaperRedirectUrl : Migration
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

            migrationBuilder.AddColumn<string>(
                name: "Redirect",
                table: "Wallpapers",
                type: "longtext",
                nullable: true)
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateIndex(
                name: "IX_TravelRecord_AwardItems_Uid_Year_Month",
                table: "TravelRecord_AwardItems",
                columns: new[] { "Uid", "Year", "Month" });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_TravelRecord_AwardItems_Uid_Year_Month",
                table: "TravelRecord_AwardItems");

            migrationBuilder.DropColumn(
                name: "Redirect",
                table: "Wallpapers");

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
