using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunkong.Core.Hoyolab;

namespace Xunkong.Desktop.Services
{
    internal class BackgroundService
    {

        private readonly ILogger<BackgroundService> _logger;

        private HoyolabService _hoyolabService;

        public BackgroundService(ILogger<BackgroundService> logger, HoyolabService hoyolabService)
        {
            _logger = logger;
            _hoyolabService = hoyolabService;
        }


        public async Task RefreshDailyNoteTilesAsync()
        {
            try
            {
                _logger.LogInformation("Start to refresh daily note tiles.");
                var allTiles = await TileHelper.FindAllAsync();
                var uids = allTiles.Where(x => x.Contains("DailyNote_")).Select(x => int.Parse(x.Replace("DailyNote_", ""))).ToList();
                _logger.LogInformation($"{uids.Count} tiles need to refresh.");
                foreach (var uid in uids)
                {
                    _logger.LogInformation("====================");
                    try
                    {
                        var role = await _hoyolabService.GetUserGameRoleInfoAsync(uid);
                        if (role == null)
                        {
                            _logger.LogWarning($"Cannot get genshin role info of uid {uid} from local database.");
                        }
                        else
                        {
                            _logger.LogInformation($"Get daily note of account {role.Nickname} ({role.Uid})");
                            var info = await _hoyolabService.GetDailyNoteInfoAsync(role);
                            if (info != null)
                            {
                                TileHelper.UpdatePinnedTile(info);
                            }
                        }
                    }
                    catch (Exception ex) when (ex is HoyolabException or HttpRequestException)
                    {
                        _logger.LogError(ex, $"Get daily note of uid {uid}");
                        NotificationHelper.SendNotification("刷新便笺磁贴时遇到错误", $"Uid {uid}\n{ex.Message}");
                    }
                }
                _logger.LogInformation("Refresh daily note tiles finished.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Refresh daily note tiles.");
                NotificationHelper.SendNotification("刷新便笺磁贴时遇到错误", ex.Message);
            }
        }



    }
}
