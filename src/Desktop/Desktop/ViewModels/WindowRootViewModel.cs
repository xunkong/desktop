using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.EntityFrameworkCore;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml;
using Xunkong.Core.Hoyolab;
using Xunkong.Desktop.Services;
using System.Net;
using System.Collections.ObjectModel;
using System.Diagnostics;
using Xunkong.Core.XunkongApi;
using CommunityToolkit.Mvvm.Messaging.Messages;
using Microsoft.UI.Dispatching;
using CommunityToolkit.WinUI.UI;

namespace Xunkong.Desktop.ViewModels
{

    [InjectService]
    internal partial class WindowRootViewModel : ObservableObject
    {

        private readonly ILogger<WindowRootViewModel> _logger;

        private readonly IDbContextFactory<XunkongDbContext> _dbFactory;

        private readonly UserSettingService _userSettingService;

        private readonly HoyolabService _hoyolabService;

        private readonly XunkongApiService _xunkongApiService;



        public WindowRootViewModel(ILogger<WindowRootViewModel> logger, IDbContextFactory<XunkongDbContext> dbFactory, UserSettingService userSettingService, HoyolabService hoyolabService, XunkongApiService xunkongApiService)
        {
            _logger = logger;
            _dbFactory = dbFactory;
            _userSettingService = userSettingService;
            _hoyolabService = hoyolabService;
            _xunkongApiService = xunkongApiService;
        }



        public void CheckWebView2Runtime()
        {
            try
            {
                _ = Microsoft.Web.WebView2.Core.CoreWebView2Environment.GetAvailableBrowserVersionString();
            }
            catch
            {
                const string url = "https://go.microsoft.com/fwlink/p/?LinkId=2124703";
                RoutedEventHandler eventHandler = (_, _) => Process.Start(new ProcessStartInfo { FileName = url, UseShellExecute = true, });
                InfoBarHelper.ShowWithButton(InfoBarSeverity.Warning, "警告", "没有找到 WebView2 运行时，会影响软件必要的功能。", "下载", eventHandler);
            }
        }




        public async void CheckVersionUpdateAsync()
        {
            if (XunkongEnvironment.Channel == ChannelType.Development)
            {
                try
                {
                    var version = await _xunkongApiService.CheckUpdateAsync(ChannelType.Development);
                    if (version.Version > XunkongEnvironment.AppVersion && !string.IsNullOrWhiteSpace(version.PackageUrl))
                    {
                        RoutedEventHandler eventHandler = (_, _) => Process.Start(new ProcessStartInfo { FileName = version.PackageUrl, UseShellExecute = true, });
                        InfoBarHelper.ShowWithButton(InfoBarSeverity.Success, $"新版本 {version.Version}", version.Abstract, "下载", eventHandler);
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error when check version update startup in {MethodName}", nameof(CheckVersionUpdateAsync));
                }
            }
        }




        private WallpaperInfo? _BackgroundWallpaper;
        public WallpaperInfo? BackgroundWallpaper
        {
            get => _BackgroundWallpaper;
            set => SetProperty(ref _BackgroundWallpaper, value);
        }


        [ICommand(AllowConcurrentExecutions = false)]
        public async Task ChangeBackgroundWallpaperAsync(string randomOrNext)
        {
            if (randomOrNext == "next")
            {
                await ChangeBackgroundWallpaperAsync(1, true);
            }
            else
            {
                await ChangeBackgroundWallpaperAsync(0, true);
            }
        }

        private async Task ChangeBackgroundWallpaperAsync(int randomOrNext = 0, bool showError = false)
        {
            try
            {
                var image = await _xunkongApiService.GetWallpaperInfoAsync(randomOrNext, BackgroundWallpaper?.Id ?? 0);
                if (!string.IsNullOrWhiteSpace(image?.Url))
                {
                    WeakReferenceMessenger.Default.Send(image);
                    BackgroundWallpaper = image;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Refresh app background image.");
                if (showError)
                {
                    InfoBarHelper.Error(ex);
                }
            }
        }



        [ICommand(AllowConcurrentExecutions = false)]
        private async Task SaveBackgroundWallpaperAsync()
        {
            if (string.IsNullOrWhiteSpace(BackgroundWallpaper?.Url))
            {
                return;
            }
            try
            {
                var storageFile = await ImageCache.Instance.GetFileFromCacheAsync(new Uri(BackgroundWallpaper.Url));
                var sourcePath = storageFile?.Path;
                if (string.IsNullOrWhiteSpace(sourcePath))
                {
                    InfoBarHelper.Warning("无法下载或缓存失效");
                    return;
                }
                if (!File.Exists(sourcePath))
                {
                    InfoBarHelper.Warning("找不到文件");
                    return;
                }
                var destFolder = Path.Combine(XunkongEnvironment.UserDataPath, "Wallpaper");
                var fileName = BackgroundWallpaper.FileName ?? Path.GetFileName(BackgroundWallpaper.Url);
                var destPath = Path.Combine(destFolder, fileName);
                Directory.CreateDirectory(destFolder);
                File.Copy(sourcePath, destPath, true);
                RoutedEventHandler eventHandler = (_, _) => Process.Start(new ProcessStartInfo { FileName = destPath, UseShellExecute = true });
                InfoBarHelper.ShowWithButton(InfoBarSeverity.Success, "已保存", fileName, "打开文件", eventHandler, 3000);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Save background wallpaper.");
                InfoBarHelper.Error(ex);
            }
        }




    }
}
