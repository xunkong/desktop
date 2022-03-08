using MiniExcelLibs.Attributes;
using Xunkong.Core.Wish;

namespace Xunkong.Desktop.Models
{
    internal class WishlogItemExcelModel
    {

        /// <summary>
        /// 用户Uid
        /// </summary>
        [ExcelColumnName("uid")]
        public int Uid { get; set; }

        /// <summary>
        /// 祈愿类型（卡池类型）
        /// </summary>
        [ExcelColumnName("gacha_type")]
        public WishType WishType { get; set; }

        /// <summary>
        /// 时间
        /// </summary>
        public DateTimeOffset Time => GetTime();

        /// <summary>
        /// 字符串形式的时间，时区与账号服务器所在地理位置相关
        /// </summary>
        [ExcelColumnName("time")]
        public string _TimeString { get; set; }

        /// <summary>
        /// 物品名称
        /// </summary>
        [ExcelColumnName("name")]
        public string Name { get; set; }

        /// <summary>
        /// 语言（如zh-cn）
        /// </summary>
        [ExcelColumnName("lang")]
        public string Language { get; set; }

        /// <summary>
        /// 物品类型（角色、武器）
        /// </summary>
        [ExcelColumnName("item_type")]
        public string ItemType { get; set; }

        /// <summary>
        /// 星级
        /// </summary>
        [ExcelColumnName("rank_type")]
        public int RankType { get; set; }

        /// <summary>
        /// 祈愿Id，这个值很重要，全服唯一
        /// </summary>
        [ExcelColumnName("id")]
        public long Id { get; set; }


        /// <summary>
        /// 查询类型，此值为自行添加的值
        /// </summary>
        [ExcelColumnName("query_type")]
        public WishType QueryType => WishType switch
        {
            WishType.CharacterEvent_2 => WishType.CharacterEvent,
            _ => WishType,
        };



        public DateTimeOffset GetTime()
        {
            var offset = Uid.ToString()[0] switch
            {
                '6' => -5,
                '7' => 1,
                _ => 8,
            };
            if (DateTime.TryParse(_TimeString, out var time))
            {
                //var time = DateTime.Parse(_TimeString);
                return new DateTimeOffset(time, TimeSpan.FromHours(offset));
            }
            else
            {
                return default;
            }
        }


    }
}
