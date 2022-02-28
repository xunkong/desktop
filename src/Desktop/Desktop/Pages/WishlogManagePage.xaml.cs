using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Windows.ApplicationModel.DataTransfer;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace Xunkong.Desktop.Pages
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class WishlogManagePage : Page
    {



        private WishlogManageViewModel vm => (DataContext as WishlogManageViewModel)!;




        public WishlogManagePage()
        {
            this.InitializeComponent();
            DataContext = ActivatorUtilities.CreateInstance<WishlogManageViewModel>(App.Current.Services);
            Loaded += async (_, _) => await vm.InitializeDataAsync();
        }



        private void _Flyout_InputWishlogUrl_Opened(object sender, object e)
        {
            _TextBox_WishlogUrl.Text = "";
            _TextBox_WishlogUrl.Focus(FocusState.Pointer);
        }




        private async void _AppBarButton_Import_Click(object sender, RoutedEventArgs e)
        {
            _Grid_ImportTip.Visibility = Visibility.Visible;
            await Task.Delay(2000);
            _Grid_ImportTip.Visibility = Visibility.Collapsed;
        }


        private void _GridView_WishlogPanel_DragOver(object sender, DragEventArgs e)
        {
            _Grid_ImportTip.Visibility = Visibility.Visible;
            e.AcceptedOperation = DataPackageOperation.Copy;
            e.DragUIOverride.IsCaptionVisible = false;
            e.DragUIOverride.IsGlyphVisible = false;
        }


        private void _GridView_WishlogPanel_DragLeave(object sender, DragEventArgs e)
        {
            _Grid_ImportTip.Visibility = Visibility.Collapsed;
        }


        private async void _GridView_WishlogPanel_Drop(object sender, DragEventArgs e)
        {
            _Grid_ImportTip.Visibility = Visibility.Collapsed;
            if (e.DataView.Contains(StandardDataFormats.StorageItems))
            {
                var items = await e.DataView.GetStorageItemsAsync();
                if (items.Count > 0)
                {
                    await vm.ImportWishlogItemsFromJsonFile(items.Select(x => x.Path));
                }
            }
        }



    }
}
