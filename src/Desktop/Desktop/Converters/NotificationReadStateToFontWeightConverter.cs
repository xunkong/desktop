namespace Xunkong.Desktop.Converters
{
    internal class NotificationReadStateToFontWeightConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            var read = (bool)value;
            if (read)
            {
                return "Normal";
            }
            else
            {
                return "Bold";
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            throw new NotImplementedException();
        }
    }
}
