using Microsoft.UI.Xaml.Controls;
using System.Collections.Concurrent;
using System.Collections.Immutable;
using System.Diagnostics;
using Xunkong.Desktop.Pages;
using Xunkong.GenshinData.Character;
using Xunkong.GenshinData.Weapon;
using Xunkong.Hoyolab;
using Xunkong.Hoyolab.Wishlog;

namespace Xunkong.Desktop.ViewModels;


internal partial class WishlogSummaryViewModel : ObservableObject
{

    private readonly WishlogService _wishlogService;

    private readonly XunkongApiService _xunkongApiService;

    private readonly ImmutableList<string> ColorSet = new[] { "#5470C6", "#91CC75", "#FAC858", "#EE6666", "#73C0DE", "#3BA272", "#FC8452", "#9A60B4", "#EA7CCC" }.ToImmutableList();


    static WishlogSummaryViewModel()
    {
        TypeAdapterConfig<CharacterInfo, WishlogSummaryPage_UpItem>.NewConfig().Map(dest => dest.Icon, src => src.FaceIcon);
        TypeAdapterConfig<WeaponInfo, WishlogSummaryPage_UpItem>.NewConfig().Map(dest => dest.Icon, src => src.Icon);
    }


    public WishlogSummaryViewModel(WishlogService wishlogService, XunkongApiService xunkongApiService)
    {
        _wishlogService = wishlogService;
        _xunkongApiService = xunkongApiService;
        progressHandler = new(str => StateText = str);
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


    private Progress<string> progressHandler;


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
                UserSetting.TrySetValue(SettingKeys.LastSelectedUidInWishlogSummaryPage, uid);
            }
        }
    }


    private List<WishlogSummaryPage_QueryTypeStats> _QueryTypeStats;
    public List<WishlogSummaryPage_QueryTypeStats> QueryTypeStats
    {
        get => _QueryTypeStats;
        set => SetProperty(ref _QueryTypeStats, value);
    }



    private List<WishlogSummaryPage_ItemThumb> _CharacterThumbs;
    public List<WishlogSummaryPage_ItemThumb> CharacterThumbs
    {
        get => _CharacterThumbs;
        set => SetProperty(ref _CharacterThumbs, value);
    }


    private List<WishlogSummaryPage_ItemThumb> _WeaponThumbs;
    public List<WishlogSummaryPage_ItemThumb> WeaponThumbs
    {
        get => _WeaponThumbs;
        set => SetProperty(ref _WeaponThumbs, value);
    }


    private List<WishlogSummaryPage_EventStats> _CharacterEventStats;
    public List<WishlogSummaryPage_EventStats> CharacterEventStats
    {
        get => _CharacterEventStats;
        set => SetProperty(ref _CharacterEventStats, value);
    }


    private List<WishlogSummaryPage_EventStats> _WeaponEventStats;
    public List<WishlogSummaryPage_EventStats> WeaponEventStats
    {
        get => _WeaponEventStats;
        set => SetProperty(ref _WeaponEventStats, value);
    }



    /// <summary>
    /// 页面初始化
    /// </summary>
    /// <returns></returns>
    [RelayCommand]
    public async void InitializePageData()
    {
        try
        {
            Uids = (_wishlogService.GetAllUids()).Select(x => x.ToString()).ToList();
            _SelectedUid = UserSetting.GetValue<int>(SettingKeys.LastSelectedUidInWishlogSummaryPage);
            await Task.Delay(500);
            OnPropertyChanged(nameof(SelectedUid));
            if (!Uids.Contains(SelectedUid) || _SelectedUid == 0)
            {
                SelectedUid = Uids.FirstOrDefault() ?? "";
            }
            if (_SelectedUid == 0)
            {
                StateText = "没有祈愿数据";
            }
        }
        catch (Exception ex)
        {
            NotificationProvider.Error(ex);
        }
    }


    /// <summary>
    /// 加载已选择uid的祈愿信息
    /// </summary>
    /// <returns></returns>
    public void LoadWishlog(bool ignoreWishlogStats = false)
    {
        IsLoading = true;
        try
        {
            // 卡池信息时区
            WishEventInfo.RegionType = WishlogService.UidToRegionType(_SelectedUid);
            // 初始化卡池信息，所有祈愿记录
            using var liteDb = DatabaseProvider.CreateLiteDB();
            var characters = liteDb.GetCollection<CharacterInfo>().FindAll().ToList();
            var dic_characters = characters.Where(x => !string.IsNullOrWhiteSpace(x.Name)).ToImmutableDictionary(x => x.Name!);
            var weapons = liteDb.GetCollection<WeaponInfo>().FindAll().ToList();
            var dic_weapons = weapons.Where(x => !string.IsNullOrWhiteSpace(x.Name)).ToImmutableDictionary(x => x.Name!);
            var wishlogs = _wishlogService.GetWishlogItemExByUid(_SelectedUid);
            var wishlogs_group_dic = wishlogs.GroupBy(x => x.Name).ToImmutableDictionary(x => x.Key);

            if (!ignoreWishlogStats)
            {
                // 根据祈愿类型分类计算
                var queryTypeGroups = wishlogs.GroupBy(x => x.QueryType);
                var statsCollection = new BlockingCollection<WishlogSummaryPage_QueryTypeStats>();
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
                    var rank5Items = items.Where(x => x.RankType == 5).Select(x => new WishlogSummaryPage_Rank5Item(x.Name, x.GuaranteeIndex, x.Time, type == WishType.Permanent || x.IsUp)).ToList();
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
                    var stats = new WishlogSummaryPage_QueryTypeStats(type,
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
                var query_character = from g in wishlogs_group_dic
                                      join c in dic_characters
                                      on g.Key equals c.Key
                                      select new WishlogSummaryPage_ItemThumb(g.Key, c.Value.Rarity, c.Value.Element, c.Value.WeaponType, g.Value.Count(), c.Value.FaceIcon, g.Value.Last().Time);
                CharacterThumbs = query_character.OrderByDescending(x => x.Rarity).ThenByDescending(x => x.Count).ThenByDescending(x => x.LastTime).ToList();
                var query_weapon = from g in wishlogs_group_dic
                                   join c in dic_weapons
                                   on g.Key equals c.Key
                                   select new WishlogSummaryPage_ItemThumb(g.Key, c.Value.Rarity, ElementType.None, c.Value.WeaponType, g.Value.Count(), c.Value.Icon, g.Value.Last().Time);
                WeaponThumbs = query_weapon.OrderByDescending(x => x.Rarity).ThenByDescending(x => x.Count).ThenByDescending(x => x.LastTime).ToList();
            }

            // 根据卡池分类计算
            var eventInfos = liteDb.GetCollection<WishEventInfo>().FindAll().ToList();

            var character_groups = eventInfos.Where(x => x.QueryType == WishType.CharacterEvent).GroupBy(x => x.StartTime).ToList();
            var character_eventStats = new List<WishlogSummaryPage_EventStats>();
            foreach (var group in character_groups)
            {
                var stats = group.FirstOrDefault()?.Adapt<WishlogSummaryPage_EventStats>()!;
                character_eventStats.Add(stats);
                stats.Name = string.Join(" ", group.Select(x => x.Name));
                stats.UpItems = group.SelectMany(x => x.Rank5UpItems).Join(dic_characters, str => str, dic => dic.Key, (str, dic) => dic.Value).ToList().Adapt<List<WishlogSummaryPage_UpItem>>();
                stats.UpItems.AddRange(group.FirstOrDefault()!.Rank4UpItems.Join(dic_characters, str => str, dic => dic.Key, (str, dic) => dic.Value).Adapt<IEnumerable<WishlogSummaryPage_UpItem>>());
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
            var weapon_eventStats = new List<WishlogSummaryPage_EventStats>();
            foreach (var group in weapon_groups)
            {
                var stats = group.FirstOrDefault()?.Adapt<WishlogSummaryPage_EventStats>()!;
                weapon_eventStats.Add(stats);
                stats.Name = string.Join(" ", group.Select(x => x.Name));
                stats.UpItems = group.SelectMany(x => x.Rank5UpItems).Join(dic_weapons, str => str, dic => dic.Key, (str, dic) => dic.Value).ToList().Adapt<List<WishlogSummaryPage_UpItem>>();
                stats.UpItems.AddRange(group.FirstOrDefault()!.Rank4UpItems.Join(dic_weapons, str => str, dic => dic.Key, (str, dic) => dic.Value).Adapt<IEnumerable<WishlogSummaryPage_UpItem>>());
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
            NotificationProvider.Error(ex);
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
    [RelayCommand]
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
            if (await _wishlogService.CheckWishlogUrlTimeoutAsync(uid))
            {
                var addCount = await _wishlogService.GetWishlogByUidAsync(uid, progressHandler, getAll);
                StateText = $"新增 {addCount} 条祈愿记录";
            }
            else
            {
                var isSea = uid.ToString()[0] > '5';
                var wishlogUrl = await _wishlogService.FindWishlogUrlFromLogFileAsync(isSea);
                uid = await _wishlogService.GetUidByWishlogUrl(wishlogUrl);
                var addCount = await _wishlogService.GetWishlogByUidAsync(uid, progressHandler, getAll);
                StateText = $"新增 {addCount} 条祈愿记录";
            }
            SelectedUid = uid.ToString();
            InitializePageData();
        }
        catch (Exception ex) when (ex is HoyolabException or XunkongException)
        {
            StateText = ex.Message;
        }
        catch (Exception ex)
        {
            StateText = "";
            NotificationProvider.Error(ex);
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
    [RelayCommand]
    private async Task GetWishlogFromLogFileAsync(bool isSea)
    {
        IsLoading = true;
        await Task.Delay(100);
        try
        {
            int uid = _SelectedUid;
            StateText = "验证祈愿记录网址的有效性";
            try
            {
                var wishlogUrl = await _wishlogService.FindWishlogUrlFromLogFileAsync(isSea);
                uid = await _wishlogService.GetUidByWishlogUrl(wishlogUrl);
            }
            catch (FileNotFoundException)
            {
                if (!await _wishlogService.CheckWishlogUrlTimeoutAsync(uid))
                {
                    StateText = "祈愿记录网址已过期，请在游戏中重新打开历史记录页面";
                    return;
                }
            }
            var addCount = await _wishlogService.GetWishlogByUidAsync(uid, progressHandler);
            StateText = $"新增 {addCount} 条祈愿记录";
            SelectedUid = uid.ToString();
            InitializePageData();
        }
        catch (Exception ex) when (ex is HoyolabException or XunkongException)
        {
            StateText = ex.Message;
        }
        catch (Exception ex)
        {
            StateText = "";
            NotificationProvider.Error(ex);
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
    [RelayCommand]
    private async Task GetWishlogFromInputWishlogUrlAsync()
    {
        var textBox = new TextBox();
        var stackPanel = new StackPanel { Spacing = 8 };
        stackPanel.Children.Add(new TextBlock { Text = "懂得都懂" });
        stackPanel.Children.Add(textBox);
        var dialog = new ContentDialog
        {
            Title = "输入祈愿记录网址",
            Content = stackPanel,
            PrimaryButtonText = "确认",
            SecondaryButtonText = "取消",
            DefaultButton = ContentDialogButton.Primary,
            XamlRoot = MainWindowHelper.XamlRoot,
        };
        var result = await dialog.ShowAsync();
        if (result == ContentDialogResult.Primary && !string.IsNullOrWhiteSpace(textBox.Text))
        {
            IsLoading = true;
            await Task.Delay(100);
            var wishlogUrl = textBox.Text;
            try
            {
                StateText = "验证祈愿记录网址的有效性";
                var uid = await _wishlogService.GetUidByWishlogUrl(wishlogUrl);
                var addCount = await _wishlogService.GetWishlogByUidAsync(uid, progressHandler);
                StateText = $"新增 {addCount} 条祈愿记录";
                SelectedUid = uid.ToString();
                InitializePageData();
            }
            catch (Exception ex) when (ex is HoyolabException or XunkongException)
            {
                StateText = ex.Message;
            }
            catch (Exception ex)
            {
                StateText = "";
                NotificationProvider.Error(ex);
            }
            finally
            {
                IsLoading = false;
            }
        }
    }


    /// <summary>
    /// 祈愿记录备份用户须知
    /// </summary>
    /// <returns></returns>
    private async Task<bool> RequestWishlogBackupAgreementAsync()
    {
        if (AppSetting.TryGetValue<bool>(SettingKeys.WishlogBackupAgreement, out var agree))
        {
            if (agree)
            {
                return true;
            }
        }
        var dialog = new ContentDialog
        {
            Title = "祈愿记录备份用户须知",
            Content = """
            我们无法保证祈愿记录备份的可靠性，您仍需自行保管好本地数据。
            此功能需要上传祈愿记录网址至服务端以验证账号信息，我们不会将您的数据用于别处。
            """,
            PrimaryButtonText = "已了解",
            SecondaryButtonText = "不接受",
            DefaultButton = ContentDialogButton.Primary,
            XamlRoot = MainWindowHelper.XamlRoot,
        };
        var result = await dialog.ShowAsync();
        if (result == ContentDialogResult.Primary)
        {
            AppSetting.SetValue(SettingKeys.WishlogBackupAgreement, true);
            return true;
        }
        else
        {
            return false;
        }
    }


    /// <summary>
    /// 查询指定uid备份在寻空服务器上的祈愿记录
    /// </summary>
    /// <returns></returns>
    [RelayCommand]
    private async Task QueryWishlogInCloudAsync()
    {
        if (_SelectedUid == 0)
        {
            return;
        }
        if (!await RequestWishlogBackupAgreementAsync())
        {
            return;
        }
        var uid = _SelectedUid;
        IsLoading = true;
        StateText = "";
        await Task.Delay(100);
        try
        {
            await _xunkongApiService.GetWishlogBackupLastItemAsync(uid, progressHandler);
        }
        catch (XunkongException ex)
        {
            StateText = ex.Message;
        }
        catch (Exception ex)
        {
            StateText = "";
            NotificationProvider.Error(ex);
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
    [RelayCommand]
    private async Task UploadWishlogToCloudAsync(bool putAll)
    {
        if (_SelectedUid == 0)
        {
            return;
        }
        if (!await RequestWishlogBackupAgreementAsync())
        {
            return;
        }
        var uid = _SelectedUid;
        IsLoading = true;
        StateText = "";
        await Task.Delay(100);
        try
        {
            await _xunkongApiService.PutWishlogListAsync(uid, progressHandler, putAll);
        }
        catch (XunkongException ex)
        {
            StateText = ex.Message;
        }
        catch (Exception ex)
        {
            StateText = "";
            NotificationProvider.Error(ex);
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
    [RelayCommand]
    private async Task DownloadWishlogFromCloudCompletelyAsync()
    {
        if (_SelectedUid == 0)
        {
            return;
        }
        if (!await RequestWishlogBackupAgreementAsync())
        {
            return;
        }
        var uid = _SelectedUid;
        IsLoading = true;
        StateText = "";
        await Task.Delay(100);
        try
        {
            await _xunkongApiService.GetWishlogBackupListAsync(uid, progressHandler, true);
        }
        catch (XunkongException ex)
        {
            StateText = ex.Message;
        }
        catch (Exception ex)
        {
            StateText = "";
            NotificationProvider.Error(ex);
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
    [RelayCommand]
    private async Task DeleteWishlogInCloudCompletelyAsync()
    {
        if (_SelectedUid == 0)
        {
            return;
        }
        if (!await RequestWishlogBackupAgreementAsync())
        {
            return;
        }
        var uid = _SelectedUid;
        IsLoading = true;
        StateText = "";
        try
        {
            var result = await _xunkongApiService.GetWishlogBackupLastItemAsync(uid, progressHandler);
            if (result.CurrentCount == 0)
            {
                StateText = "云端没有记录";
                return;
            }
            if (result.CurrentCount > _wishlogService.GetWishlogCount(uid))
            {
                var dialog = new ContentDialog
                {
                    Title = "警告",
                    Content = $"Uid {uid} 的本地祈愿记录数量少于云端，删除云端记录可能会造成数据丢失，是否继续？",
                    PrimaryButtonText = "继续删除",
                    CloseButtonText = "取消",
                    DefaultButton = ContentDialogButton.Close,
                    XamlRoot = MainWindowHelper.XamlRoot,
                };
                var dialogResult = await dialog.ShowAsync();
                if (dialogResult != ContentDialogResult.Primary)
                {
                    return;
                }
            }
            var file = await _xunkongApiService.GetWishlogAndBackupToLoalAsync(uid, progressHandler);
            Action action = () => Process.Start(new ProcessStartInfo { FileName = Path.GetDirectoryName(file), UseShellExecute = true, });
            NotificationProvider.ShowWithButton(InfoBarSeverity.Success, null, "云端祈愿记录已备份到本地", "打开文件夹", action, null, 3000);
            await _xunkongApiService.DeleteWishlogBackupAsync(uid, progressHandler);
        }
        catch (XunkongException ex)
        {
            StateText = ex.Message;
        }
        catch (Exception ex)
        {
            StateText = "";
            NotificationProvider.Error(ex);
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
    [RelayCommand]
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
            NotificationProvider.Error(ex);
        }
        finally
        {
            IsLoading = false;
        }
    }


    /// <summary>
    /// 导航到祈愿记录管理页面
    /// </summary>
    [RelayCommand]
    private void NavigateToWishlogManagePage()
    {
        try
        {
            MainPageHelper.Navigate(typeof(WishlogManagePage), _SelectedUid);
        }
        catch (Exception ex)
        {
            NotificationProvider.Error(ex);
        }
    }



}


