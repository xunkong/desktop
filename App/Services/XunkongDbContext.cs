using Microsoft.EntityFrameworkCore;
using Xunkong.Hoyolab.Account;
using Xunkong.Hoyolab.TravelNotes;
using Xunkong.Hoyolab.Wishlog;

namespace Xunkong.Desktop.Services;

internal class XunkongDbContext : DbContext
{


    private readonly string _dbConnectionString;




    public DbSet<GenshinRoleInfo> GenshinRoleInfo { get; set; }

    public DbSet<HoyolabUserInfo> HoyolabUserInfo { get; set; }

    public DbSet<TravelNotesAwardItem> TravelNotesAwardItem { get; set; }

    public DbSet<WebToolItem> WebToolItem { get; set; }

    public DbSet<WishlogItem> WishlogItem { get; set; }

    public DbSet<WishlogUrl> WishlogUrl { get; set; }


    public XunkongDbContext(string dbConnectionString)
    {
        _dbConnectionString = dbConnectionString;
    }


    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        optionsBuilder.UseSqlite(_dbConnectionString);
    }


    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<GenshinRoleInfo>().HasKey(x => x.Uid);
        modelBuilder.Entity<HoyolabUserInfo>().HasKey(x => x.Uid);
        modelBuilder.Entity<WishlogItem>().HasKey(x => new { x.Uid, x.Id });
        modelBuilder.Entity<WishlogItem>().Ignore(x => x.Count);
        modelBuilder.Entity<WishlogItem>().Ignore(x => x.ItemId);
        modelBuilder.Entity<WishlogItem>().Ignore(x => x._TimeString);
        modelBuilder.Entity<WishlogUrl>().HasKey(x => x.Uid);
    }



}
