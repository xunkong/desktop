using LiteDB;
using Microsoft.Data.Sqlite;

namespace Xunkong.Desktop.Helpers;

internal class DatabaseProvider
{


    private static readonly string _sqlitePath;

    private static readonly string _liteDbPath;

    private static readonly string _sqliteConnectionString;

    private static readonly string _liteDbConnectionString;

    private static bool _initialized;

    private const int DATABASE_VERSION = 1;


    public static string SqlitePath => _sqlitePath;
    public static string LiteDbPath => _liteDbPath;



    static DatabaseProvider()
    {
        Directory.CreateDirectory(Path.Combine(XunkongEnvironment.UserDataPath, "Database"));
        _liteDbPath = Path.Combine(XunkongEnvironment.UserDataPath, @"Database\GenshinData.db");
        _sqlitePath = Path.Combine(XunkongEnvironment.UserDataPath, @"Database\XunkongData.db");
        _sqliteConnectionString = $"Data Source={_sqlitePath};";
        _liteDbConnectionString = $"Filename={_liteDbPath};Connection=shared;";

        SqlMapper.AddTypeHandler(new DapperSqlMapper.DateTimeOffsetHandler());
        SqlMapper.AddTypeHandler(new DapperSqlMapper.TravelNotesPrimogemsMonthGroupStatsListHandler());
    }




    private static void Initialize()
    {
        using var dapper = new SqliteConnection(_sqliteConnectionString);
        dapper.Open();
        dapper.Execute(TableStructure_Init);
        var version = dapper.QueryFirstOrDefault<int>("SELECT Value FROM DatabaseVersion WHERE Key='DatabaseVersion' LIMIT 1;");
        if (version < DATABASE_VERSION)
        {
            var updatingSqls = GetUpdatingSqls(version);
            foreach (var sql in updatingSqls)
            {
                dapper.Execute(sql);
            }
        }
        _initialized = true;
    }



    public static SqliteConnection CreateConnection()
    {
        if (!_initialized)
        {
            Initialize();
        }
        var dapper = new SqliteConnection(_sqliteConnectionString);
        dapper.Open();
        return dapper;
    }


    //public static XunkongDbContext CreateContext()
    //{
    //    if (!_initialized)
    //    {
    //        Initialize();
    //    }
    //    return new XunkongDbContext(_sqliteConnectionString);
    //}



    public static LiteDatabase CreateLiteDB()
    {
        return new LiteDatabase(_liteDbConnectionString);
    }






    private static List<string> GetUpdatingSqls(int version)
    {
        return version switch
        {
            0 => new List<string> { TableStructure_v1 },
            _ => new List<string> { },
        };
    }




    private const string TableStructure_Init = """
        CREATE TABLE IF NOT EXISTS DatabaseVersion
        (
            Key   TEXT NOT NULL PRIMARY KEY,
            Value TEXT
        );
        """;


    private const string TableStructure_v1 = """
        BEGIN TRANSACTION;
        
        CREATE TABLE IF NOT EXISTS Setting
        (
            Key   TEXT NOT NULL PRIMARY KEY,
            Value TEXT
        );
        
        CREATE TABLE IF NOT EXISTS DailySignInHistory
        (
            Id   INTEGER NOT NULL PRIMARY KEY AUTOINCREMENT,
            Uid  INTEGER NOT NULL,
            Date TEXT    NOT NULL,
            Time TEXT    NOT NULL
        );
        
        CREATE TABLE IF NOT EXISTS GenshinRoleInfo
        (
            Uid        INTEGER NOT NULL PRIMARY KEY,
            GameBiz    TEXT,
            Region     INTEGER NOT NULL,
            Nickname   TEXT,
            Level      INTEGER NOT NULL,
            IsChosen   INTEGER NOT NULL,
            RegionName TEXT,
            IsOfficial INTEGER NOT NULL,
            Cookie     TEXT
        );
        
        CREATE TABLE IF NOT EXISTS HoyolabUserInfo
        (
            Uid       INTEGER NOT NULL PRIMARY KEY,
            Nickname  TEXT,
            Introduce TEXT,
            Avatar    TEXT,
            Gender    INTEGER NOT NULL,
            AvatarUrl TEXT,
            Pendant   TEXT,
            Cookie    TEXT
        );
        
        CREATE TABLE IF NOT EXISTS SpiralAbyssInfo
        (
            Id               INTEGER NOT NULL PRIMARY KEY AUTOINCREMENT,
            Uid              INTEGER NOT NULL,
            ScheduleId       INTEGER NOT NULL,
            StartTime        TEXT    NOT NULL,
            EndTime          TEXT    NOT NULL,
            TotalBattleCount INTEGER NOT NULL,
            TotalWinCount    INTEGER NOT NULL,
            MaxFloor         TEXT,
            TotalStar        INTEGER NOT NULL,
            Value            TEXT    NOT NULL
        );
        
        CREATE INDEX IF NOT EXISTS IX_SpiralAbyssInfo_MaxFloor ON SpiralAbyssInfo (MaxFloor);
        CREATE INDEX IF NOT EXISTS IX_SpiralAbyssInfo_ScheduleId ON SpiralAbyssInfo (ScheduleId);
        CREATE INDEX IF NOT EXISTS IX_SpiralAbyssInfo_TotalBattleCount ON SpiralAbyssInfo (TotalBattleCount);
        CREATE INDEX IF NOT EXISTS IX_SpiralAbyssInfo_TotalStar ON SpiralAbyssInfo (TotalStar);
        CREATE INDEX IF NOT EXISTS IX_SpiralAbyssInfo_TotalWinCount ON SpiralAbyssInfo (TotalWinCount);
        CREATE UNIQUE INDEX IF NOT EXISTS IX_SpiralAbyssInfo_Uid_ScheduleId ON SpiralAbyssInfo (Uid, ScheduleId);
        
        CREATE TABLE IF NOT EXISTS TravelNotesAwardItem
        (
            Id         INTEGER NOT NULL PRIMARY KEY AUTOINCREMENT,
            Uid        INTEGER NOT NULL,
            Year       INTEGER NOT NULL,
            Month      INTEGER NOT NULL,
            Type       INTEGER NOT NULL,
            ActionId   INTEGER NOT NULL,
            ActionName TEXT,
            Time       TEXT    NOT NULL,
            Number     INTEGER NOT NULL
        );
        
        CREATE INDEX IF NOT EXISTS IX_TravelNotesAwardItem_Uid_Year_Month ON TravelNotesAwardItem (Uid, Year, Month);
        CREATE INDEX IF NOT EXISTS IX_TravelNotesAwardItem_Type ON TravelNotesAwardItem (Type);
        CREATE INDEX IF NOT EXISTS IX_TravelNotesAwardItem_Time ON TravelNotesAwardItem (Time);
        
        CREATE TABLE IF NOT EXISTS TravelNotesMonthData
        (
            Id                    INTEGER NOT NULL PRIMARY KEY AUTOINCREMENT,
            Uid                   INTEGER NOT NULL,
            Year                  INTEGER NOT NULL,
            Month                 INTEGER NOT NULL,
            CurrentPrimogems      INTEGER NOT NULL,
            CurrentMora           INTEGER NOT NULL,
            LastPrimogems         INTEGER NOT NULL,
            LastMora              INTEGER NOT NULL,
            CurrentPrimogemsLevel INTEGER NOT NULL,
            PrimogemsChangeRate   INTEGER NOT NULL,
            MoraChangeRate        INTEGER NOT NULL,
            PrimogemsGroupBy      TEXT    NOT NULL
        );
        
        CREATE UNIQUE INDEX IF NOT EXISTS IX_TravelNotesMonthData_Uid_Year_Month ON TravelNotesMonthData (Uid, Year, Month);
        
        CREATE TABLE IF NOT EXISTS WebToolItem
        (
            Id         INTEGER NOT NULL PRIMARY KEY AUTOINCREMENT,
            Title      TEXT,
            Icon       TEXT,
            "Order"    INTEGER NOT NULL,
            Url        TEXT    NOT NULL,
            JavaScript TEXT
        );
        
        CREATE TABLE IF NOT EXISTS WishlogItem
        (
            Uid       INTEGER NOT NULL,
            Id        INTEGER NOT NULL,
            WishType  INTEGER NOT NULL,
            Time      TEXT    NOT NULL,
            Name      TEXT    NOT NULL,
            Language  TEXT    NOT NULL,
            ItemType  TEXT    NOT NULL,
            RankType  INTEGER NOT NULL,
            QueryType INTEGER NOT NULL,
            PRIMARY KEY (Uid, Id)
        );

        CREATE TABLE IF NOT EXISTS WishlogUrl
        (
            Uid      INTEGER NOT NULL PRIMARY KEY,
            Url      TEXT    NOT NULL,
            DateTime TEXT    NOT NULL
        );
        
        INSERT OR REPLACE INTO DatabaseVersion (Key, Value) VALUES ('DatabaseVersion', 1);
        
        COMMIT;
        """;





}


