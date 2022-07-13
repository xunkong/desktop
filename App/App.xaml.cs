using Microsoft.UI.Xaml;
using Xunkong.Desktop.Pages;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace Xunkong.Desktop;

/// <summary>
/// Provides application-specific behavior to supplement the default Application class.
/// </summary>
public partial class App : Application
{
    /// <summary>
    /// Initializes the singleton application object.  This is the first line of authored code
    /// executed, and as such is the logical equivalent of main() or WinMain().
    /// </summary>
    public App()
    {
        this.InitializeComponent();
        InitializeApplicationTheme();
    }


    private void InitializeApplicationTheme()
    {
        if (AppSetting.TryGetValue<int>(SettingKeys.ApplicationTheme, out var themeIndex))
        {
            if (themeIndex == 1)
            {
                RequestedTheme = ApplicationTheme.Light;
            }
            if (themeIndex == 2)
            {
                RequestedTheme = ApplicationTheme.Dark;
            }
        }
    }


    /// <summary>
    /// Invoked when the application is launched normally by the end user.  Other entry points
    /// will be used such as when the application is launched to open a specific file.
    /// </summary>
    /// <param name="args">Details about the launch request and process.</param>
    protected override void OnLaunched(LaunchActivatedEventArgs args)
    {
        m_window = new MainWindow();
        m_window.Activate();
        AppSetting.TryGetValue<bool>(SettingKeys.HasShownWelcomePage, out var shown);
        if (shown)
        {
            MainWindowHelper.Navigate(typeof(MainPage));
        }
        else
        {
            MainWindowHelper.Navigate(typeof(WelcomPage));
        }
    }

    private Window m_window;
}
