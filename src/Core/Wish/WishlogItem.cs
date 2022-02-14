using System.Text.Json;

namespace Xunkong.Core.Wish
{
    /// <summary>
    /// 祈愿记录详细信息
    /// <para/>
    /// 属性已被 <see cref="JsonAttribute"/> 标注，使用 <see cref="JsonSerializer"/> 序列化时格式与官方一致
    /// </summary>
    [Table("Wishlog_Items")]
    [Index(nameof(ItemType))]
    [Index(nameof(Name))]
    [Index(nameof(QueryType))]
    [Index(nameof(RankType))]
    [Index(nameof(Time))]
    [Index(nameof(WishType))]
    public class WishlogItem : IEquatable<WishlogItem>, IJsonOnSerializing, IJsonOnDeserialized
    {
        /// <summary>
        /// 用户Uid
        /// </summary>
        [JsonPropertyName("uid"), JsonNumberHandling(JsonNumberHandling.AllowReadingFromString | JsonNumberHandling.WriteAsString)]
        public int Uid { get; set; }

        /// <summary>
        /// 祈愿类型（卡池类型）
        /// </summary>
        [JsonPropertyName("gacha_type"), JsonConverter(typeof(WishTypeJsonConverter))]
        public WishType WishType { get; set; }

        /// <summary>
        /// 此值为空
        /// </summary>
        [NotMapped]
        [JsonPropertyName("item_id"), JsonConverter(typeof(WishlogItemIdJsonConverter))]
        public int ItemId { get; set; }

        /// <summary>
        /// 物品数量（暂时都是1）
        /// </summary>
        [NotMapped]
        [JsonPropertyName("count"), JsonNumberHandling(JsonNumberHandling.AllowReadingFromString | JsonNumberHandling.WriteAsString)]
        public int Count { get; private set; } = 1;

        /// <summary>
        /// 时间
        /// </summary>
        [JsonIgnore]
        public DateTimeOffset Time { get; set; }


        /// <summary>
        /// 字符串形式的时间，时区与账号服务器所在地理位置相关
        /// </summary>
        [NotMapped]
        [JsonInclude, JsonPropertyName("time")]
        public string _TimeString { get; private set; }

        /// <summary>
        /// 物品名称
        /// </summary>
        [JsonPropertyName("name"), MaxLength(255)]
        public string Name { get; set; }

        /// <summary>
        /// 语言（如zh-cn）
        /// </summary>
        [JsonPropertyName("lang"), MaxLength(255)]
        public string Language { get; set; }

        /// <summary>
        /// 物品类型（角色、武器）
        /// </summary>
        [JsonPropertyName("item_type"), MaxLength(255)]
        public string ItemType { get; set; }

        /// <summary>
        /// 星级
        /// </summary>
        [JsonPropertyName("rank_type"), JsonNumberHandling(JsonNumberHandling.AllowReadingFromString | JsonNumberHandling.WriteAsString)]
        public int RankType { get; set; }

        /// <summary>
        /// 祈愿Id，这个值很重要，全服唯一
        /// </summary>
        [JsonPropertyName("id"), JsonNumberHandling(JsonNumberHandling.AllowReadingFromString | JsonNumberHandling.WriteAsString)]
        public long Id { get; set; }


        /// <summary>
        /// 查询类型，此值为自行添加的值
        /// </summary>
        [JsonPropertyName("query_type"), JsonConverter(typeof(WishTypeJsonConverter))]
        public WishType QueryType { get; set; }


        public bool Equals(WishlogItem? other)
        {
            return (Uid, Id) == (other?.Uid, other?.Id);
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(Id, Uid);
        }

        public void OnSerializing()
        {
            var offset = Uid.ToString()[0] switch
            {
                '6' => -5,
                '7' => 1,
                _ => 8,
            };
            _TimeString = Time.UtcDateTime.AddHours(offset).ToString("yyyy-MM-dd HH:mm:ss");
        }

        public void OnDeserialized()
        {
            var offset = Uid.ToString()[0] switch
            {
                '6' => -5,
                '7' => 1,
                _ => 8,
            };
            var time = DateTime.Parse(_TimeString);
            Time = new DateTimeOffset(time, TimeSpan.FromHours(offset));
        }
    }


    #region Json Converter

    internal class WishTypeJsonConverter : JsonConverter<WishType>
    {
        public override WishType Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            return (WishType)int.Parse(reader.GetString()!);
        }

        public override void Write(Utf8JsonWriter writer, WishType value, JsonSerializerOptions options)
        {
            writer.WriteStringValue(((int)value).ToString());
        }
    }

    internal class WishlogItemIdJsonConverter : JsonConverter<int>
    {
        public override int Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            return 0;
        }

        public override void Write(Utf8JsonWriter writer, int value, JsonSerializerOptions options)
        {
            writer.WriteStringValue("");
        }
    }


    #endregion


}
