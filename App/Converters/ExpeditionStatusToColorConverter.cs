using Microsoft.UI;
using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Media;

namespace Xunkong.Desktop.Converters;

internal class ExpeditionStatusToColorConverter : IValueConverter
{

    private static SolidColorBrush green = new SolidColorBrush(Colors.Green);
    private static SolidColorBrush orange = new SolidColorBrush(Colors.Orange);
    private static SolidColorBrush gray = new SolidColorBrush(Colors.Gray);

    public object Convert(object value, Type targetType, object parameter, string language)
    {

        var status = (bool)value;
        if (status)
        {
            return green;
        }
        else
        {
            return orange;
        }
    }

    public object ConvertBack(object value, Type targetType, object parameter, string language)
    {
        throw new NotImplementedException();
    }
}



