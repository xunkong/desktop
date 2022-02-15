using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Xunkong.Core.Wish
{
    public class WishlogBackupModel
    {

        public string ExportApp { get; set; }


        public string AppVersion { get; set; }


        public DateTimeOffset ExportTime { get; set; }


        public int Uid { get; set; }


        public int WishlogCount { get; set; }


        public DateTimeOffset? FirstItemTime { get; set; }


        public DateTimeOffset? LastItemTime { get; set; }


        public List<WishlogItem> WishlogList { get; set; }


    }
}
