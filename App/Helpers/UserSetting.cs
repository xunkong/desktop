using System.ComponentModel;

namespace Xunkong.Desktop.Helpers;

internal static class UserSetting
{

    public static T? GetValue<T>(string key, T? defaultValue = default)
    {
        using var dapper = DatabaseProvider.CreateConnection();
        var value = dapper.QuerySingleOrDefault<string>("SELECT Value FROM Setting WHERE Key=@Key LIMIT 1;", new { Key = key });
        try
        {
            var converter = TypeDescriptor.GetConverter(typeof(T));
            if (converter == null)
            {
                return default;
            }
            return (T?)converter.ConvertFromString(value);
        }
        catch (NotSupportedException)
        {
            return default;
        }
    }


    public static void SetValue<T>(string key, T value)
    {
        using var dapper = DatabaseProvider.CreateConnection();
        dapper.Execute("INSERT OR REPLACE INTO Setting (Key, Value) VALUES (@Key, @Value);", new { Key = key, Value = value?.ToString() });
    }


    public static bool TryGetValue<T>(string key, out T? result, T? defaultValue = default)
    {
        result = defaultValue;
        try
        {
            using var dapper = DatabaseProvider.CreateConnection();
            var value = dapper.QuerySingleOrDefault<string>("SELECT Value FROM Setting WHERE Key=@Key LIMIT 1;", new { Key = key });
            try
            {
                var converter = TypeDescriptor.GetConverter(typeof(T));
                if (converter == null)
                {
                    return false;
                }
                result = (T?)converter.ConvertFromString(value);
                return true;
            }
            catch (NotSupportedException)
            {
                return false;
            }
        }
        catch
        {
            return false;
        }
    }


    public static bool TrySaveValue<T>(string key, T value)
    {
        try
        {
            using var dapper = DatabaseProvider.CreateConnection();
            dapper.Execute("INSERT OR REPLACE INTO Setting (Key, Value) VALUES (@Key, @Value);", new { Key = key, Value = value?.ToString() });
            return true;
        }
        catch
        {
            return false;
        }
    }


}
