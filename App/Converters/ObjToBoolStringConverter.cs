using Microsoft.UI.Xaml.Data;

namespace Xunkong.Desktop.Converters;

public class ObjToBoolStringConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, string language)
    {
        return value is null ? "False" : "True";
    }

    public object ConvertBack(object value, Type targetType, object parameter, string language)
    {
        throw new NotImplementedException();
    }
}
