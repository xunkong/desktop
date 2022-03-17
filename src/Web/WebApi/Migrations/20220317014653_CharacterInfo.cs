using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Xunkong.Web.Api.Migrations
{
    public partial class CharacterInfo : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Info_Character_Gender",
                table: "Info_Character");

            migrationBuilder.DropIndex(
                name: "IX_Info_Character_NameTextMapHash",
                table: "Info_Character");

            migrationBuilder.DropColumn(
                name: "AvatarIcon",
                table: "Info_Character");

            migrationBuilder.RenameColumn(
                name: "Position",
                table: "Info_Character_Constellation",
                newName: "PreviewConstellationId");

            migrationBuilder.RenameColumn(
                name: "Effect",
                table: "Info_Character_Constellation",
                newName: "Description");

            migrationBuilder.AddColumn<int>(
                name: "ConstellationId",
                table: "Info_Character_Constellation",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<long>(
                name: "DescTextMapHash",
                table: "Info_Character_Constellation",
                type: "bigint",
                nullable: false,
                defaultValue: 0L);

            migrationBuilder.AddColumn<long>(
                name: "NameTextMapHash",
                table: "Info_Character_Constellation",
                type: "bigint",
                nullable: false,
                defaultValue: 0L);

            migrationBuilder.AlterColumn<long>(
                name: "NameTextMapHash",
                table: "Info_Character",
                type: "bigint",
                nullable: false,
                defaultValue: 0L,
                oldClrType: typeof(long),
                oldType: "bigint",
                oldNullable: true);

            migrationBuilder.AlterColumn<long>(
                name: "DescTextMapHash",
                table: "Info_Character",
                type: "bigint",
                nullable: false,
                defaultValue: 0L,
                oldClrType: typeof(long),
                oldType: "bigint",
                oldNullable: true);

            migrationBuilder.AddColumn<long>(
                name: "AffiliationTextMapHash",
                table: "Info_Character",
                type: "bigint",
                nullable: false,
                defaultValue: 0L);

            migrationBuilder.AddColumn<long>(
                name: "ConstllationTextMapHash",
                table: "Info_Character",
                type: "bigint",
                nullable: false,
                defaultValue: 0L);

            migrationBuilder.AddColumn<string>(
                name: "CvChinese",
                table: "Info_Character",
                type: "varchar(255)",
                maxLength: 255,
                nullable: true)
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<long>(
                name: "CvChineseTextMapHash",
                table: "Info_Character",
                type: "bigint",
                nullable: false,
                defaultValue: 0L);

            migrationBuilder.AddColumn<string>(
                name: "CvEnglish",
                table: "Info_Character",
                type: "varchar(255)",
                maxLength: 255,
                nullable: true)
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<long>(
                name: "CvEnglishTextMapHash",
                table: "Info_Character",
                type: "bigint",
                nullable: false,
                defaultValue: 0L);

            migrationBuilder.AddColumn<string>(
                name: "CvJapanese",
                table: "Info_Character",
                type: "varchar(255)",
                maxLength: 255,
                nullable: true)
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<long>(
                name: "CvJapaneseTextMapHash",
                table: "Info_Character",
                type: "bigint",
                nullable: false,
                defaultValue: 0L);

            migrationBuilder.AddColumn<string>(
                name: "CvKorean",
                table: "Info_Character",
                type: "varchar(255)",
                maxLength: 255,
                nullable: true)
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<long>(
                name: "CvKoreanTextMapHash",
                table: "Info_Character",
                type: "bigint",
                nullable: false,
                defaultValue: 0L);

            migrationBuilder.AddColumn<int>(
                name: "Gender2",
                table: "Info_Character",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<long>(
                name: "TitleTextMapHash",
                table: "Info_Character",
                type: "bigint",
                nullable: false,
                defaultValue: 0L);

            migrationBuilder.CreateTable(
                name: "Info_Character_Talent",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    TalentId = table.Column<int>(type: "int", nullable: false),
                    CharacterInfoId = table.Column<int>(type: "int", nullable: false),
                    Order = table.Column<int>(type: "int", nullable: false),
                    Name = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Description = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Icon = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    CdTime = table.Column<float>(type: "float", nullable: false),
                    MaxChargeNumber = table.Column<int>(type: "int", nullable: false),
                    CostElementValue = table.Column<float>(type: "float", nullable: false),
                    NameTextMapHash = table.Column<long>(type: "bigint", nullable: false),
                    DescTextMapHash = table.Column<long>(type: "bigint", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Info_Character_Talent", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Info_Character_Talent_Info_Character_CharacterInfoId",
                        column: x => x.CharacterInfoId,
                        principalTable: "Info_Character",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateIndex(
                name: "IX_Info_Character_Gender2",
                table: "Info_Character",
                column: "Gender2");

            migrationBuilder.CreateIndex(
                name: "IX_Info_Character_Talent_CharacterInfoId",
                table: "Info_Character_Talent",
                column: "CharacterInfoId");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Info_Character_Talent");

            migrationBuilder.DropIndex(
                name: "IX_Info_Character_Gender2",
                table: "Info_Character");

            migrationBuilder.DropColumn(
                name: "ConstellationId",
                table: "Info_Character_Constellation");

            migrationBuilder.DropColumn(
                name: "DescTextMapHash",
                table: "Info_Character_Constellation");

            migrationBuilder.DropColumn(
                name: "NameTextMapHash",
                table: "Info_Character_Constellation");

            migrationBuilder.DropColumn(
                name: "AffiliationTextMapHash",
                table: "Info_Character");

            migrationBuilder.DropColumn(
                name: "ConstllationTextMapHash",
                table: "Info_Character");

            migrationBuilder.DropColumn(
                name: "CvChinese",
                table: "Info_Character");

            migrationBuilder.DropColumn(
                name: "CvChineseTextMapHash",
                table: "Info_Character");

            migrationBuilder.DropColumn(
                name: "CvEnglish",
                table: "Info_Character");

            migrationBuilder.DropColumn(
                name: "CvEnglishTextMapHash",
                table: "Info_Character");

            migrationBuilder.DropColumn(
                name: "CvJapanese",
                table: "Info_Character");

            migrationBuilder.DropColumn(
                name: "CvJapaneseTextMapHash",
                table: "Info_Character");

            migrationBuilder.DropColumn(
                name: "CvKorean",
                table: "Info_Character");

            migrationBuilder.DropColumn(
                name: "CvKoreanTextMapHash",
                table: "Info_Character");

            migrationBuilder.DropColumn(
                name: "Gender2",
                table: "Info_Character");

            migrationBuilder.DropColumn(
                name: "TitleTextMapHash",
                table: "Info_Character");

            migrationBuilder.RenameColumn(
                name: "PreviewConstellationId",
                table: "Info_Character_Constellation",
                newName: "Position");

            migrationBuilder.RenameColumn(
                name: "Description",
                table: "Info_Character_Constellation",
                newName: "Effect");

            migrationBuilder.AlterColumn<long>(
                name: "NameTextMapHash",
                table: "Info_Character",
                type: "bigint",
                nullable: true,
                oldClrType: typeof(long),
                oldType: "bigint");

            migrationBuilder.AlterColumn<long>(
                name: "DescTextMapHash",
                table: "Info_Character",
                type: "bigint",
                nullable: true,
                oldClrType: typeof(long),
                oldType: "bigint");

            migrationBuilder.AddColumn<string>(
                name: "AvatarIcon",
                table: "Info_Character",
                type: "longtext",
                nullable: true)
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateIndex(
                name: "IX_Info_Character_Gender",
                table: "Info_Character",
                column: "Gender");

            migrationBuilder.CreateIndex(
                name: "IX_Info_Character_NameTextMapHash",
                table: "Info_Character",
                column: "NameTextMapHash");
        }
    }
}
