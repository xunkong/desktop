using Mapster;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using System.Diagnostics;
using System.Reflection.PortableExecutable;
using System.Text.RegularExpressions;
using Xunkong.Core.Hoyolab;
using Xunkong.Core.Metadata;
using Xunkong.Core.Wish;
using Xunkong.Core.XunkongApi;

namespace Xunkong.Desktop.Services
{

    [InjectService]
    public class WishlogService
    {

        private readonly ILogger<WishlogService> _logger;

        private readonly HttpClient _httpClient;

        private readonly WishlogClient _wishlogClient;

        private readonly IDbContextFactory<XunkongDbContext> _dbContextFactory;

        private readonly DbConnectionFactory<SqliteConnection> _dbConnectionFactory;

        private readonly BackupService _backupService;

        private static readonly string UserProfile = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);

        private static readonly string LogFile_Cn = Path.Combine(UserProfile, @"AppData\LocalLow\miHoYo\原神\output_log.txt");

        private static readonly string LogFile_Sea = Path.Combine(UserProfile, @"AppData\LocalLow\miHoYo\Genshin Impact\output_log.txt");


        public WishlogService(ILogger<WishlogService> logger,
                              HttpClient httpClient,
                              WishlogClient wishlogClient,
                              IDbContextFactory<XunkongDbContext> dbContextFactory,
                              DbConnectionFactory<SqliteConnection> dbConnectionFactory,
                              BackupService backupService)
        {
            _logger = logger;
            _httpClient = httpClient;
            _wishlogClient = wishlogClient;
            _dbContextFactory = dbContextFactory;
            _dbConnectionFactory = dbConnectionFactory;
            _backupService = backupService;
        }


        static WishlogService()
        {
            TypeAdapterConfig<CharacterInfo, WishEventStatsItemInfoModel>.NewConfig().Map(dest => dest.Icon, src => src.FaceIcon).Map(dest => dest.GachaCard, src => src.GachaCard);
            TypeAdapterConfig<WeaponInfo, WishEventStatsItemInfoModel>.NewConfig().Map(dest => dest.Icon, src => src.Icon).Map(dest => dest.GachaCard, src => src.GachaIcon);
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
                '1' => RegionType.GF,
                '2' => RegionType.GF,
                '3' => RegionType.GF,
                '4' => RegionType.GF,
                '5' => RegionType.QD,
                '6' => RegionType.US,
                '7' => RegionType.EU,
                '8' => RegionType.ASIA,
                '9' => RegionType.CHT,
                _ => RegionType.None,
            };
        }




        /// <summary>
        /// 获取原神日志中的祈愿记录网址
        /// </summary>
        /// <param name="isSea"></param>
        /// <returns></returns>
        /// <exception cref="FileNotFoundException"></exception>
        public async Task<string> FindWishlogUrlFromLogFileAsync(bool isSea = false)
        {
            _logger.LogDebug("Start finding wishlog url, (is sea? {IsSea}).", isSea);
            var file = isSea ? LogFile_Sea : LogFile_Cn;
            if (!File.Exists(file))
            {
                file = isSea ? LogFile_Cn : LogFile_Sea;
            }
            if (!File.Exists(file))
            {
                _logger.LogDebug("Don't find genshin log file.");
                throw new FileNotFoundException("没有找到日志文件，请打开游戏内的祈愿记录界面。");
            }
            using var stream = File.Open(file, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
            using var reader = new StreamReader(stream);
            var log = await reader.ReadToEndAsync();
            var matches = Regex.Matches(log, @"OnGetWebViewPageFinish:(.+#/log)");
            if (matches.Any())
            {
                _logger.LogDebug("Found wishlog url, (is sea? {IsSea}).", isSea);
                return matches.Last().Value.Replace("OnGetWebViewPageFinish:", "");
            }
            else
            {
                _logger.LogDebug("Don't find wishlog url.");
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
                _logger.LogDebug("The wishlog url doesn't have any wishlog.");
                throw new HoyolabException(-1, "该网址没有对应的祈愿记录");
            }
            using var cnt = _dbConnectionFactory.CreateDbConnection();
            var model = new WishlogAuthkeyItem { Uid = uid, Url = wishlogUrl, DateTime = DateTimeOffset.Now };
            await cnt.ExecuteAsync("INSERT OR REPLACE INTO Wishlog_Authkeys (Uid,Url,DateTime) VALUES(@Uid,@Url,@DateTime);", model);
            return uid;
        }




        public async Task<bool> CheckWishlogAuthkeyExpiredAsync(int uid)
        {
            using var ctx = _dbContextFactory.CreateDbContext();
            var model = await ctx.WishlogAuthkeys.Where(x => x.Uid == uid).FirstOrDefaultAsync();
            if (model == null)
            {
                return false;
            }
            else
            {
                return model.DateTime + TimeSpan.FromDays(1) > DateTimeOffset.Now;
            }
        }



        public async Task<bool> CheckWishlogUrlTimeoutAsync(int uid)
        {
            using var ctx = _dbContextFactory.CreateDbContext();
            var model = await ctx.WishlogAuthkeys.Where(x => x.Uid == uid).FirstOrDefaultAsync();
            if (model == null)
            {
                return false;
            }
            try
            {
                var newUid = await GetUidByWishlogUrl(model.Url);
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



        public async Task<int> GetWishlogByUidAsync(int uid, EventHandler<(WishType WishType, int Page)>? onProgressChanged = null, bool getAll = false)
        {
            using var ctx = _dbContextFactory.CreateDbContext();
            var wishlogUrl = await ctx.WishlogAuthkeys.Where(x => x.Uid == uid).Select(x => x.Url).FirstOrDefaultAsync();
            if (string.IsNullOrWhiteSpace(wishlogUrl))
            {
                _logger.LogError($"Cannot find wishlog url of uid {uid}.");
                throw new XunkongException(ErrorCode.InternalException, $"Cannot find wishlog url of uid {uid}.");
            }
            long lastId = 0;
            if (!getAll)
            {
                lastId = await ctx.WishlogItems.Where(x => x.Uid == uid).OrderByDescending(x => x.Id).Select(x => x.Id).FirstOrDefaultAsync();
            }
            _logger.LogDebug("Start getting wishlog with uid {Uid} and (get all? {GetAll})", uid, getAll);
            var client = new WishlogClient(_httpClient);
            client.ProgressChanged += onProgressChanged;
            var wishlogs = await client.GetAllWishlogAsync(wishlogUrl, lastId);
            var existList = await ctx.WishlogItems.AsNoTracking().Where(x => x.Uid == uid).Select(x => x.Id).ToListAsync();
            var addList = wishlogs.ExceptBy(existList, x => x.Id).ToList();
            ctx.AddRange(addList);
            await ctx.SaveChangesAsync();
            _logger.LogInformation("Add {Count} wishlog items to uid {Uid}", addList.Count, uid);
            return addList.Count;
        }





        /// <summary>
        /// 获取原神日志中祈愿记录网址的祈愿记录，返回新增条数
        /// </summary>
        /// <param name="isSea"></param>
        /// <param name="getAll"></param>
        /// <returns></returns>
        /// <exception cref="HoyolabException"></exception>
        public async Task<int> GetWishlogsByLogFileAsync(bool isSea = false, bool getAll = false)
        {
            var wishlogUrl = await FindWishlogUrlFromLogFileAsync(isSea);
            var uid = await GetUidByWishlogUrl(wishlogUrl);
            long lastId = 0;
            if (!getAll)
            {
                using var cnt = _dbConnectionFactory.CreateDbConnection();
                lastId = await cnt.QueryFirstOrDefaultAsync<long>("SELECT Id FROM Wishlog_Items WHERE Uid=@Uid ORDER BY Id DESC;", new { Uid = uid });
            }
            _logger.LogDebug("Start getting wishlog with uid {Uid} and (get all? {GetAll})", uid, getAll);
            var wishlogs = await _wishlogClient.GetAllWishlogAsync(wishlogUrl, lastId);
            using var ctx = _dbContextFactory.CreateDbContext();
            var existList = await ctx.WishlogItems.AsNoTracking().Where(x => x.Uid == uid).Select(x => x.Id).ToListAsync();
            var addList = wishlogs.ExceptBy(existList, x => x.Id).ToList();
            ctx.AddRange(addList);
            await ctx.SaveChangesAsync();
            _logger.LogInformation("Add {Count} wishlog items to uid {Uid}", addList.Count, uid);
            return addList.Count;
        }




        public async Task<List<int>> GetAllUidsAsync()
        {
            using var ctx = _dbContextFactory.CreateDbContext();
            return await ctx.WishlogItems.Select(x => x.Uid).Distinct().ToListAsync();
        }




        public async Task<WishlogPanelModel> GetWishlogPanelModelAsync(int uid)
        {
            var model = new WishlogPanelModel { Uid = uid };
            using var ctx = _dbContextFactory.CreateDbContext();
            model.NickName = await ctx.UserGameRoleInfos.Where(x => x.Uid == uid).Select(x => x.Nickname).FirstOrDefaultAsync();
            model.LastUpdateTime = await ctx.WishlogAuthkeys.Where(x => x.Uid == uid).Select(x => x.DateTime).FirstOrDefaultAsync();
            model.WishlogCount = await ctx.WishlogItems.Where(x => x.Uid == uid).CountAsync();
            var list = (await GetWishlogItemEditableModelsByUidAsync(uid)).OrderByDescending(x => x.Id).ToList();
            foreach (var type in new[] { WishType.CharacterEvent, WishType.WeaponEvent, WishType.Permanent })
            {
                var stats = new WishlogPanelWishTypeStatsModel { WishType = type };
                model.WishTypeStats.Add(stats);
                stats.TotalCount = list.Where(x => x.QueryType == type).Count();
                if (stats.TotalCount == 0)
                {
                    continue;
                }
                stats.Rank5Count = list.Where(x => x.QueryType == type && x.RankType == 5).Count();
                stats.Rank4Count = list.Where(x => x.QueryType == type && x.RankType == 4).Count();
                stats.Rank5Guarantee = list.Where(x => x.QueryType == type).FirstOrDefault()?.GuaranteeIndex ?? 0;
                stats.Rank4Guarantee = list.Where(x => x.QueryType == type).TakeWhile(x => x.RankType < 4).Count();
                var lastRank5Item = list.Where(x => x.QueryType == type && x.RankType == 5).FirstOrDefault();
                if (lastRank5Item is not null)
                {
                    stats.LastRank5ItemName = lastRank5Item.Name;
                    stats.LastRank5ItemGuaranteeIndex = lastRank5Item.GuaranteeIndex;
                    stats.LastRank5ItemTime = lastRank5Item.Time;
                    if (lastRank5Item.ItemType == "角色")
                    {
                        stats.LastRank5ItemIcon = await ctx.CharacterInfos.Where(x => x.Name == lastRank5Item.Name).Select(x => x.SideIcon).FirstOrDefaultAsync();
                    }
                    else
                    {
                        stats.LastRank5ItemIcon = await ctx.WeaponInfos.Where(x => x.Name == lastRank5Item.Name).Select(x => x.Icon).FirstOrDefaultAsync();
                    }
                }
            }
            return model;
        }














        public async Task<List<WishlogItemEditableModel>> GetWishlogItemEditableModelsByUidAsync(int uid)
        {
            using var ctx = _dbContextFactory.CreateDbContext();
            var items = await ctx.WishlogItems.Where(x => x.Uid == uid).ToListAsync();
            var models = items.Adapt<List<WishlogItemEditableModel>>();
            var groups = models.GroupBy(x => x.QueryType);
            foreach (var group in groups)
            {
                int guaranteeIndex = 0;
                foreach (var model in group.OrderBy(x => x.Id))
                {
                    guaranteeIndex++;
                    model.GuaranteeIndex = guaranteeIndex;
                    if (model.RankType == 5)
                    {
                        guaranteeIndex = 0;
                    }
                }
            }
            return models;
        }













        public async Task<List<WishlogItemEditableModel>> GetWishlogItemEditableModelsByUidAndTypeAsync(int uid, WishType queryType)
        {
            using var ctx = _dbContextFactory.CreateDbContext();
            var items = await ctx.WishlogItems.Where(x => x.Uid == uid && x.QueryType == queryType).ToListAsync();
            var models = items.Adapt<List<WishlogItemEditableModel>>().OrderBy(x => x.Id).ToList();
            int guaranteeIndex = 0;
            foreach (var model in models)
            {
                guaranteeIndex++;
                model.GuaranteeIndex = guaranteeIndex;
                if (model.RankType == 5)
                {
                    guaranteeIndex = 0;
                }
            }
            return models;
        }


        // 获取按照卡池统计的数据，待修改
        public async Task<List<WishEventStatsModel>> GetCharacterWishEventStatsModelsByUidAsync(int uid)
        {
            WishEventInfo.RegionType = UidToRegionType(uid);
            var wishlogModels = await GetWishlogItemEditableModelsByUidAndTypeAsync(uid, WishType.CharacterEvent);
            using var ctx = _dbContextFactory.CreateDbContext();
            var eventInfos = await ctx.WishEventInfos.ToListAsync();
            var groups = eventInfos.Where(x => x.QueryType == WishType.CharacterEvent).GroupBy(x => x.StartTime).ToList();
            var dics = await ctx.CharacterInfos.ToDictionaryAsync(x => x.Name!);
            var eventModels = new List<WishEventStatsModel>();
            foreach (var group in groups)
            {
                var model = group.FirstOrDefault()?.Adapt<WishEventStatsModel>()!;
                model.Name = string.Join(" ", group.Select(x => x.Name));
                model.UpItems = group.SelectMany(x => x.Rank5UpItems).Join(dics, str => str, dic => dic.Key, (str, dic) => dic.Value).ToList().Adapt<List<WishEventStatsItemInfoModel>>();
                model.UpItems.AddRange(group.FirstOrDefault()!.Rank4UpItems.Join(dics, str => str, dic => dic.Key, (str, dic) => dic.Value).Adapt<IEnumerable<WishEventStatsItemInfoModel>>());
                model.Wishlogs = wishlogModels.Where(x => x.Time >= model.StartTime && x.Time <= model.EndTime).OrderByDescending(x => x.Id).ToList();
                model.TotalCount = model.Wishlogs.Count;
                model.Rarity3Count = model.Wishlogs.Count(x => x.RankType == 3);
                model.Rarity4Count = model.Wishlogs.Count(x => x.RankType == 4);
                model.Rarity5Count = model.Wishlogs.Count(x => x.RankType == 5);
                int index = 0;
                foreach (var item in model.Wishlogs)
                {
                    index++;
                    item.Index = index;
                }
                foreach (var item in model.UpItems)
                {
                    item.Count = model.Wishlogs.Count(x => x.Name == item.Name);
                }
                var noneUpCharacters = model.Wishlogs.Where(x => x.RankType > 3 && x.ItemType == "角色")
                                                     .ExceptBy(model.UpItems.Select(x => x.Name), x => x.Name)
                                                     .GroupBy(x => x.Name)
                                                     .Join(dics, g => g.Key, d => d.Key, (g, d) =>
                                                     {
                                                         var model = d.Value.Adapt<WishEventStatsItemInfoModel>();
                                                         model.Count = g.Count();
                                                         return model;
                                                     });
                var dics_weapon = await ctx.WeaponInfos.ToDictionaryAsync(x => x.Name!);
                var noneUpWeapons = model.Wishlogs.Where(x => x.RankType > 3 && x.ItemType == "武器")
                                                     .ExceptBy(model.UpItems.Select(x => x.Name), x => x.Name)
                                                     .GroupBy(x => x.Name)
                                                     .Join(dics_weapon, g => g.Key, d => d.Key, (g, d) =>
                                                     {
                                                         var model = d.Value.Adapt<WishEventStatsItemInfoModel>();
                                                         model.Count = g.Count();
                                                         return model;
                                                     });
                model.NoneUpItems = noneUpCharacters.Concat(noneUpWeapons).OrderByDescending(x => x.Rarity).OrderByDescending(x => x.Count).ToList();
                model.Rarity5Items = model.Wishlogs.Where(x => x.RankType == 5)
                                                   .Join(dics, item => item.Name, dic => dic.Key, (item, dic) => new WishEventStatsRarity5Item(item.Name, item.Time, item.GuaranteeIndex, dic.Value.SideIcon!, item.Id))
                                                   .OrderBy(x => x.Id)
                                                   .ToList();
                eventModels.Add(model);
            }
            return eventModels.OrderByDescending(x => x.StartTime).ToList();
        }

        // 同上，未完成
        public async Task<List<WishEventStatsModel>> GetWeaponWishEventStatsModelsByUidAsync(int uid)
        {
            WishEventInfo.RegionType = UidToRegionType(uid);
            var wishlogModels = await GetWishlogItemEditableModelsByUidAndTypeAsync(uid, WishType.WeaponEvent);
            using var ctx = _dbContextFactory.CreateDbContext();
            var eventInfos = await ctx.WishEventInfos.ToListAsync();
            var groups = eventInfos.Where(x => x.QueryType == WishType.WeaponEvent).GroupBy(x => x.StartTime).ToList();
            var dics = await ctx.WeaponInfos.ToDictionaryAsync(x => x.Name!);
            var eventModels = new List<WishEventStatsModel>();
            foreach (var group in groups)
            {
                var model = group.FirstOrDefault()?.Adapt<WishEventStatsModel>()!;
                model.Name = string.Join(" ", group.Select(x => x.Name));
                model.UpItems = group.SelectMany(x => x.Rank5UpItems).Join(dics, str => str, dic => dic.Key, (str, dic) => dic.Value).ToList().Adapt<List<WishEventStatsItemInfoModel>>();
                model.UpItems.AddRange(group.FirstOrDefault()!.Rank4UpItems.Join(dics, str => str, dic => dic.Key, (str, dic) => dic.Value).Adapt<IEnumerable<WishEventStatsItemInfoModel>>());
                model.Wishlogs = wishlogModels.Where(x => x.Time >= model.StartTime && x.Time <= model.EndTime).OrderByDescending(x => x.Id).ToList();
                model.TotalCount = model.Wishlogs.Count;
                model.Rarity3Count = model.Wishlogs.Count(x => x.RankType == 3);
                model.Rarity4Count = model.Wishlogs.Count(x => x.RankType == 4);
                model.Rarity5Count = model.Wishlogs.Count(x => x.RankType == 5);
                int index = 0;
                foreach (var item in model.Wishlogs)
                {
                    index++;
                    item.Index = index;
                }
                foreach (var item in model.UpItems)
                {
                    item.Count = model.Wishlogs.Count(x => x.Name == item.Name);
                }
                model.Rarity5Items = model.Wishlogs.Where(x => x.RankType == 5)
                                                   .Join(dics, item => item.Name, dic => dic.Key, (item, dic) => new WishEventStatsRarity5Item(item.Name, item.Time, item.GuaranteeIndex, dic.Value.Icon!, item.Id))
                                                   .OrderBy(x => x.Id)
                                                   .ToList();
                eventModels.Add(model);
            }
            return eventModels.OrderByDescending(x => x.StartTime).ToList();
        }




    }
}
