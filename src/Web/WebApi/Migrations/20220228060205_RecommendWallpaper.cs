using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Xunkong.Web.Api.Migrations
{
    public partial class RecommendWallpaper : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "Recommend",
                table: "Wallpapers",
                type: "tinyint(1)",
                nullable: false,
                defaultValue: false);

            migrationBuilder.CreateIndex(
                name: "IX_Wallpapers_Enable",
                table: "Wallpapers",
                column: "Enable");

            migrationBuilder.CreateIndex(
                name: "IX_Wallpapers_Recommend",
                table: "Wallpapers",
                column: "Recommend");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Wallpapers_Enable",
                table: "Wallpapers");

            migrationBuilder.DropIndex(
                name: "IX_Wallpapers_Recommend",
                table: "Wallpapers");

            migrationBuilder.DropColumn(
                name: "Recommend",
                table: "Wallpapers");
        }
    }
}
