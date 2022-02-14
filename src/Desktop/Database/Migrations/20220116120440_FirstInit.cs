using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Xunkong.Desktop.Migrations
{
    public partial class FirstInit : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "DailyNote_Items",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Uid = table.Column<int>(type: "INTEGER", nullable: false),
                    Time = table.Column<DateTimeOffset>(type: "TEXT", nullable: false),
                    CurrentResin = table.Column<int>(type: "INTEGER", nullable: false),
                    MaxResin = table.Column<int>(type: "INTEGER", nullable: false),
                    ResinRecoveryTime = table.Column<TimeSpan>(type: "TEXT", nullable: false),
                    FinishedTaskNumber = table.Column<int>(type: "INTEGER", nullable: false),
                    TotalTaskNumber = table.Column<int>(type: "INTEGER", nullable: false),
                    IsExtraTaskRewardReceived = table.Column<bool>(type: "INTEGER", nullable: false),
                    RemainResinDiscountNumber = table.Column<int>(type: "INTEGER", nullable: false),
                    ResinDiscountLimitedNumber = table.Column<int>(type: "INTEGER", nullable: false),
                    CurrentExpeditionNumber = table.Column<int>(type: "INTEGER", nullable: false),
                    MaxExpeditionNumber = table.Column<int>(type: "INTEGER", nullable: false),
                    CurrentHomeCoin = table.Column<int>(type: "INTEGER", nullable: false),
                    MaxHomeCoin = table.Column<int>(type: "INTEGER", nullable: false),
                    HomeCoinRecoveryTime = table.Column<TimeSpan>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DailyNote_Items", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Genshin_Users",
                columns: table => new
                {
                    Uid = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    GameBiz = table.Column<string>(type: "TEXT", nullable: true),
                    Region = table.Column<int>(type: "INTEGER", nullable: false),
                    Nickname = table.Column<string>(type: "TEXT", nullable: true),
                    Level = table.Column<int>(type: "INTEGER", nullable: false),
                    IsChosen = table.Column<bool>(type: "INTEGER", nullable: false),
                    RegionName = table.Column<string>(type: "TEXT", nullable: true),
                    IsOfficial = table.Column<bool>(type: "INTEGER", nullable: false),
                    Cookie = table.Column<string>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Genshin_Users", x => x.Uid);
                });

            migrationBuilder.CreateTable(
                name: "Hoyolab_Users",
                columns: table => new
                {
                    Uid = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Nickname = table.Column<string>(type: "TEXT", nullable: true),
                    Introduce = table.Column<string>(type: "TEXT", nullable: true),
                    Avatar = table.Column<string>(type: "TEXT", nullable: true),
                    Gender = table.Column<int>(type: "INTEGER", nullable: false),
                    AvatarUrl = table.Column<string>(type: "TEXT", nullable: true),
                    Pendant = table.Column<string>(type: "TEXT", nullable: true),
                    Cookie = table.Column<string>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Hoyolab_Users", x => x.Uid);
                });

            migrationBuilder.CreateTable(
                name: "Info_Character",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Name = table.Column<string>(type: "TEXT", maxLength: 255, nullable: true),
                    NameTextMapHash = table.Column<long>(type: "INTEGER", nullable: true),
                    Title = table.Column<string>(type: "TEXT", maxLength: 255, nullable: true),
                    Description = table.Column<string>(type: "TEXT", nullable: true),
                    Rarity = table.Column<int>(type: "INTEGER", nullable: false),
                    Gender = table.Column<string>(type: "TEXT", maxLength: 255, nullable: true),
                    Affiliation = table.Column<string>(type: "TEXT", maxLength: 255, nullable: true),
                    Element = table.Column<int>(type: "INTEGER", nullable: false),
                    WeaponType = table.Column<int>(type: "INTEGER", nullable: false),
                    Birthday = table.Column<string>(type: "TEXT", maxLength: 255, nullable: true),
                    Card = table.Column<string>(type: "TEXT", nullable: true),
                    Portrait = table.Column<string>(type: "TEXT", nullable: true),
                    FaceIcon = table.Column<string>(type: "TEXT", nullable: true),
                    SideIcon = table.Column<string>(type: "TEXT", nullable: true),
                    GachaCard = table.Column<string>(type: "TEXT", nullable: true),
                    GachaSplash = table.Column<string>(type: "TEXT", nullable: true),
                    AvatarIcon = table.Column<string>(type: "TEXT", nullable: true),
                    ConstllationName = table.Column<string>(type: "TEXT", maxLength: 255, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Info_Character", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Info_Weapon",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Name = table.Column<string>(type: "TEXT", maxLength: 255, nullable: true),
                    Description = table.Column<string>(type: "TEXT", nullable: true),
                    NameTextMapHash = table.Column<long>(type: "INTEGER", nullable: false),
                    Rarity = table.Column<int>(type: "INTEGER", nullable: false),
                    WeaponType = table.Column<int>(type: "INTEGER", nullable: false),
                    Icon = table.Column<string>(type: "TEXT", nullable: true),
                    AwakenIcon = table.Column<string>(type: "TEXT", nullable: true),
                    GachaIcon = table.Column<string>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Info_Weapon", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Info_WishEvent",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    WishType = table.Column<int>(type: "INTEGER", nullable: false),
                    Name = table.Column<string>(type: "TEXT", maxLength: 255, nullable: false),
                    Version = table.Column<string>(type: "TEXT", maxLength: 255, nullable: false),
                    StartTime = table.Column<string>(type: "TEXT", maxLength: 255, nullable: false),
                    EndTime = table.Column<string>(type: "TEXT", maxLength: 255, nullable: false),
                    Rank5UpItems = table.Column<string>(type: "TEXT", nullable: false),
                    Rank4UpItems = table.Column<string>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Info_WishEvent", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "SpiralAbyss_Infos",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Uid = table.Column<int>(type: "INTEGER", nullable: false),
                    ScheduleId = table.Column<int>(type: "INTEGER", nullable: false),
                    StartTime = table.Column<DateTimeOffset>(type: "TEXT", nullable: false),
                    EndTime = table.Column<DateTimeOffset>(type: "TEXT", nullable: false),
                    TotalBattleCount = table.Column<int>(type: "INTEGER", nullable: false),
                    TotalWinCount = table.Column<int>(type: "INTEGER", nullable: false),
                    MaxFloor = table.Column<string>(type: "TEXT", maxLength: 255, nullable: true),
                    TotalStar = table.Column<int>(type: "INTEGER", nullable: false),
                    IsUnlock = table.Column<bool>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SpiralAbyss_Infos", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "TravelRecord_AwardItems",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Uid = table.Column<int>(type: "INTEGER", nullable: false),
                    Year = table.Column<int>(type: "INTEGER", nullable: false),
                    Month = table.Column<int>(type: "INTEGER", nullable: false),
                    Type = table.Column<int>(type: "INTEGER", nullable: false),
                    ActionId = table.Column<int>(type: "INTEGER", nullable: false),
                    ActionName = table.Column<string>(type: "TEXT", maxLength: 255, nullable: true),
                    Time = table.Column<DateTime>(type: "TEXT", nullable: false),
                    Number = table.Column<int>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TravelRecord_AwardItems", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "TravelRecord_MonthDatas",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Uid = table.Column<int>(type: "INTEGER", nullable: false),
                    Year = table.Column<int>(type: "INTEGER", nullable: false),
                    Month = table.Column<int>(type: "INTEGER", nullable: false),
                    CurrentPrimogems = table.Column<int>(type: "INTEGER", nullable: false),
                    CurrentMora = table.Column<int>(type: "INTEGER", nullable: false),
                    LastPrimogems = table.Column<int>(type: "INTEGER", nullable: false),
                    LastMora = table.Column<int>(type: "INTEGER", nullable: false),
                    CurrentPrimogemsLevel = table.Column<int>(type: "INTEGER", nullable: false),
                    PrimogemsChangeRate = table.Column<int>(type: "INTEGER", nullable: false),
                    MoraChangeRate = table.Column<int>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TravelRecord_MonthDatas", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "UserSettings",
                columns: table => new
                {
                    Key = table.Column<string>(type: "TEXT", nullable: false),
                    Value = table.Column<string>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserSettings", x => x.Key);
                });

            migrationBuilder.CreateTable(
                name: "Wishlog_Authkeys",
                columns: table => new
                {
                    Uid = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Url = table.Column<string>(type: "TEXT", maxLength: 4096, nullable: true),
                    DateTime = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Wishlog_Authkeys", x => x.Uid);
                });

            migrationBuilder.CreateTable(
                name: "Wishlog_Items",
                columns: table => new
                {
                    Uid = table.Column<int>(type: "INTEGER", nullable: false),
                    Id = table.Column<long>(type: "INTEGER", nullable: false),
                    WishType = table.Column<int>(type: "INTEGER", nullable: false),
                    Time = table.Column<DateTimeOffset>(type: "TEXT", nullable: false),
                    Name = table.Column<string>(type: "TEXT", maxLength: 255, nullable: false),
                    Language = table.Column<string>(type: "TEXT", maxLength: 255, nullable: false),
                    ItemType = table.Column<string>(type: "TEXT", maxLength: 255, nullable: false),
                    RankType = table.Column<int>(type: "INTEGER", nullable: false),
                    QueryType = table.Column<int>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Wishlog_Items", x => new { x.Uid, x.Id });
                });

            migrationBuilder.CreateTable(
                name: "Info_Character_Constellation",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    CharacterInfoId = table.Column<int>(type: "INTEGER", nullable: false),
                    Name = table.Column<string>(type: "TEXT", maxLength: 255, nullable: true),
                    Effect = table.Column<string>(type: "TEXT", nullable: true),
                    Icon = table.Column<string>(type: "TEXT", nullable: true),
                    Position = table.Column<int>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Info_Character_Constellation", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Info_Character_Constellation_Info_Character_CharacterInfoId",
                        column: x => x.CharacterInfoId,
                        principalTable: "Info_Character",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "SpiralAbyss_Floors",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    SpiralAbyssInfoId = table.Column<int>(type: "INTEGER", nullable: false),
                    Index = table.Column<int>(type: "INTEGER", nullable: false),
                    Icon = table.Column<string>(type: "TEXT", nullable: false),
                    IsUnlock = table.Column<bool>(type: "INTEGER", nullable: false),
                    SettleTime = table.Column<string>(type: "TEXT", maxLength: 255, nullable: false),
                    Star = table.Column<int>(type: "INTEGER", nullable: false),
                    MaxStar = table.Column<int>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SpiralAbyss_Floors", x => x.Id);
                    table.ForeignKey(
                        name: "FK_SpiralAbyss_Floors_SpiralAbyss_Infos_SpiralAbyssInfoId",
                        column: x => x.SpiralAbyssInfoId,
                        principalTable: "SpiralAbyss_Infos",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "SpiralAbyss_Ranks",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Type = table.Column<int>(type: "INTEGER", nullable: false),
                    AvatarId = table.Column<int>(type: "INTEGER", nullable: false),
                    AvatarIcon = table.Column<string>(type: "TEXT", nullable: false),
                    Value = table.Column<int>(type: "INTEGER", nullable: false),
                    Rarity = table.Column<int>(type: "INTEGER", nullable: false),
                    SpiralAbyssInfo_DamageRank = table.Column<int>(type: "INTEGER", nullable: true),
                    SpiralAbyssInfo_DefeatRank = table.Column<int>(type: "INTEGER", nullable: true),
                    SpiralAbyssInfo_EnergySkillRank = table.Column<int>(type: "INTEGER", nullable: true),
                    SpiralAbyssInfo_NormalSkillRank = table.Column<int>(type: "INTEGER", nullable: true),
                    SpiralAbyssInfo_RevealRank = table.Column<int>(type: "INTEGER", nullable: true),
                    SpiralAbyssInfo_TakeDamageRank = table.Column<int>(type: "INTEGER", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SpiralAbyss_Ranks", x => x.Id);
                    table.ForeignKey(
                        name: "FK_SpiralAbyss_Ranks_SpiralAbyss_Infos_SpiralAbyssInfo_DamageRank",
                        column: x => x.SpiralAbyssInfo_DamageRank,
                        principalTable: "SpiralAbyss_Infos",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_SpiralAbyss_Ranks_SpiralAbyss_Infos_SpiralAbyssInfo_DefeatRank",
                        column: x => x.SpiralAbyssInfo_DefeatRank,
                        principalTable: "SpiralAbyss_Infos",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_SpiralAbyss_Ranks_SpiralAbyss_Infos_SpiralAbyssInfo_EnergySkillRank",
                        column: x => x.SpiralAbyssInfo_EnergySkillRank,
                        principalTable: "SpiralAbyss_Infos",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_SpiralAbyss_Ranks_SpiralAbyss_Infos_SpiralAbyssInfo_NormalSkillRank",
                        column: x => x.SpiralAbyssInfo_NormalSkillRank,
                        principalTable: "SpiralAbyss_Infos",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_SpiralAbyss_Ranks_SpiralAbyss_Infos_SpiralAbyssInfo_RevealRank",
                        column: x => x.SpiralAbyssInfo_RevealRank,
                        principalTable: "SpiralAbyss_Infos",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_SpiralAbyss_Ranks_SpiralAbyss_Infos_SpiralAbyssInfo_TakeDamageRank",
                        column: x => x.SpiralAbyssInfo_TakeDamageRank,
                        principalTable: "SpiralAbyss_Infos",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "TravelTecord_GroupStats",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    TravelRecordMonthDataId = table.Column<int>(type: "INTEGER", nullable: false),
                    Uid = table.Column<int>(type: "INTEGER", nullable: false),
                    Year = table.Column<int>(type: "INTEGER", nullable: false),
                    Month = table.Column<int>(type: "INTEGER", nullable: false),
                    ActionId = table.Column<int>(type: "INTEGER", nullable: false),
                    ActionName = table.Column<string>(type: "TEXT", maxLength: 255, nullable: true),
                    Number = table.Column<int>(type: "INTEGER", nullable: false),
                    Percent = table.Column<int>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TravelTecord_GroupStats", x => x.Id);
                    table.ForeignKey(
                        name: "FK_TravelTecord_GroupStats_TravelRecord_MonthDatas_TravelRecordMonthDataId",
                        column: x => x.TravelRecordMonthDataId,
                        principalTable: "TravelRecord_MonthDatas",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "SpiralAbyss_Levels",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    SpiralAbyssFloorId = table.Column<int>(type: "INTEGER", nullable: false),
                    Index = table.Column<int>(type: "INTEGER", nullable: false),
                    Star = table.Column<int>(type: "INTEGER", nullable: false),
                    MaxStar = table.Column<int>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SpiralAbyss_Levels", x => x.Id);
                    table.ForeignKey(
                        name: "FK_SpiralAbyss_Levels_SpiralAbyss_Floors_SpiralAbyssFloorId",
                        column: x => x.SpiralAbyssFloorId,
                        principalTable: "SpiralAbyss_Floors",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "SpiralAbyss_Battles",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    SpiralAbyssLevelId = table.Column<int>(type: "INTEGER", nullable: false),
                    Index = table.Column<int>(type: "INTEGER", nullable: false),
                    Time = table.Column<DateTimeOffset>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SpiralAbyss_Battles", x => x.Id);
                    table.ForeignKey(
                        name: "FK_SpiralAbyss_Battles_SpiralAbyss_Levels_SpiralAbyssLevelId",
                        column: x => x.SpiralAbyssLevelId,
                        principalTable: "SpiralAbyss_Levels",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "SpiralAbyss_Avatars",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    SpiralAbyssBattleId = table.Column<int>(type: "INTEGER", nullable: false),
                    AvatarId = table.Column<int>(type: "INTEGER", nullable: false),
                    Icon = table.Column<string>(type: "TEXT", nullable: false),
                    Level = table.Column<int>(type: "INTEGER", nullable: false),
                    Rarity = table.Column<int>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SpiralAbyss_Avatars", x => x.Id);
                    table.ForeignKey(
                        name: "FK_SpiralAbyss_Avatars_SpiralAbyss_Battles_SpiralAbyssBattleId",
                        column: x => x.SpiralAbyssBattleId,
                        principalTable: "SpiralAbyss_Battles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

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

            migrationBuilder.CreateIndex(
                name: "IX_Info_Character_Constellation_CharacterInfoId",
                table: "Info_Character_Constellation",
                column: "CharacterInfoId");

            migrationBuilder.CreateIndex(
                name: "IX_Info_WishEvent_WishType",
                table: "Info_WishEvent",
                column: "WishType");

            migrationBuilder.CreateIndex(
                name: "IX_Info_WishEvent_WishType_StartTime",
                table: "Info_WishEvent",
                columns: new[] { "WishType", "StartTime" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_SpiralAbyss_Avatars_AvatarId",
                table: "SpiralAbyss_Avatars",
                column: "AvatarId");

            migrationBuilder.CreateIndex(
                name: "IX_SpiralAbyss_Avatars_Level",
                table: "SpiralAbyss_Avatars",
                column: "Level");

            migrationBuilder.CreateIndex(
                name: "IX_SpiralAbyss_Avatars_Rarity",
                table: "SpiralAbyss_Avatars",
                column: "Rarity");

            migrationBuilder.CreateIndex(
                name: "IX_SpiralAbyss_Avatars_SpiralAbyssBattleId",
                table: "SpiralAbyss_Avatars",
                column: "SpiralAbyssBattleId");

            migrationBuilder.CreateIndex(
                name: "IX_SpiralAbyss_Battles_SpiralAbyssLevelId",
                table: "SpiralAbyss_Battles",
                column: "SpiralAbyssLevelId");

            migrationBuilder.CreateIndex(
                name: "IX_SpiralAbyss_Battles_Time",
                table: "SpiralAbyss_Battles",
                column: "Time");

            migrationBuilder.CreateIndex(
                name: "IX_SpiralAbyss_Floors_Index",
                table: "SpiralAbyss_Floors",
                column: "Index");

            migrationBuilder.CreateIndex(
                name: "IX_SpiralAbyss_Floors_SpiralAbyssInfoId",
                table: "SpiralAbyss_Floors",
                column: "SpiralAbyssInfoId");

            migrationBuilder.CreateIndex(
                name: "IX_SpiralAbyss_Floors_Star",
                table: "SpiralAbyss_Floors",
                column: "Star");

            migrationBuilder.CreateIndex(
                name: "IX_SpiralAbyss_Infos_MaxFloor",
                table: "SpiralAbyss_Infos",
                column: "MaxFloor");

            migrationBuilder.CreateIndex(
                name: "IX_SpiralAbyss_Infos_ScheduleId",
                table: "SpiralAbyss_Infos",
                column: "ScheduleId");

            migrationBuilder.CreateIndex(
                name: "IX_SpiralAbyss_Infos_TotalBattleCount",
                table: "SpiralAbyss_Infos",
                column: "TotalBattleCount");

            migrationBuilder.CreateIndex(
                name: "IX_SpiralAbyss_Infos_TotalStar",
                table: "SpiralAbyss_Infos",
                column: "TotalStar");

            migrationBuilder.CreateIndex(
                name: "IX_SpiralAbyss_Infos_TotalWinCount",
                table: "SpiralAbyss_Infos",
                column: "TotalWinCount");

            migrationBuilder.CreateIndex(
                name: "IX_SpiralAbyss_Infos_Uid_ScheduleId",
                table: "SpiralAbyss_Infos",
                columns: new[] { "Uid", "ScheduleId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_SpiralAbyss_Levels_SpiralAbyssFloorId",
                table: "SpiralAbyss_Levels",
                column: "SpiralAbyssFloorId");

            migrationBuilder.CreateIndex(
                name: "IX_SpiralAbyss_Ranks_SpiralAbyssInfo_DamageRank",
                table: "SpiralAbyss_Ranks",
                column: "SpiralAbyssInfo_DamageRank");

            migrationBuilder.CreateIndex(
                name: "IX_SpiralAbyss_Ranks_SpiralAbyssInfo_DefeatRank",
                table: "SpiralAbyss_Ranks",
                column: "SpiralAbyssInfo_DefeatRank");

            migrationBuilder.CreateIndex(
                name: "IX_SpiralAbyss_Ranks_SpiralAbyssInfo_EnergySkillRank",
                table: "SpiralAbyss_Ranks",
                column: "SpiralAbyssInfo_EnergySkillRank");

            migrationBuilder.CreateIndex(
                name: "IX_SpiralAbyss_Ranks_SpiralAbyssInfo_NormalSkillRank",
                table: "SpiralAbyss_Ranks",
                column: "SpiralAbyssInfo_NormalSkillRank");

            migrationBuilder.CreateIndex(
                name: "IX_SpiralAbyss_Ranks_SpiralAbyssInfo_RevealRank",
                table: "SpiralAbyss_Ranks",
                column: "SpiralAbyssInfo_RevealRank");

            migrationBuilder.CreateIndex(
                name: "IX_SpiralAbyss_Ranks_SpiralAbyssInfo_TakeDamageRank",
                table: "SpiralAbyss_Ranks",
                column: "SpiralAbyssInfo_TakeDamageRank");

            migrationBuilder.CreateIndex(
                name: "IX_SpiralAbyss_Ranks_Type",
                table: "SpiralAbyss_Ranks",
                column: "Type");

            migrationBuilder.CreateIndex(
                name: "IX_TravelRecord_AwardItems_ActionId",
                table: "TravelRecord_AwardItems",
                column: "ActionId");

            migrationBuilder.CreateIndex(
                name: "IX_TravelRecord_AwardItems_Month",
                table: "TravelRecord_AwardItems",
                column: "Month");

            migrationBuilder.CreateIndex(
                name: "IX_TravelRecord_AwardItems_Time",
                table: "TravelRecord_AwardItems",
                column: "Time");

            migrationBuilder.CreateIndex(
                name: "IX_TravelRecord_AwardItems_Type",
                table: "TravelRecord_AwardItems",
                column: "Type");

            migrationBuilder.CreateIndex(
                name: "IX_TravelRecord_AwardItems_Uid",
                table: "TravelRecord_AwardItems",
                column: "Uid");

            migrationBuilder.CreateIndex(
                name: "IX_TravelRecord_AwardItems_Year",
                table: "TravelRecord_AwardItems",
                column: "Year");

            migrationBuilder.CreateIndex(
                name: "IX_TravelRecord_MonthDatas_Uid_Year_Month",
                table: "TravelRecord_MonthDatas",
                columns: new[] { "Uid", "Year", "Month" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_TravelRecord_MonthDatas_Year_Month",
                table: "TravelRecord_MonthDatas",
                columns: new[] { "Year", "Month" });

            migrationBuilder.CreateIndex(
                name: "IX_TravelTecord_GroupStats_TravelRecordMonthDataId",
                table: "TravelTecord_GroupStats",
                column: "TravelRecordMonthDataId");

            migrationBuilder.CreateIndex(
                name: "IX_Wishlog_Items_ItemType",
                table: "Wishlog_Items",
                column: "ItemType");

            migrationBuilder.CreateIndex(
                name: "IX_Wishlog_Items_Name",
                table: "Wishlog_Items",
                column: "Name");

            migrationBuilder.CreateIndex(
                name: "IX_Wishlog_Items_QueryType",
                table: "Wishlog_Items",
                column: "QueryType");

            migrationBuilder.CreateIndex(
                name: "IX_Wishlog_Items_RankType",
                table: "Wishlog_Items",
                column: "RankType");

            migrationBuilder.CreateIndex(
                name: "IX_Wishlog_Items_Time",
                table: "Wishlog_Items",
                column: "Time");

            migrationBuilder.CreateIndex(
                name: "IX_Wishlog_Items_WishType",
                table: "Wishlog_Items",
                column: "WishType");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "DailyNote_Items");

            migrationBuilder.DropTable(
                name: "Genshin_Users");

            migrationBuilder.DropTable(
                name: "Hoyolab_Users");

            migrationBuilder.DropTable(
                name: "Info_Character_Constellation");

            migrationBuilder.DropTable(
                name: "Info_Weapon");

            migrationBuilder.DropTable(
                name: "Info_WishEvent");

            migrationBuilder.DropTable(
                name: "SpiralAbyss_Avatars");

            migrationBuilder.DropTable(
                name: "SpiralAbyss_Ranks");

            migrationBuilder.DropTable(
                name: "TravelRecord_AwardItems");

            migrationBuilder.DropTable(
                name: "TravelTecord_GroupStats");

            migrationBuilder.DropTable(
                name: "UserSettings");

            migrationBuilder.DropTable(
                name: "Wishlog_Authkeys");

            migrationBuilder.DropTable(
                name: "Wishlog_Items");

            migrationBuilder.DropTable(
                name: "Info_Character");

            migrationBuilder.DropTable(
                name: "SpiralAbyss_Battles");

            migrationBuilder.DropTable(
                name: "TravelRecord_MonthDatas");

            migrationBuilder.DropTable(
                name: "SpiralAbyss_Levels");

            migrationBuilder.DropTable(
                name: "SpiralAbyss_Floors");

            migrationBuilder.DropTable(
                name: "SpiralAbyss_Infos");
        }
    }
}
