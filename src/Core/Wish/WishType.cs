using System.ComponentModel;

namespace Xunkong.Core.Wish
{
    /// <summary>
    /// 祈愿类型
    /// </summary>
    public enum WishType
    {

        /// <summary>
        /// 新手祈愿
        /// </summary>
        [Description("新手祈愿")]
        Novice = 100,

        /// <summary>
        /// 常驻祈愿
        /// </summary>
        [Description("常驻祈愿")]
        Permanent = 200,

        /// <summary>
        /// 角色活动祈愿
        /// </summary>
        [Description("角色活动祈愿")]
        CharacterEvent = 301,

        /// <summary>
        /// 武器活动祈愿
        /// </summary>
        [Description("武器活动祈愿")]
        WeaponEvent = 302,

        /// <summary>
        /// 角色活动祈愿-2
        /// </summary>
        [Description("角色活动祈愿-2")]
        CharacterEvent_2 = 400,
    }
}
