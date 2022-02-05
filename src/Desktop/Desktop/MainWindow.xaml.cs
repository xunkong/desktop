using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using Vanara.PInvoke;
using Windows.Storage;
using WinRT.Interop;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace Xunkong.Desktop
{
    /// <summary>
    /// An empty window that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainWindow : Window, INotifyPropertyChanged
    {

        public event PropertyChangedEventHandler? PropertyChanged;
        private void OnPropertyChanged([CallerMemberName] string propertyName = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }



        public static new Window Current { get; private set; }

        public static XamlRoot XamlRoot => Current.Content.XamlRoot;

        public static IntPtr Hwnd { get; private set; }

        public MainWindow()
        {
            Current = this;
            Closed += MainWindow_Closed;
            this.InitializeComponent();
            ExtendsContentIntoTitleBar = true;
            SetTitleBar(_rootView.AppTitleBar);
            InfoBarHelper.Initialize(_InfoBarContainer);
            Hwnd = WindowNative.GetWindowHandle(this);
            InitializeWindowSize();
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
                showCmd = ShowWindowCommand.SW_NORMAL,
                ptMaxPosition = new System.Drawing.Point(-1, -1),
                rcNormalPosition = rect,
            };
            User32.SetWindowPlacement(Hwnd, ref wp);
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







    }
}
