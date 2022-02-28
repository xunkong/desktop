using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media.Imaging;
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
            WeakReferenceMessenger.Default.Register<WallpaperInfo>(this, (_, e) => ChangeBackgroundWallpaper(e));
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
                var dpi = User32.GetDpiForWindow(Hwnd);
                float scale = dpi / 72;
                var XLength = User32.GetSystemMetrics(User32.SystemMetric.SM_CXSCREEN);
                var YLength = User32.GetSystemMetrics(User32.SystemMetric.SM_CYSCREEN);
                var width = (int)(1280 * scale);
                var height = (int)(720 * scale);
                var x = (XLength - width) / 2;
                var y = (YLength - height) / 2;
                rect = new RECT(x, y, x + width, y + height);
            }
            var wp = new User32.WINDOWPLACEMENT
            {
                length = 44,
                showCmd = ShowWindowCommand.SW_SHOWNORMAL,
                ptMaxPosition = new System.Drawing.Point(-1, -1),
                rcNormalPosition = rect,
            };
            User32.SetWindowPlacement(Hwnd, ref wp);
            User32.SetWindowText(Hwnd, XunkongEnvironment.AppName);
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
        }


        private void _chromeButton_Tapped(object sender, TappedRoutedEventArgs e)
        {
            var point = e.GetPosition(_chromeButton);
            var x = Math.Round(point.X, 0);
            if (x < 40)
            {
                // 最小化
                User32.PostMessage(Hwnd, (uint)User32.WindowMessage.WM_SYSCOMMAND, (IntPtr)0xF020, (IntPtr)0);
            }
            else if (x < 80)
            {
                // 最大化或还原
                var wp = new User32.WINDOWPLACEMENT();
                User32.GetWindowPlacement(Hwnd, ref wp);
                if (wp.showCmd == ShowWindowCommand.SW_MAXIMIZE)
                {
                    User32.PostMessage(Hwnd, (uint)User32.WindowMessage.WM_SYSCOMMAND, (IntPtr)0xF120, (IntPtr)0);
                }
                else
                {
                    User32.PostMessage(Hwnd, (uint)User32.WindowMessage.WM_SYSCOMMAND, (IntPtr)0xF030, (IntPtr)0);
                }
            }
            else
            {
                // 关闭
                Close();
            }
        }



        private void ChangeBackgroundWallpaper(WallpaperInfo image)
        {
            DispatcherQueue.TryEnqueue(() =>
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
            });
            _logger.LogInformation($"Change background image:\n{image.Url}");
        }


    }

}
