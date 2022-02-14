using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Controls.Primitives;
using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Navigation;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Microsoft.EntityFrameworkCore;
using Xunkong.Core.Wish;
using System.Text.Json;
using Windows.ApplicationModel;
using Microsoft.Web.WebView2.Core;
using Xunkong.Desktop.Services;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace Xunkong.Desktop.Controls
{
    public sealed partial class EChartControl : UserControl
    {


        public string HtmlFileName
        {
            get { return (string)GetValue(HtmlFileNameProperty); }
            set { SetValue(HtmlFileNameProperty, value); }
        }

        public static readonly DependencyProperty HtmlFileNameProperty =
            DependencyProperty.Register("HtmlFileName", typeof(string), typeof(EChartControl), null);


        public int Uid
        {
            get { return (int)GetValue(UidProperty); }
            set { SetValue(UidProperty, value); }
        }

        public static readonly DependencyProperty UidProperty =
            DependencyProperty.Register("Uid", typeof(int), typeof(EChartControl), null);




        private readonly ILogger<EChartControl> _logger;

        private readonly IDbContextFactory<XunkongDbContext> _dbContextFactory;


        public EChartControl()
        {
            this.InitializeComponent();
            _dbContextFactory = App.Current.Services.GetService<IDbContextFactory<XunkongDbContext>>()!;
            Loaded += EChartControl_Loaded;
        }



        private async void EChartControl_Loaded(object sender, RoutedEventArgs e)
        {
            try
            {
                await _webView.EnsureCoreWebView2Async();
                _webView.CoreWebView2.WebMessageReceived += CoreWebView2_WebMessageReceived;
                var path = Path.Combine(Package.Current.InstalledPath, $@"Xunkong.Desktop\WebViews\html\{HtmlFileName}.html");
                var html = await File.ReadAllTextAsync(path);
                _webView.NavigateToString(html);
                //_webView.Source = new Uri($"file:///{path}");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Load html file {HtmlFileName} and navigate to it.");
            }

        }


        private async void CoreWebView2_WebMessageReceived(CoreWebView2 sender, CoreWebView2WebMessageReceivedEventArgs args)
        {
            var message = args.TryGetWebMessageAsString();
            if (message == "wishlog.summary.stats")
            {
                await PostWishlogSummaryStatsAsync(Uid);
            }
        }




        private async Task PostWishlogSummaryStatsAsync(int uid)
        {
            using var ctx = _dbContextFactory.CreateDbContext();
            var count_200_3 = await ctx.WishlogItems.Where(x => x.Uid == uid && x.QueryType == WishType.Permanent && x.RankType == 3).CountAsync();
            var count_200_4 = await ctx.WishlogItems.Where(x => x.Uid == uid && x.QueryType == WishType.Permanent && x.RankType == 4).CountAsync();
            var count_200_5 = await ctx.WishlogItems.Where(x => x.Uid == uid && x.QueryType == WishType.Permanent && x.RankType == 5).CountAsync();
            var count_301_3 = await ctx.WishlogItems.Where(x => x.Uid == uid && x.QueryType == WishType.CharacterEvent && x.RankType == 3).CountAsync();
            var count_301_4 = await ctx.WishlogItems.Where(x => x.Uid == uid && x.QueryType == WishType.CharacterEvent && x.RankType == 4).CountAsync();
            var count_301_5 = await ctx.WishlogItems.Where(x => x.Uid == uid && x.QueryType == WishType.CharacterEvent && x.RankType == 5).CountAsync();
            var count_302_3 = await ctx.WishlogItems.Where(x => x.Uid == uid && x.QueryType == WishType.WeaponEvent && x.RankType == 3).CountAsync();
            var count_302_4 = await ctx.WishlogItems.Where(x => x.Uid == uid && x.QueryType == WishType.WeaponEvent && x.RankType == 4).CountAsync();
            var count_302_5 = await ctx.WishlogItems.Where(x => x.Uid == uid && x.QueryType == WishType.WeaponEvent && x.RankType == 5).CountAsync();
            var count_200 = (float)(count_200_3 + count_200_4 + count_200_5);
            var count_301 = (float)(count_301_3 + count_301_4 + count_301_5);
            var count_302 = (float)(count_302_3 + count_302_4 + count_302_5);
            var total = count_200 + count_301 + count_302;
            var obj = new[]
            {
                new
                {
                    name=$"常驻 {count_200}\n{count_200/total:P1}",
                    value=count_200,
                    children = new[]
                    {
                        new
                        {
                            name=$"3星 {count_200_3}\n{count_200_3/count_200:P3}",
                            value=count_200_3,
                        },
                        new
                        {
                            name=$"4星 {count_200_4}\n{count_200_4/count_200:P3}",
                            value=count_200_4,
                        },
                        new
                        {
                            name=$"5星 {count_200_5}\n{count_200_5/count_200:P3}",
                            value=count_200_5,
                        },
                    },
                },
                new
                {
                    name=$"角色 {count_301}\n{count_301/total:P1}",
                    value=count_301,
                    children = new[]
                    {
                        new
                        {
                            name=$"3星 {count_301_3}\n{count_301_3/count_301:P3}",
                            value=count_301_3,
                        },
                        new
                        {
                            name=$"4星 {count_301_4}\n{count_301_4/count_301:P3}",
                            value=count_301_4,
                        },
                        new
                        {
                            name=$"5星 {count_301_5}\n{count_301_5/count_301:P3}",
                            value=count_301_5,
                        },
                    },
                },
                new
                {
                    name=$"武器 {count_302}\n{count_302/total:P1}",
                    value=count_302,
                    children = new[]
                    {
                        new
                        {
                            name=$"3星 {count_302_3}\n{count_302_3/count_302:P3}",
                            value=count_302_3,
                        },
                        new
                        {
                            name=$"4星 {count_302_4}\n{count_302_4/count_302:P3}",
                            value=count_302_4,
                        },
                        new
                        {
                            name=$"5星 {count_302_5}\n{count_302_5/count_302:P3}",
                            value=count_302_5,
                        },
                    },
                },
            };
            _webView.CoreWebView2.PostWebMessageAsJson(JsonSerializer.Serialize(obj));
        }






    }
}
