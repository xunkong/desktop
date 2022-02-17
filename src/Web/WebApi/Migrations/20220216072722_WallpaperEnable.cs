using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Xunkong.Web.Api.Migrations
{
    public partial class WallpaperEnable : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "Enable",
                table: "Wallpapers",
                type: "tinyint(1)",
                nullable: false,
                defaultValue: false);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Enable",
                table: "Wallpapers");
        }
    }
}
