using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Xunkong.Desktop.Migrations
{
    public partial class UserAccount : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "GenshinUserAccounts",
                columns: table => new
                {
                    UserName = table.Column<string>(type: "TEXT", nullable: false),
                    IsOversea = table.Column<bool>(type: "INTEGER", nullable: false),
                    ADLPROD = table.Column<byte[]>(type: "BLOB", nullable: false),
                    CreateTime = table.Column<DateTimeOffset>(type: "TEXT", nullable: false),
                    LastAccessTime = table.Column<DateTimeOffset>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_GenshinUserAccounts", x => x.UserName);
                });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "GenshinUserAccounts");
        }
    }
}
