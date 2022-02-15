using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Controls.Primitives;
using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Navigation;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace Xunkong.Desktop.Pages
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class WishEventStatsPage : Page
    {


        private WishEventStatsViewModel vm => (DataContext as WishEventStatsViewModel)!;

        private int _uid;


        public WishEventStatsPage()
        {
            this.InitializeComponent();
            DataContext = App.Current.Services.GetService<WishEventStatsViewModel>();
        }



        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);
            if (e.Parameter is int uid)
            {
                _uid = uid;
            }
            _TextBlock_Uid.Text = $"Uid {_uid}";
        }


        protected override void OnNavigatingFrom(NavigatingCancelEventArgs e)
        {
            base.OnNavigatingFrom(e);
            vm.WishEventStatsList?.Clear();
        }


        private async void Pivot_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (e.AddedItems.FirstOrDefault() is PivotItem item)
            {
                if (item.Tag.ToString() == "301")
                {
                    await vm.InitializeCharacterDataAsync(_uid);
                }
                if (item.Tag.ToString() == "302")
                {
                    await vm.InitializeWeaponDataAsync(_uid);
                }
                if (_SemanticZoom.IsZoomedInViewActive)
                {
                    _SemanticZoom.ToggleActiveView();
                }
                if (vm.WishEventStatsList?.Any(x => x.TotalCount > 0) ?? false)
                {
                    _SemanticZoom.CanChangeViews = true;
                }
                else
                {
                    _SemanticZoom.CanChangeViews = false;
                }
            }

        }
    }
}
