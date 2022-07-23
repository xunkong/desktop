using CommunityToolkit.Mvvm.Messaging;
using Microsoft.UI;
using Microsoft.UI.Windowing;
using Microsoft.UI.Xaml;
using Vanara.PInvoke;
using Windows.Graphics;
using WinRT.Interop;
using Xunkong.Desktop.Messages;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace Xunkong.Desktop;

/// <summary>
/// An empty window that can be used on its own or navigated to within a Frame.
/// </summary>
public sealed partial class MainWindow : Window
{

    private IntPtr _hWnd;

    private AppWindow _appWindow;

    private SystemBackdropHelper _backdrop;

    public MainWindow()
    {
        this.InitializeComponent();
        _backdrop = new SystemBackdropHelper(this, BackbdropFallBackBehavior.None);
        if (_backdrop.TrySetBackdrop())
        {
            RootGrid.Background = null;
        }
        InitializeWindowState();
        InitializeMessage();
        MainWindowHelper.Initialize(this, _hWnd, _appWindow!, RootFrame, FullScreenContent);
        NotificationProvider.Initialize(InfoBarContainer);
        Closed += MainWindow_Closed;
    }



    private void InitializeWindowState()
    {
        _hWnd = WindowNative.GetWindowHandle(this);
        var windowId = Win32Interop.GetWindowIdFromWindow(_hWnd);
        _appWindow = AppWindow.GetFromWindowId(windowId);
        // AppWindowTitleBar 的体验不是很好
        //if (AppWindowTitleBar.IsCustomizationSupported())
        //{
        //    var dpi = User32.GetDpiForWindow(_hWnd);
        //    var left = 84 * (int)dpi / 96;
        //    var top = 48 * (int)dpi / 96;
        //    var titleBar = _appWindow.TitleBar;
        //    titleBar.ExtendsContentIntoTitleBar = true;
        //    titleBar.ButtonBackgroundColor = Colors.Transparent;
        //    titleBar.ButtonInactiveBackgroundColor = Colors.Transparent;
        //    _appWindow.TitleBar.SetDragRectangles(new RectInt32[] { new RectInt32(left, 0, _appWindow.Size.Width, top) });
        //}
        //else
        //{
        this.ExtendsContentIntoTitleBar = true;
        this.SetTitleBar(AppTitleBar);
        this.Title = XunkongEnvironment.AppName;
        //}
        if (AppSetting.GetValue<bool>(SettingKeys.IsMainWindowMaximum))
        {
            User32.ShowWindow(_hWnd, ShowWindowCommand.SW_MAXIMIZE);
            return;
        }
        if (AppSetting.TryGetValue<ulong>(SettingKeys.MainWindowRect, out var rect))
        {
            unchecked
            {
                var x = (int)(short)((rect >> 0) & 0xFFFFUL);
                var y = (int)(short)((rect >> 16) & 0xFFFFUL);
                var width = (int)(short)((rect >> 32) & 0xFFFFUL);
                var height = (int)(short)((rect >> 48) & 0xFFFFUL);
                var display = DisplayArea.GetFromWindowId(windowId, DisplayAreaFallback.Primary);
                var workAreaWidth = display.WorkArea.Width;
                var workAreaHeight = display.WorkArea.Height;
                if (x > 0 && y > 0 && width < workAreaWidth && height < workAreaHeight)
                {
                    _appWindow.MoveAndResize(new RectInt32(x, y, width, height));
                }
            }
        }
    }



    private void InitializeMessage()
    {
        WeakReferenceMessenger.Default.Register<ChangeApplicationThemeMessage>(this, (_, e) => ChangeApplicationTheme(e.Theme));
    }



    private void MainWindow_Closed(object sender, WindowEventArgs args)
    {
        SaveWindowState();
        BackupService.AutoBackupDatabase();
    }


    /// <summary>
    /// 保存窗口状态
    /// </summary>
    private void SaveWindowState()
    {
        var wpl = new User32.WINDOWPLACEMENT();
        if (User32.GetWindowPlacement(_hWnd, ref wpl))
        {
            AppSetting.TrySetValue(SettingKeys.IsMainWindowMaximum, wpl.showCmd == ShowWindowCommand.SW_MAXIMIZE);
            unchecked
            {
                ulong rect = 0;
                rect |= ((ulong)(ushort)(short)wpl.rcNormalPosition.X << 0);
                rect |= ((ulong)(ushort)(short)wpl.rcNormalPosition.Y << 16);
                rect |= ((ulong)(ushort)(short)wpl.rcNormalPosition.Width << 32);
                rect |= ((ulong)(ushort)(short)wpl.rcNormalPosition.Height << 48);
                AppSetting.TrySetValue(SettingKeys.MainWindowRect, rect);
            }
        }
    }




    private void ChangeApplicationTheme(int theme)
    {
        var elementTheme = theme switch
        {
            1 => ElementTheme.Light,
            2 => ElementTheme.Dark,
            _ => ElementTheme.Default,
        };
        RootGrid.RequestedTheme = elementTheme;
    }






}
