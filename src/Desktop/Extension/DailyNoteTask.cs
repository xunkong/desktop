using Dapper;
using Microsoft.Data.Sqlite;
using Serilog;
using Xunkong.Core.Hoyolab;

namespace Xunkong.Desktop.Extension
{
    internal class DailyNoteTask
    {


        private const string sql = "INSERT INTO DailyNote_Items"
                                 + "(Uid,Time,CurrentResin,MaxResin,ResinRecoveryTime,FinishedTaskNumber,TotalTaskNumber,IsExtraTaskRewardReceived,RemainResinDiscountNumber,ResinDiscountLimitedNumber,CurrentExpeditionNumber,MaxExpeditionNumber,CurrentHomeCoin,MaxHomeCoin,HomeCoinRecoveryTime)"
                                 + "VALUES (@Uid,@Time,@CurrentResin,@MaxResin,@ResinRecoveryTime,@FinishedTaskNumber,@TotalTaskNumber,@IsExtraTaskRewardReceived,@RemainResinDiscountNumber,@ResinDiscountLimitedNumber,@CurrentExpeditionNumber,@MaxExpeditionNumber,@CurrentHomeCoin,@MaxHomeCoin,@HomeCoinRecoveryTime);";



        public static async Task RefreshDailyNoteAsync()
        {
            Log.Information("Start freshing all daily note tiles.");
            using var cnt = new SqliteConnection(Program.DbStr);
            var allRoles = await cnt.QueryAsync<UserGameRoleInfo>("SELECT * FROM Genshin_Users;");
            if (!allRoles.Any())
            {
                Log.Warning("No game roles in database.");
                return;
            }
            var allTiles = await TileHelper.FindAllAsync();
            var uids = allTiles.Where(x => x.Contains("DailyNote_")).Select(x => int.Parse(x.Replace("DailyNote_", ""))).ToList();
            Log.Information($"Pinned uid of tiles: {string.Join("  ", uids)}");
            var client = new HoyolabClient();
            var list = new List<DailyNoteInfo>(allRoles.Count());
            await Parallel.ForEachAsync(allRoles, async (role, _) =>
            {
                try
                {
                    var dailynote = client.GetDailyNoteInfoAsync(role).Result;
                    if (uids.Contains(dailynote.Uid))
                    {
                        TileHelper.UpdatePinnedTile(dailynote);
                    }
                    using var cnt1 = new SqliteConnection(Program.DbStr);
                    await cnt1.ExecuteAsync(sql, dailynote);
                    lock (list)
                    {
                        list.Add(dailynote);
                    }
                }
                catch (HoyolabException ex)
                {
                    Log.Error(ex, $"HoyolabException (Uid {role.Uid}, nickname {role.Nickname})");
                }
            });
            if (LocalSettingHelper.GetSetting<bool>(SettingKeys.EnableDailyNoteNotification))
            {
                Log.Information("Enable DailyNote Notification");
                var resinThreshold = LocalSettingHelper.GetSetting(SettingKeys.DailyNoteNotification_ResinThreshold, 150);
                var homeCoinThreshold = LocalSettingHelper.GetSetting(SettingKeys.DailyNoteNotification_HomeCoinThreshold, 0.9);
                Log.Debug($"Resin threshold {resinThreshold}, home coin threshold {homeCoinThreshold:F2}");
                var notifications = new List<(string? nickname, bool isResin, bool isHomeCoin)>();
                foreach (var item in list)
                {
                    Log.Debug($"Nickname {item.Nickname}: current resin {item.CurrentResin}, current homecoin {item.CurrentHomeCoin}, max homecoin {item.MaxHomeCoin}");
                    if (item.CurrentResin >= resinThreshold || item.CurrentHomeCoin >= item.MaxHomeCoin * homeCoinThreshold)
                    {
                        using var cnt2 = new SqliteConnection(Program.DbStr);
                        var lastNote = await cnt2.QueryFirstOrDefaultAsync<DailyNoteInfo>($"SELECT Id,Uid,CurrentResin,CurrentHomeCoin,MaxHomeCoin FROM DailyNote_Items WHERE Uid={item.Uid} ORDER BY Id DESC LIMIT 1 OFFSET 1;");
                        Log.Debug($"Nickname {lastNote.Nickname}: last resin {lastNote.CurrentResin}, last homecoin {lastNote.CurrentHomeCoin}, last homecoin {lastNote.MaxHomeCoin}");
                        var noti = ($"{item.Nickname} {item.Uid}",
                                    lastNote.CurrentResin < resinThreshold && resinThreshold <= item.CurrentResin,
                                    lastNote.CurrentHomeCoin < item.MaxHomeCoin * homeCoinThreshold && item.MaxHomeCoin * homeCoinThreshold <= item.CurrentHomeCoin);
                        Log.Debug($"Nickname {noti.Item1}: isResin {noti.Item2}, isHomeCoin {noti.Item3}");
                        notifications.Add(noti);
                    }
                }
                if (notifications.Any(x => x.isResin || x.isHomeCoin))
                {
                    var states = (notifications.Any(x => x.isResin), notifications.Any(x => x.isHomeCoin));
                    Log.Debug($"Noti states: anyResin {states.Item1}, anyHomecoin {states.Item2}");
                    var title = states switch
                    {
                        (true, true) => "您有以下账号需要清理树脂或收集洞天宝钱",
                        (true, false) => "您有以下账号需要清理树脂",
                        (false, true) => "您有以下账号需要收集洞天宝钱",
                        _ => "",
                    };
                    var nicknames = notifications.Where(x => x.isResin || x.isHomeCoin).Select(x => x.nickname).ToList();
                    var message = string.Join("\n", nicknames);
                    NotificationHelper.SendNotification(title, message);
                }
            }
            Log.Information("Fresh daily note task finished.");

        }
    }
}
