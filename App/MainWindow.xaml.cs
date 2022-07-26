using CommunityToolkit.Mvvm.Messaging;
using Microsoft.UI;
using Microsoft.UI.Windowing;
using Microsoft.UI.Xaml;
using System.Runtime.InteropServices;
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
        if (AppSetting.TryGetValue<ulong>(SettingKeys.MainWindowRect, out var value))
        {
            var display = DisplayArea.GetFromWindowId(windowId, DisplayAreaFallback.Primary);
            var workAreaWidth = display.WorkArea.Width;
            var workAreaHeight = display.WorkArea.Height;
            var rect = new WindowRect(value);
            if (rect.Left > 0 && rect.Top > 0 && rect.Right < workAreaWidth && rect.Bottom < workAreaHeight)
            {
                _appWindow.MoveAndResize(rect.ToRectInt32());
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
            var p = _appWindow.Position;
            var s = _appWindow.Size;
            var rect = new WindowRect(p.X, p.Y, s.Width, s.Height);
            AppSetting.TrySetValue(SettingKeys.MainWindowRect, rect.Value, true);
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



    [StructLayout(LayoutKind.Explicit)]
    private struct WindowRect
    {
        [FieldOffset(0)]
        public short X;

        [FieldOffset(2)]
        public short Y;

        [FieldOffset(4)]
        public short Width;

        [FieldOffset(6)]
        public short Height;

        [FieldOffset(0)]
        public ulong Value;

        public int Left => X;

        public int Top => Y;

        public int Right => X + Width;

        public int Bottom => Y + Height;

        public WindowRect(int x, int y, int width, int height)
        {
            Value = 0;
            X = (short)x;
            Y = (short)y;
            Width = (short)width;
            Height = (short)height;
        }

        public WindowRect(ulong value)
        {
            X = 0;
            Y = 0;
            Width = 0;
            Height = 0;
            Value = value;
        }

        public RectInt32 ToRectInt32()
        {
            return new RectInt32(X, Y, Width, Height);
        }

    }




}
