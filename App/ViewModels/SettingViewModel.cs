using AngleSharp;
using CommunityToolkit.Mvvm.Messaging;
using CommunityToolkit.WinUI.UI;
using System.Collections.ObjectModel;
using System.Net.Http;
using Windows.ApplicationModel.DataTransfer;
using Windows.Storage;
using Windows.Storage.Pickers;
using Windows.System;
using WinRT.Interop;
using Xunkong.Desktop.Messages;
using Xunkong.GenshinData.Character;
using Xunkong.GenshinData.Weapon;

namespace Xunkong.Desktop.ViewModels;

internal partial class SettingViewModel : ObservableObject
{



    private readonly HttpClient _httpClient;


    private readonly HoyolabService _hoyolabService;


    private readonly BackupService _backupService;



    public string AppName => XunkongEnvironment.IsStoreVersion ? "寻空 商店版" : "寻空 侧载版";

    public string AppVersion => XunkongEnvironment.AppVersion.ToString();



    public SettingViewModel(HttpClient httpClient, HoyolabService hoyolabService, BackupService backupService)
    {
        _httpClient = httpClient;
        _hoyolabService = hoyolabService;
        _backupService = backupService;
    }



    public void InitializeData()
    {
        try
        {
            SignInAllAccountsWhenStartUpApplication = UserSetting.GetValue<bool>(SettingKeys.SignInAllAccountsWhenStartUpApplication);
        }
        catch { }
    }


    [RelayCommand]
    private void CopyDevelopersMail()
    {
        var data = new DataPackage();
        data.RequestedOperation = DataPackageOperation.Copy;
        data.SetText("scighost@outlook.com");
        Clipboard.SetContent(data);
        NotificationProvider.Success("已复制开发者的邮件（如果没收到回复，可能是被识别为垃圾邮件了）", 5000);
    }


    #region Version and Theme



    /// <summary>
    /// 检查更新
    /// </summary>
    /// <returns></returns>
    [RelayCommand]
    private async Task CheckUpdateAsync()
    {
        try
        {
            Uri uri;
            if (XunkongEnvironment.IsStoreVersion)
            {
                uri = new("ms-windows-store://pdp/?productid=9N2SVG0JMT12");
            }
            else
            {
                uri = new("https://go.xunkong.cc/check-update");
            }
            await Launcher.LaunchUriAsync(uri);
        }
        catch (Exception ex)
        {
            NotificationProvider.Error(ex);
        }
    }






    private bool _EnableHomePageWallpaper = AppSetting.GetValue(SettingKeys.EnableHomePageWallpaper, true);
    public bool EnableHomePageWallpaper
    {
        get => _EnableHomePageWallpaper;
        set
        {
            AppSetting.TrySetValue(SettingKeys.EnableHomePageWallpaper, value);
            SetProperty(ref _EnableHomePageWallpaper, value);
        }
    }




    private int _SelectedThemeIndex = AppSetting.GetValue<int>(SettingKeys.ApplicationTheme);
    public int SelectedThemeIndex
    {
        get => _SelectedThemeIndex;
        set
        {
            AppSetting.TrySetValue(SettingKeys.ApplicationTheme, value);
            WeakReferenceMessenger.Default.Send(new ChangeApplicationThemeMessage(value));
            SetProperty(ref _SelectedThemeIndex, value);
        }
    }




    private int _SelectedWallpaperRefreshTimeIndex = AppSetting.GetValue<int>(SettingKeys.WallpaperRefreshTime);
    public int SelectedWallpaperRefreshTimeIndex
    {
        get => _SelectedWallpaperRefreshTimeIndex;
        set
        {
            AppSetting.TrySetValue(SettingKeys.WallpaperRefreshTime, value);
            SetProperty(ref _SelectedWallpaperRefreshTimeIndex, value);
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


    /// <summary>
    /// 初始化网页小工具
    /// </summary>
    public void InitializeWebToolItems()
    {
        try
        {
            using var dapper = DatabaseProvider.CreateConnection();
            var list = dapper.Query<WebToolItem>("SELECT * FROM WebToolItem ORDER BY [Order];");
            _WebToolItemList = new(list);
        }
        catch (Exception ex)
        {
            NotificationProvider.Error(ex, "无法加载网页小工具的数据");
        }
    }


    /// <summary>
    /// 添加新的网页小工具
    /// </summary>
    [RelayCommand]
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


    /// <summary>
    /// 删除选择的网页小工具，但未保存至数据库
    /// </summary>
    [RelayCommand]
    private void DeleteSelectedWebToolItem()
    {
        try
        {
            if (SelectedWebToolItem is not null)
            {
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
            NotificationProvider.Error(ex);
        }

    }


    /// <summary>
    /// 关闭网页小工具编辑栏
    /// </summary>
    [RelayCommand]
    private void CloseEditWebToolGrid()
    {
        SelectedWebToolItem = null;
    }


    /// <summary>
    /// 获取网页的图标
    /// </summary>
    /// <returns></returns>
    [RelayCommand]
    private async Task GetTitleAndIconByUrlAsync()
    {
        if (string.IsNullOrWhiteSpace(SelectedWebToolItem?.Url))
        {
            return;
        }
        var url = SelectedWebToolItem.Url;
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
            NotificationProvider.Error(ex);
        }
    }




    /// <summary>
    /// 保存网页小工具的修改
    /// </summary>
    [RelayCommand]
    private void SaveWebToolItem()
    {
        try
        {
            var list = WebToolItemList.Where(x => !string.IsNullOrWhiteSpace(x.Url)).ToList();
            for (int i = 0; i < list.Count; i++)
            {
                list[i].Order = i;
            }
            using var dapper = DatabaseProvider.CreateConnection();
            using var t = dapper.BeginTransaction();
            dapper.Execute("DELETE FROM WebToolItem WHERE TRUE;", t);
            dapper.Execute("INSERT OR REPLACE INTO WebToolItem (Title, Icon, [Order], Url, JavaScript) VALUES (@Title, @Icon, @Order, @Url, @JavaScript);", list, t);
            t.Commit();
            WebToolItemList = new(list);
            NotificationProvider.Success("保存成功");
            WeakReferenceMessenger.Default.Send<InitializeNavigationWebToolItemMessage>();
        }
        catch (Exception ex)
        {
            NotificationProvider.Error(ex);
        }
    }



    #endregion



    #region Hoyolab Check in Task


    private bool _SignInAllAccountsWhenStartUpApplication;
    public bool SignInAllAccountsWhenStartUpApplication
    {
        get => _SignInAllAccountsWhenStartUpApplication;
        set
        {
            UserSetting.SetValue(SettingKeys.SignInAllAccountsWhenStartUpApplication, value);
            SetProperty(ref _SignInAllAccountsWhenStartUpApplication, value);
        }
    }


    #endregion



    #region Start Game


    private string? _GameExePath = AppSetting.GetValue(SettingKeys.GameExePath, "不指定具体文件则会从注册表查找");
    public string? GameExePath
    {
        get => _GameExePath;
        set => SetProperty(ref _GameExePath, value);
    }


    private int _UnlockedFPS = AppSetting.GetValue(SettingKeys.TargetFPS, 60);
    public int UnlockedFPS
    {
        get => _UnlockedFPS;
        set
        {
            AppSetting.TrySetValue(SettingKeys.TargetFPS, value);
            SetProperty(ref _UnlockedFPS, value);
        }
    }


    private bool _UsePopupWindow = AppSetting.GetValue<bool>(SettingKeys.IsPopupWindow);
    public bool UsePopupWindow
    {
        get => _UsePopupWindow;
        set
        {
            AppSetting.TrySetValue(SettingKeys.IsPopupWindow, value);
            SetProperty(ref _UsePopupWindow, value);
        }
    }


    /// <summary>
    /// 启动游戏
    /// </summary>
    /// <returns></returns>
    [RelayCommand]
    private async Task StartGameAsync()
    {
        try
        {
            await InvokeService.StartGameAsync();
        }
        catch (Exception ex)
        {
            NotificationProvider.Error(ex);
        }
    }


    /// <summary>
    /// 更改游戏位置
    /// </summary>
    /// <returns></returns>
    [RelayCommand]
    private async Task ChangeGameExePathAsync()
    {
        try
        {
            var dialog = new FileOpenPicker();
            dialog.SuggestedStartLocation = PickerLocationId.ComputerFolder;
            dialog.FileTypeFilter.Add(".exe");
            InitializeWithWindow.Initialize(dialog, MainWindowHelper.HWND);
            var file = await dialog.PickSingleFileAsync();
            if (file != null)
            {
                var path = file.Path;
                if (path.EndsWith("YuanShen.exe") || path.EndsWith("GenshinImpact.exe"))
                {
                    AppSetting.TrySetValue(SettingKeys.GameExePath, path);
                    GameExePath = file.Path;
                }
                else
                {
                    NotificationProvider.Warning("文件名不太对", 3000);
                }
            }
        }
        catch (Exception ex)
        {
            NotificationProvider.Error(ex);
        }
    }




    #endregion




    #region Image Cache


    [RelayCommand]
    private void ClearImageCache()
    {
        try
        {
            var tempState = ApplicationData.Current.TemporaryFolder.Path;
            var imageCacheFolder = Path.Combine(tempState, "ImageCache");
            if (Directory.Exists(imageCacheFolder))
            {
                Directory.Delete(imageCacheFolder, true);
            }
            Directory.CreateDirectory(imageCacheFolder);
            NotificationProvider.Success($"完成");
        }
        catch (Exception ex)
        {
            NotificationProvider.Error(ex);
        }
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


    [RelayCommand]
    private async Task PrecacheAllImagesAsync()
    {
        try
        {
            _PrecacheImage_FinishCount = 0;
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
            var images = new List<string>();
            using var liteDb = DatabaseProvider.CreateLiteDB();
            var characters = liteDb.GetCollection<CharacterInfo>().FindAll().ToList();
            var list = characters.Where(x => !string.IsNullOrWhiteSpace(x.FaceIcon)).Select(x => x.FaceIcon).ToList();
            images.AddRange(list!);
            list = characters.Where(x => !string.IsNullOrWhiteSpace(x.SideIcon)).Select(x => x.SideIcon).ToList();
            images.AddRange(list!);
            list = characters.Where(x => !string.IsNullOrWhiteSpace(x.GachaSplash)).Select(x => x.GachaSplash).ToList();
            images.AddRange(list!);
            list = characters.SelectMany(x => x.Talents).Where(x => !string.IsNullOrWhiteSpace(x.Icon)).Select(x => x.Icon).ToList();
            images.AddRange(list!);
            list = characters.SelectMany(x => x.Constellations!).Where(x => !string.IsNullOrWhiteSpace(x.Icon)).Select(x => x.Icon).ToList();
            images.AddRange(list!);
            var weapons = liteDb.GetCollection<WeaponInfo>().FindAll().ToList();
            list = weapons.Where(x => !string.IsNullOrWhiteSpace(x.Icon)).Select(x => x.Icon).ToList();
            images.AddRange(list!);
            list = weapons.Where(x => !string.IsNullOrWhiteSpace(x.AwakenIcon)).Select(x => x.AwakenIcon).ToList();
            images.AddRange(list!);
            PrecacheImage_TotalCount = images.Count;
            var parallelOptions = new ParallelOptions { MaxDegreeOfParallelism = Environment.ProcessorCount * 4 };
            await Parallel.ForEachAsync(images, parallelOptions, async (url, _) =>
            {
                try
                {
                    var uri = new Uri(url!);
                    await ImageCache.Instance.PreCacheAsync(uri);
                }
                finally
                {
                    lock (_precacheImage_lock)
                    {
                        MainWindowHelper.DispatcherQueue.TryEnqueue(() => PrecacheImage_FinishCount++);
                    }
                }
            });
            await Task.Delay(1000);
        }
        catch (Exception ex)
        {
            NotificationProvider.Error(ex);
        }
    }


    #endregion



}
