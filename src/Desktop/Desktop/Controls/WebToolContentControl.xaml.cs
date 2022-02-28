using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Input;
using Microsoft.Web.WebView2.Core;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace Xunkong.Desktop.Controls
{

    public sealed partial class WebToolContentControl : UserControl
    {

        private readonly WebToolItem _webToolItem;

        internal WebToolItem WebToolItem => _webToolItem;

        internal WebToolContentControl(WebToolItem item)
        {
            this.InitializeComponent();
            Loaded += WebToolContentControl_LoadedAsync;
            _webToolItem = item;
            _WebView2.Source = new Uri(item.Url);
            _WebView2.NavigationStarting += _WebView2_NavigationStarting;
        }

        private void _WebView2_NavigationStarting(WebView2 sender, CoreWebView2NavigationStartingEventArgs args)
        {
            _TextBox_Address.Text = args.Uri.ToString();
        }

        private async void WebToolContentControl_LoadedAsync(object sender, RoutedEventArgs e)
        {
            await _WebView2.EnsureCoreWebView2Async();
            _WebView2.CoreWebView2.NavigationCompleted += CoreWebView2_NavigationCompleted;
        }


        private async void CoreWebView2_NavigationCompleted(CoreWebView2 sender, CoreWebView2NavigationCompletedEventArgs args)
        {
            if (args.IsSuccess && !string.IsNullOrWhiteSpace(_webToolItem?.JavaScript))
            {
                await _WebView2.CoreWebView2.ExecuteScriptAsync(_webToolItem.JavaScript);
            }
        }


        [ICommand]
        private void GoBack()
        {
            if (_WebView2.CanGoBack)
            {
                _WebView2.GoBack();
            }
        }


        [ICommand]
        private void GoForward()
        {
            if (_WebView2.CanGoForward)
            {
                _WebView2.GoForward();
            }
        }


        [ICommand]
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
                if (Uri.TryCreate(address, UriKind.RelativeOrAbsolute, out var uri))
                {
                    try
                    {
                        if (uri.Scheme == Uri.UriSchemeHttps)
                        {
                            _WebView2.Source = uri;
                        }
                    }
                    catch (Exception ex)
                    {
                        InfoBarHelper.Error(ex);
                    }
                }
            }
        }
    }
}
