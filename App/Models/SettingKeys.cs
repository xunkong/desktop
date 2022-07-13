namespace Xunkong.Desktop.Models;

internal static class SettingKeys
{
    /// <summary>
    /// 最大化窗口
    /// </summary>
    public const string IsMainWindowMaximum = nameof(IsMainWindowMaximum);

    /// <summary>
    /// 窗口位置
    /// </summary>
    public const string MainWindowRect = nameof(MainWindowRect);

    /// <summary>
    /// 最后选中的米游社uid
    /// </summary>
    public const string LastSelectUserInfoUid = nameof(LastSelectUserInfoUid);

    /// <summary>
    /// 最后选中的原神uid
    /// </summary>
    public const string LastSelectGameRoleUid = nameof(LastSelectGameRoleUid);

    /// <summary>
    /// 祈愿记录页面最后选中的uid
    /// </summary>
    public const string LastSelectedUidInWishlogSummaryPage = nameof(LastSelectedUidInWishlogSummaryPage);

    /// <summary>
    /// 上个版本
    /// </summary>
    [Obsolete("", true)]
    public const string LastVersion = nameof(LastVersion);

    /// <summary>
    /// 应用主题
    /// </summary>
    public const string ApplicationTheme = nameof(ApplicationTheme);

    /// <summary>
    /// 导航栏是否关闭
    /// </summary>
    public const string NavigationViewPaneClose = nameof(NavigationViewPaneClose);

    /// <summary>
    /// 主页壁纸
    /// </summary>
    public const string EnableHomePageWallpaper = nameof(EnableHomePageWallpaper);

    /// <summary>
    /// 显示过欢迎页面
    /// </summary>
    public const string HasShownWelcomePage = nameof(HasShownWelcomePage);

    /// <summary>
    /// 启用实时便笺通知
    /// </summary>
    [Obsolete("", true)]
    public const string EnableDailyNoteNotification = nameof(EnableDailyNoteNotification);

    /// <summary>
    /// 禁用后台任务日志
    /// </summary>
    [Obsolete("", true)]
    public const string DisableBackgroundTaskOutputLog = nameof(DisableBackgroundTaskOutputLog);

    /// <summary>
    /// 实时便笺通知，数值阈值
    /// </summary>
    [Obsolete("", true)]
    public const string DailyNoteNotification_ResinThreshold = nameof(DailyNoteNotification_ResinThreshold);

    /// <summary>
    /// 实时便笺通知，洞天宝钱阈值
    /// </summary>
    [Obsolete("", true)]
    public const string DailyNoteNotification_HomeCoinThreshold = nameof(DailyNoteNotification_HomeCoinThreshold);

    /// <summary>
    /// 签到成功通知
    /// </summary>
    [Obsolete("", true)]
    public const string DailyCheckInSuccessNotification = nameof(DailyCheckInSuccessNotification);

    /// <summary>
    /// 签到失败通知
    /// </summary>
    [Obsolete("", true)]
    public const string DailyCheckInErrorNotification = nameof(DailyCheckInErrorNotification);

    /// <summary>
    /// 游戏exe文件路径
    /// </summary>
    public const string GameExePath = nameof(GameExePath);

    /// <summary>
    /// 目标fps
    /// </summary>
    public const string TargetFPS = nameof(TargetFPS);

    /// <summary>
    /// 无边框窗口开始游戏
    /// </summary>
    public const string IsPopupWindow = nameof(IsPopupWindow);

    /// <summary>
    /// 游戏截图文件夹路径
    /// </summary>
    public const string ScreenFolderPath = nameof(ScreenFolderPath);

    /// <summary>
    /// 启动应用时自动签到
    /// </summary>
    public const string SignInAllAccountsWhenStartUpApplication = nameof(SignInAllAccountsWhenStartUpApplication);

    /// <summary>
    /// 主页图片刷新时间
    /// </summary>
    public const string WallpaperRefreshTime = nameof(WallpaperRefreshTime);

    /// <summary>
    /// 祈愿记录备份声明
    /// </summary>
    public const string WishlogBackupAgreement = nameof(WishlogBackupAgreement);

}
