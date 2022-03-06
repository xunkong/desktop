using Microsoft.EntityFrameworkCore;
using Serilog;
using Xunkong.Core.Hoyolab;
using Xunkong.Desktop.Models;

namespace Xunkong.Desktop.Extension
{
    internal class HoyolabCheckInTask
    {

        public static async Task CheckInAsync()
        {
            Log.Information("Start to check in.");
            Log.Debug("Create database context.");
            using var ctx = DbHelper.CreateDbContext();
            Log.Debug("Get account from database.");
            var roles = await ctx.UserGameRoleInfos.AsNoTracking().ToListAsync();
            Log.Debug($"Total {roles.Count} account.");
            var client = new HoyolabClient();
            foreach (var role in roles)
            {
                try
                {
                    Log.Information("====================");
                    Log.Information($"Start to check in with account {role.Nickname} {role.Uid}.");
                    Log.Debug("Get check in info from database.");
                    var localInfo = await ctx.DailyCheckInItems.AsNoTracking()
                                                               .Where(x => x.Uid == role.Uid)
                                                               .OrderByDescending(x => x.Id)
                                                               .FirstOrDefaultAsync();
                    if (localInfo != null)
                    {
                        var nowDate = DateTimeOffset.UtcNow.AddHours(8).ToString("yyyy-MM-dd");
                        var signDate = localInfo.Date;
                        if (nowDate == signDate)
                        {
                            Log.Information("Not need to check in.");
                            continue;
                        }
                    }
                    Log.Debug("Get check in info from hoyolab.");
                    var signInfo = await client.GetSignInInfoAsync(role);
                    if (signInfo.IsSign)
                    {
                        Log.Information("Not need to check in.");
                    }
                    else
                    {
                        Log.Debug("Check in.");
                        await client.SignInAsync(role);
                        Log.Debug("Check in finished.");
                        if (LocalSettingHelper.GetSetting<bool>(SettingKeys.DailyCheckInSuccessNotification))
                        {
                            NotificationHelper.SendNotification("签到成功", $"{role.Nickname} {role.Uid}");
                        }
                    }
                    localInfo = new DailyCheckInItem { Uid = role.Uid, Date = signInfo.Today.ToString("yyyy-MM-dd"), Time = DateTimeOffset.Now };
                    ctx.Add(localInfo);
                    await ctx.SaveChangesAsync();
                }
                catch (HoyolabException ex)
                {
                    Log.Error(ex, "Hoyolab exception.");
                    if (LocalSettingHelper.GetSetting<bool>(SettingKeys.DailyCheckInErrorNotification))
                    {
                        NotificationHelper.SendNotification("签到失败", $"{role.Nickname} {role.Uid}\n{ex.Message}");
                    }
                }
                catch (HttpRequestException ex)
                {
                    Log.Error(ex, "Http exception.");
                    if (LocalSettingHelper.GetSetting<bool>(SettingKeys.DailyCheckInErrorNotification))
                    {
                        NotificationHelper.SendNotification("签到失败", $"{role.Nickname} {role.Uid}\n{ex.Message}");
                    }
                }
            }
            Log.Information("====================");
            Log.Information("All account check in finished.");
        }

    }
}
