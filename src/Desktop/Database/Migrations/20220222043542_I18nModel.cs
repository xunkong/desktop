using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Xunkong.Desktop.Migrations
{
    public partial class I18nModel : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<long>(
                name: "DescTextMapHash",
                table: "Info_Weapon",
                type: "INTEGER",
                nullable: true);

            migrationBuilder.AddColumn<long>(
                name: "DescTextMapHash",
                table: "Info_Character",
                type: "INTEGER",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "i18n",
                columns: table => new
                {
                    Id = table.Column<long>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    zh_cn = table.Column<string>(type: "TEXT", nullable: true),
                    zh_tw = table.Column<string>(type: "TEXT", nullable: true),
                    de_de = table.Column<string>(type: "TEXT", nullable: true),
                    en_us = table.Column<string>(type: "TEXT", nullable: true),
                    es_es = table.Column<string>(type: "TEXT", nullable: true),
                    fr_fr = table.Column<string>(type: "TEXT", nullable: true),
                    id_id = table.Column<string>(type: "TEXT", nullable: true),
                    ja_jp = table.Column<string>(type: "TEXT", nullable: true),
                    ko_kr = table.Column<string>(type: "TEXT", nullable: true),
                    pt_pt = table.Column<string>(type: "TEXT", nullable: true),
                    ru_ru = table.Column<string>(type: "TEXT", nullable: true),
                    th_th = table.Column<string>(type: "TEXT", nullable: true),
                    vi_vn = table.Column<string>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_i18n", x => x.Id);
                });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "i18n");

            migrationBuilder.DropColumn(
                name: "DescTextMapHash",
                table: "Info_Weapon");

            migrationBuilder.DropColumn(
                name: "DescTextMapHash",
                table: "Info_Character");
        }
    }
}
