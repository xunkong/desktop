using CommunityToolkit.WinUI.UI;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Xunkong.Core.XunkongApi;

namespace Xunkong.Desktop.ViewModels
{

    internal partial class WindowRootViewModel : ObservableObject
    {

        private readonly ILogger<WindowRootViewModel> _logger;

        private readonly IDbContextFactory<XunkongDbContext> _dbFactory;

        private readonly UserSettingService _userSettingService;

        private readonly HoyolabService _hoyolabService;

        private readonly XunkongApiService _xunkongApiService;



        public WindowRootViewModel(ILogger<WindowRootViewModel> logger,
                                   IDbContextFactory<XunkongDbContext> dbFactory,
                                   UserSettingService userSettingService,
                                   HoyolabService hoyolabService,
                                   XunkongApiService xunkongApiService)
        {
            _logger = logger;
            _dbFactory = dbFactory;
            _userSettingService = userSettingService;
            _hoyolabService = hoyolabService;
            _xunkongApiService = xunkongApiService;
            WeakReferenceMessenger.Default.Register<DisableBackgroundWallpaperMessage>(this, (_, e) => DisableBackgroundWallpaper(e.Disabled));
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
                Action action = () => Process.Start(new ProcessStartInfo { FileName = url, UseShellExecute = true, });
                InfoBarHelper.ShowWithButton(InfoBarSeverity.Warning, "警告", "没有找到 WebView2 运行时，会影响软件必要的功能。", "下载", action);
            }
        }




        public async void CheckVersionUpdateAsync()
        {
            if (XunkongEnvironment.Channel != ChannelType.Development)
            {
                try
                {
                    var version = await _xunkongApiService.CheckUpdateAsync(XunkongEnvironment.Channel);
                    Version.TryParse(LocalSettingHelper.GetSetting<string>("LastTestUpdateVersion"), out var lastVersion);
                    if (version.Version > XunkongEnvironment.AppVersion && version.Version > lastVersion)
                    {
                        var button = new Button
                        {
                            Content = "下载",
                            HorizontalAlignment = HorizontalAlignment.Right,
                            Style = Application.Current.Resources["DateTimePickerFlyoutButtonStyle"] as Style,
                        };
                        button.Click += (_, _) => Process.Start(new ProcessStartInfo { FileName = version.PackageUrl, UseShellExecute = true, });
                        var infobar = new InfoBar
                        {
                            Severity = InfoBarSeverity.Success,
                            Title = $"新版本 {version.Version}",
                            Message = version.Abstract,
                            IsOpen = true,
                        };
                        infobar.Closed += (_, _) => LocalSettingHelper.SaveSetting("LastTestUpdateVersion", version.Version.ToString());
                        InfoBarHelper.Show(infobar);
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, $"Error when check version update startup in {nameof(CheckVersionUpdateAsync)}");
                }
            }
        }




        private WallpaperInfo? _BackgroundWallpaper;
        public WallpaperInfo? BackgroundWallpaper
        {
            get => _BackgroundWallpaper;
            set => SetProperty(ref _BackgroundWallpaper, value);
        }


        public async Task InitializeBackgroundWallpaperAsync()
        {
            try
            {
                var lastUrl = LocalSettingHelper.GetSetting<string>("LastSavedWallpaperInfo");
                if (!string.IsNullOrWhiteSpace(lastUrl))
                {
                    WeakReferenceMessenger.Default.Send(new WallpaperInfo { Url = lastUrl });
                }
                var image = await _xunkongApiService.GetRecommendWallpaperAsync();
                BackgroundWallpaper = image;
                WeakReferenceMessenger.Default.Send(image);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Initialize and get recommend wallpaper.");
            }
        }



        [ICommand(AllowConcurrentExecutions = false)]
        private async Task GetRandomBackgroudWallpaperAsync()
        {
            try
            {
                var image = await _xunkongApiService.GetRandomWallpaperAsync();
                if (!string.IsNullOrWhiteSpace(image?.Url))
                {
                    BackgroundWallpaper = image;
                    WeakReferenceMessenger.Default.Send(image);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Get random background image.");
                InfoBarHelper.Error(ex);
            }
        }


        [ICommand(AllowConcurrentExecutions = false)]
        private async Task GetNextBackgroudWallpaperAsync()
        {
            try
            {
                var image = await _xunkongApiService.GetNextWallpaperAsync(BackgroundWallpaper?.Id ?? 0);
                if (!string.IsNullOrWhiteSpace(image?.Url))
                {
                    BackgroundWallpaper = image;
                    WeakReferenceMessenger.Default.Send(image);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Get next background image.");
                InfoBarHelper.Error(ex);
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
                Action action = () => Process.Start(new ProcessStartInfo { FileName = destPath, UseShellExecute = true });
                InfoBarHelper.ShowWithButton(InfoBarSeverity.Success, "已保存", fileName, "打开文件", action, 3000);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Save background wallpaper.");
                InfoBarHelper.Error(ex);
            }
        }


        [ICommand]
        private void ResizeWindowToImage()
        {
            WeakReferenceMessenger.Default.Send(new ResizeWindowToImageMessage());
        }


        private async void DisableBackgroundWallpaper(bool disabled)
        {
            if (!disabled && BackgroundWallpaper is null)
            {
                await InitializeBackgroundWallpaperAsync();
            }
        }





    }
}
