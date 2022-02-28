namespace Xunkong.Core.Hoyolab
{
    [Table("DailyNote_Items")]
    public class DailyNoteInfo
    {

        [JsonIgnore]
        public int Id { get; set; }

        public int Uid { get; set; }

        [JsonIgnore, NotMapped]
        public string? Nickname { get; set; }

        public DateTimeOffset Time { get; set; }

        /// <summary>
        /// 当前树脂
        /// </summary>
        [JsonPropertyName("current_resin")]
        public int CurrentResin { get; set; }

        /// <summary>
        /// 最大树脂
        /// </summary>
        [JsonPropertyName("max_resin")]
        public int MaxResin { get; set; }

        /// <summary>
        /// 树脂恢复时间<see cref="string"/>类型的秒数
        /// </summary>
        [JsonPropertyName("resin_recovery_time"), JsonConverter(typeof(RemainedTimeJsonConverter))]
        public TimeSpan ResinRecoveryTime { get; set; }

        [JsonIgnore, NotMapped]
        public bool IsResinFull => ResinRecoveryTime == TimeSpan.Zero;

        [JsonIgnore, NotMapped]
        public DateTimeOffset ResinFullTime => Time + ResinRecoveryTime;

        [JsonIgnore, NotMapped]
        public string ResinRecoveryDayString => ResinFullTime.Date > Time.Date ? "明日" : "今日";


        /// <summary>
        /// 委托完成数
        /// </summary>
        [JsonPropertyName("finished_task_num")]
        public int FinishedTaskNumber { get; set; }

        /// <summary>
        /// 委托总数
        /// </summary>
        [JsonPropertyName("total_task_num")]
        public int TotalTaskNumber { get; set; }


        /// <summary>
        /// 4次委托额外奖励是否领取
        /// </summary>
        [JsonPropertyName("is_extra_task_reward_received")]
        public bool IsExtraTaskRewardReceived { get; set; }


        /// <summary>
        /// 剩余周本树脂减半次数
        /// </summary>
        [JsonPropertyName("remain_resin_discount_num")]
        public int RemainResinDiscountNumber { get; set; }


        /// <summary>
        /// 周本树脂减半总次数
        /// </summary>
        [JsonPropertyName("resin_discount_num_limit")]
        public int ResinDiscountLimitedNumber { get; set; }

        /// <summary>
        /// 当前派遣数
        /// </summary>
        [JsonPropertyName("current_expedition_num")]
        public int CurrentExpeditionNumber { get; set; }

        /// <summary>
        /// 最大派遣数
        /// </summary>
        [JsonPropertyName("max_expedition_num")]
        public int MaxExpeditionNumber { get; set; }

        /// <summary>
        /// 探索派遣
        /// </summary>
        [JsonPropertyName("expeditions"), NotMapped]
        public List<Expedition> Expeditions { get; set; }

        /// <summary>
        /// 当前洞天宝钱
        /// </summary>
        [JsonPropertyName("current_home_coin")]
        public int CurrentHomeCoin { get; set; }

        /// <summary>
        /// 最大洞天宝钱
        /// </summary>
        [JsonPropertyName("max_home_coin")]
        public int MaxHomeCoin { get; set; }

        /// <summary>
        /// 洞天宝钱恢复时间
        /// </summary>
        [JsonPropertyName("home_coin_recovery_time"), JsonConverter(typeof(RemainedTimeJsonConverter))]
        public TimeSpan HomeCoinRecoveryTime { get; set; }

        [JsonIgnore, NotMapped]
        public bool IsHomeCoinFull => HomeCoinRecoveryTime == TimeSpan.Zero;

        [JsonIgnore, NotMapped]
        public DateTimeOffset HomeCoinFullTime => Time + HomeCoinRecoveryTime;

        [JsonIgnore, NotMapped]
        public string HomeCoinRecoveryDayString
        {
            get
            {
                var days = (HomeCoinFullTime.Date - Time.Date).Days;
                var dayofweek = HomeCoinFullTime.DayOfWeek;
                return (days, (int)dayofweek) switch
                {
                    (0, _) => "今日",
                    (1, _) => "明日",
                    ( > 1, 1) => "周一",
                    ( > 1, 2) => "周二",
                    ( > 1, 3) => "周三",
                    ( > 1, 4) => "周四",
                    ( > 1, 5) => "周五",
                    ( > 1, 6) => "周六",
                    ( > 1, 7) => "周日",
                    _ => "",
                };
            }
        }



        public DailyNoteInfo Copy()
        {
            var info = (DailyNoteInfo)MemberwiseClone();
            info.Expeditions = new List<Expedition>(Expeditions);
            return info;
        }




    }

    public class Expedition
    {
        /// <summary>
        /// 角色侧面图
        /// </summary>
        [JsonPropertyName("avatar_side_icon")]
        public string AvatarSideIcon { get; set; }

        /// <summary>
        /// 状态 Ongoing:派遣中 Finished:已完成
        /// </summary>
        [JsonPropertyName("status")]
        public string Status { get; set; }

        /// <summary>
        /// 剩余时间
        /// </summary>
        [JsonPropertyName("remained_time"), JsonConverter(typeof(RemainedTimeJsonConverter))]
        public TimeSpan RemainedTime { get; set; }

        [JsonIgnore, NotMapped]
        public bool IsFinished => RemainedTime == TimeSpan.Zero;

        [JsonIgnore, NotMapped]
        public DateTimeOffset FinishedTime { get; set; }

        [JsonIgnore, NotMapped]
        public string FinishedDayString { get; set; }

    }


    public class RemainedTimeJsonConverter : JsonConverter<TimeSpan>
    {
        public override TimeSpan Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            var str = reader.GetString();
            if (string.IsNullOrWhiteSpace(str))
            {
                return TimeSpan.Zero;
            }
            else
            {
                return TimeSpan.FromSeconds(int.Parse(str));
            }
        }

        public override void Write(Utf8JsonWriter writer, TimeSpan value, JsonSerializerOptions options)
        {
            writer.WriteStringValue(value.TotalSeconds.ToString());
        }
    }



}
