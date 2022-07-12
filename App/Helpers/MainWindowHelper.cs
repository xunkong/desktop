using Microsoft.UI.Dispatching;
using Microsoft.UI.Windowing;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media.Animation;
using Vanara.PInvoke;

namespace Xunkong.Desktop.Helpers
{
    internal static class MainWindowHelper
    {

        private static IntPtr _hWnd;

        private static MainWindow _mainWindow;

        private static AppWindow _appWindow;

        private static Frame _rootFrame;

        private static ContentControl _fullScreenContent;

        public static DispatcherQueue DispatcherQueue => _mainWindow.DispatcherQueue;

        public static IntPtr HWND => _hWnd;
        public static XamlRoot XamlRoot => _mainWindow.Content.XamlRoot;

        private static double uiScale;

        /// <summary>
        /// UI 缩放率
        /// </summary>
        public static double UIScale
        {
            get
            {
                if (uiScale == 0)
                {
                    uiScale = (double)User32.GetDpiForWindow(_hWnd) / 96;
                }
                return uiScale;
            }
        }


        public static void Initialize(MainWindow mainWindow, IntPtr hwnd, AppWindow appWindow, Frame frame, ContentControl content)
        {
            _hWnd = hwnd;
            _mainWindow = mainWindow;
            _appWindow = appWindow;
            _rootFrame = frame;
            _fullScreenContent = content;
        }


        public static void SetDragRectangles(int leftMargin)
        {
            // AppWindowTitleBar 的体验不是很好
            //if (AppWindowTitleBar.IsCustomizationSupported())
            //{
            //    var dpi = User32.GetDpiForWindow(_hWnd);
            //    var left = leftMargin * (int)dpi / 96;
            //    var titleBar = _appWindow.TitleBar;
            //    titleBar.ResetToDefault();
            //    titleBar.ExtendsContentIntoTitleBar = true;
            //    titleBar.ButtonBackgroundColor = Colors.Transparent;
            //    titleBar.ButtonInactiveBackgroundColor = Colors.Transparent;
            //    titleBar.SetDragRectangles(new RectInt32[] { new RectInt32(left, 0, _appWindow.Size.Width, 48 * (int)dpi / 96) });
            //}
        }



        public static void SetTitleBar(UIElement titleBar)
        {
            //if (!AppWindowTitleBar.IsCustomizationSupported())
            //{
            _mainWindow.SetTitleBar(titleBar);
            //}
        }



        public static void Navigate(Type sourcePageType, object? param = null, NavigationTransitionInfo? infoOverride = null)
        {
            if (param is null)
            {
                _rootFrame.Navigate(sourcePageType);
            }
            else if (infoOverride is null)
            {
                _rootFrame.Navigate(sourcePageType, param);
            }
            else
            {
                _rootFrame.Navigate(sourcePageType, param, infoOverride);
            }
        }


        public static void OpenFullScreen(Control content)
        {
            _fullScreenContent.Visibility = Visibility.Visible;
            _fullScreenContent.Content = content;
        }


        public static void CloseFullScreen()
        {
            _fullScreenContent.Content = null;
            _fullScreenContent.Visibility = Visibility.Collapsed;
        }





    }
}
