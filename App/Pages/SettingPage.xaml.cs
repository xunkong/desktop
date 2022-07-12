using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Input;
using Xunkong.Desktop.Controls;
using Xunkong.Desktop.ViewModels;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace Xunkong.Desktop.Pages;

/// <summary>
/// An empty page that can be used on its own or navigated to within a Frame.
/// </summary>
public sealed partial class SettingPage : Page
{

    private SettingViewModel vm => (DataContext as SettingViewModel)!;


    public SettingPage()
    {
        this.InitializeComponent();
        DataContext = ServiceProvider.GetService<SettingViewModel>();
    }


    private void _Expander_WebTool_Expanding(Expander sender, ExpanderExpandingEventArgs args)
    {
        vm.InitializeWebToolItems();
    }


    private void _Expander_WebTool_Collapsed(Expander sender, ExpanderCollapsedEventArgs args)
    {
        vm.SelectedWebToolItem = null;
    }


    private void _Image_RecommendWallpaper_Tapped(object sender, TappedRoutedEventArgs e)
    {
        MainWindowHelper.OpenFullScreen(new ImageViewer { Source = "ms-appx:///Assets/Images/96839227_p0.webp" });
    }


}
