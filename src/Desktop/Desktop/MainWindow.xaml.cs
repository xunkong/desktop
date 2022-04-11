﻿using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media.Imaging;
using System.Reflection;
using Vanara.PInvoke;
using WinRT.Interop;
using Xunkong.Core.XunkongApi;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace Xunkong.Desktop
{
    /// <summary>
    /// An empty window that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainWindow : Window
    {


        private readonly ILogger<MainWindow> _logger;



        public static new Window Current { get; private set; }

        public static XamlRoot XamlRoot => Current.Content.XamlRoot;

        public static IntPtr Hwnd { get; private set; }



        public MainWindow()
        {
            Current = this;
            Closed += MainWindow_Closed;
            _logger = App.Current.Services.GetService<ILogger<MainWindow>>()!;
            this.InitializeComponent();
            ExtendsContentIntoTitleBar = true;
            SetTitleBar(_rootView.AppTitleBar);
            InfoBarHelper.Initialize(_InfoBarContainer);
            Hwnd = WindowNative.GetWindowHandle(this);
            InitializeWindowSize();
            RegisterMessage();
        }


        private void RegisterMessage()
        {
            WeakReferenceMessenger.Default.Register<WallpaperInfo>(this, (_, e) => ChangeBackgroundWallpaper(e));
            WeakReferenceMessenger.Default.Register<ChangeApplicationThemeMessage>(this, (_, e) => ChangeApplicationTheme(e.ThemeIndex));
            WeakReferenceMessenger.Default.Register<DisableBackgroundWallpaperMessage>(this, (_, e) => DisableBackgroundWallpaper(e.Disabled));
            WeakReferenceMessenger.Default.Register<HideElementMessage>(this, (_, e) => HideUIElement(e.HideElement));
            WeakReferenceMessenger.Default.Register<ResizeWindowToImageMessage>(this, (_, _) => ResizeWindowToImage());
        }



        private void InitializeWindowSize()
        {
            // 无法在启动时最大化窗口
            //var showCmd = LocalSettingHelper.GetSetting<bool>(SettingKeys.IsWindowMax) ? ShowWindowCommand.SW_MAXIMIZE : ShowWindowCommand.SW_NORMAL;
            var left = LocalSettingHelper.GetSetting<int>(SettingKeys.WindowLeft);
            var top = LocalSettingHelper.GetSetting<int>(SettingKeys.WindowTop);
            var right = LocalSettingHelper.GetSetting<int>(SettingKeys.WindowRight);
            var bottom = LocalSettingHelper.GetSetting<int>(SettingKeys.WindowBottom);
            var rect = new RECT(left, top, right, bottom);
            if (rect.Width * rect.Height == 0)
            {
                return;
            }
            var wp = new User32.WINDOWPLACEMENT
            {
                length = 44,
                showCmd = ShowWindowCommand.SW_SHOWNORMAL,
                ptMaxPosition = new System.Drawing.Point(-1, -1),
                rcNormalPosition = rect,
            };

            User32.SetWindowPlacement(Hwnd, ref wp);
            User32.SetWindowText(Hwnd, XunkongEnvironment.Channel == ChannelType.Development ? "寻空 开发版" : "寻空");
        }



        private void MainWindow_Closed(object sender, WindowEventArgs args)
        {
            var wp = new User32.WINDOWPLACEMENT();
            User32.GetWindowPlacement(Hwnd, ref wp);
            // 无法在启动时最大化窗口
            //LocalSettingHelper.SaveSetting(SettingKeys.IsWindowMax, wp.showCmd == ShowWindowCommand.SW_MAXIMIZE);
            LocalSettingHelper.SaveSetting(SettingKeys.WindowLeft, wp.rcNormalPosition.left);
            LocalSettingHelper.SaveSetting(SettingKeys.WindowTop, wp.rcNormalPosition.top);
            LocalSettingHelper.SaveSetting(SettingKeys.WindowRight, wp.rcNormalPosition.right);
            LocalSettingHelper.SaveSetting(SettingKeys.WindowBottom, wp.rcNormalPosition.bottom);
            ActivatorUtilities.GetServiceOrCreateInstance<BackupService>(App.Current.Services)?.AutoBackupDatabase();
            _logger.LogInformation("MainWindow closed.");
        }



        private void ChangeBackgroundWallpaper(WallpaperInfo image)
        {
            DispatcherQueue.TryEnqueue(() =>
            {
                _Image_Background.Visibility = Visibility.Visible;
                if (string.IsNullOrWhiteSpace(image.Url))
                {
                    _Image_Background.Source = null;
                }
                else
                {
                    var uri = new Uri(image.Url);
                    if (uri.Scheme == Uri.UriSchemeFile)
                    {
                        var source = new BitmapImage { UriSource = uri };
                        _Image_Background.Source = source;
                    }
                    else
                    {
                        _Image_Background.Source = image.Url;
                    }
                }
            });
            _logger.LogInformation($"Change background image:\n{image.Url}");
        }



        private void ChangeApplicationTheme(int themeIndex)
        {
            var theme = themeIndex switch
            {
                1 => ElementTheme.Light,
                2 => ElementTheme.Dark,
                _ => ElementTheme.Default,
            };
            _onImageBackgroud.RequestedTheme = theme;
            _rootView.RequestedTheme = theme;
            _InfoBarContainer.RequestedTheme = theme;
        }


        private void DisableBackgroundWallpaper(bool disabled)
        {
            if (disabled)
            {
                _Image_Background.Visibility = Visibility.Collapsed;
            }
            else
            {
                _Image_Background.Visibility = Visibility.Visible;
            }
        }


        private void HideUIElement(bool hide)
        {
            if (_Image_Background.Visibility == Visibility.Collapsed)
            {
                _onImageBackgroud.Opacity = 1;
                _InfoBarContainer.Opacity = 1;
                _rootView.Opacity = 1;
                return;
            }
            if (hide)
            {
                _onImageBackgroud.Opacity = 0;
                _InfoBarContainer.Opacity = 0;
                _rootView.Opacity = 0;
            }
            else
            {
                _onImageBackgroud.Opacity = 1;
                _InfoBarContainer.Opacity = 1;
                _rootView.Opacity = 1;
            }
        }


        private void ResizeWindowToImage()
        {
            var image = _Image_Background.GetType().GetProperty("Image", BindingFlags.Instance | BindingFlags.NonPublic)?.GetValue(_Image_Background) as Image;
            var source = image?.Source as BitmapImage;
            if (source is null)
            {
                return;
            }
            var imageHeight = source.PixelHeight;
            var imageWidth = source.PixelWidth;
            if (imageHeight * imageWidth == 0)
            {
                return;
            }
            var wp = new User32.WINDOWPLACEMENT();
            User32.GetWindowPlacement(Hwnd, ref wp);
            var imageWidthDivHeight = (double)imageWidth / imageHeight;
            var windowWidthDivHeight = (double)wp.rcNormalPosition.Width / wp.rcNormalPosition.Height;
            User32.SystemParametersInfo<RECT>(User32.SPI.SPI_GETWORKAREA, out var workerArea);
            var XLength = workerArea.Width;
            var YLength = workerArea.Height;
            double width, height;
            if (windowWidthDivHeight > imageWidthDivHeight)
            {
                // 宽不变，高变大
                width = wp.rcNormalPosition.Width;
                height = width / imageWidthDivHeight;
            }
            else
            {
                // 高不变，宽变大
                height = wp.rcNormalPosition.Height;
                width = height * imageWidthDivHeight;
            }
            // 超出屏幕范围，则缩小至适应屏幕
            if (width > XLength || height > YLength)
            {
                var screenWidthDivHeight = (double)XLength / YLength;
                if (screenWidthDivHeight > imageWidthDivHeight)
                {
                    height = YLength;
                    width = height * imageWidthDivHeight;
                }
                else
                {
                    width = XLength;
                    height = width / imageWidthDivHeight;
                }
            }
            var x = (XLength - width) / 2 + workerArea.X;
            var y = (YLength - height) / 2 + workerArea.Y;
            var rect = new RECT((int)x, (int)y, (int)(x + width), (int)(y + height));
            wp.showCmd = ShowWindowCommand.SW_SHOWNORMAL;
            wp.rcNormalPosition = rect;
            User32.SetWindowPlacement(Hwnd, ref wp);
        }


    }

}
