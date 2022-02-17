using Microsoft.UI.Xaml.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Xunkong.Desktop.Converters
{
    internal class DateTimeOffsetToTimeStringConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            if (value is DateTimeOffset time)
            {
                return time.LocalDateTime.ToString("HH:mm:ss");
            }
            if (value is DateTime time1)
            {
                return time1.ToString("HH:mm:ss");
            }
            return null;
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            throw new NotImplementedException();
        }
    }


    internal class DateTimeOffsetToDateTimeStringConverter : IValueConverter
    {
        public object? Convert(object value, Type targetType, object parameter, string language)
        {
            if (value is DateTimeOffset time)
            {
                return time.LocalDateTime.ToString("yyyy-MM-dd HH:mm:ss");
            }
            if (value is DateTime time1)
            {
                return time1.ToString("yyyy-MM-dd HH:mm:ss");
            }
            return null;
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            throw new NotImplementedException();
        }
    }






}
