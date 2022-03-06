using Microsoft.EntityFrameworkCore;
using Xunkong.Desktop.Services;

namespace Xunkong.Desktop.Extension
{
    internal static class DbHelper
    {

        private static string dbPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), @"Xunkong\Data\XunkongData.db");

        public static XunkongDbContext CreateDbContext()
        {
            if (!File.Exists(dbPath))
            {
                throw new FileNotFoundException("Xunkong database file is not found.");
            }
            var options = new DbContextOptionsBuilder<XunkongDbContext>().UseSqlite($"Data Source={dbPath};").Options;
            return new XunkongDbContext(options);
        }

    }
}
