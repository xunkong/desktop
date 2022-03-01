using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media.Animation;
using Windows.ApplicationModel.DataTransfer;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace Xunkong.Desktop.Pages
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class WishlogSummaryPage : Page
    {

        private const string HasShownUpdateMetadataTeachingTip = "HasShownUpdateMetadataTeachingTip";

        private WishlogSummaryViewModel vm => (DataContext as WishlogSummaryViewModel)!;


        public WishlogSummaryPage()
        {
            this.InitializeComponent();
            DataContext = ActivatorUtilities.CreateInstance<WishlogSummaryViewModel>(App.Current.Services);
            Loaded += async (_, _) => await vm.InitializePageDataAsync();
            Loaded += ShowUpdateMetadataTeachingTip;
        }

        private void ShowUpdateMetadataTeachingTip(object sender, RoutedEventArgs e)
        {
            if (!LocalSettingHelper.GetSetting<bool>(HasShownUpdateMetadataTeachingTip))
            {
                _TeachingTip_UpdateMetadata.IsOpen = true;
            }
        }

        private void _TeachingTip_UpdateMetadata_CloseButtonClick(TeachingTip sender, object args)
        {
            LocalSettingHelper.SaveSetting(HasShownUpdateMetadataTeachingTip, true);
        }

        private void _Flyout_InputWishlogUrl_Opened(object sender, object e)
        {
            _TextBox_WishlogUrl.Text = "";
            _TextBox_WishlogUrl.Focus(FocusState.Pointer);
        }

        private async void _ComboBox_Uid_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            await vm.LoadWishlogAsync();
        }

        private void _TextBlock_Rank5Item_PointerEntered(object sender, PointerRoutedEventArgs e)
        {
            if (sender is TextBlock textblock)
            {
                if (textblock.DataContext is WishlogSummary_Rank5Item item && textblock.Tag is WishlogSummary_QueryTypeStats stats)
                {
                    var name = item.Name;
                    stats.Rank5Items.ForEach(x =>
                    {
                        if (x.Name == name)
                        {
                            x.Foreground = x.Color;
                        }
                        else
                        {
                            x.Foreground = "#9AA0A8"; //gray
                        }
                    });
                }
            }
        }


        private void _ScrollViewer_Rank5Item_PointerExited(object sender, PointerRoutedEventArgs e)
        {
            if (sender is ScrollViewer scroll)
            {
                if (scroll.DataContext is WishlogSummary_QueryTypeStats stats)
                {
                    stats.Rank5Items.ForEach(x =>
                    {
                        x.Foreground = x.Color;
                    });
                }
            }
        }


        private void _Button_ExpandScrollViewer_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button button)
            {
                if (button.Tag is ScrollViewer scroll)
                {
                    if (scroll.Tag is Grid grid)
                    {
                        var sb = new Storyboard();
                        var ani = new DoubleAnimation { Duration = new Duration(TimeSpan.FromMilliseconds(167)), EnableDependentAnimation = true };
                        Storyboard.SetTarget(ani, grid);
                        Storyboard.SetTargetProperty(ani, "Height");
                        if (grid.Tag as string == "HasExpand")
                        {
                            ani.From = grid.Height;
                            ani.To = 200;
                            grid.Tag = "";
                            button.Content = "\xE70D"; //ChevronDown
                        }
                        else
                        {
                            ani.From = grid.Height;
                            if (scroll.ExtentHeight <= 152)
                            {
                                ani.To = 200;
                            }
                            else
                            {
                                ani.To = 50 + scroll.ExtentHeight;
                            }
                            grid.Tag = "HasExpand";
                            button.Content = "\xE70E"; //ChevronUp
                        }
                        sb.Children.Add(ani);
                        sb.Begin();
                    }

                }
            }
        }


        private void _AppBarButton_Import_Click(object sender, RoutedEventArgs e)
        {
            _TeachingTip_Import.IsOpen = true;
        }

        private void _AppBarButton_Import_DragOver(object sender, DragEventArgs e)
        {
            e.AcceptedOperation = DataPackageOperation.Copy;
            e.DragUIOverride.IsCaptionVisible = false;
            e.DragUIOverride.IsGlyphVisible = false;
        }

        private async void _AppBarButton_Import_Drop(object sender, DragEventArgs e)
        {
            if (e.DataView.Contains(StandardDataFormats.StorageItems))
            {
                var items = await e.DataView.GetStorageItemsAsync();
                if (items.Count > 0)
                {
                    await vm.ImportWishlogItemsFromExcelOrJsonFile(items.Select(x => x.Path));
                }
            }
        }


    }
}
