using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Xunkong.Core.XunkongApi
{

    [Flags]
    public enum ChannelType
    {

        [Description("稳定版")]
        Stable = 1,


        [Description("预览版")]
        Preview = 2,


        [Description("开发版")]
        Development = 4,


        All = 7,

    }
}
