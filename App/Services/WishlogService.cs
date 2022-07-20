using Microsoft.EntityFrameworkCore;
using System.Text.RegularExpressions;
using Xunkong.Hoyolab;
using Xunkong.Hoyolab.Wishlog;

namespace Xunkong.Desktop.Services;

internal class WishlogService
{


    private readonly WishlogClient _wishlogClient;

    private static readonly string UserProfile = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);

    private static readonly string LogFile_Cn = Path.Combine(UserProfile, @"AppData\LocalLow\miHoYo\原神\output_log.txt");

    private static readonly string LogFile_Sea = Path.Combine(UserProfile, @"AppData\LocalLow\miHoYo\Genshin Impact\output_log.txt");


    public WishlogService(WishlogClient wishlogClient)
    {
        _wishlogClient = wishlogClient;
    }



    public static WishType WishTypeToQueryType(WishType type)
    {
        return type switch
        {
            WishType.CharacterEvent_2 => WishType.CharacterEvent,
            _ => type,
        };
    }


    public static RegionType UidToRegionType(int uid)
    {
        return uid.ToString().FirstOrDefault() switch
        {
            '1' => RegionType.cn_gf01,
            '2' => RegionType.cn_gf01,
            '3' => RegionType.cn_gf01,
            '4' => RegionType.cn_gf01,
            '5' => RegionType.cn_qd01,
            '6' => RegionType.os_usa,
            '7' => RegionType.os_euro,
            '8' => RegionType.os_asia,
            '9' => RegionType.os_cht,
            _ => RegionType.None,
        };
    }




    /// <summary>
    /// 获取原神日志中最新的祈愿记录网址
    /// </summary>
    /// <param name="isSea"></param>
    /// <returns></returns>
    /// <exception cref="FileNotFoundException"></exception>
    public async Task<string> FindWishlogUrlFromLogFileAsync(bool isSea = false)
    {
        var file = isSea ? LogFile_Sea : LogFile_Cn;
        if (!File.Exists(file))
        {
            file = isSea ? LogFile_Cn : LogFile_Sea;
        }
        if (!File.Exists(file))
        {
            throw new FileNotFoundException("没有找到日志文件，请打开游戏内的祈愿记录界面。");
        }
        using var stream = File.Open(file, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
        using var reader = new StreamReader(stream);
        var log = await reader.ReadToEndAsync();
        var matches = Regex.Matches(log, @"OnGetWebViewPageFinish:(.+#/log)");
        if (matches.Any())
        {
            return matches.Last().Value.Replace("OnGetWebViewPageFinish:", "");
        }
        else
        {
            throw new FileNotFoundException("没有找到日志文件，请打开游戏内的祈愿记录界面。");
        }
    }



    /// <summary>
    /// 获取祈愿记录网址对应的uid
    /// </summary>
    /// <param name="wishlogUrl"></param>
    /// <returns></returns>
    /// <exception cref="HoyolabException"></exception>
    public async Task<int> GetUidByWishlogUrl(string wishlogUrl)
    {
        var uid = await _wishlogClient.GetUidAsync(wishlogUrl);
        if (uid == 0)
        {
            throw new HoyolabException(-1, "该网址没有对应的祈愿记录");
        }
        using var dapper = DatabaseProvider.CreateConnection();
        var model = new WishlogUrl { Uid = uid, Url = wishlogUrl, DateTime = DateTimeOffset.Now };
        dapper.Execute("INSERT OR REPLACE INTO WishlogUrl (Uid, Url, DateTime) VALUES(@Uid, @Url, @DateTime);", model);
        return uid;
    }



    /// <summary>
    /// 检查指定uid的祈愿记录认证信息是否过期，默认24小时后过期
    /// </summary>
    /// <remarks>
    /// 寻空使用祈愿记录网址确认用户身份，每次从米哈游服务器获取祈愿记录时，会保存此次使用的祈愿记录网址和当前时间，在之后的24小时内认为该认证有效
    /// </remarks>
    /// <param name="uid"></param>
    /// <returns></returns>
    public bool CheckWishlogUrlExpired(int uid)
    {
        using var dapper = DatabaseProvider.CreateConnection();
        var model = dapper.QueryFirstOrDefault<WishlogUrl>("SELECT * FROM WishlogUrl WHERE Uid=@Uid LIMIT 1;", new { Uid = uid });
        if (model == null)
        {
            return false;
        }
        else
        {
            return model.DateTime + TimeSpan.FromDays(1) > DateTimeOffset.Now;
        }
    }



    /// <summary>
    /// 检查指定uid保存的祈愿记录网址的实时有效性
    /// </summary>
    /// <remarks>
    /// 此有效性不同于祈愿记录认证信息，有效则表示使用此网址能够从米哈游服务器获取祈愿记录
    /// </remarks>
    /// <param name="uid"></param>
    /// <returns></returns>
    public async Task<bool> CheckWishlogUrlTimeoutAsync(int uid)
    {
        using var dapper = DatabaseProvider.CreateConnection();
        var url = dapper.QueryFirstOrDefault<string>("SELECT Url FROM WishlogUrl WHERE Uid=@Uid LIMIT 1;", new { Uid = uid });
        if (string.IsNullOrWhiteSpace(url))
        {
            return false;
        }
        try
        {
            var newUid = await GetUidByWishlogUrl(url);
            if (newUid == uid)
            {
                return true;
            }
            else
            {
                return false;
            }
        }
        catch (HoyolabException)
        {
            return false;
        }
    }




    /// <summary>
    /// 根据本地保存的祈愿记录网址，从米哈游服务器获取指定uid的祈愿记录，并返回新增数量
    /// </summary>
    /// <param name="uid"></param>
    /// <param name="onProgressChanged">页数变化时调用</param>
    /// <param name="getAll">是否获取全部数据</param>
    /// <returns>新增祈愿记录的数量</returns>
    /// <exception cref="XunkongException"></exception>
    /// <exception cref="HoyolabException"></exception>
    public async Task<int> GetWishlogByUidAsync(int uid, IProgress<string>? progress = null, bool getAll = false)
    {
        var uidObj = new { Uid = uid };
        using var dapper = DatabaseProvider.CreateConnection();
        var wishlogUrl = dapper.QueryFirstOrDefault<string>("SELECT Url FROM WishlogUrl WHERE Uid=@Uid LIMIT 1;", uidObj);
        if (string.IsNullOrWhiteSpace(wishlogUrl))
        {
            throw new XunkongException($"Cannot find wishlog url of uid {uid}.");
        }
        long lastId = 0;
        using var context = DatabaseProvider.CreateContext();
        if (!getAll)
        {
            //lastId = dapper.QueryFirstOrDefault<long>("SELECT Id FROM WishlogItem WHERE Uid=@Uid ORDER BY Id DESC LIMIT 1;", uidObj);
            lastId = context.WishlogItem.AsNoTracking().Where(x => x.Uid == uid).OrderByDescending(x => x.Id).Select(x => x.Id).FirstOrDefault();
        }
        var progressHanler = new Progress<(WishType QueryType, int Page)>((param) => progress?.Report($"正在获取 {param.QueryType.ToDescription()} 第 {param.Page} 页"));
        var wishlogs = await _wishlogClient.GetAllWishlogAsync(wishlogUrl, lastId, progress: progressHanler);
        progress?.Report("正在写入数据库");
        await Task.Delay(100);
        //var oldCount = dapper.QuerySingleOrDefault<int>("SELECT COUNT(*) FROM WishlogItem WHERE Uid=@Uid;", uidObj);
        var oldIds = context.WishlogItem.AsNoTracking().Where(x => x.Uid == uid).Select(x => x.Id).ToList();
        var insertItems = wishlogs.ExceptBy(oldIds, x => x.Id).ToList();
        var updateItems = wishlogs.IntersectBy(oldIds, x => x.Id).ToList();
        context.AddRange(insertItems);
        context.UpdateRange(updateItems);
        context.SaveChanges();
        //dapper.Execute("""
        //    INSERT OR REPLACE INTO WishlogItem (Uid, Id, WishType, Time, Name, Language, ItemType, RankType, QueryType)
        //    VALUES (@Uid, @Id, @WishType, @Time, @Name, @Language, @ItemType, @RankType, @QueryType);
        //    """, wishlogs);
        //var newCount = dapper.QuerySingleOrDefault<int>("SELECT COUNT(*) FROM WishlogItem WHERE Uid=@Uid;", uidObj);
        return insertItems.Count;
    }





    /// <summary>
    /// 获取原神日志中祈愿记录网址的祈愿记录，返回新增条数
    /// </summary>
    /// <param name="isSea">首先从外服日志获取</param>
    /// <param name="getAll">是否获取全部数据</param>
    /// <returns></returns>
    /// <exception cref="HoyolabException"></exception>
    public async Task<int> GetWishlogsByLogFileAsync(bool isSea = false, IProgress<string>? progress = null, bool getAll = false)
    {
        var wishlogUrl = await FindWishlogUrlFromLogFileAsync(isSea);
        var uid = await GetUidByWishlogUrl(wishlogUrl);
        var uidObj = new { Uid = uid };
        long lastId = 0;
        //using var dapper = DatabaseProvider.CreateConnection();
        using var context = DatabaseProvider.CreateContext();
        if (!getAll)
        {
            //lastId = dapper.QueryFirstOrDefault<long>("SELECT Id FROM WishlogItem WHERE Uid=@Uid ORDER BY Id DESC LIMIT 1;", uidObj);
            lastId = context.WishlogItem.AsNoTracking().Where(x => x.Uid == uid).OrderByDescending(x => x.Id).Select(x => x.Id).FirstOrDefault();
        }
        var progressHanler = new Progress<(WishType QueryType, int Page)>((param) => progress?.Report($"正在获取 {param.QueryType.ToDescription()} 第 {param.Page} 页"));
        var wishlogs = await _wishlogClient.GetAllWishlogAsync(wishlogUrl, lastId, progress: progressHanler);
        progress?.Report("正在写入数据库");
        await Task.Delay(100);
        //var oldCount = dapper.QuerySingleOrDefault<int>("SELECT COUNT(*) FROM WishlogItem WHERE Uid=@Uid;", uidObj);
        var oldIds = context.WishlogItem.AsNoTracking().Where(x => x.Uid == uid).Select(x => x.Id).ToList();
        var insertItems = wishlogs.ExceptBy(oldIds, x => x.Id).ToList();
        var updateItems = wishlogs.IntersectBy(oldIds, x => x.Id).ToList();
        context.AddRange(insertItems);
        context.UpdateRange(updateItems);
        context.SaveChanges();
        //dapper.Execute("""
        //    INSERT OR REPLACE INTO WishlogItem (Uid, Id, WishType, Time, Name, Language, ItemType, RankType, QueryType)
        //    VALUES (@Uid, @Id, @WishType, @Time, @Name, @Language, @ItemType, @RankType, @QueryType);
        //    """, wishlogs);
        //var newCount = dapper.QuerySingleOrDefault<int>("SELECT COUNT(*) FROM WishlogItem WHERE Uid=@Uid;", uidObj);
        return insertItems.Count;
    }



    public int InsertWishlogItems(int uid, IEnumerable<WishlogItem> wishlogs)
    {
        using var context = DatabaseProvider.CreateContext();
        var oldIds = context.WishlogItem.AsNoTracking().Where(x => x.Uid == uid).Select(x => x.Id).ToList();
        var insertItems = wishlogs.ExceptBy(oldIds, x => x.Id).ToList();
        var updateItems = wishlogs.IntersectBy(oldIds, x => x.Id).ToList();
        context.AddRange(insertItems);
        context.UpdateRange(updateItems);
        context.SaveChanges();
        return insertItems.Count;
    }



    /// <summary>
    /// 获取本地数据库中所有的uid
    /// </summary>
    /// <returns></returns>
    public IEnumerable<int> GetAllUids()
    {
        using var dapper = DatabaseProvider.CreateConnection();
        return dapper.Query<int>("SELECT DISTINCT Uid FROM WishlogItem;");
    }


    /// <summary>
    /// 获取指定uid的本地祈愿记录数量
    /// </summary>
    /// <param name="uid"></param>
    /// <returns></returns>
    public int GetWishlogCount(int uid)
    {
        using var dapper = DatabaseProvider.CreateConnection();
        return dapper.QuerySingleOrDefault<int>("SELECT COUNT(*) FROM WishlogItem WHERE Uid=@Uid;", new { Uid = uid });
    }




    public List<WishlogItemEx> GetWishlogItemExByUid(int uid)
    {
        using var dapper = DatabaseProvider.CreateConnection();
        using var liteDb = DatabaseProvider.CreateLiteDB();
        var col = liteDb.GetCollection<WishEventInfo>();
        var items = dapper.Query<WishlogItem>("SELECT * FROM WishlogItem WHERE Uid=@Uid;", new { Uid = uid });
        var events = col.Query().OrderBy(x => x.Id).ToList();
        var models = items.Adapt<List<WishlogItemEx>>();
        var groups = models.GroupBy(x => x.QueryType);
        Parallel.ForEach(groups, group =>
        {
            int guaranteeIndex = 0;
            foreach (var item in group.OrderBy(x => x.Id))
            {
                guaranteeIndex++;
                item.GuaranteeIndex = guaranteeIndex;
                if (item.RankType == 5)
                {
                    guaranteeIndex = 0;
                }
            }
            var queryType = group.FirstOrDefault()?.QueryType;
            if (queryType is WishType.Novice or WishType.Permanent)
            {
                var eventName = queryType switch
                {
                    WishType.Novice => "新手祈愿",
                    WishType.Permanent => "常驻祈愿",
                    _ => "",
                };
                group.ToList().ForEach(x => x.WishEventName = eventName);
            }
            if (queryType is WishType.CharacterEvent or WishType.WeaponEvent)
            {
                var thisEvents = events.Where(x => x.QueryType == queryType).ToList();
                Parallel.ForEach(thisEvents, e =>
                {
                    var eventItems = group.Where(x => x.WishType == e.WishType && x.Time >= e.StartTime && x.Time <= e.EndTime).ToList();
                    foreach (var eventItem in eventItems)
                    {
                        eventItem.Version = e.Version;
                        eventItem.WishEventName = e.Name;
                        if (eventItem.RankType == 5 && e.Rank5UpItems.Contains(eventItem.Name))
                        {
                            eventItem.IsUp = true;
                        }
                        if (eventItem.RankType == 4 && e.Rank4UpItems.Contains(eventItem.Name))
                        {
                            eventItem.IsUp = true;
                        }
                    }
                });
            }
        });
        return models;
    }



}
