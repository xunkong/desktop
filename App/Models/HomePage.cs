using Xunkong.Hoyolab.Wiki;

namespace Xunkong.Desktop.Models;

public class HomePage_DayData
{
    public int Month { get; set; }

    public int DayOfMonth { get; set; }

    public string DayOfWeekName { get; set; }

    public List<HomePage_MaterialData> Data { get; set; }
}



public class HomePage_MaterialData
{
    public string Name { get; set; }

    public string Icon { get; set; }

    public List<TalentCalendar> CharacterOrWeapon { get; set; }

}
