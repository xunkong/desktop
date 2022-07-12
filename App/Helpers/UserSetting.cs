namespace Xunkong.Desktop.Helpers;

internal static class UserSetting
{

    public static T? GetValue<T>(string key, T? defaultValue = default)
    {
        using var dapper = DatabaseProvider.CreateConnection();
        var value = dapper.QuerySingleOrDefault<T>("SELECT Value FROM Setting WHERE Key=@Key LIMIT 1;", new { Key = key });
        if (value is null)
        {
            return defaultValue;
        }
        else
        {
            return (T?)value;
        }
    }


    public static void SetValue<T>(string key, T value)
    {
        using var dapper = DatabaseProvider.CreateConnection();
        dapper.Execute("INSERT OR REPLACE INTO Setting (Key, Value) VALUES (@Key, @Value);", new { Key = key, Value = value });
    }


    public static bool TryGetValue<T>(string key, out T? result, T? defaultValue = default)
    {
        try
        {
            using var dapper = DatabaseProvider.CreateConnection();
            var value = dapper.QuerySingleOrDefault<T>("SELECT Value FROM Setting WHERE Key=@Key LIMIT 1;", new { Key = key });
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


    public static bool TrySaveValue<T>(string key, T value)
    {
        try
        {
            using var dapper = DatabaseProvider.CreateConnection();
            dapper.Execute("INSERT OR REPLACE INTO Setting (Key, Value) VALUES (@Key, @Value);", new { Key = key, Value = value });
            return true;
        }
        catch
        {
            return false;
        }
    }





}
