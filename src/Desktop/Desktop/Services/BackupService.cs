using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Encodings.Web;
using System.Text.Json;
using System.Threading.Tasks;
using Xunkong.Core.Wish;
using Xunkong.Core.XunkongApi;

namespace Xunkong.Desktop.Services
{
    public class BackupService
    {


        private readonly ILogger<BackupService> _logger;

        private readonly DbConnectionFactory<SqliteConnection> _dbConnectionFactory;

        private readonly IDbContextFactory<XunkongDbContext> _dbContextFactory;

        private readonly JsonSerializerOptions _jsonSerializerOptions;



        public BackupService(ILogger<BackupService> logger, DbConnectionFactory<SqliteConnection> dbConnectionFactory, IDbContextFactory<XunkongDbContext> dbContextFactory)
        {
            _logger = logger;
            _dbConnectionFactory = dbConnectionFactory;
            _dbContextFactory = dbContextFactory;
            _jsonSerializerOptions = new JsonSerializerOptions { Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping, WriteIndented = true };
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
            var file = Path.Combine(dir, $"wishlog_{uid}_{DateTime.Now:yyyyMMddHHmmss}.json");
            Directory.CreateDirectory(dir);
            _logger.LogInformation($"Backup file path: {file}");
            using var stream = File.Create(file);
            await JsonSerializer.SerializeAsync(stream, model, _jsonSerializerOptions);
            return file;
        }


        public async Task<string> BackupWishlogItemsAsync(int uid, bool throwError = false)
        {
            _logger.LogInformation($"Start to backup wishlog items of uid {uid}.");
            using var ctx = _dbContextFactory.CreateDbContext();
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
            var file = Path.Combine(dir, $"wishlog_{uid}_{DateTime.Now:yyyyMMddHHmmss}.json");
            Directory.CreateDirectory(dir);
            _logger.LogInformation($"Backup file path: {file}");
            using var stream = File.Create(file);
            await JsonSerializer.SerializeAsync(stream, model, _jsonSerializerOptions);
            return file;
        }

    }




}
