using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Xunkong.Web.Api.Migrations
{
    public partial class AddEnableToItemInfo : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "Enable",
                table: "Info_Weapon",
                type: "tinyint(1)",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "Enable",
                table: "Info_Character",
                type: "tinyint(1)",
                nullable: false,
                defaultValue: false);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Enable",
                table: "Info_Weapon");

            migrationBuilder.DropColumn(
                name: "Enable",
                table: "Info_Character");
        }
    }
}
