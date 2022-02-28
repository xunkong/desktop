namespace Xunkong.Core.SpiralAbyss
{
    [Table("SpiralAbyss_Infos")]
    [Index(nameof(Uid), nameof(ScheduleId), IsUnique = true)]
    [Index(nameof(ScheduleId))]
    [Index(nameof(TotalBattleCount))]
    [Index(nameof(TotalWinCount))]
    [Index(nameof(MaxFloor))]
    [Index(nameof(TotalStar))]
    public class SpiralAbyssInfo : IJsonOnDeserialized
    {

        [JsonIgnore]
        public int Id { get; set; }

        public int Uid { get; set; }

        [JsonPropertyName("schedule_id")]
        public int ScheduleId { get; set; }


        [JsonPropertyName("start_time"), JsonConverter(typeof(SpiralAbyssTimeJsonConverter))]
        public DateTimeOffset StartTime { get; set; }


        [JsonPropertyName("end_time"), JsonConverter(typeof(SpiralAbyssTimeJsonConverter))]
        public DateTimeOffset EndTime { get; set; }


        [JsonPropertyName("total_battle_times")]
        public int TotalBattleCount { get; set; }


        [JsonPropertyName("total_win_times")]
        public int TotalWinCount { get; set; }


        [JsonPropertyName("max_floor")]
        [MaxLength(255)]
        public string? MaxFloor { get; set; }

        /// <summary>
        /// 出战最多
        /// </summary>
        [JsonPropertyName("reveal_rank")]
        public List<SpiralAbyssRank> RevealRank { get; set; }

        /// <summary>
        /// 击破最多
        /// </summary>
        [JsonPropertyName("defeat_rank")]
        public List<SpiralAbyssRank> DefeatRank { get; set; }

        /// <summary>
        /// 伤害最高
        /// </summary>
        [JsonPropertyName("damage_rank")]
        public List<SpiralAbyssRank> DamageRank { get; set; }

        /// <summary>
        /// 承伤最高
        /// </summary>
        [JsonPropertyName("take_damage_rank")]
        public List<SpiralAbyssRank> TakeDamageRank { get; set; }

        /// <summary>
        /// 元素战技最多
        /// </summary>
        [JsonPropertyName("normal_skill_rank")]
        public List<SpiralAbyssRank> NormalSkillRank { get; set; }

        /// <summary>
        /// 元素爆发最多
        /// </summary>
        [JsonPropertyName("energy_skill_rank")]
        public List<SpiralAbyssRank> EnergySkillRank { get; set; }


        [JsonPropertyName("floors")]
        public List<SpiralAbyssFloor> Floors { get; set; }


        [JsonPropertyName("total_star")]
        public int TotalStar { get; set; }


        [JsonPropertyName("is_unlock")]
        public bool IsUnlock { get; set; }


        public void OnDeserialized()
        {
            if (RevealRank?.Any() ?? false)
            {
                RevealRank.ForEach(x => x.Type = SpiralAbyssRankType.RevealRank);
            }
            if (DefeatRank?.Any() ?? false)
            {
                DefeatRank.ForEach(x => x.Type = SpiralAbyssRankType.DefeatRank);
            }
            if (DamageRank?.Any() ?? false)
            {
                DamageRank.ForEach(x => x.Type = SpiralAbyssRankType.DamageRank);
            }
            if (TakeDamageRank?.Any() ?? false)
            {
                TakeDamageRank.ForEach(x => x.Type = SpiralAbyssRankType.TakeDamageRank);
            }
            if (NormalSkillRank?.Any() ?? false)
            {
                NormalSkillRank.ForEach(x => x.Type = SpiralAbyssRankType.NormalSkillRank);
            }
            if (EnergySkillRank?.Any() ?? false)
            {
                EnergySkillRank.ForEach(x => x.Type = SpiralAbyssRankType.EnergySkillRank);
            }
        }
    }



    internal class SpiralAbyssTimeJsonConverter : JsonConverter<DateTimeOffset>
    {
        public override DateTimeOffset Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            return DateTimeOffset.FromUnixTimeSeconds(long.Parse(reader.GetString()!));
        }

        public override void Write(Utf8JsonWriter writer, DateTimeOffset value, JsonSerializerOptions options)
        {
            writer.WriteStringValue(value.ToUnixTimeSeconds().ToString());
        }
    }


}
