using CommunityToolkit.WinUI.UI;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using System.Drawing;
using System.Net.Http.Json;
using Windows.Storage;
using Xunkong.Core.Hoyolab;
using Xunkong.Core.TravelRecord;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace Xunkong.Desktop.Toolbox
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    [Toolbox("旅行札记生成器", "https://file.xunkong.cc/static/travelnotes/icon.webp", "生成可用于分享的旅行札记图片")]
    [INotifyPropertyChanged]
    public sealed partial class TravelNoteTool : Page
    {

        private readonly IDbContextFactory<XunkongDbContext> _ctxFactory;

        private readonly HttpClient _httpClient;

        public TravelNoteTool()
        {
            this.InitializeComponent();
            DataContext = this;
            _ctxFactory = ActivatorUtilities.GetServiceOrCreateInstance<IDbContextFactory<XunkongDbContext>>(App.Current.Services);
            _httpClient = ActivatorUtilities.GetServiceOrCreateInstance<HttpClient>(App.Current.Services);
            Loaded += TravelNoteTool_Loaded;
            pendic.Add("活动奖励", pen_huodong);
            pendic.Add("每日奖励", pen_meiri);
            pendic.Add("深境螺旋", pen_shenyuan);
            pendic.Add("邮件奖励", pen_youjian);
            pendic.Add("冒险奖励", pen_maoxian);
            pendic.Add("任务奖励", pen_renwu);
            pendic.Add("其他", pen_qita);
        }


        private TravelNoteAsset? asset;

        private string bg;

        private List<string> emos;


        private async void TravelNoteTool_Loaded(object sender, RoutedEventArgs e)
        {
            await GetUsersAsync();
            await GetAssetAsync();
        }

        private async Task GetUsersAsync()
        {
            try
            {
                var ctx = _ctxFactory.CreateDbContext();
                var users = await ctx.UserGameRoleInfos.AsNoTracking().ToListAsync();
                Users = users;
                SelectedUser = Users?.FirstOrDefault();
                var versions = await ctx.WishEventInfos.Select(x => x.Version).Distinct().ToListAsync();
                Versions = versions.OrderByDescending(x => x).ToList();
                SelectedVersion = Versions.FirstOrDefault();
            }
            catch (Exception ex)
            {
                InfoBarHelper.Error(ex);
            }
        }


        private async Task GetAssetAsync()
        {
            if (string.IsNullOrWhiteSpace(bg) || !(emos?.Any() ?? false))
            {
                try
                {
                    asset = await _httpClient.GetFromJsonAsync<TravelNoteAsset>("https://file.xunkong.cc/static/travelnotes/asset.json");
                    if (asset is null)
                    {
                        return;
                    }
                    bg = asset.Background;
                    await ImageCache.Instance.PreCacheAsync(new Uri(bg));
                    emos = asset.Emotions;
                }
                catch (Exception ex)
                {
                    InfoBarHelper.Error(ex);
                }
            }
        }

        private async Task<string> GetBg()
        {
            if (string.IsNullOrWhiteSpace(bg))
            {
                await GetAssetAsync();
            }
            return bg;
        }


        private async Task<string> GetEmo()
        {
            if (!(emos?.Any() ?? false))
            {
                await GetAssetAsync();
            }
            var index = Random.Shared.Next(emos!.Count);
            return emos[index];
        }



        Font font_version = new Font("微软雅黑", 28);
        Font font_nickname = new Font("微软雅黑", 16);
        Font font_stats = new Font("微软雅黑", 14);
        Font font_max = new Font("微软雅黑", 12);
        SolidBrush brush_blue = new SolidBrush(Color.FromArgb(0x28, 0x38, 0x4d));
        SolidBrush brush_gold = new SolidBrush(Color.FromArgb(0xa4, 0x8e, 0x73));
        Pen pen_tick = new Pen(Color.FromArgb(0x28, 0x38, 0x4d), 1.5f);

        Pen pen_huodong = new Pen(Color.FromArgb(95, 131, 165), 24f);
        Pen pen_meiri = new Pen(Color.FromArgb(189, 157, 96), 24f);
        Pen pen_shenyuan = new Pen(Color.FromArgb(120, 156, 118), 24f);
        Pen pen_youjian = new Pen(Color.FromArgb(128, 115, 169), 24f);
        Pen pen_maoxian = new Pen(Color.FromArgb(211, 108, 110), 24f);
        Pen pen_renwu = new Pen(Color.FromArgb(109, 179, 179), 24f);
        Pen pen_qita = new Pen(Color.FromArgb(113, 169, 200), 24f);

        Dictionary<string, Pen> pendic = new Dictionary<string, Pen>();



        private List<UserGameRoleInfo> _Users;
        public List<UserGameRoleInfo> Users
        {
            get => _Users;
            set => SetProperty(ref _Users, value);
        }



        private UserGameRoleInfo? _SelectedUser;
        public UserGameRoleInfo? SelectedUser
        {
            get => _SelectedUser;
            set => SetProperty(ref _SelectedUser, value);
        }



        private List<string> _Versions;
        public List<string> Versions
        {
            get => _Versions;
            set => SetProperty(ref _Versions, value);
        }


        private string? _SelectedVersion;
        public string? SelectedVersion
        {
            get => _SelectedVersion;
            set => SetProperty(ref _SelectedVersion, value);
        }




        [ICommand(AllowConcurrentExecutions = false)]
        private async Task DrawImageAsync()
        {
            try
            {

                if (SelectedUser is null)
                {
                    return;
                }
                var user = SelectedUser;

                using var db = _ctxFactory.CreateDbContext();
                var version = SelectedVersion;
                var versionevent = await db.WishEventInfos.AsNoTracking().Where(x => x.Version == version).FirstOrDefaultAsync();
                if (versionevent is null)
                {
                    InfoBarHelper.Warning("版本不存在");
                    return;
                }
                var startTime = versionevent.StartTime.UtcDateTime.AddHours(8); ;
                var endtime = versionevent.StartTime.AddDays(42).UtcDateTime.AddHours(8);
                var uid = user.Uid;
                var rawlist_primo = await db.TravelRecordAwardItems.AsNoTracking().Where(x => x.Uid == uid & x.Type == TravelRecordAwardType.Primogems && x.Time >= startTime && x.Time <= endtime).ToListAsync();
                var rawlist_mora = await db.TravelRecordAwardItems.AsNoTracking().Where(x => x.Uid == uid & x.Type == TravelRecordAwardType.Mora && x.Time >= startTime && x.Time <= endtime).ToListAsync();

                if (!rawlist_primo.Any() && !rawlist_mora.Any())
                {
                    InfoBarHelper.Warning("没有该版本的数据");
                    return;
                }

                var uri_bg = new Uri(await GetBg());
                var path_bg = await ImageCache.Instance.GetFileFromCacheAsync(uri_bg);
                if (path_bg is null)
                {
                    await ImageCache.Instance.RemoveAsync(new[] { uri_bg });
                    await ImageCache.Instance.PreCacheAsync(uri_bg);
                    path_bg = await ImageCache.Instance.GetFileFromCacheAsync(uri_bg);
                }
                var image_bg = System.Drawing.Image.FromFile(path_bg.Path);
                using var g = Graphics.FromImage(image_bg);
                g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;

                g.DrawString($"v{version}", font_version, brush_blue, 316, 394);
                g.DrawString(s: $"{startTime:yyyy/MM/dd}  -  {endtime:yyyy/MM/dd}", font_nickname, brush_blue, 160, 458);
                g.DrawString($"昵称：{user.Nickname}", font_nickname, brush_blue, 160, 494);
                g.DrawString(rawlist_primo.Sum(x => x.Number).ToString(), font_nickname, brush_gold, 310, 594);
                g.DrawString(rawlist_mora.Sum(x => x.Number).ToString(), font_nickname, brush_gold, 310, 668);

                var uri_emo = new Uri(await GetEmo());
                var path_emo = await ImageCache.Instance.GetFileFromCacheAsync(uri_emo);
                if (path_emo is null)
                {
                    await ImageCache.Instance.RemoveAsync(new[] { uri_emo });
                    await ImageCache.Instance.PreCacheAsync(uri_emo);
                    path_emo = await ImageCache.Instance.GetFileFromCacheAsync(uri_emo);
                }
                var image_emo = System.Drawing.Image.FromFile(path_emo.Path);
                g.RotateTransform(5);
                g.DrawImage(image_emo, 504, 284, 237, 375);
                g.RotateTransform(-5);


                var groupList = new List<string> { "每日奖励", "活动奖励", "深境螺旋", "邮件奖励", "任务奖励", "冒险奖励", "其他" };
                var stats = groupList.GroupJoin(rawlist_primo, x => x, x => ActionNameToGroup(x.ActionName), (x, y) => new Stats(x, y.Sum(y => y.Number))).ToList();
                static string ActionNameToGroup(string? name)
                {
                    return name switch
                    {
                        "每日委托奖励" => "每日奖励",
                        "活动奖励" => "活动奖励",
                        "深境螺旋奖励" => "深境螺旋",
                        "邮件奖励" => "邮件奖励",
                        "任务奖励" => "任务奖励",
                        "成就奖励" or "宝箱奖励" or "传送点解锁奖励" => "冒险奖励",
                        "教学阅读奖励" or "其他" or _ => "其他"
                    };
                }
                var sum = stats.Sum(x => x.Number);

                foreach (var item in stats)
                {
                    item.Percent = (float)item.Number / sum;
                }

                var stats_qita = stats.Where(x => x.Name == "其他").FirstOrDefault();
                stats = stats.Where(x => x.Name != "其他").OrderByDescending(x => x.Number).ToList();
                if (stats_qita is null)
                {
                    stats_qita = new Stats("其他", 0);
                }
                stats.Add(stats_qita);



                var angle = -90f;
                foreach (var item in stats)
                {
                    var swipetAngle = 360 * item.Percent;
                    g.DrawArc(pendic[item.Name], 192, 816, 200, 200, angle - 1, swipetAngle + 1);
                    angle += swipetAngle;
                }


                var index = 0;
                foreach (var item in stats)
                {
                    g.FillRectangle(pendic[item.Name].Brush, 454, 800 + 36 * index, 14, 14);
                    g.DrawString(item.Name, font_stats, brush_blue, 480, 794 + 36 * index);
                    g.DrawString(item.Percent.ToString("P0"), font_stats, brush_blue, 572, 794 + 36 * index);
                    index++;
                }


                var days = Enumerable.Range(0, 43).Select(x => startTime.AddDays(x)).ToList();
                var groupbyday = days.GroupJoin(rawlist_primo, x => x.Date, x => x.Time.Date, (x, y) => y.Sum(z => z.Number)).ToList();
                if (groupbyday.Last() == 0)
                {
                    groupbyday.RemoveAt(42);
                }
                var max = groupbyday.Max();
                index = 0;
                bool hasPointMax = false;
                foreach (var item in groupbyday)
                {
                    var height = (float)item / max * 160;
                    g.FillRectangle(pen_huodong.Brush, 165 + 10.5f * index, 1270 - height, 10, height);
                    if ((index + 3) % 7 == 0)
                    {
                        g.DrawLine(pen_tick, 165 + 10.5f * index + 10.25f, 1270, 165 + 10.5f * index + 10.25f, 1275);
                    }
                    if (item == max && !hasPointMax)
                    {
                        var delta = (int)Math.Log10(max) * 5.5f;
                        g.DrawString(max.ToString(), font_max, brush_blue, 165 + 10.5f * index - delta, 1270 - height - 22);
                        hasPointMax = true;
                    }
                    index++;
                }
                g.DrawLine(pen_tick, 165, 1270, 165 + 10.5f * groupbyday.Count - 0.5f, 1270);


                g.Save();

                var savedFile = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), $@"Xunkong\Export\TravelNotes\TravelNotes_{uid}_v{version}_{DateTime.Now:yyMMddHHmmss}.png");
                Directory.CreateDirectory(Path.GetDirectoryName(savedFile)!);
                image_bg.Save(savedFile);

                Action action = () => Process.Start(new ProcessStartInfo { FileName = savedFile, UseShellExecute = true });
                InfoBarHelper.ShowWithButton(InfoBarSeverity.Success, "已保存", Path.GetFileName(savedFile), "打开图片", action, null, 3000);

            }
            catch (Exception ex)
            {
                InfoBarHelper.Error(ex);
            }
        }



        private int _TotalCount;
        public int TotalCount
        {
            get => _TotalCount;
            set => SetProperty(ref _TotalCount, value);
        }


        private int _FinishedCount;
        public int FinishedCount
        {
            get => _FinishedCount;
            set => SetProperty(ref _FinishedCount, value);
        }


        private object _precacheImage_lock = new();


        [ICommand(AllowConcurrentExecutions = false)]
        private async Task PrecacheAllImgesAsync()
        {
            try
            {
                FinishedCount = 0;
                if (!(emos?.Any() ?? false))
                {
                    await GetAssetAsync();
                }

                if (!(emos?.Any() ?? false))
                {
                    return;
                }

                TotalCount = emos.Count;

                await Task.Delay(100);
                var tempState = ApplicationData.Current.TemporaryFolder.Path;
                var imageCacheFolder = Path.Combine(tempState, "ImageCache");
                var cacheFiles = Directory.EnumerateFiles(imageCacheFolder);
                foreach (var file in cacheFiles)
                {
                    using var fs = File.Open(file, FileMode.Open);
                    if (fs.Length == 0)
                    {
                        fs.Dispose();
                        File.Delete(file);
                    }
                }

                await Parallel.ForEachAsync(emos, async (url, _) =>
                {
                    try
                    {
                        var uri = new Uri(url);
                        await ImageCache.Instance.PreCacheAsync(uri);
                        lock (_precacheImage_lock)
                        {
                            MainWindow.Current.DispatcherQueue.TryEnqueue(() => FinishedCount++);
                        }
                    }
                    catch { }
                });
            }
            catch (Exception ex)
            {
                InfoBarHelper.Error(ex);
            }
        }




        private class TravelNoteAsset
        {
            public string Background { get; set; }

            public List<string> Emotions { get; set; }

        }



        private record Stats(string Name, int Number)
        {
            public float Percent { get; set; }
        }



    }












}
