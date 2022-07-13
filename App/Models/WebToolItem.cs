namespace Xunkong.Desktop.Models;

[INotifyPropertyChanged]
public partial class WebToolItem
{

    [ObservableProperty]
    private int id;

    [ObservableProperty]
    private string? title;

    [ObservableProperty]
    private string? name;

    [ObservableProperty]
    private string? icon;

    [ObservableProperty]
    private int order;

    [ObservableProperty]
    private string url;

    [ObservableProperty]
    private string? javaScript;

}
