using System.Net.Http;
using Xunkong.ApiClient;
using Xunkong.ApiClient.Xunkong;
using Xunkong.Core;
using Xunkong.GenshinData.Character;
using Xunkong.GenshinData.Weapon;
using Xunkong.Hoyolab.Wishlog;

namespace Xunkong.Desktop.Services;

internal class XunkongApiService
{

    private readonly HttpClient _httpClient;

    private readonly XunkongApiClient _xunkongClient;

    private readonly WishlogService _wishlogService;

    private readonly BackupService _backupService;

    public XunkongApiService(HttpClient httpClient, XunkongApiClient xunkongClient, WishlogService wishlogService, BackupService backupService)
    {
        _httpClient = httpClient;
        _xunkongClient = xunkongClient;
        _wishlogService = wishlogService;
        _backupService = backupService;
    }




    #region Desktop


    public async Task<List<InfoBarContent>> GetInfoBarContentListAsync()
    {
        return await _xunkongClient.GetInfoBarContentListAsync();
    }


    #endregion



    #region Wishlog Backup



    private async Task<string> CheckWishlogUrlAsync(int uid, IProgress<string>? progress = null)
    {
        progress?.Report("检查祈愿记录网址的有效性");
        using var dapper = DatabaseProvider.CreateConnection();
        var auth = dapper.QueryFirstOrDefault<WishlogUrl>("SELECT * FROM WishlogUrl WHERE Uid=@Uid;", new { Uid = uid });
        if (auth is not null)
        {
            if (DateTimeOffset.Now < auth.DateTime + TimeSpan.FromDays(1))
            {
                return auth.Url;
            }
        }
        var isSea = uid.ToString().FirstOrDefault() > '5';
        var url = await _wishlogService.FindWishlogUrlFromLogFileAsync(isSea);
        var newUid = await _wishlogService.GetUidByWishlogUrl(url);
        if (uid == newUid)
        {
            return url;
        }
        throw new XunkongException($"Wishlog url of uid {uid} is expired or not found.");
    }




    public async Task<WishlogBackupResult> GetWishlogBackupLastItemAsync(int uid, IProgress<string>? progress = null)
    {
        var url = await CheckWishlogUrlAsync(uid);
        var model = new WishlogBackupRequest { Uid = uid, Url = url };
        progress?.Report("查询云端的最新记录");
        var result = await _xunkongClient.GetWishlogLastItemFromCloudAsync(model);
        progress?.Report($"云端现有 {result.CurrentCount} 条记录");
        return result;
    }



    public async Task<WishlogBackupResult> GetWishlogBackupListAsync(int uid, IProgress<string>? progress = null, bool getAll = false)
    {
        var url = await CheckWishlogUrlAsync(uid, progress);
        using var dapper = DatabaseProvider.CreateConnection();
        var uidObj = new { Uid = uid };
        long lastId = 0;
        if (!getAll)
        {
            lastId = dapper.QueryFirstOrDefault<long>("SELECT Id FROM WishlogItem WHERE Uid=@Uid ORDER BY Id DESC;", uidObj);
        }
        var model = new WishlogBackupRequest { Uid = uid, Url = url, LastId = lastId };
        progress?.Report("下载祈愿记录");
        var result = await _xunkongClient.GetWishlogListFromCloudAsync(model);
        var oldCount = dapper.QuerySingleOrDefault<int>("SELECT COUNT(*) FROM WishlogItem WHERE Uid=@Uid;", uidObj);
        if (result.List?.Any() ?? false)
        {
            progress?.Report("写入数据库");
            dapper.Execute("""
                 INSERT OR REPLACE INTO WishlogItem (Uid, Id, WishType, Time, Name, Language, ItemType, RankType, QueryType)
                 VALUES (@Uid, @Id, @WishType, @Time, @Name, @Language, @ItemType, @RankType, @QueryType);
                 """, result.List);
        }
        var newCount = dapper.QuerySingleOrDefault<int>("SELECT COUNT(*) FROM WishlogItem WHERE Uid=@Uid;", uidObj);
        result.PutCount = newCount - oldCount;
        progress?.Report($"本地新增 {result.PutCount} 条，本地现有 {newCount} 条，云端现有 {result.CurrentCount} 条");
        result.List = null;
        return result;
    }



    public async Task<string> GetWishlogAndBackupToLoalAsync(int uid, IProgress<string>? progress = null)
    {
        var url = await CheckWishlogUrlAsync(uid, progress);
        var model = new WishlogBackupRequest { Uid = uid, Url = url, LastId = 0 };
        progress?.Report("备份云端祈愿记录到本地");
        var result = await _xunkongClient.GetWishlogListFromCloudAsync(model);
        var backupFile = await _backupService.BackupWishlogItemsAsync(result.Uid, result.List!);
        return backupFile;
    }



    public async Task<WishlogBackupResult> PutWishlogListAsync(int uid, IProgress<string>? progress = null, bool putAll = false)
    {
        var url = await CheckWishlogUrlAsync(uid, progress);
        var model = new WishlogBackupRequest { Uid = uid, Url = url };
        long lastId = 0;
        WishlogBackupResult result = new(uid, 0, 0, 0, 0, null);
        if (!putAll)
        {
            progress?.Report("查询云端的最新记录");
            result = await _xunkongClient.GetWishlogLastItemFromCloudAsync(model);
            lastId = result.List?.LastOrDefault()?.Id ?? 0;
        }
        using var dapper = DatabaseProvider.CreateConnection();
        var list = dapper.Query<WishlogItem>("SELECT * FROM WishlogItem WHERE Uid=@Uid AND Id>@Id;", new { Uid = uid, Id = lastId });
        if (list.Any())
        {
            progress?.Report("上传祈愿记录");
            int addCount = 0;
            foreach (var chunk in list.Chunk(10000))
            {
                model.List = list;
                result = await _xunkongClient.PutWishlogListToCloudAsync(model);
                addCount += result.PutCount;
            }
        }
        progress?.Report($"上传 {result.PutCount} 条，云端现有 {result.CurrentCount} 条");
        return result;
    }


    public async Task<WishlogBackupResult> DeleteWishlogBackupAsync(int uid, IProgress<string>? progress = null)
    {
        var url = await CheckWishlogUrlAsync(uid, progress);
        var model = new WishlogBackupRequest { Uid = uid, Url = url };
        progress?.Report("发送删除请求");
        var result = await _xunkongClient.DeleteWishlogInCloudAsync(model);
        progress?.Report($"已删除 {result.Uid} 祈愿记录 {result.DeleteCount} 条");
        return result;
    }



    #endregion



    #region Genshin Data




    public async Task GetAllGenshinDataFromServerAsync(bool throwError = true)
    {
        try
        {
            await GetCharacterInfosFromServerAsync();
            await GetWeaponInfosFromServerAsync();
            await GetWishEventInfosFromServerAsync();
        }
        catch
        {
            if (throwError)
            {
                throw;
            }
        }
    }


    public async Task<IEnumerable<CharacterInfo>> GetCharacterInfosFromServerAsync()
    {
        var characterInfos = await _xunkongClient.GetCharacterInfosAsync();
        using var liteDb = DatabaseProvider.CreateLiteDB();
        var col = liteDb.GetCollection<CharacterInfo>();
        col.DeleteAll();
        col.Insert(characterInfos);
        return characterInfos;
    }


    public async Task<IEnumerable<WeaponInfo>> GetWeaponInfosFromServerAsync()
    {
        var weaponInfos = await _xunkongClient.GetWeaponInfosAsync();
        using var liteDb = DatabaseProvider.CreateLiteDB();
        var col = liteDb.GetCollection<WeaponInfo>();
        col.DeleteAll();
        col.Insert(weaponInfos);
        return weaponInfos;
    }


    public async Task<IEnumerable<WishEventInfo>> GetWishEventInfosFromServerAsync()
    {
        var wishEventInfos = await _xunkongClient.GetWishEventInfosAsync();
        using var liteDb = DatabaseProvider.CreateLiteDB();
        var col = liteDb.GetCollection<WishEventInfo>();
        col.DeleteAll();
        col.Insert(wishEventInfos);
        return wishEventInfos;
    }



    #endregion



    #region Genshin Wallpaper



    public async Task<WallpaperInfo> GetWallpaperByIdAsync(int id)
    {
        return await _xunkongClient.GetWallpaperByIdAsync(id);
    }


    public async Task<WallpaperInfo> GetRecommendWallpaperAsync()
    {
        return await _xunkongClient.GetRecommendWallpaperAsync();
    }



    public async Task<WallpaperInfo> GetRandomWallpaperAsync(int maxage = 0)
    {
        return await _xunkongClient.GetRandomWallpaperAsync(maxage);
    }


    public async Task<WallpaperInfo> GetNextWallpaperAsync(int lastId = 0)
    {
        return await _xunkongClient.GetNextWallpaperAsync(lastId);
    }


    public async Task<List<WallpaperInfo>> GetWallpaperInfoListAsync(int page, int size)
    {
        return await _xunkongClient.GetWallpaperListAsync(page, size);
    }


    public WallpaperInfo? GetPreparedWallpaper(bool throwError = false)
    {
        try
        {
            using var liteDb = DatabaseProvider.CreateLiteDB();
            var col = liteDb.GetCollection<WallpaperInfo>("RecommendWallpaper");
            var wallpaper = col.FindOne(_ => true);
            return wallpaper;
        }
        catch
        {
            if (throwError)
            {
                throw;
            }
            else
            {
                return null;
            }
        }
    }



    public async Task PrepareNextWallpaperAsync(int maxage = 0, bool throwError = false)
    {
        try
        {
            var wallpaper = await GetRandomWallpaperAsync(maxage);
            if (wallpaper is not null)
            {
                var file = CacheHelper.GetCacheFilePath(wallpaper.Url);
                if (!File.Exists(file))
                {
                    var bytes = await _httpClient.GetByteArrayAsync(wallpaper.Url);
                    Directory.CreateDirectory(Path.GetDirectoryName(file)!);
                    await File.WriteAllBytesAsync(file, bytes);
                }
                using var liteDb = DatabaseProvider.CreateLiteDB();
                var col = liteDb.GetCollection<WallpaperInfo>("RecommendWallpaper");
                col.DeleteAll();
                col.Insert(wallpaper);
            }
        }
        catch
        {
            if (throwError)
            {
                throw;
            }
        }
    }




    #endregion




}
