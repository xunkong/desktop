using Serilog;
using Windows.Storage;

namespace Xunkong.Desktop.Helpers
{
    internal static class LocalSettingHelper
    {


        private static readonly ApplicationDataContainer _container;


        static LocalSettingHelper()
        {
            _container = ApplicationData.Current.LocalSettings;
        }



        public static T? GetSetting<T>(string key, T? defaultValue = default)
        {
            var value = _container.Values[key];
            if (value is null)
            {
                return defaultValue;
            }
            else
            {
                return (T?)value;
            }
        }


        public static void SaveSetting<T>(string key, T value)
        {
            try
            {
                _container.Values[key] = value;
            }
            catch (Exception ex)
            {
                Log.Logger.Error(ex, $"Save local setting with key {key} value {value}");
            }
        }




    }
}
