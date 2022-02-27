using CommunityToolkit.Mvvm.Input;
using Microsoft.EntityFrameworkCore;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunkong.Core;
using Xunkong.Core.Hoyolab;
using Xunkong.Core.Metadata;
using Xunkong.Core.Wish;
using Xunkong.Core.XunkongApi;
using System.Diagnostics;
using System.Collections.Immutable;
using System.Collections.Concurrent;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace Xunkong.Desktop.ViewModels
{
    internal partial class WishlogSummaryViewModel : ObservableObject
    {


        private readonly ILogger<WishlogSummaryViewModel> _logger;

        private readonly IDbContextFactory<XunkongDbContext> _ctxFactory;

        private readonly WishlogService _wishlogService;

        private readonly XunkongApiService _xunkongApiService;

        private readonly UserSettingService _settingService;


        private readonly ImmutableList<string> ColorSet = new[] { "#5470C6", "#91CC75", "#FAC858", "#EE6666", "#73C0DE", "#3BA272", "#FC8452", "#9A60B4", "#EA7CCC" }.ToImmutableList();


        public WishlogSummaryViewModel(ILogger<WishlogSummaryViewModel> logger,
                                       IDbContextFactory<XunkongDbContext> ctxFactory,
                                       WishlogService wishlogService,
                                       XunkongApiService xunkongApiService,
                                       UserSettingService settingService)
        {
            _logger = logger;
            _ctxFactory = ctxFactory;
            _wishlogService = wishlogService;
            _xunkongApiService = xunkongApiService;
            _settingService = settingService;
        }


        private bool _IsLoading;
        /// <summary>
        /// 加载进度条
        /// </summary>
        public bool IsLoading
        {
            get => _IsLoading;
            set => SetProperty(ref _IsLoading, value);
        }


        private string _StateText;
        public string StateText
        {
            get => _StateText;
            set => SetProperty(ref _StateText, value);
        }


        private List<string> _Uids;
        public List<string> Uids
        {
            get => _Uids;
            set => SetProperty(ref _Uids, value);
        }


        private int _SelectedUid;
        public string SelectedUid
        {
            get => _SelectedUid.ToString();
            set
            {
                if (int.TryParse(value, out int uid))
                {
                    SetProperty(ref _SelectedUid, uid);
                    _ = _settingService.SaveSettingAsync("LastSelectedUidInWishlogSummaryPage", uid);
                }
            }
        }


        private List<WishlogSummary_QueryTypeStats> _QueryTypeStats;
        public List<WishlogSummary_QueryTypeStats> QueryTypeStats
        {
            get => _QueryTypeStats;
            set => SetProperty(ref _QueryTypeStats, value);
        }



        private List<WishlogSummary_CharacterThumb> _CharacterThumbs;
        public List<WishlogSummary_CharacterThumb> CharacterThumbs
        {
            get => _CharacterThumbs;
            set => SetProperty(ref _CharacterThumbs, value);
        }


        private List<WishlogSummary_WeaponThumb> _WeaponThumbs;
        public List<WishlogSummary_WeaponThumb> WeaponThumbs
        {
            get => _WeaponThumbs;
            set => SetProperty(ref _WeaponThumbs, value);
        }


        /// <summary>
        /// 页面初始化
        /// </summary>
        /// <returns></returns>
        [ICommand(AllowConcurrentExecutions = false)]
        public async Task InitializePageDataAsync()
        {
            try
            {
                Uids = (await _wishlogService.GetAllUidsAsync()).Select(x => x.ToString()).ToList();
                SelectedUid = await _settingService.GetSettingAsync<string>("LastSelectedUidInWishlogSummaryPage") ?? "";
                if (!Uids.Contains(SelectedUid) || _SelectedUid == 0)
                {
                    SelectedUid = Uids.FirstOrDefault() ?? "";
                }
                if (_SelectedUid == 0)
                {
                    StateText = "没有祈愿数据";
                }
                else
                {
                    await LoadWishlogAsync();
                    OnPropertyChanged(nameof(SelectedUid));
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Get uids from database.");
                InfoBarHelper.Error(ex);
            }

        }


        /// <summary>
        /// 加载已选择uid的祈愿信息
        /// </summary>
        /// <returns></returns>
        public async Task LoadWishlogAsync()
        {
            IsLoading = true;
            await Task.Delay(100);
            try
            {
                var wishlogs = await _wishlogService.GetWishlogItemExByUidAsync(_SelectedUid);
                var queryTypeGroups = wishlogs.GroupBy(x => x.QueryType);
                var statsCollection = new BlockingCollection<WishlogSummary_QueryTypeStats>();
                Parallel.ForEach(queryTypeGroups, group =>
                {
                    var type = group.Key;
                    if (type == WishType.Novice)
                    {
                        return;
                    }
                    var items = group.OrderBy(x => x.Id).ToList();
                    var totalCount = items.Count;
                    var rank5Count = items.Where(x => x.RankType == 5).Count();
                    var rank4Count = items.Where(x => x.RankType == 4).Count();
                    var startTime = items.First().Time;
                    var endTime = items.Last().Time;
                    var upCount = items.Where(x => x.RankType == 5 && x.IsUp).Count();
                    var currentGuarantee = items.Last().GuaranteeIndex;
                    var maxGuarantee = items.Where(x => x.RankType == 5).Max(x => x.GuaranteeIndex);
                    var minGuarantee = items.Where(x => x.RankType == 5).Min(x => x.GuaranteeIndex);
                    var rank5Items = items.Where(x => x.RankType == 5).Select(x => new WishlogSummary_Rank5Item(x.Name, x.GuaranteeIndex, x.Time)).ToList();
                    var colors = ColorSet.OrderBy(x => Random.Shared.Next()).ToList();
                    var gi = rank5Items.GroupBy(x => x.Name).OrderBy(x => x.Min(y => y.Time));
                    int index = 0;
                    foreach (var g in gi)
                    {
                        var color = colors[index];
                        g.ToList().ForEach(x => { x.Color = color; x.Foreground = color; });
                        index = (index + 1) % colors.Count;
                    }
                    var stats = new WishlogSummary_QueryTypeStats(type,
                                                                  totalCount,
                                                                  rank5Count,
                                                                  rank4Count,
                                                                  startTime,
                                                                  endTime,
                                                                  upCount,
                                                                  currentGuarantee,
                                                                  maxGuarantee,
                                                                  minGuarantee,
                                                                  rank5Items,
                                                                  items.LastOrDefault(x => x.RankType == 5)?.IsUp ?? false,
                                                                  items.LastOrDefault(x => x.RankType == 5)?.GuaranteeIndex ?? 0);
                    statsCollection.Add(stats);
                });
                QueryTypeStats = statsCollection.Where(x => x.QueryType == WishType.CharacterEvent)
                                                .Concat(statsCollection.Where(x => x.QueryType == WishType.WeaponEvent))
                                                .Concat(statsCollection.Where(x => x.QueryType == WishType.Permanent))
                                                .ToList();
                var ctx = _ctxFactory.CreateDbContext();
                var characters = await ctx.CharacterInfos.ToListAsync();
                var weapons = await ctx.WeaponInfos.ToListAsync();
                var query1 = from c in characters
                             join w in wishlogs
                             on c.Name equals w.Name into g
                             where g.Any()
                             select new WishlogSummary_CharacterThumb(c.Name, c.Rarity, c.Element, g.Count(), c.FaceIcon, g.Last().Time);
                CharacterThumbs = new(query1.OrderByDescending(x => x.Rarity).ThenByDescending(x => x.Count).ThenByDescending(x => x.LastTime));
                var query2 = from c in weapons
                             join w in wishlogs
                             on c.Name equals w.Name into g
                             where g.Any()
                             select new WishlogSummary_WeaponThumb(c.Name, c.Rarity, c.WeaponType, g.Count(), c.Icon, g.Last().Time);
                WeaponThumbs = new(query2.OrderByDescending(x => x.Rarity).ThenByDescending(x => x.Count).ThenByDescending(x => x.LastTime));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Get wishlogs from database.");
                InfoBarHelper.Error(ex);
            }
            finally
            {
                IsLoading = false;
            }
        }



        /// <summary>
        /// 使用已选择uid保存在本地数据库的祈愿记录网址获取新的祈愿记录
        /// </summary>
        /// <param name="getAll"></param>
        /// <returns></returns>
        [ICommand(AllowConcurrentExecutions = false)]
        private async Task GetWishlogFromSavedWishlogUrlAsync(bool getAll)
        {
            if (_SelectedUid == 0)
            {
                return;
            }
            var uid = _SelectedUid;
            IsLoading = true;
            await Task.Delay(100);
            try
            {
                StateText = "验证祈愿记录网址的有效性";
                EventHandler<(WishType WishType, int Page)> handler = (s, e) => StateText = $"正在获取 {e.WishType.ToDescriptionOrString()} 第 {e.Page} 页";
                if (await _wishlogService.CheckWishlogUrlTimeoutAsync(uid))
                {
                    var addCount = await _wishlogService.GetWishlogByUidAsync(uid, handler, getAll);
                    StateText = $"新增 {addCount} 条祈愿记录";
                }
                else
                {
                    var isSea = uid.ToString()[0] > '5';
                    var wishlogUrl = await _wishlogService.FindWishlogUrlFromLogFileAsync(isSea);
                    uid = await _wishlogService.GetUidByWishlogUrl(wishlogUrl);
                    var addCount = await _wishlogService.GetWishlogByUidAsync(uid, handler, getAll);
                    StateText = $"新增 {addCount} 条祈愿记录";
                }
                SelectedUid = uid.ToString();
                await InitializePageDataAsync();
            }
            catch (Exception ex) when (ex is HoyolabException or XunkongException)
            {
                StateText = ex.Message;
                _logger.LogError(ex, "Get wishlog from saved wishlog url.");
            }
            catch (Exception ex)
            {
                StateText = "";
                InfoBarHelper.Error(ex);
                _logger.LogError(ex, "Get wishlog from saved wishlog url.");
            }
            finally
            {
                IsLoading = false;
            }
        }


        /// <summary>
        /// 从原神日志文件获取新的祈愿记录
        /// </summary>
        /// <param name="isSea"></param>
        /// <returns></returns>
        [ICommand(AllowConcurrentExecutions = false)]
        private async Task GetWishlogFromLogFileAsync(bool isSea)
        {
            IsLoading = true;
            await Task.Delay(100);
            try
            {
                EventHandler<(WishType WishType, int Page)> handler = (s, e) => StateText = $"正在获取 {e.WishType.ToDescriptionOrString()} 第 {e.Page} 页";
                var wishlogUrl = await _wishlogService.FindWishlogUrlFromLogFileAsync(isSea);
                StateText = "验证祈愿记录网址的有效性";
                var uid = await _wishlogService.GetUidByWishlogUrl(wishlogUrl);
                var addCount = await _wishlogService.GetWishlogByUidAsync(uid, handler);
                StateText = $"新增 {addCount} 条祈愿记录";
                SelectedUid = uid.ToString();
                await InitializePageDataAsync();
            }
            catch (Exception ex) when (ex is HoyolabException or XunkongException)
            {
                StateText = ex.Message;
                _logger.LogError(ex, "Get wishlog from log file.");
            }
            catch (Exception ex)
            {
                StateText = "";
                InfoBarHelper.Error(ex);
                _logger.LogError(ex, "Get wishlog from log file.");
            }
            finally
            {
                IsLoading = false;
            }
        }


        /// <summary>
        /// 从输入的祈愿记录网址获取信息祈愿记录
        /// </summary>
        /// <param name="wishlogUrl"></param>
        /// <returns></returns>
        [ICommand(AllowConcurrentExecutions = false)]
        private async Task GetWishlogFromWishlogUrlAsync(string wishlogUrl)
        {
            if (string.IsNullOrWhiteSpace(wishlogUrl))
            {
                return;
            }
            IsLoading = true;
            await Task.Delay(100);
            try
            {
                EventHandler<(WishType WishType, int Page)> handler = (s, e) => StateText = $"正在获取 {e.WishType.ToDescriptionOrString()} 第 {e.Page} 页";
                StateText = "验证祈愿记录网址的有效性";
                var uid = await _wishlogService.GetUidByWishlogUrl(wishlogUrl);
                var addCount = await _wishlogService.GetWishlogByUidAsync(uid, handler);
                StateText = $"新增 {addCount} 条祈愿记录";
                SelectedUid = uid.ToString();
                await InitializePageDataAsync();
            }
            catch (Exception ex) when (ex is HoyolabException or XunkongException)
            {
                StateText = ex.Message;
                _logger.LogError(ex, "Get wishlog from new wishlog url.");
            }
            catch (Exception ex)
            {
                StateText = "";
                InfoBarHelper.Error(ex);
                _logger.LogError(ex, "Get wishlog from new wishlog url.");
            }
            finally
            {
                IsLoading = false;
            }
        }


        /// <summary>
        /// 查询指定uid备份在寻空服务器上的祈愿记录
        /// </summary>
        /// <returns></returns>
        [ICommand(AllowConcurrentExecutions = false)]
        private async Task QueryWishlogInCloudAsync()
        {
            if (_SelectedUid == 0)
            {
                return;
            }
            var uid = _SelectedUid;
            IsLoading = true;
            StateText = "";
            await Task.Delay(100);
            try
            {
                Action<string> handler = str => StateText = str;
                await _xunkongApiService.GetWishlogBackupLastItemAsync(uid, handler);
            }
            catch (XunkongException ex)
            {
                StateText = ex.Message;
                _logger.LogError(ex, "Query wishlog in cloud.");
            }
            catch (Exception ex)
            {
                StateText = "";
                InfoBarHelper.Error(ex);
                _logger.LogError(ex, "Query wishlog in cloud.");
            }
            finally
            {
                IsLoading = false;
            }
        }


        /// <summary>
        /// 上传指定uid的祈愿记录至寻空服务器
        /// </summary>
        /// <returns></returns>
        [ICommand(AllowConcurrentExecutions = false)]
        private async Task UploadWishlogToCloudAsync(bool putAll)
        {
            if (_SelectedUid == 0)
            {
                return;
            }
            var uid = _SelectedUid;
            IsLoading = true;
            StateText = "";
            await Task.Delay(100);
            try
            {
                Action<string> handler = str => StateText = str;
                await _xunkongApiService.PutWishlogListAsync(uid, handler, putAll);
            }
            catch (XunkongException ex)
            {
                StateText = ex.Message;
                _logger.LogError(ex, "Update wishlog to cloud.");
            }
            catch (Exception ex)
            {
                StateText = "";
                InfoBarHelper.Error(ex);
                _logger.LogError(ex, "Update wishlog to cloud.");
            }
            finally
            {
                IsLoading = false;
            }
        }


        /// <summary>
        /// 从寻空服务器下载指定uid的祈愿记录
        /// </summary>
        /// <returns></returns>
        [ICommand(AllowConcurrentExecutions = false)]
        private async Task DownloadWishlogFromCloudCompletelyAsync()
        {
            if (_SelectedUid == 0)
            {
                return;
            }
            var uid = _SelectedUid;
            IsLoading = true;
            StateText = "";
            await Task.Delay(100);
            try
            {
                Action<string> handler = str => StateText = str;
                await _xunkongApiService.GetWishlogBackupListAsync(uid, handler, true);
            }
            catch (XunkongException ex)
            {
                StateText = ex.Message;
                _logger.LogError(ex, "Download wishlog in cloud completely.");
            }
            catch (Exception ex)
            {
                StateText = "";
                InfoBarHelper.Error(ex);
                _logger.LogError(ex, "Download wishlog in cloud completely.");
            }
            finally
            {
                IsLoading = false;
            }
        }



        /// <summary>
        /// 删除指定uid备份在寻空服务器的祈愿记录，并将记录备份在本地
        /// </summary>
        /// <returns></returns>
        [ICommand(AllowConcurrentExecutions = false)]
        private async Task DeleteWishlogInCloudCompletelyAsync()
        {
            if (_SelectedUid == 0)
            {
                return;
            }
            var uid = _SelectedUid;
            IsLoading = true;
            StateText = "";
            try
            {
                _logger.LogInformation($"Start to delete wishlog in cloud with uid {uid}.");
                Action<string> handler = str => StateText = str;
                var result = await _xunkongApiService.GetWishlogBackupLastItemAsync(uid, handler);
                if (result.CurrentCount == 0)
                {
                    StateText = "云端没有记录";
                    _logger.LogInformation($"No wishlog in cloud with uid {uid}.");
                    return;
                }
                if (result.CurrentCount > await _wishlogService.GetWishlogCountAsync(uid))
                {
                    _logger.LogInformation($"Count of wishlog in cloud is more than local with uid {uid}, show warning dialog.");
                    var dialog = new ContentDialog
                    {
                        Title = "警告",
                        Content = $"Uid {uid} 的本地祈愿记录数量少于云端，删除云端记录可能会造成数据丢失，是否继续？",
                        PrimaryButtonText = "继续删除",
                        CloseButtonText = "取消",
                        DefaultButton = ContentDialogButton.Close,
                        XamlRoot = MainWindow.XamlRoot,
                    };
                    var dialogResult = await dialog.ShowAsync();
                    if (dialogResult != ContentDialogResult.Primary)
                    {
                        return;
                    }
                }
                _logger.LogInformation($"Start to download wishlog in cloud and backup to loacal with uid {uid}.");
                var file = await _xunkongApiService.GetWishlogAndBackupToLoalAsync(uid, handler);
                RoutedEventHandler eventHandler = (_, _) => Process.Start(new ProcessStartInfo { FileName = Path.GetDirectoryName(file), UseShellExecute = true, });
                InfoBarHelper.ShowWithButton(InfoBarSeverity.Success, null, "云端祈愿记录已备份到本地", "打开文件夹", eventHandler, 3000);
                await _xunkongApiService.DeleteWishlogBackupAsync(uid, handler);
            }
            catch (XunkongException ex)
            {
                StateText = ex.Message;
                _logger.LogError(ex, "Delete wishlog in cloud.");
            }
            catch (Exception ex)
            {
                StateText = "";
                InfoBarHelper.Error(ex);
                _logger.LogError(ex, "Delete wishlog in cloud.");
            }
            finally
            {
                IsLoading = false;
            }
        }



        /// <summary>
        /// 获取角色、武器、卡池、国际化等信息
        /// </summary>
        /// <returns></returns>
        [ICommand(AllowConcurrentExecutions = false)]
        private async Task GetMetadataAsync()
        {
            IsLoading = true;
            StateText = "";
            await Task.Delay(100);
            try
            {
                await _xunkongApiService.GetCharacterInfosFromServerAsync();
                await _xunkongApiService.GetWeaponInfosFromServerAsync();
                await _xunkongApiService.GetWishEventInfosFromServerAsync();
                await _xunkongApiService.GetI18nModelsFromServerAsync();
                StateText = "完成";
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Get metadata.");
                InfoBarHelper.Error(ex);
            }
            finally
            {
                IsLoading = false;
            }
        }


        /// <summary>
        /// 从 Excel 或 Json 文件导入祈愿记录
        /// </summary>
        /// <param name="files"></param>
        /// <returns></returns>
        public async Task ImportWishlogItemsFromExcelOrJsonFile(IEnumerable<string> files)
        {
            IsLoading = true;
            StateText = "导入中";
            await Task.Delay(100);
            foreach (var file in files)
            {
                _logger.LogInformation($"Imported file path: {file}.");
                try
                {
                    if (Path.GetExtension(file).ToLower() == ".json")
                    {
                        var result = await _wishlogService.ImportFromJsonFile(file);
                        InfoBarHelper.Success(result, 5000);
                        continue;
                    }
                    if (Path.GetExtension(file).ToLower() == ".xlsx")
                    {
                        var result = await _wishlogService.ImportFromExcelFile(file);
                        InfoBarHelper.Success(result, 5000);
                        continue;
                    }
                    InfoBarHelper.Warning("仅支持 Excel 或 Json 文件", 3000);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "ImportWishlogItemsFromJsonFile");
                    InfoBarHelper.Error(ex);
                }
            }
            StateText = "";
            IsLoading = false;
        }



    }



    public record WishlogSummary_CharacterThumb(string? Name, int Rarity, ElementType Element, int Count, string? Icon, DateTimeOffset LastTime);
    public record WishlogSummary_WeaponThumb(string? Name, int Rarity, WeaponType Type, int Count, string? Icon, DateTimeOffset LastTime);

    public record WishlogSummary_QueryTypeStats(WishType QueryType,
                                                int TotalCount,
                                                int Rank5Count,
                                                int Rank4Count,
                                                DateTimeOffset StartTime,
                                                DateTimeOffset EndTime,
                                                int UpItemCount,
                                                int CurrentGuarantee,
                                                int MaxGuarantee,
                                                int MinGuarantee,
                                                List<WishlogSummary_Rank5Item> Rank5Items,
                                                bool LastRank5ItemIsUp,
                                                int LastRank5ItemGuarantee)
    {

        public bool AnyData => TotalCount > 0;

        public int Rank3Count => TotalCount - Rank5Count - Rank4Count;

        public string Rank5Ratio => ((double)Rank5Count / TotalCount).ToString("P3");

        public string Rank4Ratio => ((double)Rank4Count / TotalCount).ToString("P3");

        public string Rank3Ratio => ((double)Rank3Count / TotalCount).ToString("P3");

        public string GuaranteeType => QueryType == WishType.Permanent ? "保底内" : LastRank5ItemIsUp ? "小保底内" : "大保底内";

        public int BaodiWai => QueryType == WishType.Permanent ? 0 : (Rank5Count - UpItemCount);

        public int BaodiBuwai => QueryType == WishType.Permanent ? Rank5Count : (2 * UpItemCount - Rank5Count + (LastRank5ItemIsUp ? 0 : 1));

        public string AverageRank5Count => ((TotalCount - CurrentGuarantee) / (double)Rank5Count).ToString("F2");

        public string AverageUpRank5Count => QueryType == WishType.Permanent ? "0" : ((TotalCount - CurrentGuarantee - LastRank5ItemGuarantee) / (double)UpItemCount).ToString("F2");

    }


    public record WishlogSummary_Rank5Item(string Name, int Guarantee, DateTimeOffset Time) : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler? PropertyChanged;
        private void OnPropertyChanged([CallerMemberName] string propertyName = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        public string Color { get; set; }


        private string _Foreground;
        public string Foreground
        {
            get { return _Foreground; }
            set
            {
                if (_Foreground != value)
                {
                    _Foreground = value;
                    OnPropertyChanged();
                }
            }
        }

    }



}
