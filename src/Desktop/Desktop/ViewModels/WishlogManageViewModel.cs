using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Text;
using Microsoft.UI.Xaml.Media.Animation;
using System.Threading.Tasks;
using Vanara.PInvoke;
using Xunkong.Core;
using Xunkong.Core.Wish;
using Xunkong.Core.XunkongApi;
using Xunkong.Desktop.Pages;
using static Vanara.PInvoke.Kernel32.REASON_CONTEXT;

namespace Xunkong.Desktop.ViewModels
{
    internal partial class WishlogManageViewModel : ObservableObject
    {


        private readonly ILogger<WishlogManageViewModel> _logger;

        private readonly WishlogService _wishlogService;

        private readonly XunkongApiService _xunkongApiService;

        private readonly BackupService _backupService;

        private const int delay = 5000;



        public WishlogManageViewModel(ILogger<WishlogManageViewModel> logger, WishlogService wishlogService, XunkongApiService xunkongApiService, BackupService backupService)
        {
            _logger = logger;
            _wishlogService = wishlogService;
            _xunkongApiService = xunkongApiService;
            _backupService = backupService;
        }



        private ObservableCollection<WishlogPanelModel> _WishlogPanelList = new();
        public ObservableCollection<WishlogPanelModel> WishlogPanelList
        {
            get => _WishlogPanelList;
            set => SetProperty(ref _WishlogPanelList, value);
        }


        private bool _IsPageLoadingActive;
        public bool IsPageLoadingActive
        {
            get => _IsPageLoadingActive;
            set => SetProperty(ref _IsPageLoadingActive, value);
        }




        public async Task InitializeDataAsync()
        {
            await RefreshWishlogPanelAsync();
        }



        [ICommand(AllowConcurrentExecutions = false)]
        private async Task RefreshWishlogPanelAsync()
        {
            try
            {
                IsPageLoadingActive = true;
                WishlogPanelList.Clear();
                await Task.Delay(100);
                var uids = await _wishlogService.GetAllUidsAsync();
                if (!uids.Any())
                {
                    InfoBarHelper.Warning("没有任何祈愿数据", 3000);
                    return;
                }
                foreach (var uid in uids)
                {
                    WishlogPanelList.Add(await _wishlogService.GetWishlogPanelModelAsync(uid));
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Refresh wishlog panel.");
                InfoBarHelper.Error(ex);
            }
            finally
            {
                IsPageLoadingActive = false;
            }
        }


        [ICommand(AllowConcurrentExecutions = false)]
        private async Task GetMetadataAsync()
        {
            try
            {
                IsPageLoadingActive = true;
                await _xunkongApiService.GetCharacterInfosFromServerAsync();
                await _xunkongApiService.GetWeaponInfosFromServerAsync();
                await _xunkongApiService.GetWishEventInfosFromServerAsync();
                await _xunkongApiService.GetI18nModelsFromServerAsync();
                InfoBarHelper.Success("完成");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Get metadata.");
                InfoBarHelper.Error(ex);
            }
            finally
            {
                IsPageLoadingActive = false;
            }
        }




        private async Task RefreshWishlogAsync(WishlogPanelModel model, bool getAll)
        {
            if (model.IsRunning)
            {
                return;
            }
            var randomId = Random.Shared.Next();
            try
            {
                model.IsRunning = true;
                model.RandomId = randomId;
                model.RunningStep = "验证祈愿记录网址的有效性";
                EventHandler<(WishType WishType, int Page)> handler = (s, e) => model.RunningStep = $"正在获取 {e.WishType.ToDescriptionOrString()} 第 {e.Page} 页";
                if (await _wishlogService.CheckWishlogUrlTimeoutAsync(model.Uid))
                {
                    var addCount = await _wishlogService.GetWishlogByUidAsync(model.Uid, handler, getAll);
                    model.RunningStep = $"新增 {addCount} 条祈愿记录";
                    model.LastUpdateTime = DateTimeOffset.Now;
                    var newStats = await _wishlogService.GetWishlogPanelModelAsync(model.Uid);
                    model.NickName = newStats.NickName;
                    model.WishlogCount = newStats.WishlogCount;
                    model.WishTypeStats = newStats.WishTypeStats;
                }
                else
                {
                    model.RunningStep = "网址已过期，正在查找新的网址";
                    var isSea = model.Uid.ToString().FirstOrDefault() > '5';
                    var url = await _wishlogService.FindWishlogUrlFromLogFileAsync(isSea);
                    var uid = await _wishlogService.GetUidByWishlogUrl(url);
                    if (uid == model.Uid)
                    {
                        var addCount = await _wishlogService.GetWishlogByUidAsync(model.Uid, handler, getAll);
                        model.RunningStep = $"新增 {addCount} 条祈愿记录";
                    }
                    else
                    {
                        model.IsRunning = false;
                        model.RunningStep = "";
                        var newModel = WishlogPanelList.FirstOrDefault(x => x.Uid == uid);
                        if (newModel is null)
                        {
                            newModel = new WishlogPanelModel { Uid = uid, LastUpdateTime = DateTimeOffset.Now };
                            WishlogPanelList.Add(newModel);
                            await Task.Delay(100);
                        }
                        await RefreshWishlogAsync(newModel, getAll);
                    }
                }
            }
            catch (Exception ex)
            {
                model.RunningStep = "";
                _logger.LogError(ex, "Refresh wishlog from mihoyo server.");
                InfoBarHelper.Error(ex);
            }
            finally
            {
                model.IsRunning = false;
                await Task.Delay(delay);
                if (!model.IsRunning && model.RandomId == randomId)
                {
                    model.RunningStep = "";
                }
            }
        }



        [ICommand(AllowConcurrentExecutions = false)]
        private async Task GetWishlogFromLogFileAsync(bool isSea)
        {
            try
            {
                IsPageLoadingActive = true;
                var url = await _wishlogService.FindWishlogUrlFromLogFileAsync(isSea);
                var uid = await _wishlogService.GetUidByWishlogUrl(url);
                var model = WishlogPanelList.FirstOrDefault(x => x.Uid == uid);
                if (model == null)
                {
                    model = new WishlogPanelModel { Uid = uid, LastUpdateTime = DateTimeOffset.Now };
                    WishlogPanelList.Add(model);
                    await Task.Delay(100);
                }
                IsPageLoadingActive = false;
                await RefreshWishlogAsync(model, false);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Get wishlog from log file.");
                InfoBarHelper.Error(ex);
            }
            finally
            {
                IsPageLoadingActive = false;
            }
        }



        [ICommand(AllowConcurrentExecutions = false)]
        private async Task GetWishlogFromUrlAsync(string url)
        {
            if (string.IsNullOrWhiteSpace(url))
            {
                return;
            }
            try
            {
                IsPageLoadingActive = true;
                var uid = await _wishlogService.GetUidByWishlogUrl(url);
                var model = WishlogPanelList.FirstOrDefault(x => x.Uid == uid);
                if (model == null)
                {
                    model = new WishlogPanelModel { Uid = uid, LastUpdateTime = DateTimeOffset.Now };
                    WishlogPanelList.Add(model);
                    await Task.Delay(100);
                }
                IsPageLoadingActive = false;
                await RefreshWishlogAsync(model, false);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Get wishlog from url.");
                InfoBarHelper.Error(ex);
            }
            finally
            {
                IsPageLoadingActive = false;
            }
        }




        [ICommand]
        private async Task RefreshWishlogIncrementallyAsync(WishlogPanelModel model)
        {
            await RefreshWishlogAsync(model, false);
        }



        [ICommand]
        private async Task RefreshWishlogCompletelyAsync(WishlogPanelModel model)
        {
            await RefreshWishlogAsync(model, true);
        }



        [ICommand]
        private async Task QueryWishlogInCloudAsync(WishlogPanelModel model)
        {
            if (model.IsRunning)
            {
                return;
            }
            var randomId = Random.Shared.Next();
            try
            {
                model.IsRunning = true;
                model.RandomId = randomId;
                Action<string> handler = str => model.RunningStep = str;
                await _xunkongApiService.GetWishlogBackupLastItemAsync(model.Uid, handler);
            }
            catch (Exception ex)
            {
                model.RunningStep = "";
                _logger.LogError(ex, "Query wishlog in cloud.");
                InfoBarHelper.Error(ex);
            }
            finally
            {
                model.IsRunning = false;
                await Task.Delay(delay);
                if (!model.IsRunning && model.RandomId == randomId)
                {
                    model.RunningStep = "";
                }
            }
        }



        private async Task UploadWishlogToCloudAsync(WishlogPanelModel model, bool putAll)
        {
            if (model.IsRunning)
            {
                return;
            }
            var randomId = Random.Shared.Next();
            try
            {
                model.IsRunning = true;
                model.RandomId = randomId;
                Action<string> handler = str => model.RunningStep = str;
                await _xunkongApiService.PutWishlogListAsync(model.Uid, handler, putAll);
            }
            catch (Exception ex)
            {
                model.RunningStep = "";
                _logger.LogError(ex, "Update wishlog to cloud.");
                InfoBarHelper.Error(ex);
            }
            finally
            {
                model.IsRunning = false;
                await Task.Delay(delay);
                if (!model.IsRunning && model.RandomId == randomId)
                {
                    model.RunningStep = "";
                }
            }
        }


        [ICommand]
        private async Task UploadWishlogToCloudIncrementallyAsync(WishlogPanelModel model)
        {
            await UploadWishlogToCloudAsync(model, false);
        }



        [ICommand]
        private async Task UploadWishlogToCloudCompletelyAsync(WishlogPanelModel model)
        {
            await UploadWishlogToCloudAsync(model, true);
        }



        [ICommand]
        private async Task DownloadWishlogFromCloudCompletelyAsync(WishlogPanelModel model)
        {
            if (model.IsRunning)
            {
                return;
            }
            var randomId = Random.Shared.Next();
            try
            {
                model.IsRunning = true;
                model.RandomId = randomId;
                Action<string> handler = str => model.RunningStep = str;
                await _xunkongApiService.GetWishlogBackupListAsync(model.Uid, handler, true);
                var newModel = await _wishlogService.GetWishlogPanelModelAsync(model.Uid);
                model.NickName = newModel.NickName;
                model.WishlogCount = newModel.WishlogCount;
                model.WishTypeStats = newModel.WishTypeStats;
            }
            catch (Exception ex)
            {
                model.RunningStep = "";
                _logger.LogError(ex, "Download wishlog in cloud completely.");
                InfoBarHelper.Error(ex);
            }
            finally
            {
                model.IsRunning = false;
                await Task.Delay(delay);
                if (!model.IsRunning && model.RandomId == randomId)
                {
                    model.RunningStep = "";
                }
            }
        }


        [ICommand]
        private async Task DeleteWishlogInCloudCompletelyAsync(WishlogPanelModel model)
        {
            if (model.IsRunning)
            {
                return;
            }
            var randomId = Random.Shared.Next();
            try
            {
                model.IsRunning = true;
                model.RandomId = randomId;
                _logger.LogInformation($"Start to delete wishlog in cloud with uid {model.Uid}.");
                Action<string> handler = str => model.RunningStep = str;
                var result = await _xunkongApiService.GetWishlogBackupLastItemAsync(model.Uid, handler);
                if (result.CurrentCount == 0)
                {
                    model.RunningStep = "云端没有记录";
                    _logger.LogInformation($"No wishlog in cloud with uid {model.Uid}.");
                    return;
                }
                if (result.CurrentCount > model.WishlogCount)
                {
                    _logger.LogInformation($"Count of wishlog in cloud is more than local with uid {model.Uid}, show warning dialog.");
                    var dialog = new ContentDialog
                    {
                        Title = "警告",
                        Content = $"Uid {model.Uid} 的本地祈愿记录数量少于云端，删除云端记录可能会造成数据丢失，是否继续？",
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
                _logger.LogInformation($"Start to download wishlog in cloud and backup to loacal with uid {model.Uid}.");
                var file = await _xunkongApiService.GetWishlogAndBackupToLoalAsync(model.Uid, handler);
                RoutedEventHandler eventHandler = (_, _) => Process.Start(new ProcessStartInfo { FileName = Path.GetDirectoryName(file), UseShellExecute = true, });
                InfoBarHelper.ShowWithButton(InfoBarSeverity.Success, null, "云端祈愿记录已备份到本地", "打开文件夹", eventHandler, 3000);
                await _xunkongApiService.DeleteWishlogBackupAsync(model.Uid, handler);
            }
            catch (Exception ex)
            {
                model.RunningStep = "";
                _logger.LogError(ex, "Delete wishlog in cloud.");
                InfoBarHelper.Error(ex);
            }
            finally
            {
                model.IsRunning = false;
                await Task.Delay(delay);
                if (!model.IsRunning && model.RandomId == randomId)
                {
                    model.RunningStep = "";
                }
            }
        }



        [ICommand]
        private void NavigateToWishEventStatsPage(WishlogPanelModel model)
        {
            NavigationHelper.NavigateTo(typeof(WishEventStatsPage), model.Uid, new SlideNavigationTransitionInfo { Effect = SlideNavigationTransitionEffect.FromRight });
        }



        public async Task ImportWishlogItemsFromJsonFile(IEnumerable<string> files)
        {
            IsPageLoadingActive = true;
            int count = 0;
            foreach (var file in files)
            {
                _logger.LogInformation($"Imported file path: {file}.");
                try
                {
                    if (Path.GetExtension(file).ToLower() == ".json")
                    {
                        var result = await _wishlogService.ImportFromJsonFile(file);
                        InfoBarHelper.Success(result, 5000);
                        count++;
                        continue;
                    }
                    if (Path.GetExtension(file).ToLower() == ".xlsx")
                    {
                        var result = await _wishlogService.ImportFromExcelFile(file);
                        InfoBarHelper.Success(result, 5000);
                        count++;
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
            if (count > 0)
            {
                await RefreshWishlogPanelAsync();
            }
            IsPageLoadingActive = false;
        }



    }
}
