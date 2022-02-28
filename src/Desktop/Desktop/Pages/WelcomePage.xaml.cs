using Microsoft.UI.Xaml;
using Microsoft.Web.WebView2.Core;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using Windows.Storage;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace Xunkong.Desktop.Pages
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class WelcomePage : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler? PropertyChanged;
        private void OnPropertyChanged([CallerMemberName] string propertyName = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }



        private readonly ILogger<WelcomePage> _logger;

        private readonly HttpClient _httpClient;



        public WelcomePage()
        {
            this.InitializeComponent();
            _logger = App.Current.Services.GetService<ILogger<WelcomePage>>()!;
            _httpClient = App.Current.Services.GetService<HttpClient>()!;
            Loading += WelcomPage_Loading;
        }





        private string _WebView2State;
        public string WebView2State
        {
            get { return _WebView2State; }
            set
            {
                _WebView2State = value;
                OnPropertyChanged();
            }
        }


        private void WelcomPage_Loading(FrameworkElement sender, object args)
        {
            try
            {
                var version = CoreWebView2Environment.GetAvailableBrowserVersionString();
                WebView2State = $"已安装 ({version})";
            }
            catch
            {
                WebView2State = "未检测到 WebView2 Runtime";
            }
        }




        private string GetXunkongDataPath()
        {
            return XunkongEnvironment.UserDataPath;
        }


        private void _Button_OpenDataFolder_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                Process.Start(new ProcessStartInfo
                {
                    FileName = XunkongEnvironment.UserDataPath,
                    UseShellExecute = true,
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Start process to open userdata folder.");
                InfoBarHelper.Error(ex);
            }
        }


        private void _Button_RefreshStats_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var version = CoreWebView2Environment.GetAvailableBrowserVersionString();
                WebView2State = $"已安装 ({version})";
            }
            catch
            {
                WebView2State = "未检测到 WebView2 Runtime";
            }
        }


        private void _Button_Download_Click(object sender, RoutedEventArgs e)
        {
            const string url = "https://go.microsoft.com/fwlink/p/?LinkId=2124703";
            try
            {
                var startInfo = new ProcessStartInfo
                {
                    UseShellExecute = true,
                    FileName = url,
                };
                Process.Start(startInfo);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Start process to download webview2 runtime.");
                InfoBarHelper.Error(ex);
            }

        }

        private void _Button_NotShowWelcomePage_Click(object sender, RoutedEventArgs e)
        {
            LocalSettingHelper.SaveSetting(SettingKeys.HasShownWelcomePage, true);
            InfoBarHelper.Information("不再显示欢迎界面");
        }

        private async void _Button_InstallFont_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var file = await StorageFile.GetFileFromApplicationUriAsync(new Uri("ms-appx:///Assets/Fonts/Segoe Fluent Icons.ttf"));
                Process.Start(new ProcessStartInfo
                {
                    FileName = file.Path,
                    UseShellExecute = true,
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Install font Segoe Fluent Icons");
                InfoBarHelper.Error(ex);
            }
        }
    }
}
