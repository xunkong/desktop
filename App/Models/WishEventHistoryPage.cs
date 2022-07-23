using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Xunkong.Desktop.Models;


public class WishEventHistoryPage_TopHeader
{
    public int Rarity { get; set; }
    public string Icon { get; set; }
}


public class WishEventHistoryPage_LeftHeader
{
    public string Version { get; set; }

    public DateTimeOffset StartTime { get; set; }

    public DateTimeOffset EndTime { get; set; }

    public List<string> UpItems { get; set; }

}




public class WishEventHistoryPage_Column
{
    public string Name { get; set; }

    public string Icon { get; set; }

    public int Rarity { get; set; }

    public List<WishEventHistoryPage_Cell> Cells { get; set; } = new();

    public int CurrentIndex { get; set; } = -1;

    public ElementType Element { get; set; }

}


public class WishEventHistoryPage_Cell
{
    public int Index { get; set; }

    public string IndexString => Index > 0 ? Index.ToString() : "";

    public string Icon { get; set; }

    public double Alpha { get; set; }

    public int Raity { get; set; }

    public ElementType Element { get; set; }
}