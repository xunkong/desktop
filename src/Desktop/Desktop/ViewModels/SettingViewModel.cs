using AngleSharp;
using CommunityToolkit.WinUI.UI;
using Microsoft.UI.Xaml.Controls;
using Windows.ApplicationModel.Background;
using Windows.ApplicationModel.DataTransfer;
using Windows.Storage.Pickers;
using Windows.UI.StartScreen;
using WinRT.Interop;
using Xunkong.Core.Hoyolab;
using Xunkong.Core.Metadata;
using Xunkong.Core.XunkongApi;

namespace Xunkong.Desktop.ViewModels
{

    internal partial class SettingViewModel : ObservableObject
    {

        private readonly ILogger<SettingViewModel> _logger;

        private readonly IDbContextFactory<XunkongDbContext> _ctxFactory;

        private readonly DbConnectionFactory<SqliteConnection> _cntFactory;

        private readonly HttpClient _httpClient;

        private readonly XunkongApiService _xunkongApiService;

        private readonly WishlogService _wishlogService;

        private readonly HoyolabService _hoyolabService;

        private readonly InvokeService _invokeService;

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
                                HoyolabService hoyolabService,
                                InvokeService backgroundService)
        {
            _logger = logger;
            _ctxFactory = dbContextFactory;
            _cntFactory = dbConnectionFactory;
            _httpClient = httpClient;
            _xunkongApiService = xunkongApiService;
            _wishlogService = wishlogService;
            _hoyolabService = hoyolabService;
            _invokeService = backgroundService;
        }



        public void InitializeData()
        {
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
                    Action action = () => Process.Start(new ProcessStartInfo { FileName = version.PackageUrl, UseShellExecute = true, });
                    InfoBarHelper.ShowWithButton(InfoBarSeverity.Success, $"新版本 {version.Version}", version.Abstract, "下载", action);
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


        private XunkongDbContext _webToolItemDbContext;


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


        public async Task InitializeWebToolItemsAsync()
        {
            if (_webToolItemDbContext is null)
            {
                try
                {
                    _webToolItemDbContext = _ctxFactory.CreateDbContext();
                    var list = await _webToolItemDbContext.WebToolItems.OrderBy(x => x.Order).ToListAsync();
                    _WebToolItemList = new(list);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error when init webtool setting items.");
                    InfoBarHelper.Error(ex, "无法加载网页小工具的数据");
                }
            }
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



        #region Start Game


        private string? _GameExePath = LocalSettingHelper.GetSetting(SettingKeys.GameExePath, "不指定具体文件则会从注册表查找");
        public string? GameExePath
        {
            get => _GameExePath;
            set => SetProperty(ref _GameExePath, value);
        }


        private int _UnlockedFPS = LocalSettingHelper.GetSetting(SettingKeys.TargetFPS, 60);
        public int UnlockedFPS
        {
            get => _UnlockedFPS;
            set
            {
                LocalSettingHelper.SaveSetting(SettingKeys.TargetFPS, value);
                SetProperty(ref _UnlockedFPS, value);
            }
        }


        private bool _UsePopupWindow = LocalSettingHelper.GetSetting<bool>(SettingKeys.IsPopupWindow);
        public bool UsePopupWindow
        {
            get => _UsePopupWindow;
            set
            {
                LocalSettingHelper.SaveSetting(SettingKeys.IsPopupWindow, value);
                SetProperty(ref _UsePopupWindow, value);
            }
        }


        [ICommand(AllowConcurrentExecutions = false)]
        private async Task StartGameAsync()
        {
            try
            {
                await _invokeService.StartGameAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Start game in app.");
                InfoBarHelper.Error(ex);
            }
        }


        [ICommand(AllowConcurrentExecutions = false)]
        private async Task ChangeGameExePathAsync()
        {
            try
            {
                var dialog = new FileOpenPicker();
                dialog.SuggestedStartLocation = PickerLocationId.ComputerFolder;
                dialog.FileTypeFilter.Add(".exe");
                InitializeWithWindow.Initialize(dialog, MainWindow.Hwnd);
                var file = await dialog.PickSingleFileAsync();
                if (file != null)
                {
                    var path = file.Path;
                    if (path.EndsWith("YuanShen.exe") || path.EndsWith("GenshinImpact.exe"))
                    {
                        LocalSettingHelper.SaveSetting(SettingKeys.GameExePath, path);
                        GameExePath = file.Path;
                    }
                    else
                    {
                        InfoBarHelper.Warning("文件名不太对", 3000);
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Change genshin exe file path.");
                InfoBarHelper.Error(ex);
            }
        }


        private ObservableCollection<GenshinUserAccount> _UserAccounts;
        public ObservableCollection<GenshinUserAccount> UserAccounts
        {
            get => _UserAccounts;
            set => SetProperty(ref _UserAccounts, value);
        }


        private GenshinUserAccount? _SelectedUserAccount;
        public GenshinUserAccount? SelectedUserAccount
        {
            get => _SelectedUserAccount;
            set => SetProperty(ref _SelectedUserAccount, value);
        }



        public async Task InitializeGenshinUserAccountsAsync(bool fource = false)
        {
            if (fource || UserAccounts == null)
            {
                try
                {
                    using var ctx = _ctxFactory.CreateDbContext();
                    var accounts = await ctx.GenshinUsersAccounts.AsNoTracking().ToListAsync();
                    UserAccounts = new(accounts);
                }
                catch (Exception ex)
                {
                    InfoBarHelper.Error(ex);
                    _logger.LogError(ex, "Initialze genshin user account.");
                }
            }
            try
            {
                var jumpList = await JumpList.LoadCurrentAsync();
                var deleting = jumpList.Items.Where(x => x.Arguments.StartsWith("startgame_")).ToList();
                foreach (var item in deleting)
                {
                    jumpList.Items.Remove(item);
                }
                if (UserAccounts is not null)
                {
                    foreach (var item in UserAccounts)
                    {
                        var jumpitem = JumpListItem.CreateWithArguments($"startgame_{item.UserName}", item.UserName);
                        jumpitem.Logo = new Uri("ms-appx:///Assets/Images/Transparent.png");
                        jumpitem.GroupName = "启动游戏";
                        jumpList.Items.Add(jumpitem);
                    }
                }
                await jumpList.SaveAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Refresh jumplist for start game.");
            }
        }



        [ICommand(AllowConcurrentExecutions = false)]
        private async Task AddGenshinUserAccountAsync(bool isOversea)
        {
            try
            {
                var ADLPROD = await InvokeService.GetADLPROD(isOversea);
                using var ctx = _ctxFactory.CreateDbContext();
                var account = await ctx.GenshinUsersAccounts.AsNoTracking().Where(x => x.ADLPROD == ADLPROD).FirstOrDefaultAsync();
                if (account != null)
                {
                    InfoBarHelper.Warning("该账号已存在", 5000);
                    return;
                }
                var dialog = new ContentDialog
                {
                    Title = "起一个好记的名字吧",
                    Content = new TextBox(),
                    PrimaryButtonText = "确认",
                    SecondaryButtonText = "取消",
                    DefaultButton = ContentDialogButton.Primary,
                    XamlRoot = MainWindow.XamlRoot,
                };
                var result = await dialog.ShowAsync();
                if (result == ContentDialogResult.Primary)
                {
                    var userName = (dialog.Content as TextBox)?.Text;
                    if (string.IsNullOrWhiteSpace(userName))
                    {
                        InfoBarHelper.Warning("用户名不可为空");
                        return;
                    }
                    if (await ctx.GenshinUsersAccounts.AnyAsync(x => x.UserName == userName))
                    {
                        InfoBarHelper.Warning("用户名与已保存账号重复");
                        return;
                    }
                    var now = DateTimeOffset.Now;
                    account = new GenshinUserAccount
                    {
                        UserName = userName,
                        IsOversea = isOversea,
                        ADLPROD = ADLPROD,
                        CreateTime = now,
                        LastAccessTime = now,
                    };
                    ctx.Add(account);
                    await ctx.SaveChangesAsync();
                    InfoBarHelper.Success("账号已保存，在开始菜单的图标上使用右键快速切换账号", 10000);
                    await InitializeGenshinUserAccountsAsync(true);
                }
            }
            catch (Exception ex)
            {
                InfoBarHelper.Error(ex);
                _logger.LogError(ex, "Save current genshin user account.");
            }
        }


        [ICommand(AllowConcurrentExecutions = false)]
        private async Task DeleteSelectedGenshinUserAccountAsync()
        {
            if (SelectedUserAccount is null)
            {
                return;
            }
            try
            {
                using var cnt = _cntFactory.CreateDbConnection();
                var userName = SelectedUserAccount.UserName;
                await cnt.ExecuteAsync($"DELETE FROM GenshinUserAccounts WHERE UserName=@UserName", SelectedUserAccount);
                SelectedUserAccount = null;
                InfoBarHelper.Success($"账号 {userName} 已删除");
                await InitializeGenshinUserAccountsAsync(true);
            }
            catch (Exception ex)
            {
                InfoBarHelper.Error(ex);
                _logger.LogError(ex, "Delete selected genshin user account.");
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
                using var ctx = _ctxFactory.CreateDbContext();
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



        private int _PrecacheImage_TotalCount;
        public int PrecacheImage_TotalCount
        {
            get => _PrecacheImage_TotalCount;
            set => SetProperty(ref _PrecacheImage_TotalCount, value);
        }



        private int _PrecacheImage_FinishCount;
        public int PrecacheImage_FinishCount
        {
            get => _PrecacheImage_FinishCount;
            set => SetProperty(ref _PrecacheImage_FinishCount, value);
        }


        private object _precacheImage_lock = new();


        [ICommand(AllowConcurrentExecutions = false)]
        private async Task PrecacheAllImagesAsync()
        {
            try
            {
                var images = new List<string>();
                using var ctx = _ctxFactory.CreateDbContext();
                var list = await ctx.CharacterInfos.Where(x => !string.IsNullOrWhiteSpace(x.FaceIcon)).Select(x => x.FaceIcon).ToListAsync();
                images.AddRange(list!);
                list = await ctx.CharacterInfos.Where(x => !string.IsNullOrWhiteSpace(x.SideIcon)).Select(x => x.SideIcon).ToListAsync();
                images.AddRange(list!);
                list = await ctx.CharacterInfos.Where(x => !string.IsNullOrWhiteSpace(x.GachaSplash)).Select(x => x.GachaSplash).ToListAsync();
                images.AddRange(list!);
                list = await ctx.WeaponInfos.Where(x => !string.IsNullOrWhiteSpace(x.Icon)).Select(x => x.Icon).ToListAsync();
                images.AddRange(list!);
                list = await ctx.WeaponInfos.Where(x => !string.IsNullOrWhiteSpace(x.AwakenIcon)).Select(x => x.AwakenIcon).ToListAsync();
                images.AddRange(list!);
                list = await ctx.Set<CharacterTalentInfo>().Where(x => !string.IsNullOrWhiteSpace(x.Icon)).Select(x => x.Icon).ToListAsync();
                images.AddRange(list!);
                list = await ctx.Set<CharacterConstellationInfo>().Where(x => !string.IsNullOrWhiteSpace(x.Icon)).Select(x => x.Icon).ToListAsync();
                images.AddRange(list!);
                _PrecacheImage_FinishCount = 0;
                PrecacheImage_TotalCount = images.Count;
                await Parallel.ForEachAsync(images, async (url, _) =>
                {
                    try
                    {
                        var uri = new Uri(url!);
                        await ImageCache.Instance.PreCacheAsync(uri);
                        lock (_precacheImage_lock)
                        {
                            MainWindow.Current.DispatcherQueue.TryEnqueue(() => PrecacheImage_FinishCount++);
                        }
                    }
                    catch { }
                });
                await Task.Delay(3000);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Precache all images.");
                InfoBarHelper.Error(ex);
            }
        }



        #endregion


    }
}
