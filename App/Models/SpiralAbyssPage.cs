using Xunkong.Hoyolab.SpiralAbyss;

namespace Xunkong.Desktop.Models;

public class SpiralAbyssPage_LeftPanel
{
    public int Id { get; set; }

    public int Uid { get; set; }

    public int ScheduleId { get; set; }

    public DateTimeOffset StartTime { get; set; }

    public DateTimeOffset EndTime { get; set; }

    public int TotalBattleCount { get; set; }

    public string? MaxFloor { get; set; }

    public int TotalStar { get; set; }

    public string TimeName => GetTimeName();

    private string GetTimeName()
    {
        var middleTime = StartTime + (EndTime - StartTime) / 2;
        return $"{middleTime.Year}年{middleTime.Month}月{(middleTime.Day < 15 ? "上" : "下")}";
    }
}


public class SpiralAbyssPage_AbyssInfo
{
    public int Id { get; set; }

    public int Uid { get; set; }

    public int ScheduleId { get; set; }

    public DateTimeOffset StartTime { get; set; }

    public DateTimeOffset EndTime { get; set; }

    public int TotalBattleCount { get; set; }

    public string? MaxFloor { get; set; }

    public int TotalStar { get; set; }

    /// <summary>
    /// 出战最多
    /// </summary>
    public List<SpiralAbyssRank> RevealRank { get; set; }

    /// <summary>
    /// 击破最多
    /// </summary>
    public SpiralAbyssRank DefeatRank { get; set; }

    /// <summary>
    /// 伤害最高
    /// </summary>
    public SpiralAbyssRank DamageRank { get; set; }

    /// <summary>
    /// 承伤最高
    /// </summary>
    public SpiralAbyssRank TakeDamageRank { get; set; }

    /// <summary>
    /// 元素战技最多
    /// </summary>
    public SpiralAbyssRank NormalSkillRank { get; set; }

    /// <summary>
    /// 元素爆发最多
    /// </summary>
    public SpiralAbyssRank EnergySkillRank { get; set; }


    public List<SpiralAbyssFloor> Floors { get; set; }

}
