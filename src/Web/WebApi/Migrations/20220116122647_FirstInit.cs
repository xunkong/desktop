using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Xunkong.Web.Api.Migrations
{
    public partial class FirstInit : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterDatabase()
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "DailyNote_Items",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    Uid = table.Column<int>(type: "int", nullable: false),
                    Time = table.Column<DateTimeOffset>(type: "datetime(6)", nullable: false),
                    CurrentResin = table.Column<int>(type: "int", nullable: false),
                    MaxResin = table.Column<int>(type: "int", nullable: false),
                    ResinRecoveryTime = table.Column<TimeSpan>(type: "time(6)", nullable: false),
                    FinishedTaskNumber = table.Column<int>(type: "int", nullable: false),
                    TotalTaskNumber = table.Column<int>(type: "int", nullable: false),
                    IsExtraTaskRewardReceived = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    RemainResinDiscountNumber = table.Column<int>(type: "int", nullable: false),
                    ResinDiscountLimitedNumber = table.Column<int>(type: "int", nullable: false),
                    CurrentExpeditionNumber = table.Column<int>(type: "int", nullable: false),
                    MaxExpeditionNumber = table.Column<int>(type: "int", nullable: false),
                    CurrentHomeCoin = table.Column<int>(type: "int", nullable: false),
                    MaxHomeCoin = table.Column<int>(type: "int", nullable: false),
                    HomeCoinRecoveryTime = table.Column<TimeSpan>(type: "time(6)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DailyNote_Items", x => x.Id);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "i18n",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    zh_cn = table.Column<string>(type: "varchar(255)", maxLength: 255, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    zh_tw = table.Column<string>(type: "varchar(255)", maxLength: 255, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    de_de = table.Column<string>(type: "varchar(255)", maxLength: 255, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    en_us = table.Column<string>(type: "varchar(255)", maxLength: 255, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    es_es = table.Column<string>(type: "varchar(255)", maxLength: 255, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    fr_fr = table.Column<string>(type: "varchar(255)", maxLength: 255, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    id_id = table.Column<string>(type: "varchar(255)", maxLength: 255, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    ja_jp = table.Column<string>(type: "varchar(255)", maxLength: 255, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    ko_kr = table.Column<string>(type: "varchar(255)", maxLength: 255, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    pt_pt = table.Column<string>(type: "varchar(255)", maxLength: 255, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    ru_ru = table.Column<string>(type: "varchar(255)", maxLength: 255, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    th_th = table.Column<string>(type: "varchar(255)", maxLength: 255, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    vi_vn = table.Column<string>(type: "varchar(255)", maxLength: 255, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_i18n", x => x.Id);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "Info_Character",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    Name = table.Column<string>(type: "varchar(255)", maxLength: 255, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    NameTextMapHash = table.Column<long>(type: "bigint", nullable: true),
                    Title = table.Column<string>(type: "varchar(255)", maxLength: 255, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Description = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Rarity = table.Column<int>(type: "int", nullable: false),
                    Gender = table.Column<string>(type: "varchar(255)", maxLength: 255, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Affiliation = table.Column<string>(type: "varchar(255)", maxLength: 255, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Element = table.Column<int>(type: "int", nullable: false),
                    WeaponType = table.Column<int>(type: "int", nullable: false),
                    Birthday = table.Column<string>(type: "varchar(255)", maxLength: 255, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Card = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Portrait = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    FaceIcon = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    SideIcon = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    GachaCard = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    GachaSplash = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    AvatarIcon = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    ConstllationName = table.Column<string>(type: "varchar(255)", maxLength: 255, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Info_Character", x => x.Id);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "Info_Weapon",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    Name = table.Column<string>(type: "varchar(255)", maxLength: 255, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Description = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    NameTextMapHash = table.Column<long>(type: "bigint", nullable: false),
                    Rarity = table.Column<int>(type: "int", nullable: false),
                    WeaponType = table.Column<int>(type: "int", nullable: false),
                    Icon = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    AwakenIcon = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    GachaIcon = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Info_Weapon", x => x.Id);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "Info_WishEvent",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    WishType = table.Column<int>(type: "int", nullable: false),
                    Name = table.Column<string>(type: "varchar(255)", maxLength: 255, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Version = table.Column<string>(type: "varchar(255)", maxLength: 255, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    StartTime = table.Column<string>(type: "varchar(255)", maxLength: 255, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    EndTime = table.Column<string>(type: "varchar(255)", maxLength: 255, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Rank5UpItems = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Rank4UpItems = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Info_WishEvent", x => x.Id);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "Record_All",
                columns: table => new
                {
                    RequestId = table.Column<string>(type: "varchar(255)", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    DateTime = table.Column<DateTimeOffset>(type: "datetime(6)", nullable: false),
                    Path = table.Column<string>(type: "varchar(255)", maxLength: 255, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Method = table.Column<string>(type: "varchar(255)", maxLength: 255, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    StatusCode = table.Column<int>(type: "int", nullable: false),
                    ReturnCode = table.Column<int>(type: "int", nullable: true),
                    Message = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    DeviceId = table.Column<string>(type: "varchar(255)", maxLength: 255, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    UserAgent = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Ip = table.Column<string>(type: "varchar(255)", maxLength: 255, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Record_All", x => x.RequestId);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "SpiralAbyss_Infos",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    Uid = table.Column<int>(type: "int", nullable: false),
                    ScheduleId = table.Column<int>(type: "int", nullable: false),
                    StartTime = table.Column<DateTimeOffset>(type: "datetime(6)", nullable: false),
                    EndTime = table.Column<DateTimeOffset>(type: "datetime(6)", nullable: false),
                    TotalBattleCount = table.Column<int>(type: "int", nullable: false),
                    TotalWinCount = table.Column<int>(type: "int", nullable: false),
                    MaxFloor = table.Column<string>(type: "varchar(255)", maxLength: 255, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    TotalStar = table.Column<int>(type: "int", nullable: false),
                    IsUnlock = table.Column<bool>(type: "tinyint(1)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SpiralAbyss_Infos", x => x.Id);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "TravelRecord_AwardItems",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    Uid = table.Column<int>(type: "int", nullable: false),
                    Year = table.Column<int>(type: "int", nullable: false),
                    Month = table.Column<int>(type: "int", nullable: false),
                    Type = table.Column<int>(type: "int", nullable: false),
                    ActionId = table.Column<int>(type: "int", nullable: false),
                    ActionName = table.Column<string>(type: "varchar(255)", maxLength: 255, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Time = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    Number = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TravelRecord_AwardItems", x => x.Id);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "TravelRecord_MonthDatas",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    Uid = table.Column<int>(type: "int", nullable: false),
                    Year = table.Column<int>(type: "int", nullable: false),
                    Month = table.Column<int>(type: "int", nullable: false),
                    CurrentPrimogems = table.Column<int>(type: "int", nullable: false),
                    CurrentMora = table.Column<int>(type: "int", nullable: false),
                    LastPrimogems = table.Column<int>(type: "int", nullable: false),
                    LastMora = table.Column<int>(type: "int", nullable: false),
                    CurrentPrimogemsLevel = table.Column<int>(type: "int", nullable: false),
                    PrimogemsChangeRate = table.Column<int>(type: "int", nullable: false),
                    MoraChangeRate = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TravelRecord_MonthDatas", x => x.Id);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "Wishlog_Authkeys",
                columns: table => new
                {
                    Url = table.Column<string>(type: "varchar(4096)", maxLength: 4096, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Uid = table.Column<int>(type: "int", nullable: false),
                    DateTime = table.Column<DateTime>(type: "datetime(6)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Wishlog_Authkeys", x => x.Url);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "Wishlog_Items",
                columns: table => new
                {
                    Uid = table.Column<int>(type: "int", nullable: false),
                    Id = table.Column<long>(type: "bigint", nullable: false),
                    WishType = table.Column<int>(type: "int", nullable: false),
                    Time = table.Column<DateTimeOffset>(type: "datetime(6)", nullable: false),
                    Name = table.Column<string>(type: "varchar(255)", maxLength: 255, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Language = table.Column<string>(type: "varchar(255)", maxLength: 255, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    ItemType = table.Column<string>(type: "varchar(255)", maxLength: 255, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    RankType = table.Column<int>(type: "int", nullable: false),
                    QueryType = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Wishlog_Items", x => new { x.Uid, x.Id });
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "Info_Character_Constellation",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    CharacterInfoId = table.Column<int>(type: "int", nullable: false),
                    Name = table.Column<string>(type: "varchar(255)", maxLength: 255, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Effect = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Icon = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Position = table.Column<int>(type: "int", nullable: false)
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
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "Record_Wishlog",
                columns: table => new
                {
                    RequestId = table.Column<string>(type: "varchar(255)", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Uid = table.Column<int>(type: "int", nullable: false),
                    Operation = table.Column<string>(type: "varchar(255)", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    CurrentCount = table.Column<int>(type: "int", nullable: false),
                    GetCount = table.Column<int>(type: "int", nullable: false),
                    PutCount = table.Column<int>(type: "int", nullable: false),
                    DeleteCount = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Record_Wishlog", x => x.RequestId);
                    table.ForeignKey(
                        name: "FK_Record_Wishlog_Record_All_RequestId",
                        column: x => x.RequestId,
                        principalTable: "Record_All",
                        principalColumn: "RequestId",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "SpiralAbyss_Floors",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    SpiralAbyssInfoId = table.Column<int>(type: "int", nullable: false),
                    Index = table.Column<int>(type: "int", nullable: false),
                    Icon = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    IsUnlock = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    SettleTime = table.Column<string>(type: "varchar(255)", maxLength: 255, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Star = table.Column<int>(type: "int", nullable: false),
                    MaxStar = table.Column<int>(type: "int", nullable: false)
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
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "SpiralAbyss_Ranks",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    Type = table.Column<int>(type: "int", nullable: false),
                    AvatarId = table.Column<int>(type: "int", nullable: false),
                    AvatarIcon = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Value = table.Column<int>(type: "int", nullable: false),
                    Rarity = table.Column<int>(type: "int", nullable: false),
                    SpiralAbyssInfo_DamageRank = table.Column<int>(type: "int", nullable: true),
                    SpiralAbyssInfo_DefeatRank = table.Column<int>(type: "int", nullable: true),
                    SpiralAbyssInfo_EnergySkillRank = table.Column<int>(type: "int", nullable: true),
                    SpiralAbyssInfo_NormalSkillRank = table.Column<int>(type: "int", nullable: true),
                    SpiralAbyssInfo_RevealRank = table.Column<int>(type: "int", nullable: true),
                    SpiralAbyssInfo_TakeDamageRank = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SpiralAbyss_Ranks", x => x.Id);
                    table.ForeignKey(
                        name: "FK_SpiralAbyss_Ranks_SpiralAbyss_Infos_SpiralAbyssInfo_DamageRa~",
                        column: x => x.SpiralAbyssInfo_DamageRank,
                        principalTable: "SpiralAbyss_Infos",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_SpiralAbyss_Ranks_SpiralAbyss_Infos_SpiralAbyssInfo_DefeatRa~",
                        column: x => x.SpiralAbyssInfo_DefeatRank,
                        principalTable: "SpiralAbyss_Infos",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_SpiralAbyss_Ranks_SpiralAbyss_Infos_SpiralAbyssInfo_EnergySk~",
                        column: x => x.SpiralAbyssInfo_EnergySkillRank,
                        principalTable: "SpiralAbyss_Infos",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_SpiralAbyss_Ranks_SpiralAbyss_Infos_SpiralAbyssInfo_NormalSk~",
                        column: x => x.SpiralAbyssInfo_NormalSkillRank,
                        principalTable: "SpiralAbyss_Infos",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_SpiralAbyss_Ranks_SpiralAbyss_Infos_SpiralAbyssInfo_RevealRa~",
                        column: x => x.SpiralAbyssInfo_RevealRank,
                        principalTable: "SpiralAbyss_Infos",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_SpiralAbyss_Ranks_SpiralAbyss_Infos_SpiralAbyssInfo_TakeDama~",
                        column: x => x.SpiralAbyssInfo_TakeDamageRank,
                        principalTable: "SpiralAbyss_Infos",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "TravelTecord_GroupStats",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    TravelRecordMonthDataId = table.Column<int>(type: "int", nullable: false),
                    Uid = table.Column<int>(type: "int", nullable: false),
                    Year = table.Column<int>(type: "int", nullable: false),
                    Month = table.Column<int>(type: "int", nullable: false),
                    ActionId = table.Column<int>(type: "int", nullable: false),
                    ActionName = table.Column<string>(type: "varchar(255)", maxLength: 255, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Number = table.Column<int>(type: "int", nullable: false),
                    Percent = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TravelTecord_GroupStats", x => x.Id);
                    table.ForeignKey(
                        name: "FK_TravelTecord_GroupStats_TravelRecord_MonthDatas_TravelRecord~",
                        column: x => x.TravelRecordMonthDataId,
                        principalTable: "TravelRecord_MonthDatas",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "SpiralAbyss_Levels",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    SpiralAbyssFloorId = table.Column<int>(type: "int", nullable: false),
                    Index = table.Column<int>(type: "int", nullable: false),
                    Star = table.Column<int>(type: "int", nullable: false),
                    MaxStar = table.Column<int>(type: "int", nullable: false)
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
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "SpiralAbyss_Battles",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    SpiralAbyssLevelId = table.Column<int>(type: "int", nullable: false),
                    Index = table.Column<int>(type: "int", nullable: false),
                    Time = table.Column<DateTimeOffset>(type: "datetime(6)", nullable: false)
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
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "SpiralAbyss_Avatars",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    SpiralAbyssBattleId = table.Column<int>(type: "int", nullable: false),
                    AvatarId = table.Column<int>(type: "int", nullable: false),
                    Icon = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Level = table.Column<int>(type: "int", nullable: false),
                    Rarity = table.Column<int>(type: "int", nullable: false)
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
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateIndex(
                name: "IX_i18n_de_de",
                table: "i18n",
                column: "de_de");

            migrationBuilder.CreateIndex(
                name: "IX_i18n_en_us",
                table: "i18n",
                column: "en_us");

            migrationBuilder.CreateIndex(
                name: "IX_i18n_es_es",
                table: "i18n",
                column: "es_es");

            migrationBuilder.CreateIndex(
                name: "IX_i18n_fr_fr",
                table: "i18n",
                column: "fr_fr");

            migrationBuilder.CreateIndex(
                name: "IX_i18n_id_id",
                table: "i18n",
                column: "id_id");

            migrationBuilder.CreateIndex(
                name: "IX_i18n_ja_jp",
                table: "i18n",
                column: "ja_jp");

            migrationBuilder.CreateIndex(
                name: "IX_i18n_ko_kr",
                table: "i18n",
                column: "ko_kr");

            migrationBuilder.CreateIndex(
                name: "IX_i18n_pt_pt",
                table: "i18n",
                column: "pt_pt");

            migrationBuilder.CreateIndex(
                name: "IX_i18n_ru_ru",
                table: "i18n",
                column: "ru_ru");

            migrationBuilder.CreateIndex(
                name: "IX_i18n_th_th",
                table: "i18n",
                column: "th_th");

            migrationBuilder.CreateIndex(
                name: "IX_i18n_vi_vn",
                table: "i18n",
                column: "vi_vn");

            migrationBuilder.CreateIndex(
                name: "IX_i18n_zh_cn",
                table: "i18n",
                column: "zh_cn");

            migrationBuilder.CreateIndex(
                name: "IX_i18n_zh_tw",
                table: "i18n",
                column: "zh_tw");

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
                name: "IX_Record_All_DateTime",
                table: "Record_All",
                column: "DateTime");

            migrationBuilder.CreateIndex(
                name: "IX_Record_All_DeviceId",
                table: "Record_All",
                column: "DeviceId");

            migrationBuilder.CreateIndex(
                name: "IX_Record_All_Ip",
                table: "Record_All",
                column: "Ip");

            migrationBuilder.CreateIndex(
                name: "IX_Record_All_Path",
                table: "Record_All",
                column: "Path");

            migrationBuilder.CreateIndex(
                name: "IX_Record_All_ReturnCode",
                table: "Record_All",
                column: "ReturnCode");

            migrationBuilder.CreateIndex(
                name: "IX_Record_All_StatusCode",
                table: "Record_All",
                column: "StatusCode");

            migrationBuilder.CreateIndex(
                name: "IX_Record_Wishlog_Operation",
                table: "Record_Wishlog",
                column: "Operation");

            migrationBuilder.CreateIndex(
                name: "IX_Record_Wishlog_Uid",
                table: "Record_Wishlog",
                column: "Uid");

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
                name: "i18n");

            migrationBuilder.DropTable(
                name: "Info_Character_Constellation");

            migrationBuilder.DropTable(
                name: "Info_Weapon");

            migrationBuilder.DropTable(
                name: "Info_WishEvent");

            migrationBuilder.DropTable(
                name: "Record_Wishlog");

            migrationBuilder.DropTable(
                name: "SpiralAbyss_Avatars");

            migrationBuilder.DropTable(
                name: "SpiralAbyss_Ranks");

            migrationBuilder.DropTable(
                name: "TravelRecord_AwardItems");

            migrationBuilder.DropTable(
                name: "TravelTecord_GroupStats");

            migrationBuilder.DropTable(
                name: "Wishlog_Authkeys");

            migrationBuilder.DropTable(
                name: "Wishlog_Items");

            migrationBuilder.DropTable(
                name: "Info_Character");

            migrationBuilder.DropTable(
                name: "Record_All");

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
