using Mapster;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunkong.Core.Metadata;

namespace Xunkong.Desktop.Models
{
    public class WishEventStatsItemInfoModel
    {


        public string Name { get; set; }


        public int Rarity { get; set; }


        public string Icon { get; set; }


        public string GachaCard { get; set; }


        public ElementType Element { get; set; }


        public WeaponType WeaponType { get; set; }


        public int Count { get; set; }


        public bool Any => Count > 0;



    }
}
