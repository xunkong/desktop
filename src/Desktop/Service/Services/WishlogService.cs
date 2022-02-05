using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using System.Text.RegularExpressions;
using Xunkong.Core.Hoyolab;
using Xunkong.Core.Wish;

namespace Xunkong.Desktop.Services
{

    [InjectService]
    public class WishlogService
    {

        private readonly ILogger<WishlogService> _logger;

        private readonly WishlogClient _wishlogClient;

        private readonly IDbContextFactory<XunkongDbContext> _dbContextFactory;

        private readonly DbConnectionFactory<SqliteConnection> _dbConnectionFactory;

        private static readonly string UserProfile = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);

        private static readonly string LogFile_Cn = Path.Combine(UserProfile, @"AppData\LocalLow\miHoYo\原神\output_log.txt");

        private static readonly string LogFile_Sea = Path.Combine(UserProfile, @"AppData\LocalLow\miHoYo\Genshin Impact\output_log.txt");


        public WishlogService(ILogger<WishlogService> logger, WishlogClient wishlogClient, IDbContextFactory<XunkongDbContext> dbContextFactory, DbConnectionFactory<SqliteConnection> dbConnectionFactory)
        {
            _logger = logger;
            _wishlogClient = wishlogClient;
            _dbContextFactory = dbContextFactory;
            _dbConnectionFactory = dbConnectionFactory;
        }








        /// <summary>
        /// 获取原神日志中祈愿记录网址
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



        /// <summary>
        /// 获取原神日志中祈愿记录网址的祈愿记录，返回新增条数
        /// </summary>
        /// <param name="isSea"></param>
        /// <param name="getAll"></param>
        /// <returns></returns>
        /// <exception cref="HoyolabException"></exception>
        public async Task<int> GetWishlogByLogFileAsync(bool isSea = false, bool getAll = false)
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









    }
}
