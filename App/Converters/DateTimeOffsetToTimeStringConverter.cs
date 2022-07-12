using Microsoft.UI.Xaml.Data;

namespace Xunkong.Desktop.Converters;

internal class DateTimeOffsetToTimeStringConverter : IValueConverter
{
    public object? Convert(object value, Type targetType, object parameter, string language)
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



internal class DateTimeOffsetToDayStringConverter : IValueConverter
{
    public object? Convert(object value, Type targetType, object parameter, string language)
    {
        if (value is DateTimeOffset time)
        {
            var days = time.Date - DateTimeOffset.Now.Date;
            if (days.Days == 0)
            {
                return "今日";
            }
            if (days.Days == 1)
            {
                return "明日";
            }
            switch (time.DayOfWeek)
            {
                case DayOfWeek.Friday:
                    return "周五";
                case DayOfWeek.Monday:
                    return "周一";
                case DayOfWeek.Saturday:
                    return "周六";
                case DayOfWeek.Sunday:
                    return "周日";
                case DayOfWeek.Thursday:
                    return "周四";
                case DayOfWeek.Tuesday:
                    return "周二";
                case DayOfWeek.Wednesday:
                    return "周三";
            }
        }
        if (value is DateTime time1)
        {
            var days = time1.Date - DateTimeOffset.Now.Date;
            if (days.Days == 0)
            {
                return "今日";
            }
            if (days.Days == 1)
            {
                return "明日";
            }
            switch (time1.DayOfWeek)
            {
                case DayOfWeek.Friday:
                    return "周五";
                case DayOfWeek.Monday:
                    return "周一";
                case DayOfWeek.Saturday:
                    return "周六";
                case DayOfWeek.Sunday:
                    return "周日";
                case DayOfWeek.Thursday:
                    return "周四";
                case DayOfWeek.Tuesday:
                    return "周二";
                case DayOfWeek.Wednesday:
                    return "周三";
            }
        }
        return null;
    }

    public object ConvertBack(object value, Type targetType, object parameter, string language)
    {
        throw new NotImplementedException();
    }
}





