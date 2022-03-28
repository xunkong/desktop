using Microsoft.UI.Xaml.Controls;
using System.Collections.Concurrent;
using System.Collections.Immutable;
using Windows.Storage.Pickers;
using WinRT.Interop;
using Xunkong.Core.Hoyolab;
using Xunkong.Core.Metadata;
using Xunkong.Core.Wish;
using Xunkong.Core.XunkongApi;

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


        static WishlogSummaryViewModel()
        {
            TypeAdapterConfig<CharacterInfo, WishlogSummary_UpItem>.NewConfig().Map(dest => dest.Icon, src => src.FaceIcon);
            TypeAdapterConfig<WeaponInfo, WishlogSummary_UpItem>.NewConfig().Map(dest => dest.Icon, src => src.Icon);
        }


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



        private List<WishlogSummary_ItemThumb> _CharacterThumbs;
        public List<WishlogSummary_ItemThumb> CharacterThumbs
        {
            get => _CharacterThumbs;
            set => SetProperty(ref _CharacterThumbs, value);
        }


        private List<WishlogSummary_ItemThumb> _WeaponThumbs;
        public List<WishlogSummary_ItemThumb> WeaponThumbs
        {
            get => _WeaponThumbs;
            set => SetProperty(ref _WeaponThumbs, value);
        }


        private List<WishlogSummary_EventStats> _CharacterEventStats;
        public List<WishlogSummary_EventStats> CharacterEventStats
        {
            get => _CharacterEventStats;
            set => SetProperty(ref _CharacterEventStats, value);
        }


        private List<WishlogSummary_EventStats> _WeaponEventStats;
        public List<WishlogSummary_EventStats> WeaponEventStats
        {
            get => _WeaponEventStats;
            set => SetProperty(ref _WeaponEventStats, value);
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
                _SelectedUid = await _settingService.GetSettingAsync<int>("LastSelectedUidInWishlogSummaryPage");
                OnPropertyChanged(nameof(SelectedUid));
                if (!Uids.Contains(SelectedUid) || _SelectedUid == 0)
                {
                    SelectedUid = Uids.FirstOrDefault() ?? "";
                }
                if (_SelectedUid == 0)
                {
                    StateText = "没有祈愿数据";
                    await LoadWishlogAsync(true);
                }
                else
                {
                    await LoadWishlogAsync();
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
        public async Task LoadWishlogAsync(bool ignoreWishlogStats = false)
        {
            IsLoading = true;
            await Task.Delay(100);
            try
            {
                WishEventInfo.RegionType = WishlogService.UidToRegionType(_SelectedUid);
                var ctx = _ctxFactory.CreateDbContext();
                var characters = await ctx.CharacterInfos.ToListAsync();
                var weapons = await ctx.WeaponInfos.ToListAsync();
                var wishlogs = await _wishlogService.GetWishlogItemExByUidAsync(_SelectedUid);

                if (!ignoreWishlogStats)
                {
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
                        var currentGuarantee = items.Last().RankType == 5 ? 0 : items.Last().GuaranteeIndex;
                        var maxGuarantee = rank5Count == 0 ? currentGuarantee : items.Where(x => x.RankType == 5).Max(x => x.GuaranteeIndex);
                        var minGuarantee = rank5Count == 0 ? currentGuarantee : items.Where(x => x.RankType == 5).Min(x => x.GuaranteeIndex);
                        var rank5Items = items.Where(x => x.RankType == 5).Select(x => new WishlogSummary_Rank5Item(x.Name, x.GuaranteeIndex, x.Time, type == WishType.Permanent ? true : x.IsUp)).ToList();
                        rank5Items.Where(x => !x.IsUp).ToList().ForEach(x => { x.Color = "#808080"; x.Foreground = "#808080"; });
                        var colors = ColorSet.OrderBy(x => Random.Shared.Next()).ToList();
                        var gi = rank5Items.Where(x => x.IsUp).GroupBy(x => x.Name).OrderBy(x => x.Min(y => y.Time));
                        int index = 0;
                        foreach (var g in gi)
                        {
                            var color = colors[index];
                            index = (index + 1) % colors.Count;
                            g.ToList().ForEach(x => { x.Color = color; x.Foreground = color; });
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
                    var query1 = from c in characters
                                 join w in wishlogs
                                 on c.Name equals w.Name into g
                                 where g.Any()
                                 select new WishlogSummary_ItemThumb(c.Name, c.Rarity, c.Element, c.WeaponType, g.Count(), c.FaceIcon, g.Last().Time);
                    CharacterThumbs = new(query1.OrderByDescending(x => x.Rarity).ThenByDescending(x => x.Count).ThenByDescending(x => x.LastTime));
                    var query2 = from c in weapons
                                 join w in wishlogs
                                 on c.Name equals w.Name into g
                                 where g.Any()
                                 select new WishlogSummary_ItemThumb(c.Name, c.Rarity, ElementType.None, c.WeaponType, g.Count(), c.Icon, g.Last().Time);
                    WeaponThumbs = new(query2.OrderByDescending(x => x.Rarity).ThenByDescending(x => x.Count).ThenByDescending(x => x.LastTime));
                }

                var eventInfos = await ctx.WishEventInfos.ToListAsync();

                var character_groups = eventInfos.Where(x => x.QueryType == WishType.CharacterEvent).GroupBy(x => x.StartTime).ToList();
                var character_dics = characters.ToDictionary(x => x.Name!);
                var character_eventStats = new List<WishlogSummary_EventStats>();
                foreach (var group in character_groups)
                {
                    var stats = group.FirstOrDefault()?.Adapt<WishlogSummary_EventStats>()!;
                    character_eventStats.Add(stats);
                    stats.Name = string.Join(" ", group.Select(x => x.Name));
                    stats.UpItems = group.SelectMany(x => x.Rank5UpItems).Join(character_dics, str => str, dic => dic.Key, (str, dic) => dic.Value).ToList().Adapt<List<WishlogSummary_UpItem>>();
                    stats.UpItems.AddRange(group.FirstOrDefault()!.Rank4UpItems.Join(character_dics, str => str, dic => dic.Key, (str, dic) => dic.Value).Adapt<IEnumerable<WishlogSummary_UpItem>>());
                    var currentEventItems = wishlogs.Where(x => x.QueryType == WishType.CharacterEvent && stats.StartTime <= x.Time && x.Time <= stats.EndTime).OrderByDescending(x => x.Id).ToList();
                    stats.TotalCount = currentEventItems.Count;
                    stats.Rank3Count = currentEventItems.Count(x => x.RankType == 3);
                    stats.Rank4Count = currentEventItems.Count(x => x.RankType == 4);
                    stats.Rank5Count = currentEventItems.Count(x => x.RankType == 5);
                    Parallel.ForEach(stats.UpItems, x => x.Count = currentEventItems.Count(y => y.Name == x.Name));
                    stats.Rank5Items = QueryTypeStats?.FirstOrDefault(x => x.QueryType == WishType.CharacterEvent)?.Rank5Items
                                                      .Where(x => x.Time >= stats.StartTime && x.Time <= stats.EndTime)
                                                      .ToList() ?? new();
                }
                CharacterEventStats = character_eventStats.OrderByDescending(x => x.StartTime).ToList();

                var weapon_groups = eventInfos.Where(x => x.QueryType == WishType.WeaponEvent).GroupBy(x => x.StartTime).ToList();
                var weapon_dics = weapons.ToDictionary(x => x.Name!);
                var weapon_eventStats = new List<WishlogSummary_EventStats>();
                foreach (var group in weapon_groups)
                {
                    var stats = group.FirstOrDefault()?.Adapt<WishlogSummary_EventStats>()!;
                    weapon_eventStats.Add(stats);
                    stats.Name = string.Join(" ", group.Select(x => x.Name));
                    stats.UpItems = group.SelectMany(x => x.Rank5UpItems).Join(weapon_dics, str => str, dic => dic.Key, (str, dic) => dic.Value).ToList().Adapt<List<WishlogSummary_UpItem>>();
                    stats.UpItems.AddRange(group.FirstOrDefault()!.Rank4UpItems.Join(weapon_dics, str => str, dic => dic.Key, (str, dic) => dic.Value).Adapt<IEnumerable<WishlogSummary_UpItem>>());
                    var currentEventItems = wishlogs.Where(x => x.QueryType == WishType.WeaponEvent && stats.StartTime <= x.Time && x.Time <= stats.EndTime).OrderByDescending(x => x.Id).ToList();
                    stats.TotalCount = currentEventItems.Count;
                    stats.Rank3Count = currentEventItems.Count(x => x.RankType == 3);
                    stats.Rank4Count = currentEventItems.Count(x => x.RankType == 4);
                    stats.Rank5Count = currentEventItems.Count(x => x.RankType == 5);
                    Parallel.ForEach(stats.UpItems, x => x.Count = currentEventItems.Count(y => y.Name == x.Name));
                    stats.Rank5Items = QueryTypeStats?.FirstOrDefault(x => x.QueryType == WishType.WeaponEvent)?.Rank5Items
                                                      .Where(x => x.Time >= stats.StartTime && x.Time <= stats.EndTime)
                                                      .ToList() ?? new();
                }
                WeaponEventStats = weapon_eventStats.OrderByDescending(x => x.StartTime).ToList();

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
                Action action = () => Process.Start(new ProcessStartInfo { FileName = Path.GetDirectoryName(file), UseShellExecute = true, });
                InfoBarHelper.ShowWithButton(InfoBarSeverity.Success, null, "云端祈愿记录已备份到本地", "打开文件夹", action, null, 3000);
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


        private bool _IsRefreshPageTeachingTipOpen;
        public bool IsRefreshPageTeachingTipOpen
        {
            get => _IsRefreshPageTeachingTipOpen;
            set => SetProperty(ref _IsRefreshPageTeachingTipOpen, value);
        }



        /// <summary>
        /// 获取角色、武器、卡池、国际化等信息
        /// </summary>
        /// <returns></returns>
        [ICommand(AllowConcurrentExecutions = false)]
        private async Task GetMetadataAsync()
        {
            IsLoading = true;
            StateText = "正在更新元数据";
            await Task.Delay(100);
            try
            {
                await _xunkongApiService.GetAllGenshinDataFromServerAsync();
                StateText = "完成";
                IsRefreshPageTeachingTipOpen = true;
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
            if (!files.Any())
            {
                return;
            }
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


        /// <summary>
        /// 选择导入的文件
        /// </summary>
        /// <returns></returns>
        [ICommand(AllowConcurrentExecutions = false)]
        private async Task OpenImportedFilesAsync()
        {
            try
            {
                var dialog = new FileOpenPicker();
                dialog.SuggestedStartLocation = PickerLocationId.DocumentsLibrary;
                dialog.FileTypeFilter.Add("*");
                dialog.FileTypeFilter.Add(".json");
                dialog.FileTypeFilter.Add(".xlsx");
                InitializeWithWindow.Initialize(dialog, MainWindow.Hwnd);
                var files = await dialog.PickMultipleFilesAsync();
                await ImportWishlogItemsFromExcelOrJsonFile(files.Select(x => x.Path));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Open imported files.");
                InfoBarHelper.Error(ex);
            }
        }



    }


}
