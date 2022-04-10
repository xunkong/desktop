using Microsoft.UI.Xaml.Controls;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace Xunkong.Desktop.Pages
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class SpiralAbyssPage : Page
    {

        private SpiralAbyssViewModel vm => (DataContext as SpiralAbyssViewModel)!;



        public SpiralAbyssPage()
        {
            this.InitializeComponent();
            DataContext = ActivatorUtilities.GetServiceOrCreateInstance<SpiralAbyssViewModel>(App.Current.Services);
            Loaded += async (_, _) => await vm.InitializeDataAsync();
        }


        private async void _ListView_LeftPanel_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var selection = _ListView_LeftPanel.SelectedItem as SpiralAbyss_LeftPanel;
            if (selection != null)
            {
                await vm.SelectedAbyssInfoChangedAsync(selection.Id);
            }
        }


    }
}
