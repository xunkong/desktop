using Windows.Storage;

namespace Xunkong.Desktop.Helpers;

internal static class AppSetting
{

    private static readonly ApplicationDataContainer _container;

    static AppSetting()
    {
        _container = ApplicationData.Current.LocalSettings;
    }


    public static T? GetValue<T>(string key, T? defaultValue = default, bool throwError = true)
    {
        try
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
        catch
        {
            if (throwError)
            {
                throw;
            }
            return defaultValue;
        }

    }


    public static void SetValue<T>(string key, T value)
    {
        _container.Values[key] = value;
    }


    public static bool TryGetValue<T>(string key, out T? result, T? defaultValue = default)
    {
        try
        {
            var value = _container.Values[key];
            if (value is null)
            {
                result = defaultValue;
                return false;
            }
            else
            {
                result = (T?)value;
                return true;
            }
        }
        catch
        {
            result = defaultValue;
            return false;
        }
    }


    public static bool TrySetValue<T>(string key, T value, bool setNullWhenError = false)
    {
        try
        {
            _container.Values[key] = value;
            return true;
        }
        catch
        {
            if (setNullWhenError)
            {
                _container.Values[key] = null;
            }
            return false;
        }
    }


}
