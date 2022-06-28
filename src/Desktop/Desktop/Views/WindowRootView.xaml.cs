using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Navigation;
using Windows.ApplicationModel;
using Windows.ApplicationModel.DataTransfer;
using Windows.Foundation.Metadata;
using Xunkong.Core.XunkongApi;
using Xunkong.Desktop.Pages;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace Xunkong.Desktop.Views
{
    public sealed partial class WindowRootView : UserControl
    {

        private WindowRootViewModel vm => (DataContext as WindowRootViewModel)!;

        public UIElement AppTitleBar => _appTitleBar;

        private readonly DbConnectionFactory<SqliteConnection> _dbConnectionFactory;

        private readonly ILogger<WindowRootView> _logger;


        public WindowRootView()
        {
            this.InitializeComponent();
            DataContext = ActivatorUtilities.CreateInstance<WindowRootViewModel>(App.Current.Services);
            _dbConnectionFactory = App.Current.Services.GetService<DbConnectionFactory<SqliteConnection>>()!;
            _logger = App.Current.Services.GetService<ILogger<WindowRootView>>()!;
            RegisterMessage();
            Loading += WindowRootView_Loading;
            Loaded += WindowRootView_Loaded;
        }


        private void RegisterMessage()
        {
            WeakReferenceMessenger.Default.Register<RefreshWebToolNavItemMessage>(this, async (_, _) => await RefreshWebToolNavItemAsync());
            WeakReferenceMessenger.Default.Register<NavigateMessage>(this, (_, m) => NavigateTo(m));
            //WeakReferenceMessenger.Default.Register<DisableBackgroundWallpaperMessage>(this, (_, m) => DisableBackgroundWallpaper(m.Disabled));
            WeakReferenceMessenger.Default.Register<OpenOrCloseNavigationPaneMessage>(this, (_, _) =>
            {
                _NavigationView.IsPaneOpen = !_NavigationView.IsPaneOpen;
                LocalSettingHelper.SaveSetting(SettingKeys.NavigationViewPaneClose, !_NavigationView.IsPaneOpen);
            });
        }


        private void WindowRootView_Loading(FrameworkElement sender, object args)
        {
            if (LocalSettingHelper.GetSetting<bool>(SettingKeys.NavigationViewPaneClose))
            {
                _NavigationView.IsPaneOpen = false;
            }
        }


        private async void WindowRootView_Loaded(object sender, RoutedEventArgs e)
        {
            await RefreshWebToolNavItemAsync();
            if (!LocalSettingHelper.GetSetting<bool>(SettingKeys.HasShownWelcomePage))
            {
                NavigationHelper.NavigateTo(typeof(WelcomePage));
            }
            if (LocalSettingHelper.GetSetting<bool>(SettingKeys.DisableBackgroundWallpaper))
            {
                var customPath = LocalSettingHelper.GetSetting<string>("CustomWallpaperPathWhenDisableXunkongWallpaper");
                if (File.Exists(customPath))
                {
                    WeakReferenceMessenger.Default.Send(new WallpaperInfo { Url = customPath });
                }
            }
            else
            {
                await vm.InitializeBackgroundWallpaperAsync();
            }
            await Task.Delay(1000);
            CheckNotifications();
            vm.CheckVersionUpdateAsync();
            GetTitleBarTextAsync();
            vm.CheckWebView2Runtime();
            vm.GetAllGenshinDataAsync();
            await vm.SignInAllAccountsAsync();
        }




        #region NavigationViewTranslation


        private string GetAppTitleFromPackage()
        {
            return Package.Current.DisplayName;
        }


        private void _NavigationView_DisplayModeChanged(NavigationView sender, NavigationViewDisplayModeChangedEventArgs args)
        {
            Thickness currMargin = _appTitleBar.Margin;
            if (sender.DisplayMode == NavigationViewDisplayMode.Minimal)
            {
                _appTitleBar.Margin = new Thickness(sender.CompactPaneLength * 2, currMargin.Top, currMargin.Right, currMargin.Bottom);
                _NavigationView.IsPaneToggleButtonVisible = true;
            }
            else
            {
                _appTitleBar.Margin = new Thickness(sender.CompactPaneLength, currMargin.Top, currMargin.Right, currMargin.Bottom);
                _NavigationView.IsPaneToggleButtonVisible = false;
            }
            //UpdateAppTitleMargin(sender);
        }


        private void _NavigationView_PaneOpening(NavigationView sender, object args)
        {
            //UpdateAppTitleMargin(sender);
        }

        private void _NavigationView_PaneClosing(NavigationView sender, NavigationViewPaneClosingEventArgs args)
        {
            //UpdateAppTitleMargin(sender);
        }

        private void UpdateAppTitleMargin(NavigationView sender)
        {
            const int smallLeftIndent = 4, largeLeftIndent = 24;

            if (ApiInformation.IsApiContractPresent("Windows.Foundation.UniversalApiContract", 7))
            {
                AppTitle.TranslationTransition = new Vector3Transition();

                if ((sender.DisplayMode == NavigationViewDisplayMode.Expanded && sender.IsPaneOpen) ||
                         sender.DisplayMode == NavigationViewDisplayMode.Minimal)
                {
                    AppTitle.Translation = new System.Numerics.Vector3(smallLeftIndent, 0, 0);
                }
                else
                {
                    AppTitle.Translation = new System.Numerics.Vector3(largeLeftIndent, 0, 0);
                }
            }
            else
            {
                Thickness currMargin = AppTitle.Margin;

                if ((sender.DisplayMode == NavigationViewDisplayMode.Expanded && sender.IsPaneOpen) ||
                         sender.DisplayMode == NavigationViewDisplayMode.Minimal)
                {
                    AppTitle.Margin = new Thickness(smallLeftIndent, currMargin.Top, currMargin.Right, currMargin.Bottom);
                }
                else
                {
                    AppTitle.Margin = new Thickness(largeLeftIndent, currMargin.Top, currMargin.Right, currMargin.Bottom);
                }
            }
        }





        #endregion




        private void _NavigationView_ItemInvoked(NavigationView sender, NavigationViewItemInvokedEventArgs args)
        {
            try
            {
                if (args.InvokedItemContainer.IsSelected)
                {
                    return;
                }
                if (args.IsSettingsInvoked)
                {
                    _logger.LogInformation("Navigate to {PageName}", "SettingPage");
                    _rootFrame.Navigate(typeof(SettingPage));
                }
                else
                {
                    if (args.InvokedItemContainer.DataContext is WebToolItem webToolItem)
                    {
                        _logger.LogInformation("Navigate to {PageName} with title {Title} and url {Url}", "WebToolPage", webToolItem.Title, webToolItem.Url);
                        _rootFrame.Navigate(typeof(WebToolPage), webToolItem);
                    }
                    else
                    {
                        var tag = args.InvokedItemContainer.Tag as string;
                        if (tag == "HomePage")
                        {
                            var webTool_HomePage = new WebToolItem { Id = -1, Url = "https://xunkong.cc" };
                            _rootFrame.Navigate(typeof(WebToolPage), webTool_HomePage);
                            return;
                        }
                        var asm = typeof(WindowRootView).Assembly;
                        var type = asm.GetType($"Xunkong.Desktop.Pages.{tag}");
                        if (type is not null)
                        {
                            _logger.LogInformation("Navigate to {PageName}", tag);
                            _rootFrame.Navigate(type);
                        }
                        else
                        {
                            _logger.LogWarning("Navigate to unfount page {PageName}", tag);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                InfoBarHelper.Error(ex, $"Naviate to page {args.InvokedItemContainer.Tag}");
                _logger.LogError(ex, $"Naviate to page {args.InvokedItemContainer.Tag}");
            }
        }



        private void _NavigationView_BackRequested(NavigationView sender, NavigationViewBackRequestedEventArgs args)
        {
            TryGoBack();
        }


        private bool TryGoBack()
        {
            if (_rootFrame.CanGoBack)
            {
                _rootFrame.GoBack();
                _NavigationView.SelectedItem = null;
                return true;
            }
            else
            {
                return false;
            }
        }



        private void NavigateTo(NavigateMessage message)
        {
            _logger.LogInformation("Navigate to {PageName} with paramter {Parameter}", message.Type.Name, message.Parameter);
            _rootFrame.DispatcherQueue.TryEnqueue(() => _rootFrame.Navigate(message.Type, message.Parameter, message.TransitionInfo));
        }




        private async Task RefreshWebToolNavItemAsync()
        {
            const string WEBTOOL = "WebTool";
            try
            {
                var removing = _NavigationView.MenuItems.Where(x => (x as FrameworkElement)?.Tag as string == WEBTOOL).ToList();
                foreach (var item in removing)
                {
                    _NavigationView.MenuItems.Remove(item);
                }
                using var con = _dbConnectionFactory.CreateDbConnection();
                var sql = "SELECT * FROM WebToolItems ORDER BY [Order]";
                var list = await con.QueryAsync<WebToolItem>(sql);
                if (list.Any())
                {
                    _NavigationView.MenuItems.Add(new NavigationViewItemSeparator { Tag = WEBTOOL });
                    _NavigationView.MenuItems.Add(new NavigationViewItemHeader { Content = "小工具", Tag = WEBTOOL });
                    foreach (var item in list)
                    {
                        _NavigationView.MenuItems.Add(new NavigationViewItem
                        {
                            Icon = new BitmapIcon { UriSource = new Uri(item.Icon ?? ""), ShowAsMonochrome = false },
                            Content = item.Title,
                            DataContext = item,
                            Tag = WEBTOOL,
                        });
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in {MethodName}", nameof(RefreshWebToolNavItemAsync));
                InfoBarHelper.Error(ex, $"加载网页小工具");
            }
        }






        private void CheckNotifications()
        {
            Task.Run(async () =>
            {
                try
                {
                    var xunkongApiService = App.Current.Services.GetService<XunkongApiService>()!;
                    var hasUnread = await xunkongApiService.HasUnreadNotification();
                    if (hasUnread)
                    {
                        _Badge_Notification.DispatcherQueue.TryEnqueue(() => _Badge_Notification.Visibility = Visibility.Visible);
                    }
                    var channel = XunkongEnvironment.Channel;
                    var version = XunkongEnvironment.AppVersion;
                    var hasNew = await xunkongApiService.GetNotificationsAsync(channel, version);
                    if (hasNew)
                    {
                        _Badge_Notification.DispatcherQueue.TryEnqueue(() =>
                        {
                            _Badge_Notification.Visibility = Visibility.Visible;
                            _TeachingTip_NewNotification.IsOpen = true;
                        });
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error in {MethodName}", nameof(CheckNotifications));
                }
            });
        }



        private async void GetTitleBarTextAsync()
        {
            try
            {
                var client = App.Current.Services.GetService<HttpClient>();
                client!.DefaultRequestHeaders.Add("User-Agent", "Xunkong.Desktop/AppTitleBarText");
                var text = await client.GetStringAsync("https://api.xunkong.cc/v0.1/desktop/titlebartext");
                TextBlock_TitleBar.Text = text;
            }
            catch { }
        }



        private void _Flyout_Notification_Opened(object sender, object e)
        {
            _Badge_Notification.Visibility = Visibility.Collapsed;
        }

        private void _rootFrame_Navigating(object sender, NavigatingCancelEventArgs e)
        {
            if (_rootFrame.Background is null)
            {
                _rootFrame.Background = Application.Current.Resources["CustomAcrylicBrush"] as Brush;
            }
        }

        private void _rootFrame_NavigationFailed(object sender, NavigationFailedEventArgs e)
        {
            InfoBarHelper.Error(e.Exception.Message);
            _logger.LogError(e.Exception, "Navigation failed.");
            e.Handled = true;
        }

        private void _PaneFooter_BackgroundWallpaper_DragOver(object sender, DragEventArgs e)
        {
            e.AcceptedOperation = DataPackageOperation.Copy;
            e.DragUIOverride.IsCaptionVisible = false;
            e.DragUIOverride.IsGlyphVisible = false;
        }

        private async void _PaneFooter_BackgroundWallpaper_Drop(object sender, DragEventArgs e)
        {
            if (e.DataView.Contains(StandardDataFormats.StorageItems))
            {
                var items = await e.DataView.GetStorageItemsAsync();
                if (items.Any())
                {
                    var file = items.FirstOrDefault()?.Path;
                    if (!string.IsNullOrWhiteSpace(file))
                    {
                        try
                        {
                            using var stream = File.OpenRead(file);
                            var format = SixLabors.ImageSharp.Image.DetectFormatAsync(stream);
                            if (format.Result is not null)
                            {
                                WeakReferenceMessenger.Default.Send(new WallpaperInfo { Url = file });
                                if (LocalSettingHelper.GetSetting<bool>(SettingKeys.DisableBackgroundWallpaper))
                                {
                                    LocalSettingHelper.SaveSetting("CustomWallpaperPathWhenDisableXunkongWallpaper", file);
                                }
                            }
                        }
                        catch (Exception ex)
                        {
                            _logger.LogError(ex, "Change background wallpaper from local file.");
                        }
                    }
                }
            }
        }



        private void DisableBackgroundWallpaper(bool disabled)
        {
            if (disabled)
            {
                _PaneFooter_BackgroundWallpaper.Visibility = Visibility.Collapsed;
            }
            else
            {
                _PaneFooter_BackgroundWallpaper.Visibility = Visibility.Visible;
            }
        }


        private bool enableHideElement;

        private async void _Button_ResizeWindow_PointerEntered(object sender, Microsoft.UI.Xaml.Input.PointerRoutedEventArgs e)
        {
            enableHideElement = true;
            await Task.Delay(300);
            if (enableHideElement)
            {
                WeakReferenceMessenger.Default.Send(new HideElementMessage(true));
            }
        }

        private void _Button_ResizeWindow_PointerExited(object sender, Microsoft.UI.Xaml.Input.PointerRoutedEventArgs e)
        {
            enableHideElement = false;
            WeakReferenceMessenger.Default.Send(new HideElementMessage(false));
        }


    }


}
