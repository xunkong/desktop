namespace Xunkong.Desktop.Converters
{
    class EnumToDescriptionOrStringConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            var e = (Enum)value;
            return e.ToDescriptionOrString();
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            throw new NotImplementedException();
        }
    }
}
