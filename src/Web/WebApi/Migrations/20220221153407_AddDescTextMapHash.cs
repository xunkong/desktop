using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Xunkong.Web.Api.Migrations
{
    public partial class AddDescTextMapHash : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<long>(
                name: "DescTextMapHash",
                table: "Info_Weapon",
                type: "bigint",
                nullable: true);

            migrationBuilder.AddColumn<long>(
                name: "DescTextMapHash",
                table: "Info_Character",
                type: "bigint",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "DescTextMapHash",
                table: "Info_Weapon");

            migrationBuilder.DropColumn(
                name: "DescTextMapHash",
                table: "Info_Character");
        }
    }
}
