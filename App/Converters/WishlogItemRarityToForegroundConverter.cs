using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Media;

namespace Xunkong.Desktop.Converters;

internal class WishlogItemRarityToForegroundConverter : IValueConverter
{

    public object Convert(object value, Type targetType, object parameter, string language)
    {
        var rarity = (int)value;
        var brush = rarity switch
        {
            5 => Application.Current.Resources["Rarity5ForegroundBrush"] as SolidColorBrush,
            4 => Application.Current.Resources["Rarity4ForegroundBrush"] as SolidColorBrush,
            _ => Application.Current.Resources["TextFillColorSecondaryBrush"] as SolidColorBrush,
        };
        return brush!;
    }

    public object ConvertBack(object value, Type targetType, object parameter, string language)
    {
        throw new NotImplementedException();
    }
}
