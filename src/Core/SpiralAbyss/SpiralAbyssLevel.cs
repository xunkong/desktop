namespace Xunkong.Core.SpiralAbyss
{
    /// <summary>
    /// 深境螺旋间
    /// </summary>
    [Table("SpiralAbyss_Levels")]
    public class SpiralAbyssLevel
    {

        [JsonIgnore]
        public int Id { get; set; }

        [JsonIgnore]
        public int SpiralAbyssFloorId { get; set; }


        [JsonPropertyName("index")]
        public int Index { get; set; }


        [JsonPropertyName("star")]
        public int Star { get; set; }


        [JsonPropertyName("max_star")]
        public int MaxStar { get; set; }


        [JsonPropertyName("battles")]
        public List<SpiralAbyssBattle> Battles { get; set; }


        [JsonIgnore, NotMapped]
        public DateTimeOffset FirstBattleTime => Battles.FirstOrDefault()?.Time ?? new();

    }
}
