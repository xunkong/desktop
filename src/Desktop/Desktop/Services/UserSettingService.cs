using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel;

namespace Xunkong.Desktop.Services
{

    [InjectService]
    public class UserSettingService
    {


        private readonly DbConnectionFactory<SqliteConnection> _connectionFactory;

        private readonly ILogger<UserSettingService> _logger;


        public UserSettingService(DbConnectionFactory<SqliteConnection> connectionFactory, ILogger<UserSettingService> logger)
        {
            _connectionFactory = connectionFactory;
            _logger = logger;
        }


        public async Task<T?> GetSettingAsync<T>(string key, bool notPrintLog = false)
        {
            using var con = _connectionFactory.CreateDbConnection();
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
                using var con = _connectionFactory.CreateDbConnection();
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
