using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace Xunkong.Desktop.Services
{
    internal class XunkongDbContextFactory : IDesignTimeDbContextFactory<XunkongDbContext>
    {
        public XunkongDbContext CreateDbContext(string[] args)
        {
            var options = new DbContextOptionsBuilder<XunkongDbContext>().UseSqlite("Data Source=test.db;").Options;
            return new XunkongDbContext(options);
        }
    }
}
