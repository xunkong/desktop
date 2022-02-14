using Xunkong.Core.Hoyolab;

namespace Xunkong.Core.Wish
{
    [Table("Info_WishEvent")]
    [Index(nameof(WishType))]
    [Index(nameof(WishType), nameof(_StartTimeString), IsUnique = true)]
    public class WishEventInfo
    {

        public int Id { get; set; }


        public WishType WishType { get; set; }

        [JsonIgnore, NotMapped]
        public WishType QueryType => WishTypeToQueryType(WishType);

        [MaxLength(255)]
        public string Name { get; set; }

        [MaxLength(255)]
        public string Version { get; set; }


        [JsonIgnore, NotMapped]
        public DateTimeOffset StartTime => TimeStringToTimeOffset(_StartTimeString);


        [JsonIgnore, NotMapped]
        public DateTimeOffset EndTime => TimeStringToTimeOffset(_EndTimeString);


        [JsonPropertyName("StartTime"), JsonInclude]
        [Column("StartTime"), MaxLength(255)]
        public string _StartTimeString { get; private set; }


        [JsonPropertyName("EndTime"), JsonInclude]
        [Column("EndTime"), MaxLength(255)]
        public string _EndTimeString { get; private set; }

        /// <summary>
        /// Up5星物品，此值不要使用SQL查询
        /// </summary>
        public List<string> Rank5UpItems { get; set; }

        /// <summary>
        /// Up4星物品，此值不要使用SQL查询
        /// </summary>
        public List<string> Rank4UpItems { get; set; }




        private WishType WishTypeToQueryType(WishType type)
        {
            return type switch
            {
                WishType.CharacterEvent_2 => WishType.CharacterEvent,
                _ => type,
            };
        }



        [JsonIgnore, NotMapped]
        public static RegionType RegionType { get; set; }


        private static DateTimeOffset TimeStringToTimeOffset(string str)
        {
            if (str.Contains("+"))
            {
                return DateTimeOffset.Parse(str);
            }
            else
            {
                var offset = RegionType switch
                {
                    RegionType.US => "-05:00",
                    RegionType.EU => "+01:00",
                    _ => "+08:00",
                };
                return DateTimeOffset.Parse($"{str} {offset}");
            }
        }


    }
}
