using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Controls.Primitives;
using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Documents;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Navigation;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Numerics;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Text.RegularExpressions;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI;
using Xunkong.Core.Metadata;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace Xunkong.Desktop.Pages
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class CharacterInfoPage : Page
    {

        private CharacterInfoViewModel vm => (DataContext as CharacterInfoViewModel)!;


        private ScrollViewer? _ScrollViewer_SideIcon;


        public CharacterInfoPage()
        {
            this.InitializeComponent();
            DataContext = ActivatorUtilities.GetServiceOrCreateInstance<CharacterInfoViewModel>(App.Current.Services);
            Loaded += CharacterInfoPage_Loaded;
        }

        private async void CharacterInfoPage_Loaded(object sender, RoutedEventArgs e)
        {
            _ScrollViewer_SideIcon = (VisualTreeHelper.GetChild(_ListBox_SideIcon, 0) as Border)?.Child as ScrollViewer;
            await vm.InitializeDataAsync();
            await Task.Delay(100);
            ArcList();
        }

        private void _ListBox_SideIcon_PointerWheelChanged(object sender, PointerRoutedEventArgs e)
        {
            var pointer = e.GetCurrentPoint(_ListBox_SideIcon);
            var delta = pointer.Properties.MouseWheelDelta;
            _ScrollViewer_SideIcon?.ScrollToHorizontalOffset(_ScrollViewer_SideIcon.HorizontalOffset + delta);
        }


        private void ArcList()
        {
            var width = _Grid_GachaSplash.ActualWidth;
            var height = _Grid_GachaSplash.ActualHeight;
            for (int i = 0; i < _ItemsControl_Talents.Items.Count; i++)
            {
                var element = _ItemsControl_Talents.ContainerFromIndex(i) as UIElement;
                if (element is null)
                {
                    break;
                }
                if (double.IsNaN(element.Translation.X))
                {
                    element.Translation = new Vector3();
                }
                var pos = element.TransformToVisual(_Grid_GachaSplash).TransformPoint(new Point());
                var x = pos.X - 400 - element.Translation.X;
                var y = height / 2 - pos.Y ;
                var deltaX = Math.Sqrt(Math.Pow(x, 2) - Math.Pow(y, 2)) - x;
                element.Translation = new Vector3((float)deltaX, 0, 0);
            }
            for (int i = 0; i < _ItemsControl_Constellations.Items.Count; i++)
            {
                var element = _ItemsControl_Constellations.ContainerFromIndex(i) as UIElement;
                if (element is null)
                {
                    break;
                }
                if (double.IsNaN(element.Translation.X))
                {
                    element.Translation = new Vector3();
                }
                var pos = element.TransformToVisual(_Grid_GachaSplash).TransformPoint(new Point());
                var x = pos.X - 550 - element.Translation.X;
                var y = pos.Y - height / 2 ;
                var deltaX = Math.Sqrt(Math.Pow(x, 2) - Math.Pow(y, 2)) - x;
                element.Translation = new Vector3((float)deltaX, 0, 0);
            }
        }



        private void _Character_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            ArcList();
        }

        private void ShowAttachedFlyout(object sender, TappedRoutedEventArgs e)
        {
            var stack = sender as StackPanel;
            var flyout = FlyoutBase.GetAttachedFlyout(sender as FrameworkElement);
            var desc = (stack?.DataContext as CharacterInfo_Constellation)?.Description;
            if (desc is null)
            {
                desc = (stack?.DataContext as CharacterTalentInfo)?.Description;
            }
            if (desc is not null)
            {
                try
                {
                    var text = new TextBlock { TextWrapping = TextWrapping.Wrap, MaxWidth = 400 };
                    var subs = desc.Split("\\n");
                    foreach (var sub in subs)
                    {
                        var matches = Regex.Matches(sub, @"<color=([^>]+)>([^<]+)</color>");
                        string head, remain = sub;
                        foreach (Match match in matches)
                        {
                            var origin = match.Groups[0].Value;
                            head = remain.Substring(0, remain.IndexOf(origin));
                            remain = remain.Substring(remain.IndexOf(origin) + origin.Length);
                            if (!string.IsNullOrWhiteSpace(head))
                            {
                                text.Inlines.Add(new Run { Text = head });
                            }
                            var colorStr = match.Groups[1].Value;
                            var content = match.Groups[2].Value;
                            var color = System.Drawing.ColorTranslator.FromHtml(colorStr.Substring(0, 7));
                            text.Inlines.Add(new Run
                            {
                                Text = content,
                                Foreground = new SolidColorBrush(Color.FromArgb(color.A, color.R, color.G, color.B)),
                            });
                        }
                        if (!string.IsNullOrWhiteSpace(remain))
                        {
                            if (remain.Contains("<i>"))
                            {
                                var quote = remain.Replace("<i>", "").Replace("</i>", "");
                                text.Inlines.Add(new Run { Text = quote, FontFamily = new FontFamily("楷体") });
                            }
                            else
                            {
                                text.Inlines.Add(new Run { Text = remain });
                            }
                        }
                        text.Inlines.Add(new LineBreak());
                    }
                    if (text.Inlines.LastOrDefault() is LineBreak @break)
                    {
                        text.Inlines.Remove(@break);
                    }
                    if (flyout is Flyout f)
                    {
                        f.Content = text;
                    }
                }
                catch (Exception ex)
                {

                }
            }
            FlyoutBase.ShowAttachedFlyout(sender as FrameworkElement);
        }


    }
}
