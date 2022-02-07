using Microsoft.UI.Xaml.Controls;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Numerics;
using System.Diagnostics;
using Microsoft.UI.Xaml.Media.Animation;

namespace Xunkong.Desktop.Helpers
{
    internal static class InfoBarHelper
    {

        private static readonly System.Timers.Timer _timer = new(60000);


        private static StackPanel _container;


        public static void Initialize(StackPanel container)
        {
            _container = container;
            _timer.AutoReset = true;
            _timer.Elapsed += _timer_Elapsed;
            _timer.Start();
        }

        private static void _timer_Elapsed(object? sender, System.Timers.ElapsedEventArgs e)
        {
            if (_container is null)
            {
                return;
            }
            _container.DispatcherQueue.TryEnqueue(() =>
            {
                var c = _container.Children;
                foreach (var item in c)
                {
                    var size = item.ActualSize.X * item.ActualSize.Y;
                    if (size == 0)
                    {
                        c.Remove(item);
                    }
                }
            });

        }


        private static void AddInfoBarToContainer(InfoBarSeverity severity, string? title, string? message, int delay)
        {
            _container.DispatcherQueue.TryEnqueue(async () =>
            {
                var infoBar = new InfoBar
                {
                    Severity = severity,
                    Title = title,
                    Message = message,
                    IsOpen = true,
                };
                _container.Children.Add(infoBar);
                if (delay > 0)
                {

                    await Task.Delay(delay);
                    infoBar.IsOpen = false;
                }
            });
        }



        public static void Information(string message, int delay = 3000)
        {
            AddInfoBarToContainer(InfoBarSeverity.Informational, null, message, delay);
        }


        public static void Information(string title, string message, int delay = 3000)
        {
            AddInfoBarToContainer(InfoBarSeverity.Informational, title, message, delay);
        }


        public static void Success(string message, int delay = 3000)
        {
            AddInfoBarToContainer(InfoBarSeverity.Success, null, message, delay);
        }


        public static void Success(string title, string message, int delay = 3000)
        {
            AddInfoBarToContainer(InfoBarSeverity.Success, title, message, delay);
        }


        public static void Warning(string message, int delay = 0)
        {
            AddInfoBarToContainer(InfoBarSeverity.Warning, null, message, delay);
        }


        public static void Warning(string title, string message, int delay = 0)
        {
            AddInfoBarToContainer(InfoBarSeverity.Warning, title, message, delay);
        }


        public static void Error(string message, int delay = 0)
        {
            AddInfoBarToContainer(InfoBarSeverity.Error, null, message, delay);
        }


        public static void Error(string title, string message, int delay = 0)
        {
            AddInfoBarToContainer(InfoBarSeverity.Error, title, message, delay);
        }


        public static void Error(Exception ex, int delay = 0)
        {
            AddInfoBarToContainer(InfoBarSeverity.Error, ex.GetType().Name, ex.Message, delay);
        }


        public static void Error(Exception ex, string step, int delay = 0)
        {
            AddInfoBarToContainer(InfoBarSeverity.Error, ex.GetType().Name, $"{step}\n{ex.Message}", delay);
        }


        public static void Show(InfoBar infoBar, int delay = 0)
        {
            _container.DispatcherQueue.TryEnqueue(async () =>
            {
                _container.Children.Add(infoBar);
                if (delay > 0)
                {
                    await Task.Delay(delay);
                    infoBar.IsOpen = false;
                }
            });
        }






    }
}
