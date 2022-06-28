using Microsoft.UI.Xaml.Controls;
using System.Collections.Concurrent;
using Xunkong.Core.Hoyolab;

namespace Xunkong.Desktop.ViewModels
{

    internal partial class UserPanelViewModel : ObservableObject
    {


        const string LastSelectUserInfoUid = "LastSelectUserInfoUid";
        const string LastSelectGameRoleUid = "LastSelectGameRoleUid";


        private readonly UserSettingService _userSettingService;

        private readonly HoyolabService _hoyolabService;

        private readonly ILogger<UserPanelViewModel> _logger;


        public Action HideUserPanelSelectorFlyout { get; set; }



        public UserPanelViewModel(UserSettingService userSettingService, HoyolabService hoyolabService, ILogger<UserPanelViewModel> logger)
        {
            _userSettingService = userSettingService;
            _hoyolabService = hoyolabService;
            _logger = logger;
        }



        private UserPanelModel? _SelectedUserPanelModel;
        public UserPanelModel? SelectedUserPanelModel
        {
            get => _SelectedUserPanelModel;
            set
            {
                SetProperty(ref _SelectedUserPanelModel, value);
                SaveLastSelectSetting(value);
            }
        }


        private async void SaveLastSelectSetting(UserPanelModel? model)
        {
            try
            {
                await _userSettingService.SaveSettingAsync(LastSelectUserInfoUid, model?.UserInfo?.Uid ?? 0);
                await _userSettingService.SaveSettingAsync(LastSelectGameRoleUid, model?.GameRoleInfo?.Uid ?? 0);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Exception in method {nameof(SaveLastSelectSetting)}.");
            }
        }



        private ObservableCollection<UserPanelModel> _UserPanelModelList;
        public ObservableCollection<UserPanelModel> UserPanelModelList
        {
            get => _UserPanelModelList;
            set => SetProperty(ref _UserPanelModelList, value);
        }



        private bool _IsLoadingActived;
        public bool IsLoadingActived
        {
            get => _IsLoadingActived;
            set => SetProperty(ref _IsLoadingActived, value);
        }




        private void ShowGenshinElementLoading()
        {
            IsLoadingActived = true;
            if (SelectedUserPanelModel is not null)
            {
                SelectedUserPanelModel.DailyNoteInfo = null;
            }
        }



        public async Task InitializeDataAsync()
        {
            ShowGenshinElementLoading();
            _logger.LogDebug("Initlize User Panel.");
            try
            {
                var userInfo = await _hoyolabService.GetLastSelectedOrFirstUserInfoAsync();
                if (userInfo == null)
                {
                    _logger.LogDebug("No saved hoyolab userinfo.");
                    return;
                }
                var gameRole = await _hoyolabService.GetLastSelectedOrFirstUserGameRoleInfoAsync();
                var model = new UserPanelModel { UserInfo = userInfo, GameRoleInfo = gameRole };
                SelectedUserPanelModel = model;
                await RefreshAllUserPanelModelAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Exception in method {nameof(InitializeDataAsync)}.");
                InfoBarHelper.Error(ex);
            }
            finally
            {
                await Task.Delay(100);
                IsLoadingActived = false;
            }
        }


        [ICommand(AllowConcurrentExecutions = false)]
        public async Task RefreshAllUserPanelModelAsync()
        {
            ShowGenshinElementLoading();
            _logger.LogDebug("Start refresh all hoyolab and genshin user info.");
            HideUserPanelSelectorFlyout?.Invoke();
            try
            {
                var oldUsers = await _hoyolabService.GetUserInfoListAsync();
                // 更新米游社账号数据
                var tmpUsers = new BlockingCollection<UserInfo>();
                await Parallel.ForEachAsync(oldUsers, async (x, _) =>
                {
                    try
                    {
                        // 同时会更新至数据库
                        var user = await _hoyolabService.GetUserInfoAsync(x);
                        if (user is not null)
                        {
                            // cookie 有效的账号才进行下一步
                            tmpUsers.Add(user);
                        }
                    }
                    catch (HoyolabException ex)
                    {
                        _logger.LogError(ex, $"Catch HoyolabException when get hoyolab user info (Nickname {x.Nickname}, Uid {x.Uid}).");
                        InfoBarHelper.Error(ex, $"获取米游社账号信息 (Nickname {x.Nickname}, Uid {x.Uid})");
                    }
                });
                // 更新原神账号数据
                var tmpRoles = new BlockingCollection<UserGameRoleInfo>();
                await Parallel.ForEachAsync(tmpUsers, async (x, _) =>
                {
                    try
                    {
                        // 同时会更新至数据库
                        var roles = await _hoyolabService.GetUserGameRoleInfoListAsync(x.Cookie!);
                        if (roles.Any())
                        {
                            // cookie 有效的账号才进行下一步
                            foreach (var item in roles)
                            {
                                tmpRoles.Add(item);
                            }
                        }
                    }
                    catch (HoyolabException ex)
                    {
                        _logger.LogError(ex, $"Catch HoyolabException when get genshin user info (Nickname {x.Nickname}, Uid {x.Uid}).");
                        InfoBarHelper.Error(ex, $"获取原神账号信息 (Nickname {x.Nickname}, Uid {x.Uid})");
                    }
                });
                // 更新实时便笺数据
                var dailyNotes = new BlockingCollection<DailyNoteInfo>();
                await Parallel.ForEachAsync(tmpRoles, async (x, _) =>
                {
                    try
                    {
                        // 同时会更新至数据库
                        var note = await _hoyolabService.GetDailyNoteInfoAsync(x);
                        if (note is not null)
                        {
                            dailyNotes.Add(note);
                        }
                    }
                    catch (HoyolabException ex)
                    {
                        _logger.LogError(ex, $"Catch HoyolabException when get daily note info (Nickname {x.Nickname}, Uid {x.Uid}).");
                        InfoBarHelper.Error(ex, $"获取实时便笺 (Nickname {x.Nickname}, Uid {x.Uid})");
                    }
                });
                // 重新从数据库获取所有数据，包含cookie过期的账号
                var userInfos = await _hoyolabService.GetUserInfoListAsync();
                var gameRoles = await _hoyolabService.GetUserGameRoleInfoListAsync();
                // UserInfo UserGameRoleInfo DailyNoteInfo 相对应，应该没问题
                var query = from user in userInfos
                            join role in gameRoles
                            on user.Cookie equals role.Cookie into userGroup
                            from r in userGroup.DefaultIfEmpty()
                            join note in dailyNotes
                            on r.Uid equals note.Uid into roleGroup
                            from n in roleGroup.DefaultIfEmpty()
                            select new UserPanelModel { UserInfo = user, GameRoleInfo = r, DailyNoteInfo = n };
                var list = new ObservableCollection<UserPanelModel>(query);
                var pinnedList = await TileHelper.FindAllAsync();
                // 更新磁贴
                foreach (var item in list)
                {
                    if (pinnedList.Any(x => x == $"DailyNote_{item.GameRoleInfo?.Uid}"))
                    {
                        item.IsPinned = true;
                        if (item.DailyNoteInfo is not null)
                        {
                            TileHelper.UpdatePinnedTile(item.DailyNoteInfo);
                        }
                    }
                }
                UserPanelModelList = list;
                var lastRoleUid = await _userSettingService.GetSettingAsync<int>(LastSelectGameRoleUid);
                var lastModel = list.FirstOrDefault(x => x?.GameRoleInfo?.Uid == lastRoleUid) ?? list.FirstOrDefault();
                SelectedUserPanelModel = lastModel;
                _logger.LogDebug("Finish refresh all hoyolab and genshin user info.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Catch Exception when refresh all hoyolab and genshin user info.");
                InfoBarHelper.Error(ex, $"刷新所有账号信息");
            }
            finally
            {
                IsLoadingActived = false;
            }
        }



        [ICommand]
        private void HoyolabLogin_WebLogin()
        {
            Task.Run(() => InfoBarHelper.Warning("此功能尚未完成（咕咕", 3000));
        }



        [ICommand(AllowConcurrentExecutions = false)]
        private async Task HoyolabLogin_InputCookieAsync()
        {
            _logger.LogDebug("Login Input Cookie button has been pressed.");
            var stackPanel = new StackPanel { Spacing = 12 };
            stackPanel.Children.Add(new TextBlock { Text = "仅支持国服米游社；\n需要包含 cookie_token 值，否则会出现 HoyolabException (-100) 错误。" });
            var textBox = new TextBox();
            stackPanel.Children.Add(textBox);
            var dialog = new ContentDialog
            {
                Title = "输入Cookie",
                Content = stackPanel,
                PrimaryButtonText = "确认",
                SecondaryButtonText = "取消",
                DefaultButton = ContentDialogButton.Primary,
                XamlRoot = MainWindow.XamlRoot,
            };
            var result = await dialog.ShowAsync();
            HideUserPanelSelectorFlyout?.Invoke();
            if (result is ContentDialogResult.Primary)
            {
                var cookie = textBox.Text;
                if (string.IsNullOrWhiteSpace(cookie))
                {
                    _logger.LogDebug("Input box has nothing.");
                    return;
                }
                try
                {
                    ShowGenshinElementLoading();
                    _logger.LogDebug("Start get info by cookie.");
                    var userInfo = await _hoyolabService.GetUserInfoAsync(cookie);
                    var gameRoles = await _hoyolabService.GetUserGameRoleInfoListAsync(cookie);
                    var list = gameRoles.Select(x => new UserPanelModel { UserInfo = userInfo, GameRoleInfo = x }).ToList();
                    var pinnedList = await TileHelper.FindAllAsync();
                    foreach (var item in list)
                    {
                        if (pinnedList.Any(x => x == $"DailyNote_{item.GameRoleInfo?.Uid}"))
                        {
                            item.IsPinned = true;
                        }
                    }
                    if (UserPanelModelList is null)
                    {
                        UserPanelModelList = new ObservableCollection<UserPanelModel>(list);
                    }
                    else
                    {
                        foreach (var item in list)
                        {
                            var match = UserPanelModelList.FirstOrDefault(x => x.GameRoleInfo?.Uid == item.GameRoleInfo?.Uid);
                            if (match is not null)
                            {
                                UserPanelModelList.Remove(match);
                            }
                            UserPanelModelList.Add(item);
                        }
                    }
                    var firstModel = list.FirstOrDefault();
                    if (firstModel is null)
                    {
                        _logger.LogDebug("No relative game roles.");
                        InfoBarHelper.Warning("没有相关联的角色");
                        return;
                    }
                    SelectedUserPanelModel = firstModel;
                    firstModel.DailyNoteInfo = await _hoyolabService.GetDailyNoteInfoAsync(firstModel.GameRoleInfo!);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, $"Error in {nameof(HoyolabLogin_InputCookieAsync)}");
                    InfoBarHelper.Error(ex);
                }
                finally
                {
                    IsLoadingActived = false;
                }
            }
            else
            {
                _logger.LogDebug("Canceled Input Cookie command.");
            }
        }


        [ICommand(AllowConcurrentExecutions = false)]
        public async Task RefreshDailyNoteAsync()
        {
            var model = SelectedUserPanelModel;
            if (model is null || model.GameRoleInfo is null)
            {
                return;
            }
            try
            {
                ShowGenshinElementLoading();
                _logger.LogDebug($"Refresh daily note with genshin nickname {model.GameRoleInfo.Nickname} uid {model.GameRoleInfo.Uid}");
                var info = await _hoyolabService.GetDailyNoteInfoAsync(model.GameRoleInfo);
                model.DailyNoteInfo = info;
                if (info is not null && model.IsPinned)
                {
                    TileHelper.UpdatePinnedTile(info);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error in {nameof(RefreshDailyNoteAsync)}");
                InfoBarHelper.Error(ex);
            }
            finally
            {
                IsLoadingActived = false;
            }
        }



        public object? InitGroupSource()
        {
            if (UserPanelModelList is null)
            {
                return null;
            }
            return from model in UserPanelModelList group model by model.UserInfo;
        }



        public async Task DeleteUserInfoAsync(IEnumerable<UserPanelModel> models)
        {
            HideUserPanelSelectorFlyout?.Invoke();
            if (models.Any())
            {
                var hoyolabUser = models.FirstOrDefault()?.UserInfo;
                _logger.LogDebug($"Delete hoyolab user info with nickname {hoyolabUser?.Nickname} uid {hoyolabUser?.Uid}");
            }
            try
            {
                foreach (var model in models)
                {
                    if (model.UserInfo is not null)
                    {
                        await _hoyolabService.DeleteUserInfoAsync(model.UserInfo.Uid);
                    }
                    if (model.GameRoleInfo is not null)
                    {
                        _logger.LogDebug($"Delete genshin user info with nickname {model.GameRoleInfo.Nickname} uid {model.GameRoleInfo.Uid}");
                        await _hoyolabService.DeleteUserGameRoleInfoAsync(model.GameRoleInfo.Uid);
                        if (model.IsPinned)
                        {
                            await TileHelper.RequestUnpinTileAsync(model.GameRoleInfo.Uid);
                        }
                    }
                    if (UserPanelModelList?.Contains(model) ?? false)
                    {
                        UserPanelModelList.Remove(model);
                    }
                }
                _logger.LogDebug("Finished delete user info.");
                InfoBarHelper.Success("删除成功");
                if (SelectedUserPanelModel is null)
                {
                    SelectedUserPanelModel = UserPanelModelList?.FirstOrDefault();
                }
                else
                {
                    if (UserPanelModelList?.Contains(SelectedUserPanelModel) ?? false)
                    {
                        return;
                    }
                    else
                    {
                        SelectedUserPanelModel = UserPanelModelList?.FirstOrDefault();
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error in {nameof(DeleteUserInfoAsync)}");
                InfoBarHelper.Error(ex);
            }
        }



        public async Task PinOrUnpinTileAsync(UserPanelModel model)
        {
            var note = model?.DailyNoteInfo;
            if (model is null || note is null)
            {
                return;
            }
            try
            {
                if (model.IsPinned)
                {
                    model.IsPinned = !await TileHelper.RequestUnpinTileAsync(note);
                }
                else
                {
                    model.IsPinned = await TileHelper.RequestPinTileAsync(note);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Pin or unpin tile.");
                InfoBarHelper.Error(ex);
            }
        }



    }
}
