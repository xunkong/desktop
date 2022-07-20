using Microsoft.UI.Xaml.Data;

namespace Xunkong.Desktop.Converters;

class EnumToDescriptionOrStringConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, string language)
    {
        var e = (Enum)value;
        return e.ToDescription();
    }

    public object ConvertBack(object value, Type targetType, object parameter, string language)
    {
        throw new NotImplementedException();
    }
}
