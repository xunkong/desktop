using System.ComponentModel;
using System.Runtime.CompilerServices;
using Xunkong.Hoyolab.Wishlog;

namespace Xunkong.Desktop.Models;



public record WishlogSummaryPage_ItemThumb(string? Name, int Rarity, ElementType Element, WeaponType WeaponType, int Count, string? Icon, DateTimeOffset LastTime);



public record WishlogSummaryPage_QueryTypeStats(WishType QueryType,
                                            int TotalCount,
                                            int Rank5Count,
                                            int Rank4Count,
                                            DateTimeOffset StartTime,
                                            DateTimeOffset EndTime,
                                            int UpItemCount,
                                            int CurrentGuarantee,
                                            int MaxGuarantee,
                                            int MinGuarantee,
                                            List<WishlogSummaryPage_Rank5Item> Rank5Items,
                                            bool LastRank5ItemIsUp,
                                            int LastRank5ItemGuarantee)
{

    public bool AnyData => TotalCount > 0;

    public int Rank3Count => TotalCount - Rank5Count - Rank4Count;

    public string Rank5Ratio => ((double)Rank5Count / TotalCount).ToString("P3");

    public string Rank4Ratio => ((double)Rank4Count / TotalCount).ToString("P3");

    public string Rank3Ratio => ((double)Rank3Count / TotalCount).ToString("P3");

    public string GuaranteeType => QueryType == WishType.Permanent ? "保底内" : LastRank5ItemIsUp ? "小保底内" : "大保底内";

    public int BaodiWai => QueryType == WishType.Permanent ? 0 : (Rank5Count - UpItemCount);

    public int BaodiBuwai => QueryType == WishType.Permanent ? Rank5Count : Rank5Count == 0 ? 0 : (2 * UpItemCount - Rank5Count + (LastRank5ItemIsUp ? 0 : 1));

    public string AverageRank5Count => ((TotalCount - CurrentGuarantee) / (double)Rank5Count).ToString("F2");

    public string AverageUpRank5Count => QueryType == WishType.Permanent ? "0" : ((TotalCount - CurrentGuarantee - (LastRank5ItemIsUp ? 0 : LastRank5ItemGuarantee)) / (double)UpItemCount).ToString("F2");

}



public record WishlogSummaryPage_Rank5Item(string Name, int Guarantee, DateTimeOffset Time, bool IsUp) : INotifyPropertyChanged
{
    public event PropertyChangedEventHandler? PropertyChanged;
    private void OnPropertyChanged([CallerMemberName] string propertyName = "")
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }

    public string Color { get; set; }


    private string _Foreground;
    public string Foreground
    {
        get { return _Foreground; }
        set
        {
            if (_Foreground != value)
            {
                _Foreground = value;
                OnPropertyChanged();
            }
        }
    }

}



public class WishlogSummaryPage_EventStats
{

    public string Name { get; set; }


    public string Version { get; set; }


    public DateTimeOffset StartTime { get; set; }


    public DateTimeOffset EndTime { get; set; }


    public int TotalCount { get; set; }


    public int Rank5Count { get; set; }


    public int Rank4Count { get; set; }


    public int Rank3Count { get; set; }


    public List<WishlogSummaryPage_UpItem> UpItems { get; set; }


    public List<WishlogSummaryPage_Rank5Item> Rank5Items { get; set; }

}



public class WishlogSummaryPage_UpItem
{

    public string Name { get; set; }


    public int Rarity { get; set; }


    public string Icon { get; set; }


    public ElementType Element { get; set; }


    public WeaponType WeaponType { get; set; }


    public int Count { get; set; }


    public bool AnyData => Count > 0;

}
