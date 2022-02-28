using Windows.Storage;

namespace Xunkong.Desktop.Extension
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
            _container.Values[key] = value;
        }




    }
}
