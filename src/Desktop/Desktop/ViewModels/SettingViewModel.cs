using AngleSharp;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging.Messages;
using CommunityToolkit.WinUI.UI;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.UI.Xaml.Controls;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using Windows.ApplicationModel.Background;
using Windows.Storage;
using Xunkong.Core.XunkongApi;
using Xunkong.Desktop.Services;

namespace Xunkong.Desktop.ViewModels
{

    [InjectService]
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
                _EnableDailyNoteNotification = LocalSettingHelper.GetSetting<bool>(SettingKeys.EnableDailyNoteNotification);
                OnPropertyChanged(nameof(EnableDailyNoteNotification));
                _DailyNoteNotification_ResinThreshold = LocalSettingHelper.GetSetting(SettingKeys.DailyNoteNotification_ResinThreshold, 160);
                OnPropertyChanged(nameof(DailyNoteNotification_ResinThreshold));
                _DailyNoteNotification_HomeCoinThreshold = LocalSettingHelper.GetSetting(SettingKeys.DailyNoteNotification_HomeCoinThreshold, 1.0);
                OnPropertyChanged(nameof(DailyNoteNotification_HomeCoinThreshold));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error when get background task setting.");
                InfoBarHelper.Error(ex, "无法获取后台任务的设置");
            }

        }





        private ChannelType _SelectedChannel = XunkongEnvironment.Channel;
        public ChannelType SelectedChannel
        {
            get => _SelectedChannel;
            set => SetProperty(ref _SelectedChannel, value);
        }


        public List<ChannelType> ChannelTypeList { get; set; } = Enum.GetValues<ChannelType>().Take(3).ToList();



        [ICommand(AllowConcurrentExecutions = false)]
        private async Task CheckUpdateAsync()
        {
            try
            {
                var version = await _xunkongApiService.CheckUpdateAsync(SelectedChannel);
                if (version.Version > XunkongEnvironment.AppVersion && !string.IsNullOrWhiteSpace(version.PackageUrl))
                {
                    var button = new Button { Content = "下载", HorizontalAlignment = Microsoft.UI.Xaml.HorizontalAlignment.Right };
                    button.Click += (_, _) => Process.Start(new ProcessStartInfo
                    {
                        FileName = version.PackageUrl,
                        UseShellExecute = true,
                    });
                    var infoBar = new InfoBar
                    {
                        Severity = InfoBarSeverity.Success,
                        Title = $"新版本 {version.Version}",
                        Message = version.Abstract,
                        ActionButton = button,
                        IsOpen = true,
                    };
                    InfoBarHelper.Show(infoBar);
                }
                else
                {
                    InfoBarHelper.Information("已是最新版本");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in {MethodName}", nameof(CheckUpdateAsync));
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
                SetProperty(ref _SelectedThemeIndex, value);
            }
        }






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
                _logger.LogError(ex, "Error in {MethodName}", nameof(DeleteSelectedWebToolItem));
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
            _logger.LogDebug("Get WebToolItem title and icon by url {Url}", url);
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
                _logger.LogError(ex, "Error in {MethodName}", nameof(GetTitleAndIconByUrlAsync));
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
                _logger.LogError(ex, "Error in {MethodName}", nameof(SaveWebToolItemAsync));
                InfoBarHelper.Error(ex);
            }
        }



        #endregion



        #region Background Task



        private bool _IsRegisterDailyNoteTask;
        public bool IsRegisterDailyNoteTask
        {
            get => _IsRegisterDailyNoteTask;
            set
            {
                if (_IsRegisterDailyNoteTask != value)
                {
                    ChangeDailyNoteBackgroundTaskStateAsync(value);
                }
                SetProperty(ref _IsRegisterDailyNoteTask, value);
            }
        }


        private async void ChangeDailyNoteBackgroundTaskStateAsync(bool enable)
        {
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
                _IsRegisterDailyNoteTask = !IsRegisterDailyNoteTask;
                OnPropertyChanged(nameof(IsRegisterDailyNoteTask));
                _logger.LogError(ex, "Error in {MethodName}", nameof(ChangeDailyNoteBackgroundTaskStateAsync));
                InfoBarHelper.Error(ex);
            }
        }



        private bool _EnableDailyNoteNotification;
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


        private int _DailyNoteNotification_ResinThreshold;
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


        private double _DailyNoteNotification_HomeCoinThreshold;
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



        [ICommand(AllowConcurrentExecutions = false)]
        private async Task GetTravelRecordSummary()
        {
            try
            {
                InfoBarHelper.Information("请稍等", 6000);
                var role = await _hoyolabService.GetLastSelectedOrFirstUserGameRoleInfoAsync();
                var now = DateTime.UtcNow.AddHours(8);
                var month = now.Month;
                var r = await _hoyolabService.GetTravelRecordSummaryAsync(role, month);
                var s = await _hoyolabService.GetTravelRecordDetailAsync(role, month, Core.TravelRecord.TravelRecordAwardType.Primogems);
                var t = await _hoyolabService.GetTravelRecordDetailAsync(role, month, Core.TravelRecord.TravelRecordAwardType.Mora);
                month = now.AddMonths(-1).Month;
                r = await _hoyolabService.GetTravelRecordSummaryAsync(role, month);
                s = await _hoyolabService.GetTravelRecordDetailAsync(role, month, Core.TravelRecord.TravelRecordAwardType.Primogems);
                t = await _hoyolabService.GetTravelRecordDetailAsync(role, month, Core.TravelRecord.TravelRecordAwardType.Mora);
                month = now.AddMonths(-2).Month;
                r = await _hoyolabService.GetTravelRecordSummaryAsync(role, month);
                s = await _hoyolabService.GetTravelRecordDetailAsync(role, month, Core.TravelRecord.TravelRecordAwardType.Primogems);
                t = await _hoyolabService.GetTravelRecordDetailAsync(role, month, Core.TravelRecord.TravelRecordAwardType.Mora);
                InfoBarHelper.Success($"{role.Nickname}的旅行札记保存成功");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Get travel record summary.");
                InfoBarHelper.Error(ex);
            }
        }


        [ICommand(AllowConcurrentExecutions = false)]
        private async Task GetSpiralAbyssInfoSummary()
        {
            try
            {
                InfoBarHelper.Information("请稍等", 2000);
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


        [ICommand]
        private void ChangeAppBackgroundWallpaper()
        {
            WeakReferenceMessenger.Default.Send(new ChangeBackgroundWallpaperMessage());
        }



        [ICommand(AllowConcurrentExecutions = false)]
        private async Task SaveBackgroundWallpaperAsync()
        {
            var wallpaper = WeakReferenceMessenger.Default.Send<RequestMessage<WallpaperInfo?>>()?.Response;
            if (string.IsNullOrWhiteSpace(wallpaper?.Url))
            {
                return;
            }
            try
            {
                var storageFile = await ImageCache.Instance.GetFileFromCacheAsync(new Uri(wallpaper.Url));
                var sourcePath = storageFile?.Path;
                if (string.IsNullOrWhiteSpace(sourcePath))
                {
                    InfoBarHelper.Warning("无法下载或缓存失效");
                    return;
                }
                if (!File.Exists(sourcePath))
                {
                    InfoBarHelper.Warning("找不到文件");
                    return;
                }
                var destFolder = Path.Combine(XunkongEnvironment.UserDataPath, "Wallpaper");
                var fileName = wallpaper.FileName ?? Path.GetFileName(wallpaper.Url);
                var destPath = Path.Combine(destFolder, fileName);
                Directory.CreateDirectory(destFolder);
                File.Copy(sourcePath, destPath, true);
                var button = new Button
                {
                    Content = "打开文件",
                    HorizontalAlignment = Microsoft.UI.Xaml.HorizontalAlignment.Right,
                };
                button.Click += (_, _) => Process.Start(new ProcessStartInfo
                {
                    FileName = destPath,
                    UseShellExecute = true,
                });
                var infobar = new InfoBar
                {
                    Severity = InfoBarSeverity.Success,
                    Title = "已保存",
                    Message = fileName,
                    ActionButton = button,
                    IsOpen = true,
                };
                InfoBarHelper.Show(infobar, 3000);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Save background wallpaper.");
                InfoBarHelper.Error(ex);
            }
        }


    }
}
