using System.ComponentModel;

namespace Xunkong.Desktop.Services
{


    public class UserSettingService
    {


        private readonly ILogger<UserSettingService> _logger;

        private readonly DbConnectionFactory<SqliteConnection> _cntFactory;


        public UserSettingService(ILogger<UserSettingService> logger, DbConnectionFactory<SqliteConnection> connectionFactory)
        {
            _logger = logger;
            _cntFactory = connectionFactory;
        }


        public async Task<T?> GetSettingAsync<T>(string key, bool notPrintLog = false)
        {
            using var con = _cntFactory.CreateDbConnection();
            var value = await con.QueryFirstOrDefaultAsync<string>($"SELECT Value FROM UserSettings WHERE Key=@Key;", new { Key = key });
            if (!notPrintLog)
            {
                _logger.LogTrace("Query UserSetting by key {Key} with value {Value}", key, value);
            }
            if (value == null)
            {
                return default;
            }
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



        public async Task SaveSettingAsync<T>(string key, T value, bool notPrintLog = false)
        {
            if (!notPrintLog)
            {
                _logger.LogTrace("Save UserSetting with key {Key}, value {Value}", key, value);
            }
            try
            {
                using var con = _cntFactory.CreateDbConnection();
                var setting = new UserSettingModel
                {
                    Key = key,
                    Value = value?.ToString(),
                };
                await con.ExecuteAsync("INSERT OR REPLACE INTO UserSettings VALUES (@Key,@Value);", setting);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Save UserSetting.");
            }

        }




    }
}
