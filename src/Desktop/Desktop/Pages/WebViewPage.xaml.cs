using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Navigation;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace Xunkong.Desktop.Pages
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class WebViewPage : Page
    {


        public WebViewPage()
        {
            this.InitializeComponent();
            Loaded += WebViewPage_Loaded;
        }


        private async void WebViewPage_Loaded(object sender, RoutedEventArgs e)
        {
            await _WebView2.EnsureCoreWebView2Async();
        }



        protected override async void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);
            if (e.Parameter is string url)
            {
                _WebView2.Source = new Uri(url);
            }
            if (e.Parameter is ValueTuple<int, string> model)
            {
                // html content
                if (model.Item1 == 0)
                {
                    await _WebView2.EnsureCoreWebView2Async();
                    _WebView2.NavigateToString(model.Item2);
                }
            }
        }



    }
}
