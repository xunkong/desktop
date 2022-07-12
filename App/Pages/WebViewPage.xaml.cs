using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Navigation;
using Microsoft.Web.WebView2.Core;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace Xunkong.Desktop.Pages;

/// <summary>
/// An empty page that can be used on its own or navigated to within a Frame.
/// </summary>
public sealed partial class WebViewPage : Page
{


    public WebViewPage()
    {
        this.InitializeComponent();
    }



    protected override void OnNavigatedTo(NavigationEventArgs e)
    {
        if (e.Parameter is string url)
        {
            try
            {
                if (Uri.TryCreate(url, UriKind.Absolute, out var uri))
                {
                    _WebView2.Source = uri;
                }
                else
                {
                    _WebView2.Source = new Uri("https://xunkong.cc");
                }
            }
            catch (Exception ex)
            {
                NotificationProvider.Error(ex);
            }
        }
    }



    private void _WebView2_NavigationStarting(WebView2 sender, CoreWebView2NavigationStartingEventArgs args)
    {
        _TextBox_Address.Text = args.Uri.ToString();
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
            if (!(address.StartsWith("https://") || !address.StartsWith("http://")))
            {
                address = $"https://{address}";
            }
            try
            {
                if (Uri.TryCreate(address, UriKind.Absolute, out var uri))
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



}
