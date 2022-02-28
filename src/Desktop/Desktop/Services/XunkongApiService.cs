using Xunkong.Core.Metadata;
using Xunkong.Core.Wish;
using Xunkong.Core.XunkongApi;

namespace Xunkong.Desktop.Services
{


    public class XunkongApiService
    {

        private readonly ILogger<XunkongApiService> _logger;

        private readonly XunkongApiClient _xunkongClient;

        private readonly IDbContextFactory<XunkongDbContext> _ctxFactory;

        private readonly DbConnectionFactory<SqliteConnection> _cntFactory;

        private readonly WishlogService _wishlogService;

        private readonly BackupService _backupService;

        public XunkongApiService(ILogger<XunkongApiService> logger,
                                 XunkongApiClient xunkongClient,
                                 IDbContextFactory<XunkongDbContext> dbContextFactory,
                                 DbConnectionFactory<SqliteConnection> dbConnectionFactory,
                                 WishlogService wishlogService,
                                 BackupService backupService)
        {
            _logger = logger;
            _xunkongClient = xunkongClient;
            _ctxFactory = dbContextFactory;
            _cntFactory = dbConnectionFactory;
            _wishlogService = wishlogService;
            _backupService = backupService;
        }


        #region Desktop Version


        public async Task<DesktopUpdateVersion> CheckUpdateAsync(ChannelType channel)
        {
            return await _xunkongClient.CheckDesktopUpdateAsync(channel);
        }


        public async Task<DesktopChangelog> GetChangelogAsync(Version version)
        {
            return await _xunkongClient.GetDesktopChangelogAsync(version);
        }



        public async Task<bool> HasUnreadNotification()
        {
            using var cnt = _cntFactory.CreateDbConnection();
            var notification = await cnt.QueryFirstOrDefaultAsync<int>("SELECT Id FROM Notifications WHERE HasRead=FALSE LIMIT 1;");
            return notification != 0;
        }



        public async Task<bool> GetNotificationsAsync(ChannelType channel, Version version)
        {
            using var ctx = _ctxFactory.CreateDbContext();
            var lastId = await ctx.NotificationItems.OrderByDescending(x => x.Id).Select(x => x.Id).FirstOrDefaultAsync();
            var wrapper = await _xunkongClient.GetNotificationsAsync<NotificationDesktopModel>(channel, version, lastId);
            var list = wrapper.List;
            if (list.Any())
            {
                var existList = await ctx.NotificationItems.AsNoTracking().Select(x => x.Id).ToListAsync();
                var addList = list.ExceptBy(existList, x => x.Id);
                if (addList.Any())
                {
                    ctx.AddRange(addList);
                    await ctx.SaveChangesAsync();
                    return true;
                }
            }
            return false;
        }


        #endregion



        #region Wishlog Backup



        private async Task<string> CheckWishlogAuthkey(int uid, Action<string>? progressHandler = null)
        {
            _logger.LogDebug("Start chech wishlog authkey with uid {Uid}.", uid);
            progressHandler?.Invoke("检查祈愿记录网址的有效性");
            using var ctx = _ctxFactory.CreateDbContext();
            var auth = await ctx.WishlogAuthkeys.AsNoTracking().FirstOrDefaultAsync(x => x.Uid == uid);
            if (auth is not null)
            {
                if (DateTimeOffset.Now < auth.DateTime + TimeSpan.FromDays(1))
                {
                    _logger.LogDebug("Wishlog url of uid {Uid} is available.", uid);
                    return auth.Url;
                }
            }
            _logger.LogDebug("Wishlog url of uid {Uid} is expired or not found.", uid);
            var isSea = uid.ToString().FirstOrDefault() > '5';
            _logger.LogDebug("Start finding wishlog url of uid {Uid} from genshin log file.", uid);
            var url = await _wishlogService.FindWishlogUrlFromLogFileAsync(isSea);
            var newUid = await _wishlogService.GetUidByWishlogUrl(url);
            if (uid == newUid)
            {
                _logger.LogDebug("Wishlog url of uid {Uid} from genshin log file was found.", uid);
                return url;
            }
            _logger.LogDebug("Didn't find wishlog url of uid {Uid} from genshin log file.", uid);
            throw new XunkongException(ErrorCode.UidNotFound, $"Wishlog url of uid {uid} is expired or not found.");
        }




        public async Task<WishlogCloudBackupResult> GetWishlogBackupLastItemAsync(int uid, Action<string>? progressHandler = null)
        {
            _logger.LogInformation("Start getting wishlog backup last item wish uid {Uid}.", uid);
            var url = await CheckWishlogAuthkey(uid);
            _logger.LogInformation("Pass checking authkey of uid {Uid}.", uid);
            var model = new WishlogCloudBackupRequestModel { Uid = uid, Url = url };
            progressHandler?.Invoke("查询云端的最新记录");
            var result = await _xunkongClient.GetWishlogLastItemFromCloudAsync(model);
            progressHandler?.Invoke($"云端现有 {result.CurrentCount} 条记录");
            return result;
        }



        public async Task<WishlogCloudBackupResult> GetWishlogBackupListAsync(int uid, Action<string>? progressHandler = null, bool getAll = false)
        {
            _logger.LogInformation("Start getting wishlog backup list wish uid {Uid}, get all {getAll}.", uid, getAll);
            var url = await CheckWishlogAuthkey(uid, progressHandler);
            _logger.LogInformation("Pass checking authkey of uid {Uid}.", uid);
            long lastId = 0;
            using var cnt = _cntFactory.CreateDbConnection();
            if (!getAll)
            {
                lastId = await cnt.QueryFirstOrDefaultAsync<long>($"SELECT Id FROM Wishlog_Items WHERE Uid={uid} ORDER BY Id DESC;");
                _logger.LogDebug("The last wishlog id of uid {Uid} is {Id}.", uid, lastId);
            }
            var model = new WishlogCloudBackupRequestModel { Uid = uid, Url = url, LastId = lastId };
            progressHandler?.Invoke("下载祈愿记录");
            var result = await _xunkongClient.GetWishlogListFromCloudAsync(model);
            _logger.LogInformation("Wishlog backup result: Uid {Uid}, Current {CurrentCount}, Get {GetCount}, Put {PutCount}, Delete {DeleteCount}.", result);
            if (result.List?.Any() ?? false)
            {
                progressHandler?.Invoke("写入数据库");
                var existing = await cnt.QueryAsync<long>($"SELECT Id FROM Wishlog_Items WHERE Uid={uid};");
                var inserting = result.List.ExceptBy(existing, x => x.Id).ToList();
                result.PutCount = inserting.Count;
                if (inserting.Any())
                {
                    var insertCount = await cnt.ExecuteAsync("INSERT INTO Wishlog_Items (Uid, Id, WishType, Time, Name, Language, ItemType, RankType, QueryType) VALUES (@Uid, @Id, @WishType, @Time, @Name, @Language, @ItemType, @RankType, @QueryType)", inserting);
                    _logger.LogDebug("Inserted {InsertCount} wishlog items of uid {Uid} to database.", insertCount, uid);
                }
            }
            var localCount = await cnt.QueryFirstOrDefaultAsync<int>($"SELECT COUNT(*) FROM Wishlog_Items WHERE Uid={uid}");
            progressHandler?.Invoke($"本地新增 {result.PutCount} 条，本地现有 {localCount} 条，云端现有 {result.CurrentCount} 条");
            result.List = null;
            return result;
        }



        public async Task<string> GetWishlogAndBackupToLoalAsync(int uid, Action<string>? progressHandler = null)
        {
            _logger.LogInformation("Start getting wishlog backup list and backup to local wish uid {Uid}, get all {getAll}.", uid);
            var url = await CheckWishlogAuthkey(uid, progressHandler);
            _logger.LogInformation("Pass checking authkey of uid {Uid}.", uid);
            var model = new WishlogCloudBackupRequestModel { Uid = uid, Url = url, LastId = 0 };
            progressHandler?.Invoke("备份云端祈愿记录到本地");
            var result = await _xunkongClient.GetWishlogListFromCloudAsync(model);
            _logger.LogInformation("Wishlog backup result: Uid {Uid}, Current {CurrentCount}, Get {GetCount}, Put {PutCount}, Delete {DeleteCount}.", result);
            var backupFile = await _backupService.BackupWishlogItemsAsync(result.Uid, result.List!);
            return backupFile;
        }



        public async Task<WishlogCloudBackupResult> PutWishlogListAsync(int uid, Action<string>? progressHandler = null, bool putAll = false)
        {
            _logger.LogInformation("Start putting wishlog backup list wish uid {Uid}, put all {putAll}.", uid, putAll);
            var url = await CheckWishlogAuthkey(uid, progressHandler);
            _logger.LogInformation("Pass checking authkey of uid {Uid}.", uid);
            var model = new WishlogCloudBackupRequestModel { Uid = uid, Url = url };
            long lastId = 0;
            WishlogCloudBackupResult result = new(uid, 0, 0, 0, 0, null);
            if (!putAll)
            {
                progressHandler?.Invoke("查询云端的最新记录");
                result = await _xunkongClient.GetWishlogLastItemFromCloudAsync(model);
                lastId = result.List?.LastOrDefault()?.Id ?? 0;
            }
            using var ctx = _ctxFactory.CreateDbContext();
            var list = await ctx.WishlogItems.AsNoTracking().Where(x => x.Uid == uid && x.Id > lastId).ToListAsync();
            if (list.Any())
            {
                _logger.LogInformation("{Count} wishlog items of uid {Uid} need to backup.", list.Count, uid);
                progressHandler?.Invoke("上传祈愿记录");
                int addCount = 0;
                foreach (var chunk in list.Chunk(10000))
                {
                    model.List = list;
                    result = await _xunkongClient.PutWishlogListToCloudAsync(model);
                    addCount += result.PutCount;
                    _logger.LogInformation("Wishlog backup result: Uid {Uid}, Current {CurrentCount}, Get {GetCount}, Put {PutCount}, Delete {DeleteCount}.", result);
                }
            }
            progressHandler?.Invoke($"上传 {result.PutCount} 条，云端现有 {result.CurrentCount} 条");
            return result;
        }


        public async Task<WishlogCloudBackupResult> DeleteWishlogBackupAsync(int uid, Action<string>? progressHandler = null)
        {
            _logger.LogInformation("Start deleting wishlog backup wish uid {Uid}", uid);
            var url = await CheckWishlogAuthkey(uid, progressHandler);
            _logger.LogInformation("Pass checking authkey of uid {Uid}", uid);
            var model = new WishlogCloudBackupRequestModel { Uid = uid, Url = url };
            progressHandler?.Invoke("发送删除请求");
            var result = await _xunkongClient.DeleteWishlogInCloudAsync(model);
            _logger.LogInformation("Wishlog backup result: Uid {Uid}, Current {CurrentCount}, Get {GetCount}, Put {PutCount}, Delete {DeleteCount}.", result);
            progressHandler?.Invoke($"已删除 {result.Uid} 祈愿记录 {result.DeleteCount} 条");
            return result;
        }







        #endregion



        #region Genshin Metadata


        public async Task<IEnumerable<CharacterInfo>> GetCharacterInfosFromServerAsync()
        {
            var characterInfos = await _xunkongClient.GetCharacterInfosAsync();
            using var ctx = _ctxFactory.CreateDbContext();
            var ids = characterInfos.Select(x => x.Id).ToList();
            using var t = ctx.Database.BeginTransaction();
            try
            {
                await ctx.Database.ExecuteSqlRawAsync($"DELETE FROM Info_Character WHERE Id IN ({string.Join(",", ids)});");
                ctx.AddRange(characterInfos);
                await ctx.SaveChangesAsync();
                await t.CommitAsync();
            }
            catch (Exception ex)
            {
                await t.RollbackAsync();
                throw;
            }
            return characterInfos;
        }


        public async Task<IEnumerable<WeaponInfo>> GetWeaponInfosFromServerAsync()
        {
            var weaponInfos = await _xunkongClient.GetWeaponInfosAsync();
            using var ctx = _ctxFactory.CreateDbContext();
            var ids = weaponInfos.Select(x => x.Id).ToList();
            using var t = ctx.Database.BeginTransaction();
            try
            {
                await ctx.Database.ExecuteSqlRawAsync($"DELETE FROM Info_Weapon WHERE Id IN ({string.Join(",", ids)});");
                ctx.AddRange(weaponInfos);
                await ctx.SaveChangesAsync();
                await t.CommitAsync();
            }
            catch (Exception ex)
            {
                await t.RollbackAsync();
                throw;
            }
            return weaponInfos;
        }


        public async Task<IEnumerable<WishEventInfo>> GetWishEventInfosFromServerAsync()
        {
            var wishEventInfos = await _xunkongClient.GetWishEventInfosAsync();
            using var ctx = _ctxFactory.CreateDbContext();
            var ids = wishEventInfos.Select(x => x.Id).ToList();
            using var t = ctx.Database.BeginTransaction();
            try
            {
                await ctx.Database.ExecuteSqlRawAsync($"DELETE FROM Info_WishEvent WHERE Id IN ({string.Join(",", ids)});");
                ctx.AddRange(wishEventInfos);
                await ctx.SaveChangesAsync();
                await t.CommitAsync();
            }
            catch (Exception ex)
            {
                await t.RollbackAsync();
                throw;
            }
            return wishEventInfos;
        }


        public async Task<IEnumerable<I18nModel>> GetI18nModelsFromServerAsync()
        {
            var i18ns = await _xunkongClient.GetI18nModelsAsync();
            using var ctx = _ctxFactory.CreateDbContext();
            var ids = i18ns.Select(x => x.Id).ToList();
            using var t = ctx.Database.BeginTransaction();
            try
            {
                await ctx.Database.ExecuteSqlRawAsync($"DELETE FROM i18n WHERE Id IN ({string.Join(",", ids)});");
                ctx.AddRange(i18ns);
                await ctx.SaveChangesAsync();
                await t.CommitAsync();
            }
            catch (Exception ex)
            {
                await t.RollbackAsync();
                throw;
            }
            return i18ns;
        }



        #endregion



        #region Genshin Wallpaper


        public async Task<WallpaperInfo?> GetWallpaperInfoAsync(int randomOrNext, int excludeId = 0)
        {
            if (randomOrNext == 0)
            {
                return await _xunkongClient.GetRandomWallpaperAsync(excludeId);
            }
            else
            {
                return await _xunkongClient.GetNextWallpaperAsync(excludeId);
            }
        }


        #endregion


    }
}
