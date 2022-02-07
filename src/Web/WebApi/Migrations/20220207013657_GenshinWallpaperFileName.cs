using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Xunkong.Web.Api.Migrations
{
    public partial class GenshinWallpaperFileName : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "FileName",
                table: "Wallpapers",
                type: "varchar(255)",
                maxLength: 255,
                nullable: true)
                .Annotation("MySql:CharSet", "utf8mb4");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "FileName",
                table: "Wallpapers");
        }
    }
}
