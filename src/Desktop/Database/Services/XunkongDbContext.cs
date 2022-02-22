using Microsoft.EntityFrameworkCore;
using Xunkong.Core.Hoyolab;
using Xunkong.Core.Metadata;
using Xunkong.Core.SpiralAbyss;
using Xunkong.Core.TravelRecord;
using Xunkong.Core.Wish;

namespace Xunkong.Desktop.Services
{
    public class XunkongDbContext : DbContext
    {

        public DbSet<UserSettingModel> UserSettings { get; set; }

        public DbSet<UserInfo> UserInfos { get; set; }

        public DbSet<UserGameRoleInfo> UserGameRoleInfos { get; set; }

        public DbSet<WishlogItem> WishlogItems { get; set; }

        public DbSet<WishlogAuthkeyItem> WishlogAuthkeys { get; set; }

        public DbSet<CharacterInfo> CharacterInfos { get; set; }

        public DbSet<WeaponInfo> WeaponInfos { get; set; }

        public DbSet<WishEventInfo> WishEventInfos { get; set; }

        public DbSet<I18nModel> I18nModels { get; set; }

        public DbSet<TravelRecordMonthData> TravelRecordMonthDatas { get; set; }

        public DbSet<TravelRecordAwardItem> TravelRecordAwardItems { get; set; }

        public DbSet<SpiralAbyssInfo> SpiralAbyssInfos { get; set; }

        public DbSet<DailyNoteInfo> DailyNoteInfos { get; set; }

        public DbSet<NotificationDesktopModel> NotificationItems { get; set; }

        public DbSet<WebToolItem> WebToolItems { get; set; }


        public XunkongDbContext(DbContextOptions<XunkongDbContext> options) : base(options)
        {
        }



        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            base.OnConfiguring(optionsBuilder);
        }


        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            modelBuilder.Entity<WishlogItem>().HasKey(x => new { x.Uid, x.Id });
            modelBuilder.Entity<WishlogAuthkeyItem>().HasKey(x => x.Uid);
            modelBuilder.Entity<CharacterInfo>().Ignore(x => x.Enable);
            modelBuilder.Entity<WeaponInfo>().Ignore(x => x.Enable);
            modelBuilder.Entity<WishEventInfo>().Property(x => x.Rank5UpItems).HasConversion(list => string.Join(",", list), s => s.Split(",", StringSplitOptions.None).ToList());
            modelBuilder.Entity<WishEventInfo>().Property(x => x.Rank4UpItems).HasConversion(list => string.Join(",", list), s => s.Split(",", StringSplitOptions.None).ToList());
            modelBuilder.Entity<SpiralAbyssRank>(e =>
            {
                e.HasOne<SpiralAbyssInfo>().WithMany(x => x.RevealRank).HasForeignKey("SpiralAbyssInfo_RevealRank").OnDelete(DeleteBehavior.Cascade);
                e.HasOne<SpiralAbyssInfo>().WithMany(x => x.DefeatRank).HasForeignKey("SpiralAbyssInfo_DefeatRank").OnDelete(DeleteBehavior.Cascade);
                e.HasOne<SpiralAbyssInfo>().WithMany(x => x.DamageRank).HasForeignKey("SpiralAbyssInfo_DamageRank").OnDelete(DeleteBehavior.Cascade);
                e.HasOne<SpiralAbyssInfo>().WithMany(x => x.TakeDamageRank).HasForeignKey("SpiralAbyssInfo_TakeDamageRank").OnDelete(DeleteBehavior.Cascade);
                e.HasOne<SpiralAbyssInfo>().WithMany(x => x.NormalSkillRank).HasForeignKey("SpiralAbyssInfo_NormalSkillRank").OnDelete(DeleteBehavior.Cascade);
                e.HasOne<SpiralAbyssInfo>().WithMany(x => x.EnergySkillRank).HasForeignKey("SpiralAbyssInfo_EnergySkillRank").OnDelete(DeleteBehavior.Cascade);
            });
        }


    }
}
