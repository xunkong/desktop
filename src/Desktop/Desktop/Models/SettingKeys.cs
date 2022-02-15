namespace Xunkong.Desktop.Models
{
    public class SettingKeys
    {

        public const string IsWindowMax = nameof(IsWindowMax);

        public const string WindowLeft = nameof(WindowLeft);

        public const string WindowTop = nameof(WindowTop);

        public const string WindowRight = nameof(WindowRight);

        public const string WindowBottom = nameof(WindowBottom);

        public const string LastSelectUserInfoUid = nameof(LastSelectUserInfoUid);

        public const string LastSelectGameRoleUid = nameof(LastSelectGameRoleUid);

        [Obsolete("暂时不提供更改个人数据文件夹的功能")]
        public const string UserDataPath = nameof(UserDataPath);

        public const string LastVersion = nameof(LastVersion);

        public const string ApplicationTheme = nameof(ApplicationTheme);

        public const string NavigationViewPaneClose = nameof(NavigationViewPaneClose);

        public const string DisableBackgroundWallpaper = nameof(DisableBackgroundWallpaper);

        public const string HasShownWelcomePage = nameof(HasShownWelcomePage);

        public const string EnableDailyNoteNotification = nameof(EnableDailyNoteNotification);

        public const string DisableBackgroundTaskOutputLog = nameof(DisableBackgroundTaskOutputLog);

        public const string DailyNoteNotification_ResinThreshold = nameof(DailyNoteNotification_ResinThreshold);

        public const string DailyNoteNotification_HomeCoinThreshold = nameof(DailyNoteNotification_HomeCoinThreshold);

    }
}
