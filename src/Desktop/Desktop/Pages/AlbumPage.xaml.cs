using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Controls.Primitives;
using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Navigation;
using Windows.ApplicationModel.DataTransfer;
using Windows.Storage.Streams;
using Windows.Storage;
using Microsoft.UI.Xaml.Media.Animation;
using Windows.Foundation.Metadata;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace Xunkong.Desktop.Pages
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class AlbumPage : Page
    {


        private AlbumViewModel vm => (DataContext as AlbumViewModel)!;

        private FileInfo? _selectedItem;


        public AlbumPage()
        {
            this.InitializeComponent();
            DataContext = App.Current.Services.GetService<AlbumViewModel>();
            Loaded += async (_, _) => await vm.InitializeDataAsync();
        }



        private async void _Button_CopyImage_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button button)
            {
                if (button.DataContext is FileInfo fileInfo)
                {
                    try
                    {
                        var path = fileInfo.FullName;
                        var data = new DataPackage();
                        data.RequestedOperation = DataPackageOperation.Copy;
                        var file = await StorageFile.GetFileFromPathAsync(path);
                        var reference = RandomAccessStreamReference.CreateFromFile(file);
                        data.SetBitmap(reference);
                        Clipboard.SetContent(data);
                        button.Content = new SymbolIcon(Symbol.Accept);
                        await Task.Delay(3000);
                        button.Content = new SymbolIcon(Symbol.Copy);
                    }
                    catch (Exception ex)
                    {
                        InfoBarHelper.Error(ex);
                    }

                }
            }
        }



        private void _GridView_Images_ItemClick(object sender, ItemClickEventArgs e)
        {
            ConnectedAnimation animation = null;

            // Get the collection item corresponding to the clicked item.
            if (_GridView_Images.ContainerFromItem(e.ClickedItem) is GridViewItem container)
            {
                // Stash the clicked item for use later. We'll need it when we connect back from the detailpage.
                _selectedItem = container.Content as FileInfo;
                _FlipView_DetailImage.SelectedItem = _selectedItem;

                // Prepare the connected animation.
                // Notice that the stored item is passed in, as well as the name of the connected element. 
                // The animation will actually start on the Detailed info page.
                animation = _GridView_Images.PrepareConnectedAnimation("openImageAnimation", _selectedItem, "_Image_Thumb");
                animation.Configuration = new DirectConnectedAnimationConfiguration();
            }

            _Grid_DetailImage.Visibility = Visibility.Visible;

            animation?.TryStart(_FlipView_DetailImage);
        }



        private async void _Button_CloseDetailImage_Click(object sender, RoutedEventArgs e)
        {
            ConnectedAnimation animation = ConnectedAnimationService.GetForCurrentView().PrepareToAnimate("closeImageAnimation", _FlipView_DetailImage);

            _selectedItem = _FlipView_DetailImage.SelectedItem as FileInfo;
            // Collapse the smoke when the animation completes.
            animation.Completed += (_, _) => _Grid_DetailImage.Visibility = Visibility.Collapsed;
            // If the connected item appears outside the viewport, scroll it into view.
            _GridView_Images.ScrollIntoView(_selectedItem, ScrollIntoViewAlignment.Default);
            _GridView_Images.UpdateLayout();

            // Use the Direct configuration to go back (if the API is available). 
            if (ApiInformation.IsApiContractPresent("Windows.Foundation.UniversalApiContract", 7))
            {
                animation.Configuration = new DirectConnectedAnimationConfiguration();
            }

            // Play the second connected animation. 
            await _GridView_Images.TryStartConnectedAnimationAsync(animation, _selectedItem, "_Image_Thumb");
        }

     
    }
}
