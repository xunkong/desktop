using Xunkong.Core.Wish;

namespace Xunkong.Desktop.Services
{

    public class BackupService
    {

        private readonly ILogger<BackupService> _logger;

        private readonly DbConnectionFactory<SqliteConnection> _cntFactory;

        private readonly IDbContextFactory<XunkongDbContext> _ctxFactory;

        private readonly JsonSerializerOptions _jsonSerializerOptions;


        public BackupService(ILogger<BackupService> logger, DbConnectionFactory<SqliteConnection> dbConnectionFactory, IDbContextFactory<XunkongDbContext> dbContextFactory, JsonSerializerOptions jsonSerializerOptions)
        {
            _logger = logger;
            _cntFactory = dbConnectionFactory;
            _ctxFactory = dbContextFactory;
            _jsonSerializerOptions = jsonSerializerOptions;
        }


        public async Task<string> BackupWishlogItemsAsync(int uid, List<WishlogItem> wishlogItems)
        {
            if (wishlogItems == null || !wishlogItems.Any())
            {
                throw new ArgumentNullException(nameof(wishlogItems), "Input wishlog items is null.");
            }
            _logger.LogInformation($"Start to backup wishlog items of uid {uid}.");
            if (wishlogItems.Any(x => x.Uid != uid))
            {
                _logger.LogWarning($"Input wishlog items has one that do not match the uid {uid}.");
                throw new ArgumentOutOfRangeException($"Input wishlog items has one that do not match the uid {uid}.");
            }
            var orderedItems = wishlogItems.OrderBy(x => x.Id).ToList();
            var model = new WishlogBackupModel
            {
                ExportApp = XunkongEnvironment.AppName,
                AppVersion = XunkongEnvironment.AppVersion.ToString(),
                ExportTime = DateTimeOffset.Now,
                Uid = uid,
                WishlogCount = wishlogItems.Count,
                FirstItemTime = wishlogItems.First().Time,
                LastItemTime = wishlogItems.Last().Time,
                WishlogList = orderedItems,
            };
            var dir = Path.Combine(XunkongEnvironment.UserDataPath, @"Backup\Wishlog");
            var file = Path.Combine(dir, $"Wishlog_{uid}_{DateTime.Now:yyyyMMdd_HHmmss}.json");
            Directory.CreateDirectory(dir);
            _logger.LogInformation($"Backup file path: {file}");
            using var stream = File.Create(file);
            await JsonSerializer.SerializeAsync(stream, model, _jsonSerializerOptions);
            return file;
        }


        public async Task<string> BackupWishlogItemsAsync(int uid)
        {
            _logger.LogInformation($"Start to backup wishlog items of uid {uid}.");
            using var ctx = _ctxFactory.CreateDbContext();
            var orderedItems = await ctx.WishlogItems.Where(x => x.Uid == uid).OrderBy(x => x.Id).ToListAsync();
            var model = new WishlogBackupModel
            {
                ExportApp = XunkongEnvironment.AppName,
                AppVersion = XunkongEnvironment.AppVersion.ToString(),
                ExportTime = DateTimeOffset.Now,
                Uid = uid,
                WishlogCount = orderedItems.Count,
                FirstItemTime = orderedItems.FirstOrDefault()?.Time,
                LastItemTime = orderedItems.LastOrDefault()?.Time,
                WishlogList = orderedItems,
            };
            var dir = Path.Combine(XunkongEnvironment.UserDataPath, @"Backup\Wishlog");
            var file = Path.Combine(dir, $"Wishlog_{uid}_{DateTime.Now:yyyyMMdd_HHmmss}.json");
            Directory.CreateDirectory(dir);
            _logger.LogInformation($"Backup file path: {file}");
            using var stream = File.Create(file);
            await JsonSerializer.SerializeAsync(stream, model, _jsonSerializerOptions);
            return file;
        }



        public async Task<string> BackupAndCompressDatabaseAsync()
        {
            var filePath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), $@"Xunkong\Backup\Database\XunkongData_{DateTimeOffset.Now:yyyyMMdd_HHmmss}.db");
            Directory.CreateDirectory(Path.GetDirectoryName(filePath)!);
            using var backupConnection = new SqliteConnection($"Data Source={filePath};");
            using var cnt = _cntFactory.CreateDbConnection();
            backupConnection.Open();
            cnt.Open();
            cnt.BackupDatabase(backupConnection);
            var latestDailyCheckId = await cnt.QueryFirstAsync<int>("SELECT Id FROM DailyCheckInItems ORDER BY Id DESC LIMIT 1;");
            await cnt.ExecuteAsync($"DELETE FROM DailyCheckInItems WHERE Id<{latestDailyCheckId};");
            var latestDailyNoteId = await cnt.QueryFirstAsync<int>("SELECT Id FROM DailyNote_Items ORDER BY Id DESC LIMIT 1;");
            await cnt.ExecuteAsync($"DELETE FROM DailyNote_Items WHERE Id<{latestDailyNoteId};");
            await cnt.ExecuteAsync("VACUUM;");
            return filePath;
        }


        public async Task<string> BackupAndMigrateDatabaseAsync()
        {
            var filePath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), $@"Xunkong\Backup\Database\XunkongData_{DateTimeOffset.Now:yyyyMMdd_HHmmss}.db");
            Directory.CreateDirectory(Path.GetDirectoryName(filePath)!);
            var backupConnection = new SqliteConnection($"Data Source={filePath};");
            using var cnt = _cntFactory.CreateDbConnection();
            cnt.BackupDatabase(backupConnection);
            using var ctx = _ctxFactory.CreateDbContext();
            await ctx.Database.MigrateAsync();
            return filePath;
        }



        public void AutoBackupDatabase()
        {
            try
            {
                var dir = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), $@"Xunkong\Backup\Database\");
                if (Directory.Exists(dir))
                {
                    var files = Directory.GetFiles(dir);
                    var filename = Path.GetFileNameWithoutExtension(files.LastOrDefault());
                    if (!string.IsNullOrWhiteSpace(filename))
                    {
                        var fileData = filename.Substring(filename.IndexOf("_") + 1);
                        var nowData = DateTime.Now.AddDays(-7).ToString("yyyyMMdd_HHmmss");
                        if (fileData.CompareTo(nowData) == 1)
                        {
                            return;
                        }
                    }
                }
                var filePath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), $@"Xunkong\Backup\Database\XunkongData_{DateTimeOffset.Now:yyyyMMdd_HHmmss}.db");
                Directory.CreateDirectory(Path.GetDirectoryName(filePath)!);
                var backupConnection = new SqliteConnection($"Data Source={filePath};");
                using var cnt = _cntFactory.CreateDbConnection();
                backupConnection.Open();
                cnt.Open();
                cnt.BackupDatabase(backupConnection);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, nameof(AutoBackupDatabase));
            }
        }



    }




}
