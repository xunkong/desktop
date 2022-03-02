using Xunkong.Core.Hoyolab;

namespace Xunkong.Core.TravelRecord
{
    public class TravelRecordSummary : TravelRecordBase, IJsonOnDeserialized
    {
        /// <summary>
        /// 查询月份的上个月
        /// </summary>
        [JsonPropertyName("data_last_month")]
        public int DataLastMonth { get; set; }

        /// <summary>
        /// 今日数据，查询月份不是当前月时，数据内容均为0
        /// </summary>
        [JsonPropertyName("day_data")]
        public TravelRecordDayData DayData { get; set; }

        /// <summary>
        /// 查询月数据
        /// </summary>
        [JsonPropertyName("month_data")]
        public TravelRecordMonthData MonthData { get; set; }

        /// <summary>
        /// 不知道是什么
        /// </summary>
        [JsonPropertyName("lantern")]
        public bool Lantern { get; set; }

        public void OnDeserialized()
        {
            var year = DataMonth > Date.Month ? Date.Year - 1 : Date.Year;
            MonthData.Uid = Uid;
            MonthData.Year = year;
            MonthData.Month = DataMonth;
            var groupByItems = MonthData?.PrimogemsGroupBy;
            if (groupByItems is null)
            {
                return;
            }
            foreach (var item in groupByItems)
            {
                item.Uid = Uid;
                item.Year = year;
                item.Month = DataMonth;
            }
        }
    }
}
