using Xunkong.Core;
using Xunkong.Hoyolab;
using Xunkong.Hoyolab.Account;
using Xunkong.Hoyolab.Avatar;
using Xunkong.Hoyolab.DailyNote;
using Xunkong.Hoyolab.GameRecord;
using Xunkong.Hoyolab.SpiralAbyss;
using Xunkong.Hoyolab.TravelNotes;

namespace Xunkong.Desktop.Services;

internal class HoyolabService
{


    private readonly HoyolabClient _hoyolabClient;



    public HoyolabService(HoyolabClient hoyolabClient)
    {
        _hoyolabClient = hoyolabClient;
    }


    public HoyolabUserInfo? GetLastSelectedOrFirstHoyolabUserInfo()
    {
        using var dapper = DatabaseProvider.CreateConnection();
        const string sql = $"SELECT u.* FROM HoyolabUserInfo u LEFT JOIN Setting s ON u.Uid=s.Value WHERE s.Key='{SettingKeys.LastSelectUserInfoUid}' LIMIT 1;";
        var info = dapper.QueryFirstOrDefault<HoyolabUserInfo>(sql);
        if (info is not null)
        {
            return info;
        }
        else
        {
            return dapper.QueryFirstOrDefault<HoyolabUserInfo>($"SELECT * FROM HoyolabUserInfo LIMIT 1;");
        }
    }


    public GenshinRoleInfo? GetLastSelectedOrFirstGenshinRoleInfo()
    {
        using var dapper = DatabaseProvider.CreateConnection();
        const string sql = $"SELECT g.* FROM GenshinRoleInfo g LEFT JOIN Setting s ON g.Uid=s.Value WHERE s.Key='{SettingKeys.LastSelectGameRoleUid}' LIMIT 1;";
        var info = dapper.QueryFirstOrDefault<GenshinRoleInfo>(sql);
        if (info is not null)
        {
            return info;
        }
        else
        {
            return dapper.QueryFirstOrDefault<GenshinRoleInfo>($"SELECT * FROM GenshinRoleInfo LIMIT 1;");
        }
    }



    public IEnumerable<HoyolabUserInfo> GetHoyolabUserInfoList()
    {
        using var dapper = DatabaseProvider.CreateConnection();
        return dapper.Query<HoyolabUserInfo>("SELECT * FROM HoyolabUserInfo;");
    }


    public IEnumerable<GenshinRoleInfo> GetGenshinRoleInfoList()
    {
        using var dapper = DatabaseProvider.CreateConnection();
        return dapper.Query<GenshinRoleInfo>("SELECT * FROM GenshinRoleInfo;");
    }



    public async Task<HoyolabUserInfo> GetHoyolabUserInfoAsync(string cookie)
    {
        var user = await _hoyolabClient.GetHoyolabUserInfoAsync(cookie);
        using var dapper = DatabaseProvider.CreateConnection();
        const string sql = """
                INSERT OR REPLACE INTO HoyolabUserInfo
                (Uid, Nickname, Introduce, Avatar, Gender, AvatarUrl, Pendant, Cookie)
                VALUES (@Uid, @Nickname, @Introduce, @Avatar, @Gender, @AvatarUrl, @Pendant, @Cookie);
                """;
        dapper.Execute(sql, user);
        return user;
    }



    public async Task<List<GenshinRoleInfo>> GetGenshinRoleInfoListAsync(string cookie)
    {
        var roles = await _hoyolabClient.GetGenshinRoleInfosAsync(cookie);
        using var dapper = DatabaseProvider.CreateConnection();
        const string sql = """
                INSERT OR REPLACE INTO GenshinRoleInfo
                (Uid, GameBiz, Region, Nickname, Level, IsChosen, RegionName, IsOfficial, Cookie)
                VALUES (@Uid, @GameBiz, @Region, @Nickname, @Level, @IsChosen, @RegionName, @IsOfficial, @Cookie);
                """;
        dapper.Execute(sql, roles);
        return roles;
    }



    public async Task UpdateAllAccountsAsync()
    {
        foreach (var item in GetHoyolabUserInfoList())
        {
            await GetHoyolabUserInfoAsync(item.Cookie!);
            await GetGenshinRoleInfoListAsync(item.Cookie!);
        }
    }



    public HoyolabUserInfo? GetHoyolabUserInfoFromDatabaseByCookie(string cookie)
    {
        using var dapper = DatabaseProvider.CreateConnection();
        return dapper.QueryFirstOrDefault<HoyolabUserInfo>("SELECT * FROM HoyolabUserInfo WHERE Cookie=@Cookie;", new { Cookie = cookie });
    }




    public HoyolabUserInfo? GetHoyolabUserInfo(int uid)
    {
        using var dapper = DatabaseProvider.CreateConnection();
        return dapper.QueryFirstOrDefault<HoyolabUserInfo>("SELECT * FROM HoyolabUserInfo WHERE Uid=@Uid;", new { Uid = uid });
    }

    public GenshinRoleInfo? GetGenshinRoleInfo(int uid)
    {
        using var dapper = DatabaseProvider.CreateConnection();
        return dapper.QueryFirstOrDefault<GenshinRoleInfo>("SELECT * FROM GenshinRoleInfo WHERE Uid=@Uid;", new { Uid = uid });
    }


    public HoyolabUserInfo? GetHoyolabUserInfoOrFirst(int uid)
    {
        using var dapper = DatabaseProvider.CreateConnection();
        var info = GetHoyolabUserInfo(uid);
        if (info is not null)
        {
            return info;
        }
        else
        {
            return dapper.QueryFirstOrDefault<HoyolabUserInfo>("SELECT * FROM HoyolabUserInfo LIMIT 1;");
        }
    }



    public async Task<GenshinRoleInfo?> GetGenshinRoleInfoOrFirstAsync(GenshinRoleInfo role)
    {
        if (role == null || string.IsNullOrWhiteSpace(role.Cookie))
        {
            return null;
        }
        var list = await GetGenshinRoleInfoListAsync(role.Cookie);
        var newRole = list?.FirstOrDefault(x => x.Uid == role.Uid);
        if (newRole != null)
        {
            return newRole;
        }
        else
        {
            return list?.FirstOrDefault();
        }
    }



    public void DeleteHoyolabUserInfo(int uid)
    {
        using var dapper = DatabaseProvider.CreateConnection();
        dapper.Execute($"DELETE FROM HoyolabUserInfo WHERE Uid=@Uid;", new { Uid = uid });
    }


    public void DeleteHoyolabUserInfo(string cookie)
    {
        using var dapper = DatabaseProvider.CreateConnection();
        dapper.Execute($"DELETE FROM HoyolabUserInfo WHERE Cookie=@Cookie;", new { Cookie = cookie });
    }



    public void DeleteGenshinRoleInfo(int uid)
    {
        using var dapper = DatabaseProvider.CreateConnection();
        dapper.Execute($"DELETE FROM GenshinRoleInfo WHERE Uid=@Uid;", new { Uid = uid });
    }


    public void DeleteGenshinRoleInfo(string cookie)
    {
        using var dapper = DatabaseProvider.CreateConnection();
        dapper.Execute($"DELETE FROM GenshinRoleInfo WHERE Cookie=@Cookie;", new { Cookie = cookie });
    }



    public async Task<SignInInfo> GetSignInInfoAsync(GenshinRoleInfo role)
    {
        return await _hoyolabClient.GetSignInInfoAsync(role);
    }



    /// <summary>
    /// 签到
    /// </summary>
    /// <param name="role"></param>
    /// <returns>true 签到成功，false 无需签到</returns>
    public async Task<bool> SignInAsync(GenshinRoleInfo role)
    {
        using var dapper = DatabaseProvider.CreateConnection();
        var localLastSignDate = dapper.QueryFirstOrDefault<string>("SELECT Date FROM DailySignInHistory WHERE Uid=@Uid ORDER BY Id DESC LIMIT 1;", new { Uid = role.Uid });
        var nowDate = DateTimeOffset.UtcNow.AddHours(8).ToString("yyyy-MM-dd");
        if (nowDate == localLastSignDate)
        {
            // 今日已签到，并且有记录
            return false;
        }
        var signInfo = await _hoyolabClient.GetSignInInfoAsync(role);
        bool result = false;
        if (!signInfo.IsSign)
        {
            // 签到
            await _hoyolabClient.SignInAsync(role);
            result = true;
        }
        dapper.Execute("INSERT INTO DailySignInHistory (Uid, Date, Time) VALUES (@Uid, @Date, @Time);", new { Uid = role.Uid, Date = nowDate, Time = DateTimeOffset.Now });
        return result;
    }


    /// <summary>
    /// 深境螺旋
    /// </summary>
    /// <param name="role"></param>
    /// <param name="schedule">1当期，2上期</param>
    /// <returns></returns>
    public async Task<SpiralAbyssInfo> GetSpiralAbyssInfoAsync(GenshinRoleInfo role, int schedule = 1)
    {
        var info = await _hoyolabClient.GetSpiralAbyssInfoAsync(role, schedule);
        using var dapper = DatabaseProvider.CreateConnection();
        using var t = dapper.BeginTransaction();
        dapper.Execute($"DELETE FROM SpiralAbyssInfo WHERE Uid=@Uid AND ScheduleId=@ScheduleId;", new { Uid = info.Uid, ScheduleId = info.ScheduleId }, t);
        const string sql = """
                INSERT OR REPLACE INTO SpiralAbyssInfo
                (Uid, ScheduleId, StartTime, EndTime, TotalBattleCount, TotalWinCount, MaxFloor, TotalStar, Value)
                VALUES (@Uid, @ScheduleId, @StartTime, @EndTime, @TotalBattleCount, @TotalWinCount, @MaxFloor, @TotalStar, @Value);
                """;
        var obj = new
        {
            Uid = info.Uid,
            ScheduleId = info.ScheduleId,
            StartTime = info.StartTime,
            EndTime = info.EndTime,
            TotalBattleCount = info.TotalBattleCount,
            TotalWinCount = info.TotalWinCount,
            MaxFloor = info.MaxFloor,
            TotalStar = info.TotalStar,
            Value = JsonSerializer.Serialize(info),
        };
        dapper.Execute(sql, obj, t);
        t.Commit();
        return info;
    }



    public List<SpiralAbyssInfo> GetSpiralAbyssInfos(int uid)
    {
        const string sql = "SELECT Id, Uid, ScheduleId, StartTime, EndTime, TotalBattleCount, TotalWinCount, MaxFloor, TotalStar FROM SpiralAbyssInfo WHERE Uid=@Uid AND TotalBattleCount>0 ORDER BY ScheduleId DESC;";
        using var dapper = DatabaseProvider.CreateConnection();
        return dapper.Query<SpiralAbyssInfo>(sql, new { Uid = uid }).ToList();
    }


    public SpiralAbyssInfo? GetSpiralAbyssInfo(int id)
    {
        const string sql = "SELECT Value FROM SpiralAbyssInfo WHERE Id=@Id LIMIT 1;";
        using var dapper = DatabaseProvider.CreateConnection();
        var value = dapper.QueryFirstOrDefault<string>(sql, new { Id = id });
        return JsonSerializer.Deserialize<SpiralAbyssInfo>(value);
    }


    public SpiralAbyssInfo? GetSpiralAbyssInfo(int uid, int scheduleId)
    {
        const string sql = "SELECT Value FROM SpiralAbyssInfo WHERE Uid=@Uid AND ScheduleId=@ScheduleId LIMIT 1;";
        using var dapper = DatabaseProvider.CreateConnection();
        var value = dapper.QueryFirstOrDefault<string>(sql, new { Uid = uid, ScheduleId = scheduleId });
        return JsonSerializer.Deserialize<SpiralAbyssInfo>(value);
    }






    public async Task<GameRecordSummary> GetGameRecordSummaryAsync(GenshinRoleInfo role)
    {
        return await _hoyolabClient.GetGameRecordSummaryAsync(role);
    }



    public async Task<List<AvatarDetail>> GetAvatarDetailsAsync(GenshinRoleInfo role)
    {
        return await _hoyolabClient.GetAvatarDetailsAsync(role);
    }



    public async Task<List<AvatarSkill>> GetAvatarSkillLevelAsync(GenshinRoleInfo role, int characterId)
    {
        return await _hoyolabClient.GetAvatarSkillLevelAsync(role, characterId);
    }


    public async Task<DailyNoteInfo?> GetDailyNoteAsync(GenshinRoleInfo role)
    {
        return await _hoyolabClient.GetDailyNoteAsync(role);
    }



    public async Task<TravelNotesSummary> GetTravelNotesSummaryAsync(GenshinRoleInfo role, int month)
    {
        var summary = await _hoyolabClient.GetTravelNotesSummaryAsync(role, month);
        if (summary.MonthData is null)
        {
            return summary;
        }
        var m = summary.MonthData;
        using var dapper = DatabaseProvider.CreateConnection();
        var existData = dapper.QueryFirstOrDefault<TravelNotesMonthData>("SELECT * FROM TravelNotesMonthData WHERE Uid=@Uid AND Year=@Year AND Month=@Month LIMIT 1;", m);
        if (existData is not null)
        {
            if (existData.CurrentPrimogems == m.CurrentPrimogems && existData.CurrentMora == m.CurrentMora)
            {
                return summary;
            }
        }
        dapper.Execute("""
            INSERT OR REPLACE INTO TravelNotesMonthData
            (Uid, Year, Month, CurrentPrimogems, CurrentMora, LastPrimogems, LastMora, CurrentPrimogemsLevel, PrimogemsChangeRate, MoraChangeRate, PrimogemsGroupBy)
            VALUES (@Uid, @Year, @Month, @CurrentPrimogems, @CurrentMora, @LastPrimogems, @LastMora, @CurrentPrimogemsLevel, @PrimogemsChangeRate, @MoraChangeRate, @PrimogemsGroupBy);
            """, m);
        return summary;
    }



    public async Task<int> GetTravelRecordDetailAsync(GenshinRoleInfo role, int month, TravelNotesAwardType type, int limit = 100)
    {
        var detail = await _hoyolabClient.GetTravelNotesDetailAsync(role, month, type, limit);
        if (detail.List is null || !detail.List.Any())
        {
            return 0;
        }
        var list = detail.List;
        var dataYear = list[0].Year;
        var dataMonth = list[0].Month;
        if (list.Any(x => x.Year != dataYear) || list.Any(x => x.Month != dataMonth))
        {
            // 检查是否有超出本月的记录项
            throw new XunkongException("Some of travel record items is out of the request month.");
        }
        using var dapper = DatabaseProvider.CreateConnection();
        var existCount = dapper.QuerySingleOrDefault<int>("SELECT COUNT(*) FROM TravelNotesAwardItem WHERE Uid=@Uid AND Year=@Year AND Month=@Month AND Type=@Type;", list.FirstOrDefault());
        if (existCount == list.Count)
        {
            return 0;
        }
        using var t = dapper.BeginTransaction();
        dapper.Execute($"DELETE FROM TravelNotesAwardItem WHERE Uid=@Uid AND Year=@Year AND Month=@Month AND Type=@Type;", list.FirstOrDefault(), t);
        dapper.Execute("""
                INSERT INTO TravelNotesAwardItem (Uid, Year, Month, Type, ActionId, ActionName, Time, Number)
                VALUES (@Uid, @Year, @Month, @Type, @ActionId, @ActionName, @Time, @Number);
                """, list, t);
        t.Commit();
        return list.Count - existCount;
    }



    /// <summary>
    /// 获取旅行原石和摩拉奖励项，不分原石和摩拉
    /// </summary>
    /// <param name="uid"></param>
    /// <param name="type"></param>
    /// <param name="startTime"></param>
    /// <param name="endTime"></param>
    /// <returns></returns>
    public IEnumerable<TravelNotesAwardItem> GetTravelNotesAwardItems(int uid, DateTime? startTime = null, DateTime? endTime = null)
    {
        const string sql = "SELECT * FROM TravelNotesAwardItem WHERE Uid=@Uid AND Time>=@StartTime AND Time<=@EndTime;";
        var obj = new { Uid = uid, StartTime = startTime ?? DateTime.MinValue, EndTime = endTime ?? DateTime.MaxValue };
        using var dapper = DatabaseProvider.CreateConnection();
        return dapper.Query<TravelNotesAwardItem>(sql, obj);
    }



    /// <summary>
    /// 获取旅行原石和摩拉奖励项，区分原石和摩拉
    /// </summary>
    /// <param name="uid"></param>
    /// <param name="type"></param>
    /// <param name="startTime"></param>
    /// <param name="endTime"></param>
    /// <returns></returns>
    public IEnumerable<TravelNotesAwardItem> GetTravelNotesAwardItems(int uid, TravelNotesAwardType type, DateTime? startTime = null, DateTime? endTime = null)
    {
        const string sql = "SELECT * FROM TravelNotesAwardItem WHERE Uid=@Uid AND Type=@Type AND Time>=@StartTime AND Time<=@EndTime;";
        var obj = new { Uid = uid, Type = type, StartTime = startTime ?? DateTime.MinValue, EndTime = endTime ?? DateTime.MaxValue };
        using var dapper = DatabaseProvider.CreateConnection();
        return dapper.Query<TravelNotesAwardItem>(sql, obj);
    }



    /// <summary>
    /// 获取旅行札记月信息
    /// </summary>
    /// <param name="uid"></param>
    /// <returns></returns>
    public IEnumerable<TravelNotesMonthData> GetTravelNotesMonthData(int uid)
    {
        const string sql = "SELECT * FROM TravelNotesMonthData WHERE Uid=@Uid;";
        using var dapper = DatabaseProvider.CreateConnection();
        return dapper.Query<TravelNotesMonthData>(sql, new { Uid = uid });
    }




}
