using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Xunkong.Core.Metadata;
using Xunkong.Core.Wish;
using Xunkong.Core.XunkongApi;

namespace Xunkong.Desktop.Services
{

    [InjectService]
    public class XunkongApiService
    {


        private readonly ILogger<XunkongApiService> _logger;

        private readonly XunkongApiClient _xunkongClient;

        private readonly IDbContextFactory<XunkongDbContext> _dbContextFactory;

        private readonly DbConnectionFactory<SqliteConnection> _dbConnectionFactory;

        private readonly WishlogService _wishlogService;

        public XunkongApiService(ILogger<XunkongApiService> logger,
                                 XunkongApiClient xunkongClient,
                                 IDbContextFactory<XunkongDbContext> dbContextFactory,
                                 DbConnectionFactory<SqliteConnection> dbConnectionFactory,
                                 WishlogService wishlogService)
        {
            _logger = logger;
            _xunkongClient = xunkongClient;
            _dbContextFactory = dbContextFactory;
            _dbConnectionFactory = dbConnectionFactory;
            _wishlogService = wishlogService;
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
            using var cnt = _dbConnectionFactory.CreateDbConnection();
            var notification = await cnt.QueryFirstOrDefaultAsync<int>("SELECT Id FROM Notifications WHERE HasRead=FALSE LIMIT 1;");
            return notification != 0;
        }



        public async Task<bool> GetNotificationsAsync(ChannelType channel, Version version)
        {
            using var ctx = _dbContextFactory.CreateDbContext();
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



        private async Task<string> CheckWishlogAuthkey(int uid)
        {
            _logger.LogDebug("Start chech wishlog authkey with uid {Uid}.", uid);
            using var ctx = _dbContextFactory.CreateDbContext();
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
            var isSea = uid.ToString()[0] switch
            {
                > '5' => true,
                _ => false,
            };
            _logger.LogDebug("Start finding wishlog url of uid {Uid} from genshin log file.", uid);
            var url = await _wishlogService.FindWishlogUrlFromLogFileAsync(isSea);
            var newUid = await _wishlogService.GetUidByWishlogUrl(url);
            if (uid == newUid)
            {
                _logger.LogDebug("Wishlog url of uid {Uid} from genshin log file was found.", uid);
                return url;
            }
            _logger.LogDebug("Didn't find wishlog url of uid {Uid} from genshin log file.", uid);
            throw new XunkongException(ErrorCode.UidNotFound, $"Wishlog url of uid {uid} not found.");
        }




        public async Task<WishlogBackupResult> GetWishlogBackupLastItemAsync(int uid)
        {
            _logger.LogInformation("Start getting wishlog backup last item wish uid {Uid}.", uid);
            var url = await CheckWishlogAuthkey(uid);
            _logger.LogInformation("Pass checking authkey of uid {Uid}.", uid);
            var model = new WishlogBackupRequestModel { Uid = uid, Url = url };
            var result = await _xunkongClient.GetWishlogBackupLastItemAsync(model);
            return result;
        }



        public async Task<WishlogBackupResult> GetWishlogBackupListAsync(int uid, bool getAll = false)
        {
            _logger.LogInformation("Start getting wishlog backup list wish uid {Uid}, get all {getAll}.", uid, getAll);
            var url = await CheckWishlogAuthkey(uid);
            _logger.LogInformation("Pass checking authkey of uid {Uid}.", uid);
            long lastId = 0;
            using var cnt = _dbConnectionFactory.CreateDbConnection();
            if (!getAll)
            {
                lastId = await cnt.QueryFirstOrDefaultAsync<long>($"SELECT Id FROM Wishlog_Items WHERE Uid={uid} ORDER BY Id DESC;");
                _logger.LogDebug("The last wishlog id of uid {Uid} is {Id}.", uid, lastId);
            }
            var model = new WishlogBackupRequestModel { Uid = uid, Url = url, LastId = lastId };
            var result = await _xunkongClient.GetWishlogBackupListAsync(model);
            _logger.LogInformation("Wishlog backup result: Uid {Uid}, Current {CurrentCount}, Get {GetCount}, Put {PutCount}, Delete {DeleteCount}.", result);
            if (result.List?.Any() ?? false)
            {
                var existing = await cnt.QueryAsync<long>($"SELECT Id FROM Wishlog_Items WHERE Uid={uid};");
                var inserting = result.List.ExceptBy(existing, x => x.Id).ToList();
                if (inserting.Any())
                {
                    var insertCount = await cnt.ExecuteAsync("INSERT INTO Wishlog_Items (Uid, Id, WishType, Time, Name, Language, ItemType, RankType, QueryType) VALUES (@Uid, @Id, @WishType, @Time, @Name, @Language, @ItemType, @RankType, @QueryType)", inserting);
                    _logger.LogDebug("Inserted {InsertCount} wishlog items of uid {Uid} to database.", insertCount, uid);
                }
            }
            result.List = null;
            return result;
        }



        public async Task<WishlogBackupResult> PutWishlogListAsync(int uid, bool putAll = false)
        {
            _logger.LogInformation("Start putting wishlog backup list wish uid {Uid}, put all {putAll}.", uid, putAll);
            var url = await CheckWishlogAuthkey(uid);
            _logger.LogInformation("Pass checking authkey of uid {Uid}.", uid);
            var model = new WishlogBackupRequestModel { Uid = uid, Url = url };
            long lastId = 0;
            WishlogBackupResult result = new(uid, 0, 0, 0, 0, null);
            if (!putAll)
            {
                result = await _xunkongClient.GetWishlogBackupLastItemAsync(model);
                lastId = result.List?.LastOrDefault()?.Id ?? 0;
            }
            using var ctx = _dbContextFactory.CreateDbContext();
            var list = await ctx.WishlogItems.AsNoTracking().Where(x => x.Uid == uid && x.Id > lastId).ToListAsync();
            if (list.Any())
            {
                _logger.LogInformation("{Count} wishlog items of uid {Uid} need to backup.", list.Count, uid);
                foreach (var chunk in list.Chunk(10000))
                {
                    model.List = list;
                    result = await _xunkongClient.PutWishlogListAsync(model);
                    _logger.LogInformation("Wishlog backup result: Uid {Uid}, Current {CurrentCount}, Get {GetCount}, Put {PutCount}, Delete {DeleteCount}.", result);
                }
            }
            else
            {
                _logger.LogInformation("Don't need to backup wishlog items of uid {Uid}.", uid);
                throw new XunkongException(ErrorCode.Ok, $"Don't need to backup wishlog items of uid {uid}.");
            }
            return result;
        }


        public async Task<WishlogBackupResult> DeleteWishlogBackupAsync(int uid)
        {
            _logger.LogInformation("Start deleting wishlog backup wish uid {Uid}", uid);
            var url = await CheckWishlogAuthkey(uid);
            _logger.LogInformation("Pass checking authkey of uid {Uid}", uid);
            var model = new WishlogBackupRequestModel { Uid = uid, Url = url };
            var result = await _xunkongClient.DeleteWishlogBackupAsync(model);
            _logger.LogInformation("Wishlog backup result: Uid {Uid}, Current {CurrentCount}, Get {GetCount}, Put {PutCount}, Delete {DeleteCount}.", result);
            return result;
        }







        #endregion



        #region Genshin Metadata


        public async Task<IEnumerable<CharacterInfo>> GetCharacterInfosFromServerAsync()
        {
            var characterInfos = await _xunkongClient.GetCharacterInfosAsync();
            using var ctx = _dbContextFactory.CreateDbContext();
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
            using var ctx = _dbContextFactory.CreateDbContext();
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
            using var ctx = _dbContextFactory.CreateDbContext();
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



        #endregion


        #region Genshin Wallpaper


        public async Task<WallpaperInfo?> GetWallpaperInfoAsync(int excludeId = 0)
        {
            return await _xunkongClient.GetWallpaperInfoAsync(excludeId);
        }


        #endregion


    }
}
