using System.Data.Common;

namespace Xunkong.Desktop.Services
{
    public class DbConnectionFactory<TDbConnection> where TDbConnection : DbConnection, new()
    {

        private readonly string _connectionString;

        public DbConnectionFactory(string connectionString)
        {
            _connectionString = connectionString;
        }


        public TDbConnection CreateDbConnection()
        {
            return new TDbConnection { ConnectionString = _connectionString };
        }

    }
}
