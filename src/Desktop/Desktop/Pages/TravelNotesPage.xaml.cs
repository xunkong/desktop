using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Input;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace Xunkong.Desktop.Pages
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class TravelNotesPage : Page
    {

        private TravelNotesViewModel vm => (DataContext as TravelNotesViewModel)!;

        private readonly IDbContextFactory<XunkongDbContext> _ctxFactory;

        public TravelNotesPage()
        {
            this.InitializeComponent();
            DataContext = ActivatorUtilities.GetServiceOrCreateInstance<TravelNotesViewModel>(App.Current.Services);
            _ctxFactory = App.Current.Services.GetService<IDbContextFactory<XunkongDbContext>>()!;
            Loaded += async (_, _) => await vm.InitializeDataAsync();
        }

        private void SfCartesianChart_PointerWheelChanged(object sender, PointerRoutedEventArgs e)
        {
            e.Handled = true;
        }

    }


}
