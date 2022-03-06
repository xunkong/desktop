namespace Xunkong.Core.TravelRecord
{

    /// <summary>
    /// 旅行记录原石或摩拉获取记录
    /// </summary>
    [Table("TravelRecord_AwardItems")]
    [Index(nameof(Uid), nameof(Year), nameof(Month))]
    [Index(nameof(Type))]
    [Index(nameof(ActionId))]
    [Index(nameof(Time))]
    public class TravelRecordAwardItem
    {
        [JsonIgnore]
        public int Id { get; set; }

        public int Uid { get; set; }

        [JsonIgnore]
        public int Year { get; set; }

        [JsonIgnore]
        public int Month { get; set; }

        public TravelRecordAwardType Type { get; set; }


        [JsonPropertyName("action_id")]
        public int ActionId { get; set; }


        [JsonPropertyName("action"), MaxLength(255)]
        public string? ActionName { get; set; }


        [JsonPropertyName("time"), JsonConverter(typeof(DataTimeJsonConverter))]
        public DateTime Time { get; set; }


        [JsonPropertyName("num")]
        public int Number { get; set; }



    }


    internal class DataTimeJsonConverter : JsonConverter<DateTime>
    {
        public override DateTime Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            return DateTime.Parse(reader.GetString()!);
        }

        public override void Write(Utf8JsonWriter writer, DateTime value, JsonSerializerOptions options)
        {
            writer.WriteStringValue(value.ToString("yyyy-MM-dd HH:mm:ss"));
        }
    }
}
