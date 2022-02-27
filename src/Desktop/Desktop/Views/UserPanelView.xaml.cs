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
using Xunkong.Desktop.Controls;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace Xunkong.Desktop.Views
{
    public sealed partial class UserPanelView : UserControl
    {


        public static readonly DependencyProperty NavigationViewPaneOpenProperty;

        public bool NavigationViewPaneOpen
        {
            get => (bool)GetValue(NavigationViewPaneOpenProperty);
            set => SetValue(NavigationViewPaneOpenProperty, value);
        }


        static UserPanelView()
        {
            NavigationViewPaneOpenProperty = DependencyProperty.Register(nameof(NavigationViewPaneOpen), typeof(bool), typeof(UserPanelView), null);
        }



        private UserPanelViewModel vm => (DataContext as UserPanelViewModel)!;



        public UserPanelView()
        {
            this.InitializeComponent();
            DataContext = ActivatorUtilities.CreateInstance<UserPanelViewModel>(App.Current.Services);
            Loaded += async (_, _) => await vm.InitializeDataAsync();
            vm.HideUserPanelSelectorFlyout += () => _Flyout_UserPanelSelector.DispatcherQueue.TryEnqueue(() => _Flyout_UserPanelSelector.Hide());
        }





        private void ShowAttachedFlyout(object sender, TappedRoutedEventArgs e)
        {
            FlyoutBase.ShowAttachedFlyout((FrameworkElement)sender);
        }


        private void OpenOrCloseNavigationViewPane(object sender, TappedRoutedEventArgs e)
        {
            WeakReferenceMessenger.Default.Send(new OpenOrCloseNavigationPaneMessage());
        }

        private void _Flyout_UserPanelSelector_Opening(object sender, object e)
        {
            _userPanelViewSource.Source = vm.InitGroupSource();
            var selectedUid = vm.SelectedUserPanelModel?.GameRoleInfo?.Uid;
            if (selectedUid == 0)
            {
                return;
            }
            _ListView_UserPanelModels.SelectedItem = _ListView_UserPanelModels.Items.FirstOrDefault(x => (x as UserPanelModel)?.GameRoleInfo?.Uid == selectedUid);
        }


        private void _ListView_UserPanelModels_ItemClick(object sender, ItemClickEventArgs e)
        {
            vm.SelectedUserPanelModel = e.ClickedItem as UserPanelModel;
            _Flyout_UserPanelSelector.Hide();
        }

        private async void _Button_DeleteUserInfo_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button button)
            {
                if (button.DataContext is IEnumerable<UserPanelModel> models)
                {
                    await vm.DeleteUserInfoAsync(models);
                }
            }
            _Flyout_UserPanelSelector.Hide();
        }



        private async void _Button_PinTile_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button button)
            {
                if (button.DataContext is UserPanelModel model)
                {
                    await vm.PinOrUnpinTileAsync(model);
                }
            }
        }


    }
}
