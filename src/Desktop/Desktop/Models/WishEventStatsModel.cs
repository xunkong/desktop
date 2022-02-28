using Xunkong.Core.Wish;

namespace Xunkong.Desktop.Models
{
    public class WishEventStatsModel
    {

        public int Id { get; set; }


        public WishType QueryType { get; set; }


        public string Name { get; set; }


        public string Version { get; set; }



        public DateTimeOffset StartTime { get; set; }



        public DateTimeOffset EndTime { get; set; }


        public List<WishEventStatsItemInfoModel> UpItems { get; set; }


        public List<WishEventStatsItemInfoModel> NoneUpItems { get; set; }


        public List<WishEventStatsRarity5Item> Rarity5Items { get; set; }


        public List<WishlogItemEx> Wishlogs { get; set; }


        public int TotalCount { get; set; }


        public int Rarity5Count { get; set; }


        public int Rarity4Count { get; set; }


        public int Rarity3Count { get; set; }






        public override string ToString()
        {
            return $"{Version} {Name}";
        }

    }
}
