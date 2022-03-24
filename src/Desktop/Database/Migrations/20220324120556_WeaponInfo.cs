using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Xunkong.Desktop.Migrations
{
    public partial class WeaponInfo : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "DescTextMapHash",
                table: "Info_Weapon");

            migrationBuilder.DropColumn(
                name: "NameTextMapHash",
                table: "Info_Weapon");

            migrationBuilder.AddColumn<string>(
                name: "Story",
                table: "Info_Weapon",
                type: "TEXT",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "Info_Weapon_Skill",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    WeaponInfoId = table.Column<int>(type: "INTEGER", nullable: false),
                    Level = table.Column<int>(type: "INTEGER", nullable: false),
                    Name = table.Column<string>(type: "TEXT", nullable: true),
                    Description = table.Column<string>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Info_Weapon_Skill", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Info_Weapon_Skill_Info_Weapon_WeaponInfoId",
                        column: x => x.WeaponInfoId,
                        principalTable: "Info_Weapon",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Info_Weapon_Skill_WeaponInfoId",
                table: "Info_Weapon_Skill",
                column: "WeaponInfoId");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Info_Weapon_Skill");

            migrationBuilder.DropColumn(
                name: "Story",
                table: "Info_Weapon");

            migrationBuilder.AddColumn<long>(
                name: "DescTextMapHash",
                table: "Info_Weapon",
                type: "INTEGER",
                nullable: true);

            migrationBuilder.AddColumn<long>(
                name: "NameTextMapHash",
                table: "Info_Weapon",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0L);
        }
    }
}
