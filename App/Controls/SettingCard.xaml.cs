using Microsoft.UI.Xaml.Controls;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace Xunkong.Desktop.Controls;

[INotifyPropertyChanged]
public sealed partial class SettingCard : UserControl
{
    public SettingCard()
    {
        this.InitializeComponent();
    }



    [ObservableProperty]
    private object icon;


    [ObservableProperty]
    private object content;


    [ObservableProperty]
    private object selector;


}
