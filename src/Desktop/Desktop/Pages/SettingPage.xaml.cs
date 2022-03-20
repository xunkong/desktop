using Microsoft.UI.Xaml.Controls;
using Windows.Globalization.NumberFormatting;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace Xunkong.Desktop.Pages
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class SettingPage : Page
    {



        private SettingViewModel vm => (DataContext as SettingViewModel)!;





        public SettingPage()
        {
            this.InitializeComponent();
            DataContext = ActivatorUtilities.CreateInstance<SettingViewModel>(App.Current.Services);
            Loaded += (_, _) => vm.InitializeData();
            SetNumberBoxNumberFormatter();
        }


        private async void _Expander_WebTool_Expanding(Expander sender, ExpanderExpandingEventArgs args)
        {
            await vm.InitializeWebToolItemsAsync();
        }


        private void _Expander_WebTool_Collapsed(Expander sender, ExpanderCollapsedEventArgs args)
        {
            vm.SelectedWebToolItem = null;
        }


        private void SetNumberBoxNumberFormatter()
        {
            IncrementNumberRounder rounder = new IncrementNumberRounder();
            rounder.Increment = 0.01;
            rounder.RoundingAlgorithm = RoundingAlgorithm.RoundUp;

            DecimalFormatter formatter = new DecimalFormatter();
            formatter.IntegerDigits = 1;
            formatter.FractionDigits = 2;
            formatter.NumberRounder = rounder;
            _NumberBox_HomeCoinThreshold.NumberFormatter = formatter;
        }


        private async void _Expander_StartGame_Expanding(Expander sender, ExpanderExpandingEventArgs args)
        {
            await vm.InitializeGenshinUserAccountsAsync();
        }


    }
}
