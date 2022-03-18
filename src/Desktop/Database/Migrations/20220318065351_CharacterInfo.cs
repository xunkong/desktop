using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Xunkong.Desktop.Migrations
{
    public partial class CharacterInfo : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Info_Character_Element",
                table: "Info_Character");

            migrationBuilder.DropIndex(
                name: "IX_Info_Character_Gender",
                table: "Info_Character");

            migrationBuilder.DropIndex(
                name: "IX_Info_Character_Name",
                table: "Info_Character");

            migrationBuilder.DropIndex(
                name: "IX_Info_Character_NameTextMapHash",
                table: "Info_Character");

            migrationBuilder.DropIndex(
                name: "IX_Info_Character_Rarity",
                table: "Info_Character");

            migrationBuilder.DropIndex(
                name: "IX_Info_Character_WeaponType",
                table: "Info_Character");

            migrationBuilder.DropColumn(
                name: "AvatarIcon",
                table: "Info_Character");

            migrationBuilder.DropColumn(
                name: "DescTextMapHash",
                table: "Info_Character");

            migrationBuilder.DropColumn(
                name: "NameTextMapHash",
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
                type: "INTEGER",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<string>(
                name: "CvChinese",
                table: "Info_Character",
                type: "TEXT",
                maxLength: 255,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "CvEnglish",
                table: "Info_Character",
                type: "TEXT",
                maxLength: 255,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "CvJapanese",
                table: "Info_Character",
                type: "TEXT",
                maxLength: 255,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "CvKorean",
                table: "Info_Character",
                type: "TEXT",
                maxLength: 255,
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "Gender2",
                table: "Info_Character",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateTable(
                name: "Info_Character_Talent",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    TalentId = table.Column<int>(type: "INTEGER", nullable: false),
                    CharacterInfoId = table.Column<int>(type: "INTEGER", nullable: false),
                    Order = table.Column<int>(type: "INTEGER", nullable: false),
                    Name = table.Column<string>(type: "TEXT", nullable: true),
                    Description = table.Column<string>(type: "TEXT", nullable: true),
                    Icon = table.Column<string>(type: "TEXT", nullable: true),
                    CdTime = table.Column<float>(type: "REAL", nullable: false),
                    MaxChargeNumber = table.Column<int>(type: "INTEGER", nullable: false),
                    CostElementValue = table.Column<float>(type: "REAL", nullable: false)
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
                });

            migrationBuilder.CreateIndex(
                name: "IX_Info_Character_Talent_CharacterInfoId",
                table: "Info_Character_Talent",
                column: "CharacterInfoId");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Info_Character_Talent");

            migrationBuilder.DropColumn(
                name: "ConstellationId",
                table: "Info_Character_Constellation");

            migrationBuilder.DropColumn(
                name: "CvChinese",
                table: "Info_Character");

            migrationBuilder.DropColumn(
                name: "CvEnglish",
                table: "Info_Character");

            migrationBuilder.DropColumn(
                name: "CvJapanese",
                table: "Info_Character");

            migrationBuilder.DropColumn(
                name: "CvKorean",
                table: "Info_Character");

            migrationBuilder.DropColumn(
                name: "Gender2",
                table: "Info_Character");

            migrationBuilder.RenameColumn(
                name: "PreviewConstellationId",
                table: "Info_Character_Constellation",
                newName: "Position");

            migrationBuilder.RenameColumn(
                name: "Description",
                table: "Info_Character_Constellation",
                newName: "Effect");

            migrationBuilder.AddColumn<string>(
                name: "AvatarIcon",
                table: "Info_Character",
                type: "TEXT",
                nullable: true);

            migrationBuilder.AddColumn<long>(
                name: "DescTextMapHash",
                table: "Info_Character",
                type: "INTEGER",
                nullable: true);

            migrationBuilder.AddColumn<long>(
                name: "NameTextMapHash",
                table: "Info_Character",
                type: "INTEGER",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Info_Character_Element",
                table: "Info_Character",
                column: "Element");

            migrationBuilder.CreateIndex(
                name: "IX_Info_Character_Gender",
                table: "Info_Character",
                column: "Gender");

            migrationBuilder.CreateIndex(
                name: "IX_Info_Character_Name",
                table: "Info_Character",
                column: "Name");

            migrationBuilder.CreateIndex(
                name: "IX_Info_Character_NameTextMapHash",
                table: "Info_Character",
                column: "NameTextMapHash");

            migrationBuilder.CreateIndex(
                name: "IX_Info_Character_Rarity",
                table: "Info_Character",
                column: "Rarity");

            migrationBuilder.CreateIndex(
                name: "IX_Info_Character_WeaponType",
                table: "Info_Character",
                column: "WeaponType");
        }
    }
}
