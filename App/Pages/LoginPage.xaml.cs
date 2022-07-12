using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Navigation;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace Xunkong.Desktop.Pages;

/// <summary>
/// An empty page that can be used on its own or navigated to within a Frame.
/// </summary>
public sealed partial class LoginPage : Page
{


    private Action<string> addCookie;



    public LoginPage()
    {
        this.InitializeComponent();
        Loaded += LoginPage_Loaded;
    }



    protected override void OnNavigatedTo(NavigationEventArgs e)
    {
        if (e.Parameter is Action<string> action)
        {
            addCookie = action;
        }
    }



    private async void LoginPage_Loaded(object sender, RoutedEventArgs e)
    {
        try
        {
            await _WebView2.EnsureCoreWebView2Async();
            var manager = _WebView2.CoreWebView2.CookieManager;
            var cookies = await manager.GetCookiesAsync("https://bbs.mihoyo.com/ys/");
            foreach (var item in cookies)
            {
                manager.DeleteCookie(item);
            }
            _WebView2.CoreWebView2.Navigate("https://bbs.mihoyo.com/ys/");
            _TeachTip_Ok.IsOpen = true;
        }
        catch (Exception ex)
        {
            NotificationProvider.Error(ex);
        }
    }



    [RelayCommand]
    private void GoBack()
    {
        if (_WebView2.CanGoBack)
        {
            _WebView2.GoBack();
        }
    }


    [RelayCommand]
    private void GoForward()
    {
        if (_WebView2.CanGoForward)
        {
            _WebView2.GoForward();
        }
    }


    [RelayCommand]
    private void Refresh()
    {
        _WebView2.Reload();
    }


    private void _TextBox_Address_KeyDown(object sender, KeyRoutedEventArgs e)
    {
        if (e.Key == Windows.System.VirtualKey.Enter)
        {
            var address = _TextBox_Address.Text;
            if (!(address.StartsWith("https://") || address.StartsWith("http://")))
            {
                address = $"https://{address}";
            }
            try
            {
                if (Uri.TryCreate(address, UriKind.RelativeOrAbsolute, out var uri))
                {
                    if (uri.Scheme == Uri.UriSchemeHttps)
                    {
                        _WebView2.Source = uri;
                    }
                }
            }
            catch (Exception ex)
            {
                NotificationProvider.Error(ex);
            }
        }
    }



    private void _WebView2_NavigationStarting(WebView2 sender, Microsoft.Web.WebView2.Core.CoreWebView2NavigationStartingEventArgs args)
    {
        _TextBox_Address.Text = args.Uri.ToString();
    }


    [RelayCommand]
    private async Task FinishAndAddCookie()
    {
        try
        {
            var manager = _WebView2.CoreWebView2.CookieManager;
            var cookies = await manager.GetCookiesAsync("https://bbs.mihoyo.com/ys/");
            var str = string.Join(";", cookies.Select(x => $"{x.Name}={x.Value}"));
            addCookie(str);
        }
        catch (Exception ex)
        {
            NotificationProvider.Error(ex);
        }
    }




}
