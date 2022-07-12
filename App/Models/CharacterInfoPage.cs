using Xunkong.GenshinData.Character;
using Xunkong.Hoyolab.Avatar;

namespace Xunkong.Desktop.Models;

public class CharacterInfoPage_Character : CharacterInfo
{

    public bool IsOwn { get; set; }

    public int Level { get; set; }

    public int Fetter { get; set; }

    public int ActivedConstellationNumber { get; set; }

    public bool IsGettingSkillLevel { get; set; }

    public bool GotSkillLevel { get; set; }

    public new List<CharacterInfoPage_Talent> Talents { get; set; }

    public new List<CharacterInfoPage_Constellation> Constellations { get; set; }

    public CharacterInfoPage_Weapon Weapon { get; set; }

    public List<AvatarReliquary> Reliquaries { get; set; }

    public List<WishlogItemEx>? Wishlogs { get; set; }

}



[INotifyPropertyChanged]
public partial class CharacterInfoPage_Talent : CharacterTalentInfo
{

    [ObservableProperty]
    private int _CurrentLevel;


    [ObservableProperty]
    private bool _ShowSkillLevel;

}


public class CharacterInfoPage_Constellation : CharacterConstellationInfo
{

    public bool IsActived { get; set; }

}


public class CharacterInfoPage_Weapon : AvatarWeapon
{

    public string AwakenIcon { get; set; }

}
