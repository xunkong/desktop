using Xunkong.Core.Hoyolab;

namespace Xunkong.Core.TravelRecord
{
    public abstract class TravelRecordBase
    {

        [JsonPropertyName("uid")]
        public int Uid { get; set; }


        [JsonPropertyName("region"), JsonConverter(typeof(RegionTypeJsonConverter))]
        public RegionType Region { get; set; }

        /// <summary>
        /// 米游社 ID
        /// </summary>
        [JsonPropertyName("account_id")]
        public int AccountId { get; set; }


        [JsonPropertyName("nickname")]
        public string? Nickname { get; set; }

        /// <summary>
        /// 当前日期
        /// </summary>
        [JsonPropertyName("date"), JsonConverter(typeof(DataOnlyJsonConverter))]
        public DateOnly Date { get; set; }

        /// <summary>
        /// 当前月
        /// </summary>
        [JsonPropertyName("month")]
        public int CurrentMonth { get; set; }

        /// <summary>
        /// 可查询月份
        /// </summary>
        [JsonPropertyName("optional_month")]
        public List<int>? OptionalMonth { get; set; }

        /// <summary>
        /// 获取的数据所在的月份
        /// </summary>
        [JsonPropertyName("data_month")]
        public int DataMonth { get; set; }

    }


    public class DataOnlyJsonConverter : JsonConverter<DateOnly>
    {
        public override DateOnly Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            var value = reader.GetString();
            return DateOnly.Parse(value!);
        }

        public override void Write(Utf8JsonWriter writer, DateOnly value, JsonSerializerOptions options)
        {
            writer.WriteStringValue(value.ToString("yyyy-MM-dd"));
        }
    }


}
