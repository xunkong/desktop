using System.Text.RegularExpressions;
using Xunkong.Core.Hoyolab;
using Xunkong.Core.Wish;
using Xunkong.Core.XunkongApi;

namespace Xunkong.Desktop.Services
{


    public class WishlogService
    {

        private readonly ILogger<WishlogService> _logger;

        private readonly HttpClient _httpClient;

        private readonly WishlogClient _wishlogClient;

        private readonly IDbContextFactory<XunkongDbContext> _ctxFactory;

        private readonly DbConnectionFactory<SqliteConnection> _cntFactory;

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
            _ctxFactory = dbContextFactory;
            _cntFactory = dbConnectionFactory;
            _backupService = backupService;
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
        /// 获取原神日志中最新的祈愿记录网址
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
            using var cnt = _cntFactory.CreateDbConnection();
            var model = new WishlogAuthkeyItem { Uid = uid, Url = wishlogUrl, DateTime = DateTimeOffset.Now };
            await cnt.ExecuteAsync("INSERT OR REPLACE INTO Wishlog_Authkeys (Uid,Url,DateTime) VALUES(@Uid,@Url,@DateTime);", model);
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
        public async Task<bool> CheckWishlogAuthkeyExpiredAsync(int uid)
        {
            using var ctx = _ctxFactory.CreateDbContext();
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
            using var ctx = _ctxFactory.CreateDbContext();
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




        /// <summary>
        /// 根据本地保存的祈愿记录网址，从米哈游服务器获取指定uid的祈愿记录，并返回新增数量
        /// </summary>
        /// <param name="uid"></param>
        /// <param name="onProgressChanged">页数变化时调用</param>
        /// <param name="getAll">是否获取全部数据</param>
        /// <returns>新增祈愿记录的数量</returns>
        /// <exception cref="XunkongException"></exception>
        /// <exception cref="HoyolabException"></exception>
        public async Task<int> GetWishlogByUidAsync(int uid, EventHandler<(WishType WishType, int Page)>? onProgressChanged = null, bool getAll = false)
        {
            using var ctx = _ctxFactory.CreateDbContext();
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
        /// <param name="isSea">首先从外服日志获取</param>
        /// <param name="getAll">是否获取全部数据</param>
        /// <returns></returns>
        /// <exception cref="HoyolabException"></exception>
        public async Task<int> GetWishlogsByLogFileAsync(bool isSea = false, bool getAll = false)
        {
            var wishlogUrl = await FindWishlogUrlFromLogFileAsync(isSea);
            var uid = await GetUidByWishlogUrl(wishlogUrl);
            long lastId = 0;
            if (!getAll)
            {
                using var cnt = _cntFactory.CreateDbConnection();
                lastId = await cnt.QueryFirstOrDefaultAsync<long>("SELECT Id FROM Wishlog_Items WHERE Uid=@Uid ORDER BY Id DESC;", new { Uid = uid });
            }
            _logger.LogDebug("Start getting wishlog with uid {Uid} and (get all? {GetAll})", uid, getAll);
            var wishlogs = await _wishlogClient.GetAllWishlogAsync(wishlogUrl, lastId);
            using var ctx = _ctxFactory.CreateDbContext();
            var existList = await ctx.WishlogItems.AsNoTracking().Where(x => x.Uid == uid).Select(x => x.Id).ToListAsync();
            var addList = wishlogs.ExceptBy(existList, x => x.Id).ToList();
            ctx.AddRange(addList);
            await ctx.SaveChangesAsync();
            _logger.LogInformation("Add {Count} wishlog items to uid {Uid}", addList.Count, uid);
            return addList.Count;
        }



        /// <summary>
        /// 获取本地数据库中所有的uid
        /// </summary>
        /// <returns></returns>
        public async Task<List<int>> GetAllUidsAsync()
        {
            using var ctx = _ctxFactory.CreateDbContext();
            return await ctx.WishlogItems.Select(x => x.Uid).Distinct().ToListAsync();
        }


        /// <summary>
        /// 获取指定uid的本地祈愿记录数量
        /// </summary>
        /// <param name="uid"></param>
        /// <returns></returns>
        public async Task<int> GetWishlogCountAsync(int uid)
        {
            using var ctx = _ctxFactory.CreateDbContext();
            return await ctx.WishlogItems.Where(x => x.Uid == uid).CountAsync();
        }




        public async Task<string> ImportFromJsonFile(string file)
        {
            _logger.LogInformation($"Start to import wishlog from json file: {file}");
            var str = await File.ReadAllTextAsync(file);
            var importer = new JsonImporter();
            var list = importer.Deserialize(str);
            _logger.LogInformation($"Total deserialozed wishlog count: {list.Count}");
            if (!list.Any())
            {
                throw new XunkongException(ErrorCode.InternalException, "No wishlogs in the imported file.");
            }
            if (list.Any(x => x.Uid == 0))
            {
                throw new XunkongException(ErrorCode.InternalException, "Imported wishlogs have items without uid.");
            }
            if (list.DistinctBy(x => x.Uid).Count() > 1)
            {
                throw new XunkongException(ErrorCode.InternalException, "Imported wishlogs have more than one uid.");
            }
            var uid = list.FirstOrDefault()!.Uid;
            _logger.LogInformation($"Imported uid of wishlog is {uid}");
            using var ctx = _ctxFactory.CreateDbContext();
            if (await ctx.WishlogItems.AnyAsync(x => x.Uid == uid))
            {
                await _backupService.BackupWishlogItemsAsync(uid);
            }
            var existing = await ctx.WishlogItems.Where(x => x.Uid == uid).Select(x => x.Id).ToListAsync();
            _logger.LogInformation($"Uid {uid} has existed wishlog count {existing.Count}");
            var adding = list.ExceptBy(existing, x => x.Id).ToList();
            _logger.LogInformation($"This import action need to add wishlog count {adding.Count}");
            ctx.AddRange(adding);
            await ctx.SaveChangesAsync();
            _logger.LogInformation("Import action and operation database is finished.");
            var addCount = adding.Count;
            var totalCount = await ctx.WishlogItems.Where(x => x.Uid == uid).CountAsync();
            return $"账号 {uid} 此次导入新增 {addCount} 条记录，导入后总计 {totalCount} 条";
        }




        public async Task<string> ImportFromExcelFile(string file)
        {
            _logger.LogInformation($"Start to import wishlog from excel file: {file}");
            var items = await MiniExcelLibs.MiniExcel.QueryAsync<WishlogItemExcelModel>(file, "原始数据");
            var list = items.Adapt<List<WishlogItem>>().Where(x => x.Uid != 0).ToList();
            _logger.LogInformation($"Total deserialozed wishlog count: {list.Count}");
            if (!list.Any())
            {
                throw new XunkongException(ErrorCode.InternalException, "No wishlogs in the imported file.");
            }
            if (list.Any(x => x.Uid == 0))
            {
                throw new XunkongException(ErrorCode.InternalException, "Imported wishlogs have items without uid.");
            }
            if (list.DistinctBy(x => x.Uid).Count() > 1)
            {
                throw new XunkongException(ErrorCode.InternalException, "Imported wishlogs have more than one uid.");
            }
            var uid = list.FirstOrDefault()!.Uid;
            _logger.LogInformation($"Imported uid of wishlog is {uid}");
            using var ctx = _ctxFactory.CreateDbContext();
            if (await ctx.WishlogItems.AnyAsync(x => x.Uid == uid))
            {
                await _backupService.BackupWishlogItemsAsync(uid);
            }
            var existing = await ctx.WishlogItems.Where(x => x.Uid == uid).Select(x => x.Id).ToListAsync();
            _logger.LogInformation($"Uid {uid} has existed wishlog count {existing.Count}");
            var adding = list.ExceptBy(existing, x => x.Id).ToList();
            _logger.LogInformation($"This import action need to add wishlog count {adding.Count}");
            ctx.AddRange(adding);
            await ctx.SaveChangesAsync();
            _logger.LogInformation("Import action and operation database is finished.");
            var addCount = adding.Count;
            var totalCount = await ctx.WishlogItems.Where(x => x.Uid == uid).CountAsync();
            return $"账号 {uid} 此次导入新增 {addCount} 条记录，导入后总计 {totalCount} 条";
        }









        public async Task<List<WishlogItemEx>> GetWishlogItemExByUidAsync(int uid)
        {
            using var ctx = _ctxFactory.CreateDbContext();
            var items = await ctx.WishlogItems.Where(x => x.Uid == uid).ToListAsync();
            var events = await ctx.WishEventInfos.ToListAsync();
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
                if (queryType is WishType.CharacterEvent or WishType.WeaponEvent)
                {
                    var eventGroup = events.Where(x => x.QueryType == queryType).GroupBy(x => x.StartTime);
                    var rank45Items = group.Where(x => x.RankType > 3).ToList();
                    Parallel.ForEach(eventGroup, eg =>
                    {
                        var upNames = eg.SelectMany(x => x.Rank5UpItems).Concat(eg.SelectMany(x => x.Rank4UpItems).Distinct()).ToList();
                        var firstEvent = eg.First();
                        var queryUp = rank45Items.Where(x => firstEvent.StartTime <= x.Time && x.Time <= firstEvent.EndTime).Where(x => upNames.Contains(x.Name));
                        foreach (var item in queryUp)
                        {
                            item.IsUp = true;
                        }
                    });
                }
            });
            return models;
        }













        public async Task<List<WishlogItemEx>> GetWishlogItemEditableModelsByUidAndTypeAsync(int uid, WishType queryType)
        {
            using var ctx = _ctxFactory.CreateDbContext();
            var items = await ctx.WishlogItems.Where(x => x.Uid == uid && x.QueryType == queryType).ToListAsync();
            var models = items.Adapt<List<WishlogItemEx>>().OrderBy(x => x.Id).ToList();
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




    }
}
