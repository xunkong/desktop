using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Media;

namespace Xunkong.Desktop.Converters
{
    internal class RankToColorConverter : IValueConverter
    {

        private static SolidColorBrush Rank5Brush = new SolidColorBrush(Windows.UI.Color.FromArgb(0xFF, 0xBD, 0x69, 0x32));

        private static SolidColorBrush Rank4Brush = new SolidColorBrush(Windows.UI.Color.FromArgb(0xFF, 0xA2, 0x56, 0xE1));

        private static SolidColorBrush Rank3Brush = new SolidColorBrush(Windows.UI.Color.FromArgb(0xFF, 0x5F, 0x9C, 0xB8));

        private static SolidColorBrush TextFillColorSecondaryBrush = (Application.Current.Resources["TextFillColorSecondaryBrush"] as SolidColorBrush)!;


        public object Convert(object value, Type targetType, object parameter, string language)
        {
            var rank = (int)value;
            return rank switch
            {
                5 => Rank5Brush,
                4 => Rank4Brush,
                _ => (Application.Current.Resources["TextFillColorSecondaryBrush"] as SolidColorBrush)!,
            };
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            throw new NotImplementedException();
        }
    }
}
