namespace Xunkong.Core.Hoyolab
{
    public class SignInInfo
    {

        /// <summary>
        /// 累积签到天数
        /// </summary>
        [JsonPropertyName("total_sign_day")]
        public int TotalSignDays { get; set; }

        /// <summary>
        /// 今天是...
        /// </summary>
        [JsonPropertyName("today"), JsonConverter(typeof(SignInTodayJsonConverter))]
        public DateOnly Today { get; set; }

        /// <summary>
        /// 今日是否已签到
        /// </summary>
        [JsonPropertyName("is_sign")]
        public bool IsSign { get; set; }


        [JsonPropertyName("is_sub")]
        public bool IsSub { get; set; }


        [JsonPropertyName("first_bind")]
        public bool FirstBind { get; set; }


        [JsonPropertyName("month_first")]
        public bool IsFirstDayOfMonth { get; set; }


        [JsonPropertyName("sign_cnt_missed")]
        public int MissedCount { get; set; }

    }


    internal class SignInTodayJsonConverter : JsonConverter<DateOnly?>
    {
        public override DateOnly? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            var value = reader.GetString();
            if (string.IsNullOrWhiteSpace(value))
            {
                return null;
            }
            else
            {
                return DateOnly.Parse(value);
            }
        }

        public override void Write(Utf8JsonWriter writer, DateOnly? value, JsonSerializerOptions options)
        {
            writer.WriteStringValue(value?.ToString("yyyy-MM-dd") ?? "");
        }
    }
}
