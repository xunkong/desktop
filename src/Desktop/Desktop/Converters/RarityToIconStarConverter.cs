namespace Xunkong.Desktop.Converters
{
    internal class RarityToIconStarConverter : IValueConverter
    {

        public object Convert(object value, Type targetType, object parameter, string language)
        {
            var rarity = (int)value;
            return $"ms-appx:///Assets/Images/Icon_{rarity}_Stars.png";
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            throw new NotImplementedException();
        }
    }
}
