using Microsoft.Win32;
using System.ComponentModel;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using Windows.ApplicationModel;
using Windows.Storage;
using Xunkong.Core.Hoyolab;
using Xunkong.Core.XunkongApi;

namespace Xunkong.Desktop.Services
{
    internal class InvokeService
    {

        private readonly ILogger<InvokeService> _logger;

        private HoyolabService _hoyolabService;

        public InvokeService(ILogger<InvokeService> logger, HoyolabService hoyolabService)
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


        public async Task StartGameAsync()
        {
            try
            {
                _logger.LogInformation("Start to start game.");
                var ps = Process.GetProcessesByName("YuanShen.exe").Concat(Process.GetProcessesByName("GenshinImpact.exe"));
                if (ps.Any())
                {
                    _logger.LogInformation("YuanShen has already started.");
                    throw new XunkongException(ErrorCode.InternalException, "已有游戏进程正在运行");
                }
                var exePath = LocalSettingHelper.GetSetting<string>(SettingKeys.GameExePath);
                if (!File.Exists(exePath))
                {
                    _logger.LogInformation($"Cannot find exe path from local setting, start to get from registry.");
                    const string REG_PATH = @"HKEY_LOCAL_MACHINE\SOFTWARE\Microsoft\Windows\CurrentVersion\Uninstall\原神\";
                    const string REG_KEY = "InstallPath";
                    var launcherPath = Registry.GetValue(REG_PATH, REG_KEY, null) as string;
                    _logger.LogInformation($"Launcher path is {launcherPath}.");
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
                        throw new XunkongException(ErrorCode.InternalException, "没有找到 YuanShen.exe");
                    }
                }
                _logger.LogInformation($"Exe path is {exePath}.");
                if (!(exePath.EndsWith("YuanShen.exe") || exePath.EndsWith("GenshinImpact.exe")))
                {
                    throw new XunkongException(ErrorCode.InternalException, "文件名不为 YuanShen.exe 或 GenshinImpact.exe");
                }
                var fps = LocalSettingHelper.GetSetting(SettingKeys.TargetFPS, 60);
                var isPopup = LocalSettingHelper.GetSetting<bool>(SettingKeys.IsPopupWindow);
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
            }
            catch (Win32Exception ex)
            {
                _logger.LogError(ex, "Start game exception.");
                if (ex.NativeErrorCode == 1223)
                {
                    throw new Win32Exception(1223, "操作已取消");
                }
                throw;
            }
        }


        public static async Task StartGameWishoutLogAsync()
        {
            try
            {
                var ps = Process.GetProcessesByName("YuanShen").Concat(Process.GetProcessesByName("GenshinImpact"));
                if (ps.Any())
                {
                    NotificationHelper.SendNotification("无法启动游戏", "已有游戏进程在运行");
                    return;
                }
                var exePath = LocalSettingHelper.GetSetting<string>(SettingKeys.GameExePath);
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
                        NotificationHelper.SendNotification("无法启动游戏", "没有找到 YuanShen.exe");
                        return;
                    }
                }
                if (!(exePath.EndsWith("YuanShen.exe") || exePath.EndsWith("GenshinImpact.exe")))
                {
                    NotificationHelper.SendNotification("无法启动游戏", "文件名不为 YuanShen.exe 或 GenshinImpact.exe");
                    return;
                }
                var fps = LocalSettingHelper.GetSetting(SettingKeys.TargetFPS, 60);
                var isPopup = LocalSettingHelper.GetSetting<bool>(SettingKeys.IsPopupWindow);
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
            }
            catch (Exception ex)
            {
                if (ex is Win32Exception ex32)
                {
                    if (ex32.NativeErrorCode == 1223)
                    {
                        NotificationHelper.SendNotification("无法启动游戏", "操作已取消");
                        return;
                    }
                }
                NotificationHelper.SendNotification("无法启动游戏", ex.Message);
            }
            finally
            {
                // 等通知发出去
                await Task.Delay(100);
            }
        }


        public static async Task StartGameWishAccountAsync(string userName)
        {
            try
            {
                var ps = Process.GetProcessesByName("YuanShen").Concat(Process.GetProcessesByName("GenshinImpact"));
                if (ps.Any())
                {
                    NotificationHelper.SendNotification("无法启动游戏", "已有游戏进程在运行");
                    return;
                }
                var dbPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), @"Xunkong\Data\XunkongData.db");
                if (!File.Exists(dbPath))
                {
                    NotificationHelper.SendNotification("无法启动游戏", "数据库文件不存在");
                    return;
                }
                var exePath = LocalSettingHelper.GetSetting<string>(SettingKeys.GameExePath);
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
                        NotificationHelper.SendNotification("无法启动游戏", "没有找到 YuanShen.exe");
                        return;
                    }
                }
                if (!(exePath.EndsWith("YuanShen.exe") || exePath.EndsWith("GenshinImpact.exe")))
                {
                    NotificationHelper.SendNotification("无法启动游戏", "文件名不为 YuanShen.exe 或 GenshinImpact.exe");
                    return;
                }
                using var dapper = new SqliteConnection($"Data Source={dbPath};");
                var account = await dapper.QueryFirstOrDefaultAsync<GenshinUserAccount>("SELECT UserName,IsOversea,ADLPROD FROM GenshinUserAccounts WHERE UserName=@UserName", new { UserName = userName });
                if (account == null)
                {
                    NotificationHelper.SendNotification("无法启动游戏", "没有找到对应的账号");
                    return;
                }
                await SetADLPROD(account.ADLPROD, account.IsOversea);
                await dapper.ExecuteAsync("UPDATE GenshinUserAccounts SET LastAccessTime=@LastAccessTime WHERE UserName=@UserName", new { UserName = userName, LastAccessTime = DateTimeOffset.Now });
                var fps = LocalSettingHelper.GetSetting(SettingKeys.TargetFPS, 60);
                var isPopup = LocalSettingHelper.GetSetting<bool>(SettingKeys.IsPopupWindow);
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
            }
            catch (Exception ex)
            {
                if (ex is Win32Exception ex32)
                {
                    if (ex32.NativeErrorCode == 1223)
                    {
                        NotificationHelper.SendNotification("无法启动游戏", "操作已取消");
                        return;
                    }
                }
                NotificationHelper.SendNotification("无法启动游戏", ex.Message);
            }
            finally
            {
                // 等通知发出去
                await Task.Delay(100);
            }
        }


        public static async Task<byte[]> GetADLPROD(bool isOversea = false)
        {
            return await RegInvokeAsync("GetADLPROD", isOversea);
        }


        public static async Task SetADLPROD(byte[] bytes, bool isOversea = false)
        {
            await RegInvokeAsync("SetADLPROD", isOversea, bytes);
        }


        private static async Task<byte[]> RegInvokeAsync(string command, bool isOversea, byte[]? ADLPROD = null)
        {
            var path = Path.Combine(ApplicationData.Current.LocalCacheFolder.Path, $"Extension/RegInvoke/{XunkongEnvironment.AppVersion}/Xunkong.Desktop.RegInvoke.exe");
#if !DEBUG
            if (!File.Exists(path))
#endif
            {
                Directory.CreateDirectory(Path.GetDirectoryName(path)!);
                File.Copy(Path.Combine(Package.Current.InstalledPath, "Xunkong.Desktop.RegInvoke/Xunkong.Desktop.RegInvoke.exe"), path, true);
            }
            var sb = new StringBuilder();
            sb.Append(command);
            if (isOversea)
            {
                sb.Append(" --isSea");
            }
            if (ADLPROD is { Length: > 0 })
            {
                sb.Append(" --ADLPROD ");
                sb.Append(Convert.ToHexString(ADLPROD));
            }
            var p = new Process
            {
                StartInfo = new ProcessStartInfo
                {
                    FileName = path,
                    Arguments = sb.ToString(),
                    UseShellExecute = false,
                    RedirectStandardOutput = true,
                    StandardOutputEncoding = Encoding.UTF8,
                },
            };
            p.Start();
            using var sw = p.StandardOutput;
            await p.WaitForExitAsync();
            var message = sw.ReadToEnd().Trim();
            byte[] data;
            if (string.IsNullOrWhiteSpace(message))
            {
                throw new Exception("Return message is null.");
            }
            if (p.ExitCode == 0)
            {
                data = Convert.FromHexString(message);
                return data;
            }
            else
            {
                throw new Exception(message);
            }
        }


    }
}
