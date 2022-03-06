using AngleSharp;
using CommunityToolkit.WinUI.UI;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Windows.ApplicationModel.Background;
using Windows.ApplicationModel.DataTransfer;
using Xunkong.Core.Hoyolab;
using Xunkong.Core.XunkongApi;

namespace Xunkong.Desktop.ViewModels
{

    internal partial class SettingViewModel : ObservableObject
    {

        private readonly ILogger<SettingViewModel> _logger;

        private readonly IDbContextFactory<XunkongDbContext> _dbContextFactory;

        private readonly DbConnectionFactory<SqliteConnection> _dbConnectionFactory;

        private readonly XunkongDbContext _webToolItemDbContext;

        private readonly HttpClient _httpClient;

        private readonly XunkongApiService _xunkongApiService;

        private readonly WishlogService _wishlogService;

        private readonly HoyolabService _hoyolabService;



        public string AppName => XunkongEnvironment.AppName;

        public string AppVersion => XunkongEnvironment.AppVersion.ToString();


        private const string DailyNoteRefreshBackgroundTask = "DailyNoteRefreshBackgroundTask";

        private const string HoyolabDailyCheckInTask = "HoyolabDailyCheckInTask";


        public SettingViewModel(ILogger<SettingViewModel> logger,
                                IDbContextFactory<XunkongDbContext> dbContextFactory,
                                DbConnectionFactory<SqliteConnection> dbConnectionFactory,
                                HttpClient httpClient,
                                XunkongApiService xunkongApiService,
                                WishlogService wishlogService,
                                HoyolabService hoyolabService)
        {
            _logger = logger;
            _dbContextFactory = dbContextFactory;
            _dbConnectionFactory = dbConnectionFactory;
            _webToolItemDbContext = dbContextFactory.CreateDbContext();
            _httpClient = httpClient;
            _xunkongApiService = xunkongApiService;
            _wishlogService = wishlogService;
            _hoyolabService = hoyolabService;
        }



        public async Task InitializeDataAsync()
        {
            try
            {
                var list = await _webToolItemDbContext.WebToolItems.OrderBy(x => x.Order).ToListAsync();
                _WebToolItemList = new(list);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error when init webtool setting items.");
                InfoBarHelper.Error(ex, "无法加载网页小工具的数据");
            }
            try
            {
                var allTasks = BackgroundTaskRegistration.AllTasks;
                if (allTasks.Any(x => x.Value.Name == DailyNoteRefreshBackgroundTask))
                {
                    _IsRegisterDailyNoteTask = true;
                    OnPropertyChanged(nameof(IsRegisterDailyNoteTask));
                }
                if (allTasks.Any(x => x.Value.Name == HoyolabDailyCheckInTask))
                {
                    _IsRegisterHoyolabCheckInTask = true;
                    OnPropertyChanged(nameof(IsRegisterHoyolabCheckInTask));
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error when get background task setting.");
                InfoBarHelper.Error(ex, "无法获取后台任务的设置");
            }

        }



        #region Version and Theme


        private ChannelType _SelectedChannel = XunkongEnvironment.Channel;
        public ChannelType SelectedChannel
        {
            get => _SelectedChannel;
            set => SetProperty(ref _SelectedChannel, value);
        }


        public List<ChannelType> ChannelTypeList { get; set; } = Enum.GetValues<ChannelType>().Take(XunkongEnvironment.Channel == ChannelType.Development ? 3 : 2).ToList();



        [ICommand(AllowConcurrentExecutions = false)]
        private async Task CheckUpdateAsync()
        {
            try
            {
                var version = await _xunkongApiService.CheckUpdateAsync(SelectedChannel);
                if (version.Version > XunkongEnvironment.AppVersion && !string.IsNullOrWhiteSpace(version.PackageUrl))
                {
                    RoutedEventHandler eventHandler = (_, _) => Process.Start(new ProcessStartInfo { FileName = version.PackageUrl, UseShellExecute = true, });
                    InfoBarHelper.ShowWithButton(InfoBarSeverity.Success, $"新版本 {version.Version}", version.Abstract, "下载", eventHandler);
                }
                else
                {
                    InfoBarHelper.Information("已是最新版本");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error in {nameof(CheckUpdateAsync)}");
                InfoBarHelper.Error(ex);
            }
        }




        private string _SelectedTheme;
        public string SelectedTheme
        {
            get => _SelectedTheme;
            set => SetProperty(ref _SelectedTheme, value);
        }

        private int _SelectedThemeIndex = LocalSettingHelper.GetSetting<int>(SettingKeys.ApplicationTheme);
        public int SelectedThemeIndex
        {
            get => _SelectedThemeIndex;
            set
            {
                LocalSettingHelper.SaveSetting(SettingKeys.ApplicationTheme, value);
                WeakReferenceMessenger.Default.Send(new ChangeApplicationThemeMessage(value));
                SetProperty(ref _SelectedThemeIndex, value);
            }
        }



        private bool _DisableBackgroundWallpaper = LocalSettingHelper.GetSetting<bool>(SettingKeys.DisableBackgroundWallpaper);
        public bool DisableBackgroundWallpaper
        {
            get => _DisableBackgroundWallpaper;
            set
            {
                LocalSettingHelper.SaveSetting(SettingKeys.DisableBackgroundWallpaper, value);
                WeakReferenceMessenger.Default.Send(new DisableBackgroundWallpaperMessage(value));
                SetProperty(ref _DisableBackgroundWallpaper, value);
            }
        }

        #endregion




        #region WebTool Setting



        private ObservableCollection<WebToolItem> _WebToolItemList;
        public ObservableCollection<WebToolItem> WebToolItemList
        {
            get => _WebToolItemList;
            set => SetProperty(ref _WebToolItemList, value);
        }


        private WebToolItem? _SelectedWebToolItem;
        public WebToolItem? SelectedWebToolItem
        {
            get => _SelectedWebToolItem;
            set => SetProperty(ref _SelectedWebToolItem, value);
        }



        [ICommand]
        private void AddWebToolItem()
        {
            var newItem = new WebToolItem();
            if (WebToolItemList is null)
            {
                WebToolItemList = new();
            }
            WebToolItemList.Add(newItem);
            SelectedWebToolItem = newItem;
        }



        [ICommand]
        private void DeleteSelectedWebToolItem()
        {
            try
            {
                if (SelectedWebToolItem is not null)
                {
                    if (SelectedWebToolItem.Id != 0)
                    {
                        _logger.LogDebug("Deleting WebToolItem {Title}", SelectedWebToolItem.Title);
                        _webToolItemDbContext.Remove(SelectedWebToolItem);
                    }
                    var index = WebToolItemList.IndexOf(SelectedWebToolItem);
                    WebToolItemList.Remove(SelectedWebToolItem);
                    if (WebToolItemList.Count < index + 1)
                    {
                        SelectedWebToolItem = WebToolItemList.LastOrDefault();
                    }
                    else
                    {
                        SelectedWebToolItem = WebToolItemList.ElementAt(index);
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error in {nameof(DeleteSelectedWebToolItem)}");
                InfoBarHelper.Error(ex);
            }

        }



        [ICommand]
        private void CloseEditWebToolGrid()
        {
            SelectedWebToolItem = null;
        }



        [ICommand]
        private async Task GetTitleAndIconByUrlAsync()
        {
            if (string.IsNullOrWhiteSpace(SelectedWebToolItem?.Url))
            {
                return;
            }
            var url = SelectedWebToolItem.Url;
            _logger.LogDebug($"Get WebToolItem title and icon by url {url}");
            try
            {
                var uri = new Uri(url);
                var baseUri = new Uri($"{uri.Scheme}://{uri.Host}");
                var html = await _httpClient.GetStringAsync(url);
                var context = BrowsingContext.New(Configuration.Default);
                var dom = await context.OpenAsync(x => { x.Content(html); x.Address(url); });
                SelectedWebToolItem.Title = dom.Title;
                var iconList = dom.Head?.Children.Where(x => x.Attributes["rel"]?.Value.Contains("icon") ?? false);
                var href = iconList?.FirstOrDefault()?.Attributes["href"]?.Value;
                if (string.IsNullOrWhiteSpace(href))
                {
                    SelectedWebToolItem.Icon = new Uri(baseUri, "favicon.ico").ToString();
                }
                else
                {
                    SelectedWebToolItem.Icon = new Uri(baseUri, href).ToString();
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error in {nameof(GetTitleAndIconByUrlAsync)}");
                InfoBarHelper.Error(ex);
            }
        }





        [ICommand]
        private async Task SaveWebToolItemAsync()
        {
            try
            {
                var list = WebToolItemList.Where(x => !string.IsNullOrWhiteSpace(x.Url)).ToList();
                for (int i = 0; i < list.Count; i++)
                {
                    list[i].Order = i;
                }
                WebToolItemList = new(list);
                _webToolItemDbContext.UpdateRange(list);
                await _webToolItemDbContext.SaveChangesAsync();
                _logger.LogDebug("Saved the above WebToolItem changes.");
                InfoBarHelper.Success("保存成功");
                WeakReferenceMessenger.Default.Send<RefreshWebToolNavItemMessage>();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error in {nameof(SaveWebToolItemAsync)}");
                InfoBarHelper.Error(ex);
            }
        }



        #endregion



        #region Refresh Daily Note Tile Task


        private bool _IsRegisterDailyNoteTask;
        public bool IsRegisterDailyNoteTask
        {
            get => _IsRegisterDailyNoteTask;
            set
            {
                ChangeDailyNoteBackgroundTaskState(value);
                SetProperty(ref _IsRegisterDailyNoteTask, value);
            }
        }


        private async void ChangeDailyNoteBackgroundTaskState(bool enable, bool ignoreDuplicateCheck = false)
        {
            if (_IsRegisterDailyNoteTask == enable && !ignoreDuplicateCheck)
            {
                return;
            }
            try
            {
                var allTasks = BackgroundTaskRegistration.AllTasks;
                foreach (var item in allTasks)
                {
                    if (item.Value.Name == DailyNoteRefreshBackgroundTask)
                    {
                        item.Value.Unregister(true);
                    }
                }
                if (enable)
                {
                    var requestStatus = await BackgroundExecutionManager.RequestAccessAsync();
                    var builder = new BackgroundTaskBuilder();
                    builder.Name = DailyNoteRefreshBackgroundTask;
                    builder.SetTrigger(new TimeTrigger(16, false));
                    builder.TaskEntryPoint = "Xunkong.Desktop.BackgroundTask.DailyNoteTask";
                    BackgroundTaskRegistration task = builder.Register();
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in {MethodName}", nameof(ChangeDailyNoteBackgroundTaskState));
                InfoBarHelper.Error(ex);
                IsRegisterDailyNoteTask = false;
            }
        }



        private bool _EnableDailyNoteNotification = LocalSettingHelper.GetSetting<bool>(SettingKeys.EnableDailyNoteNotification);
        public bool EnableDailyNoteNotification
        {
            get => _EnableDailyNoteNotification;
            set
            {
                if (_EnableDailyNoteNotification != value)
                {
                    LocalSettingHelper.SaveSetting(SettingKeys.EnableDailyNoteNotification, value);
                }
                SetProperty(ref _EnableDailyNoteNotification, value);
            }
        }



        private bool _DisableBackgroundTaskOutputLog = LocalSettingHelper.GetSetting<bool>(SettingKeys.DisableBackgroundTaskOutputLog);
        public bool DisableBackgroundTaskOutputLog
        {
            get => _DisableBackgroundTaskOutputLog;
            set
            {
                if (_DisableBackgroundTaskOutputLog != value)
                {
                    LocalSettingHelper.SaveSetting(SettingKeys.DisableBackgroundTaskOutputLog, value);
                }
                SetProperty(ref _DisableBackgroundTaskOutputLog, value);
            }
        }



        private int _DailyNoteNotification_ResinThreshold = LocalSettingHelper.GetSetting(SettingKeys.DailyNoteNotification_ResinThreshold, 150);
        public int DailyNoteNotification_ResinThreshold
        {
            get => _DailyNoteNotification_ResinThreshold;
            set
            {
                value = Math.Clamp(value, 0, 160);
                if (_DailyNoteNotification_ResinThreshold != value)
                {
                    _DailyNoteNotification_ResinThreshold = value;
                    LocalSettingHelper.SaveSetting(SettingKeys.DailyNoteNotification_ResinThreshold, value);
                }
                OnPropertyChanged();
            }
        }


        private double _DailyNoteNotification_HomeCoinThreshold = LocalSettingHelper.GetSetting(SettingKeys.DailyNoteNotification_HomeCoinThreshold, 0.9);
        public double DailyNoteNotification_HomeCoinThreshold
        {
            get => _DailyNoteNotification_HomeCoinThreshold;
            set
            {
                value = Math.Clamp(value, 0, 1);
                if (_DailyNoteNotification_HomeCoinThreshold != value)
                {
                    _DailyNoteNotification_HomeCoinThreshold = value;
                    LocalSettingHelper.SaveSetting(SettingKeys.DailyNoteNotification_HomeCoinThreshold, value);
                }
                OnPropertyChanged();
            }
        }


        #endregion



        #region Hoyolab Check in Task


        private bool _IsRegisterHoyolabCheckInTask;
        public bool IsRegisterHoyolabCheckInTask
        {
            get => _IsRegisterHoyolabCheckInTask;
            set
            {
                ChangeHoyolabCheckinTaskState(value);
                SetProperty(ref _IsRegisterHoyolabCheckInTask, value);
            }
        }


        private async void ChangeHoyolabCheckinTaskState(bool enable, bool ignoreDuplicateCheck = false)
        {
            if (_IsRegisterHoyolabCheckInTask == enable && !ignoreDuplicateCheck)
            {
                return;
            }
            try
            {
                var allTasks = BackgroundTaskRegistration.AllTasks;
                foreach (var item in allTasks)
                {
                    if (item.Value.Name == HoyolabDailyCheckInTask)
                    {
                        item.Value.Unregister(true);
                    }
                }
                if (enable)
                {
                    var requestStatus = await BackgroundExecutionManager.RequestAccessAsync();
                    var builder = new BackgroundTaskBuilder();
                    builder.Name = HoyolabDailyCheckInTask;
                    builder.SetTrigger(new TimeTrigger((uint)HoyolabCheckinTimeSpan.TotalMinutes, false));
                    builder.TaskEntryPoint = "Xunkong.Desktop.BackgroundTask.HoyolabCheckInTask";
                    BackgroundTaskRegistration task = builder.Register();
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in {MethodName}", nameof(ChangeHoyolabCheckinTaskState));
                InfoBarHelper.Error(ex);
                IsRegisterDailyNoteTask = false;
            }
        }



        private TimeSpan _HoyolabCheckinTimeSpan = LocalSettingHelper.GetSetting("HoyolabCheckinTimeSpan", TimeSpan.FromHours(2));
        public TimeSpan HoyolabCheckinTimeSpan
        {
            get => _HoyolabCheckinTimeSpan;
            set
            {
                ChangeHoyolabCheckinTimeSpan(value);
                SetProperty(ref _HoyolabCheckinTimeSpan, value);
            }
        }


        private void ChangeHoyolabCheckinTimeSpan(TimeSpan value)
        {
            if (value < TimeSpan.FromMinutes(15))
            {
                InfoBarHelper.Warning("间隔时间至少为 15 分钟", 3000);
                return;
            }
            if (_HoyolabCheckinTimeSpan == value)
            {
                return;
            }
            LocalSettingHelper.SaveSetting("HoyolabCheckinTimeSpan", value);
            ChangeHoyolabCheckinTaskState(IsRegisterHoyolabCheckInTask, true);
        }



        private bool _DailyCheckInSuccessNotification = LocalSettingHelper.GetSetting<bool>(SettingKeys.DailyCheckInSuccessNotification);
        public bool DailyCheckInSuccessNotification
        {
            get => _DailyCheckInSuccessNotification;
            set
            {
                LocalSettingHelper.SaveSetting(SettingKeys.DailyCheckInSuccessNotification, value);
                SetProperty(ref _DailyCheckInSuccessNotification, value);
            }
        }


        private bool _DailyCheckInErrorNotification = LocalSettingHelper.GetSetting<bool>(SettingKeys.DailyCheckInErrorNotification);
        public bool DailyCheckInErrorNotification
        {
            get => _DailyCheckInErrorNotification;
            set
            {
                LocalSettingHelper.SaveSetting(SettingKeys.DailyCheckInErrorNotification, value);
                SetProperty(ref _DailyCheckInErrorNotification, value);
            }
        }


        [ICommand(AllowConcurrentExecutions = false)]
        private async Task CheckInNowAsync()
        {
            try
            {
                var roles = await _hoyolabService.GetUserGameRoleInfoListAsync();
                foreach (var role in roles)
                {
                    try
                    {
                        if (await _hoyolabService.SignInAsync(role))
                        {
                            InfoBarHelper.Success($"{role.Nickname} ({role.Uid}) 签到成功");
                        }
                        else
                        {
                            InfoBarHelper.Information($"{role.Nickname} ({role.Uid}) 无需签到");
                        }
                    }
                    catch (HoyolabException ex)
                    {
                        _logger.LogError(ex, $"Check in with account: {role.Nickname} ({role.Uid}).");
                        InfoBarHelper.Error(ex, $"{role.Nickname} ({role.Uid})");
                    }
                    catch (HttpRequestException ex)
                    {
                        _logger.LogError(ex, $"Check in with account: {role.Nickname} ({role.Uid}).");
                        InfoBarHelper.Error(ex, $"{role.Nickname} ({role.Uid})");
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Check in but other exception.");
                InfoBarHelper.Error(ex);
            }
            finally
            {
                InfoBarHelper.Information("签到结束");
            }
        }




        #endregion



        #region Others



        [ICommand(AllowConcurrentExecutions = false)]
        private async Task GetSpiralAbyssInfoSummary()
        {
            try
            {
                var role = await _hoyolabService.GetLastSelectedOrFirstUserGameRoleInfoAsync();
                var c = await _hoyolabService.GetSpiralAbyssInfoAsync(role, 1);
                var l = await _hoyolabService.GetSpiralAbyssInfoAsync(role, 2);
                InfoBarHelper.Success($"{role.Nickname}的深渊记录保存成功");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Get spiral abyss summary info.");
                InfoBarHelper.Error(ex);
            }
        }


        [ICommand(AllowConcurrentExecutions = false)]
        private async Task MigrateDatabaseAsync()
        {
            try
            {
                using var ctx = _dbContextFactory.CreateDbContext();
                await ctx.Database.MigrateAsync();
                InfoBarHelper.Success($"成功");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "MigrateDatabaseAsync.");
                InfoBarHelper.Error(ex);
            }
        }


        [ICommand(AllowConcurrentExecutions = false)]
        private async Task ClearImageCacheAsync()
        {
            try
            {
                await ImageCache.Instance.ClearAsync();
                InfoBarHelper.Success($"成功");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "ClearImageCacheAsync.");
                InfoBarHelper.Error(ex);
            }
        }


        [ICommand]
        private void CopyDevelopersMail()
        {
            var data = new DataPackage();
            data.RequestedOperation = DataPackageOperation.Copy;
            data.SetText("scighost@outlook.com");
            Clipboard.SetContent(data);
            InfoBarHelper.Success("已复制开发者的邮件（如果没收到回复，可能是被识别为垃圾邮件了）", 5000);
        }


        #endregion


    }
}
