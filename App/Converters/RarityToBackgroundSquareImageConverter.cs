using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Media.Imaging;

namespace Xunkong.Desktop.Converters;

internal class RarityToBackgroundSquareImageBrushConverter : IValueConverter
{
    private const string Rarity1Background = "ms-appx:///Assets/Images/Rarity_1_Background_Square.png";
    private const string Rarity2Background = "ms-appx:///Assets/Images/Rarity_2_Background_Square.png";
    private const string Rarity3Background = "ms-appx:///Assets/Images/Rarity_3_Background_Square.png";
    private const string Rarity4Background = "ms-appx:///Assets/Images/Rarity_4_Background_Square.png";
    private const string Rarity5Background = "ms-appx:///Assets/Images/Rarity_5_Background_Square.png";

    private static BitmapImage Rarity1BackgroundBrush = new BitmapImage(new Uri(Rarity1Background));
    private static BitmapImage Rarity2BackgroundBrush = new BitmapImage(new Uri(Rarity2Background));
    private static BitmapImage Rarity3BackgroundBrush = new BitmapImage(new Uri(Rarity3Background));
    private static BitmapImage Rarity4BackgroundBrush = new BitmapImage(new Uri(Rarity4Background));
    private static BitmapImage Rarity5BackgroundBrush = new BitmapImage(new Uri(Rarity5Background));


    public object Convert(object value, Type targetType, object parameter, string language)
    {
        var rarity = (int)value;
        return rarity switch
        {
            1 => Rarity1BackgroundBrush,
            2 => Rarity2BackgroundBrush,
            3 => Rarity3BackgroundBrush,
            4 => Rarity4BackgroundBrush,
            5 => Rarity5BackgroundBrush,
            _ => Rarity1BackgroundBrush,
        };
    }

    public object ConvertBack(object value, Type targetType, object parameter, string language)
    {
        throw new NotImplementedException();
    }
}
