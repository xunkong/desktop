using Microsoft.UI.Xaml.Data;

namespace Xunkong.Desktop.Converters;

internal class RarityToIconStarConverter : IValueConverter
{

    private const string Rarity1StarIcon = "ms-appx:///Assets/Images/Icon_1_Stars.png";
    private const string Rarity2StarIcon = "ms-appx:///Assets/Images/Icon_2_Stars.png";
    private const string Rarity3StarIcon = "ms-appx:///Assets/Images/Icon_3_Stars.png";
    private const string Rarity4StarIcon = "ms-appx:///Assets/Images/Icon_4_Stars.png";
    private const string Rarity5StarIcon = "ms-appx:///Assets/Images/Icon_5_Stars.png";

    public object Convert(object value, Type targetType, object parameter, string language)
    {
        var rarity = (int)value;
        return rarity switch
        {
            1 => Rarity1StarIcon,
            2 => Rarity2StarIcon,
            3 => Rarity3StarIcon,
            4 => Rarity4StarIcon,
            5 => Rarity5StarIcon,
            _ => Rarity1StarIcon,
        };
    }

    public object ConvertBack(object value, Type targetType, object parameter, string language)
    {
        throw new NotImplementedException();
    }


}
