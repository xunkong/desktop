namespace Xunkong.Core.TravelRecord
{
    public class TravelRecordDetail : TravelRecordBase, IJsonOnDeserialized
    {

        [JsonPropertyName("page")]
        public int Page { get; set; }


        [JsonPropertyName("list")]
        public List<TravelRecordAwardItem> List { get; set; }


        /// <summary>
        /// 反序列化后<see cref="TravelRecordAwardItem.Type"/>没有赋值
        /// </summary>
        public void OnDeserialized()
        {
            if (List is null)
            {
                return;
            }
            var year = List[0].Time.Year;
            var month = List[0].Time.Month;
            foreach (var item in List)
            {
                item.Uid = Uid;
                item.Year = year;
                item.Month = month;
            }
        }
    }
}
