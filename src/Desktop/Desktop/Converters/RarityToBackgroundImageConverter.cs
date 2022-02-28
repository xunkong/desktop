namespace Xunkong.Desktop.Converters
{
    internal class RarityToBackgroundImageConverter : IValueConverter
    {

        private const string Rarity1Background = "ms-appx:///Assets/Images/Rarity_1_Background.png";
        private const string Rarity2Background = "ms-appx:///Assets/Images/Rarity_2_Background.png";
        private const string Rarity3Background = "ms-appx:///Assets/Images/Rarity_3_Background.png";
        private const string Rarity4Background = "ms-appx:///Assets/Images/Rarity_4_Background.png";
        private const string Rarity5Background = "ms-appx:///Assets/Images/Rarity_5_Background.png";

        public object Convert(object value, Type targetType, object parameter, string language)
        {
            var rarity = (int)value;
            return rarity switch
            {
                1 => Rarity1Background,
                2 => Rarity2Background,
                3 => Rarity3Background,
                4 => Rarity4Background,
                5 => Rarity5Background,
                _ => Rarity1Background,
            };
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            throw new NotImplementedException();
        }


    }
}
