// Usage:
// In the Window Constructor:
// bool micaApplied = new SystemBackdropHelper(this).TrySetBackdrop();

// Copyright (c) DGP Studio. All rights reserved.
// Licensed under the MIT license.
// https://gist.github.com/Lightczx/c43a2806f221d523d70316459ac399d8

using Microsoft.UI.Composition;
using Microsoft.UI.Composition.SystemBackdrops;
using Microsoft.UI.Xaml;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.InteropServices;
using Windows.System;
using WinRT;

namespace Xunkong.Desktop.Helpers;

/// <summary>
/// 回退行为
/// </summary>
public enum BackbdropFallBackBehavior
{
    /// <summary>
    /// 回退到无
    /// </summary>
    None,

    /// <summary>
    /// 回退到亚克力
    /// </summary>
    Acrylic,
}

/// <summary>
/// 系统背景帮助类
/// </summary>
public class SystemBackdropHelper
{
    private readonly Window window;
    private readonly BackbdropFallBackBehavior fallBackBehavior;

    private WindowsSystemDispatcherQueueHelper? dispatcherQueueHelper;
    private ISystemBackdropControllerWithTargets? backdropController;
    private SystemBackdropConfiguration? configurationSource;

    /// <summary>
    /// 构造一个新的系统背景帮助类
    /// </summary>
    /// <param name="window">窗体</param>
    /// <param name="fallBackBehavior">回退行为</param>
    public SystemBackdropHelper(Window window, BackbdropFallBackBehavior fallBackBehavior = BackbdropFallBackBehavior.Acrylic)
    {
        this.window = window;
        this.fallBackBehavior = fallBackBehavior;
    }

    private enum BackDropType
    {
        None,
        Acrylic,
        Mica,
    }

    /// <summary>
    /// 尝试设置背景
    /// </summary>
    /// <returns>是否设置成功</returns>
    public bool TrySetBackdrop()
    {
        BackDropType backDropType;
        if (MicaController.IsSupported())
        {
            backDropType = BackDropType.Mica;
        }
        else
        {
            backDropType = BackDropType.None;
            if (fallBackBehavior == BackbdropFallBackBehavior.Acrylic)
            {
                if (DesktopAcrylicController.IsSupported())
                {
                    backDropType = BackDropType.Acrylic;
                }
            }
        }

        if (backDropType == BackDropType.None)
        {
            return false;
        }
        else
        {
            dispatcherQueueHelper = new WindowsSystemDispatcherQueueHelper();
            dispatcherQueueHelper.EnsureWindowsSystemDispatcherQueueController();

            // Hooking up the policy object
            configurationSource = new SystemBackdropConfiguration();
            window.Activated += WindowActivated;
            window.Closed += WindowClosed;
            ((FrameworkElement)window.Content).ActualThemeChanged += WindowThemeChanged;

            // Initial configuration state.
            configurationSource.IsInputActive = true;
            SetConfigurationSourceTheme();

            backdropController = backDropType switch
            {
                BackDropType.Mica => new MicaController(),
                BackDropType.Acrylic => new DesktopAcrylicController(),
                _ => throw new ArgumentException("backDropType invalid"),
            };

            backdropController.AddSystemBackdropTarget(window.As<ICompositionSupportsSystemBackdrop>());
            backdropController.SetSystemBackdropConfiguration(configurationSource);

            return true;
        }
    }

    private void WindowActivated(object sender, WindowActivatedEventArgs args)
    {
        if (configurationSource is not null)
        {
            configurationSource.IsInputActive = args.WindowActivationState != WindowActivationState.Deactivated;
        }
    }

    private void WindowClosed(object sender, WindowEventArgs args)
    {
        // Make sure any Mica/Acrylic controller is disposed so it doesn't try to
        // use this closed window.
        if (backdropController != null)
        {
            backdropController.Dispose();
            backdropController = null;
        }

        window.Activated -= WindowActivated;
        configurationSource = null;
    }

    private void WindowThemeChanged(FrameworkElement sender, object args)
    {
        if (configurationSource != null)
        {
            SetConfigurationSourceTheme();
        }
    }

    private void SetConfigurationSourceTheme()
    {
        configurationSource!.Theme = ((FrameworkElement)window.Content).ActualTheme switch
        {
            ElementTheme.Dark => SystemBackdropTheme.Dark,
            ElementTheme.Light => SystemBackdropTheme.Light,
            ElementTheme.Default => SystemBackdropTheme.Default,
            _ => SystemBackdropTheme.Default,
        };
    }

    private class WindowsSystemDispatcherQueueHelper
    {
        private object dispatcherQueueController = null!;

        /// <summary>
        /// 确保系统调度队列控制器存在
        /// </summary>
        public void EnsureWindowsSystemDispatcherQueueController()
        {
            if (DispatcherQueue.GetForCurrentThread() != null)
            {
                // one already exists, so we'll just use it.
                return;
            }

            if (dispatcherQueueController == null)
            {
                DispatcherQueueOptions options;
                options.dwSize = Marshal.SizeOf(typeof(DispatcherQueueOptions));
                options.threadType = 2;    // DQTYPE_THREAD_CURRENT
                options.apartmentType = 2; // DQTAT_COM_STA

                _ = CreateDispatcherQueueController(options, ref dispatcherQueueController!);
            }
        }

        [DllImport("CoreMessaging.dll")]
        private static extern int CreateDispatcherQueueController(
            [In] DispatcherQueueOptions options,
            [In, Out, MarshalAs(UnmanagedType.IUnknown)] ref object dispatcherQueueController);

        [SuppressMessage("", "SA1307")]
        [StructLayout(LayoutKind.Sequential)]
        private struct DispatcherQueueOptions
        {
            internal int dwSize;
            internal int threadType;
            internal int apartmentType;
        }
    }
}