using System.ComponentModel;

namespace Xunkong.Core.XunkongApi
{

    [Flags]
    public enum ChannelType
    {

        [Description("正式版")]
        Stable = 1,


        [Description("预览版")]
        Preview = 2,


        [Description("开发版")]
        Development = 4,


        All = 7,

    }
}
