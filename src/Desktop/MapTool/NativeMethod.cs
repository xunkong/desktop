using System;
using System.Runtime.InteropServices;
using System.Security;

namespace Xunkong.Desktop.MapTool
{
    internal static class NativeMethod
    {

        [DllImport("user32.dll", ExactSpelling = true, SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static extern bool RegisterHotKey(IntPtr hWnd, int id, HotKeyModifiers fsModifiers, uint vk);


        [DllImport("user32.dll", ExactSpelling = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static extern bool SetForegroundWindow(IntPtr hWnd);


        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        [SecurityCritical]
        public static extern int GetWindowLong(IntPtr hWnd, WindowLongFlags nIndex);


        [DllImport("user32.dll", EntryPoint = "SetWindowLongPtr", SetLastError = true)]
        public static extern IntPtr SetWindowLong(IntPtr hWnd, WindowLongFlags nIndex, uint dwNewLong);

        [DllImport("user32.dll", ExactSpelling = true, SetLastError = true)]
        [SecurityCritical]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static extern bool GetWindowRect(IntPtr hWnd, out RECT lpRect);

    }



    [Flags]
    public enum HotKeyModifiers
    {
        MOD_NONE = 0x0,
        MOD_ALT = 0x1,
        MOD_CONTROL = 0x2,
        MOD_SHIFT = 0x4,
        MOD_WIN = 0x8,
        MOD_NOREPEAT = 0x4000
    }


    public enum WindowLongFlags
    {
        GWL_EXSTYLE = -20,
        GWL_HINSTANCE = -6,
        GWL_HWNDPARENT = -8,
        GWL_ID = -12,
        GWL_STYLE = -16,
        GWL_USERDATA = -21,
        GWL_WNDPROC = -4,
        DWLP_USER = 8,
        DWLP_MSGRESULT = 0,
        DWLP_DLGPROC = 4
    }


    [Flags]
    public enum WindowStylesEx : uint
    {
        WS_EX_ACCEPTFILES = 0x10u,
        WS_EX_APPWINDOW = 0x40000u,
        WS_EX_CLIENTEDGE = 0x200u,
        WS_EX_COMPOSITED = 0x2000000u,
        WS_EX_CONTEXTHELP = 0x400u,
        WS_EX_CONTROLPARENT = 0x10000u,
        WS_EX_DLGMODALFRAME = 0x1u,
        WS_EX_LAYERED = 0x80000u,
        WS_EX_LAYOUTRTL = 0x400000u,
        WS_EX_LEFT = 0x0u,
        WS_EX_LEFTSCROLLBAR = 0x4000u,
        WS_EX_LTRREADING = 0x0u,
        WS_EX_MDICHILD = 0x40u,
        WS_EX_NOACTIVATE = 0x8000000u,
        WS_EX_NOINHERITLAYOUT = 0x100000u,
        WS_EX_NOPARENTNOTIFY = 0x4u,
        WS_EX_NOREDIRECTIONBITMAP = 0x200000u,
        WS_EX_OVERLAPPEDWINDOW = 0x300u,
        WS_EX_PALETTEWINDOW = 0x188u,
        WS_EX_RIGHT = 0x1000u,
        WS_EX_RIGHTSCROLLBAR = 0x0u,
        WS_EX_RTLREADING = 0x2000u,
        WS_EX_STATICEDGE = 0x20000u,
        WS_EX_TOOLWINDOW = 0x80u,
        WS_EX_TOPMOST = 0x8u,
        WS_EX_TRANSPARENT = 0x20u,
        WS_EX_WINDOWEDGE = 0x100u
    }


    public struct RECT
    {
        public int Left;

        public int Top;

        public int Right;

        public int Bottom;

        public int Width => Right - Left;

        public int Height => Bottom - Top;

    }



}
