using Microsoft.Win32;
using System.ComponentModel;
using System.Diagnostics;
using System.Net.Http;
using System.Text;
using System.Text.RegularExpressions;
using Windows.ApplicationModel;
using Xunkong.Hoyolab;

namespace Xunkong.Desktop.Services;

internal class InvokeService
{



    public static async Task RefreshDailyNoteTilesAsync()
    {
        try
        {
            var allTiles = await SecondaryTileProvider.FindAllAsync();
            var uids = allTiles.Where(x => x.Contains("DailyNote_")).Select(x => int.Parse(x.Replace("DailyNote_", ""))).ToList();
            if (uids.Any())
            {
                var service = new HoyolabService(new HoyolabClient());
                foreach (var uid in uids)
                {
                    try
                    {
                        var role = service.GetGenshinRoleInfo(uid);
                        if (role != null)
                        {
                            var info = await service.GetDailyNoteAsync(role);
                            if (info != null)
                            {
                                await SecondaryTileProvider.RequestPinTileAsync(info);
                            }
                        }
                    }
                    catch (Exception ex) when (ex is HoyolabException or HttpRequestException)
                    {
                        await ToastProvider.SendAsync("刷新磁贴时遇到错误", $"Uid {uid}\n{ex.Message}");
                    }
                }
            }
            else
            {
                SecondaryTileProvider.UnregisterTask();
            }
        }
        catch (Exception ex)
        {
            await ToastProvider.SendAsync("刷新磁贴时遇到错误", ex.Message);
        }
    }




    public static async Task<bool> StartGameAsync()
    {
        try
        {
            var ps = Process.GetProcessesByName("YuanShen").Concat(Process.GetProcessesByName("GenshinImpact"));
            if (ps.Any())
            {
                await ToastProvider.SendAsync("出错了", "已有游戏进程在运行");
                return false;
            }
            var exePath = AppSetting.GetValue<string>(SettingKeys.GameExePath);
            if (!File.Exists(exePath))
            {
                const string REG_PATH = @"HKEY_LOCAL_MACHINE\SOFTWARE\Microsoft\Windows\CurrentVersion\Uninstall\原神\";
                const string REG_KEY = "InstallPath";
                var launcherPath = Registry.GetValue(REG_PATH, REG_KEY, null) as string;
                if (!string.IsNullOrWhiteSpace(launcherPath))
                {
                    var configPath = Path.Combine(launcherPath, "config.ini");
                    if (File.Exists(configPath))
                    {
                        var str = await File.ReadAllTextAsync(configPath);
                        var gamePath = Regex.Match(str, @"game_install_path=(.+)").Groups[1].Value.Trim();
                        exePath = Path.Combine(gamePath, "YuanShen.exe");
                    }
                }
                if (!File.Exists(exePath))
                {
                    await ToastProvider.SendAsync("出错了", "没有找到 YuanShen.exe");
                    return false;
                }
            }
            if (!(exePath.EndsWith("YuanShen.exe") || exePath.EndsWith("GenshinImpact.exe")))
            {
                await ToastProvider.SendAsync("出错了", "文件名不为 YuanShen.exe 或 GenshinImpact.exe");
                return false;
            }
            var fps = AppSetting.GetValue(SettingKeys.TargetFPS, 60);
            var isPopup = AppSetting.GetValue<bool>(SettingKeys.IsPopupWindow);
            var command = new StringBuilder();
            command.Append("-exe ");
            command.Append($@"""{exePath}""");
            command.Append(" -fps ");
            command.Append(fps);
            if (isPopup)
            {
                command.Append(" -popupwindow");
            }
            var info = new ProcessStartInfo
            {
                FileName = Path.Combine(Package.Current.InstalledPath, "Xunkong.Desktop.FpsUnlocker/Xunkong.Desktop.FpsUnlocker.exe"),
                Arguments = command.ToString(),
                UseShellExecute = true,
                Verb = "runas",
            };
            Process.Start(info);
            return true;
        }
        catch (Exception ex)
        {
            if (ex is Win32Exception ex32)
            {
                if (ex32.NativeErrorCode == 1223)
                {
                    // 操作已取消
                    return false;
                }
            }
            await ToastProvider.SendAsync("出错了", ex.Message);
            return false;
        }
    }




    public static async Task CheckTransformerReachedAsync()
    {
        try
        {
            var service = new HoyolabService(new HoyolabClient());
            var users = service.GetGenshinRoleInfoList();
            if (!users.Any())
            {
                return;
            }
            var nicknames = new List<string>(users.Count());
            foreach (var user in users)
            {
                try
                {
                    var dailynote = await service.GetDailyNoteAsync(user);
                    if (dailynote != null && dailynote.Transformer.Obtained && dailynote.Transformer.RecoveryTime.Reached)
                    {
                        nicknames.Add(user.Nickname!);
                    }
                }
                catch { }
            }
            if (nicknames.Any())
            {
                await ToastProvider.SendAsync("您有以下账号可以使用参量质变仪", string.Join("\n", nicknames));
            }
        }
        catch (Exception ex)
        {
            await ToastProvider.SendAsync("出错了", ex.Message);
        }
    }



}
