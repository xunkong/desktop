using Microsoft.UI;
using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Media;
using Xunkong.Hoyolab.DailyNote;

namespace Xunkong.Desktop.Converters
{
    internal class QuestEventStatsColorConverter : IValueConverter
    {

        private static SolidColorBrush green = new SolidColorBrush(Colors.Green);
        private static SolidColorBrush orangeRed = new SolidColorBrush(Colors.OrangeRed);
        private static SolidColorBrush gray = new SolidColorBrush(Colors.Gray);



        public object Convert(object value, Type targetType, object parameter, string language)
        {
            var info = value as DailyNoteInfo;
            if (info != null)
            {
                if (info.IsExtraTaskRewardReceived)
                {
                    return gray;
                }
                else
                {
                    return orangeRed;
                }
            }
            return gray;
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            throw new NotImplementedException();
        }
    }


    internal class QuestEventStatsColorForXboxWidgetConverter : IValueConverter
    {

        private static SolidColorBrush orangeRed = new SolidColorBrush(Colors.OrangeRed);
        private static SolidColorBrush black = new SolidColorBrush(Colors.Black);

        public object Convert(object value, Type targetType, object parameter, string language)
        {
            var info = value as DailyNoteInfo;
            if (info != null)
            {
                if (info.IsExtraTaskRewardReceived)
                {
                    return black;
                }
                else
                {
                    return orangeRed;
                }
            }
            return black;
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            throw new NotImplementedException();
        }
    }


    internal class QuestEventStatsStringConverter : IValueConverter
    {


        public object Convert(object value, Type targetType, object parameter, string language)
        {
            var info = value as DailyNoteInfo;
            if (info != null)
            {
                if (info.IsExtraTaskRewardReceived)
                {
                    return "奖励已领取";
                }
                if (info.FinishedTaskNumber == info.TotalTaskNumber)
                {
                    return "奖励未领取";
                }
                return "任务未完成";
            }
            return "";
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            throw new NotImplementedException();
        }
    }
}
