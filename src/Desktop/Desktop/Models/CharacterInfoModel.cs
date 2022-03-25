using Xunkong.Core.Hoyolab;
using Xunkong.Core.Metadata;

namespace Xunkong.Desktop.Models;

internal class CharacterInfo_Character : CharacterInfo
{

    public bool IsOwn { get; set; }

    public int Level { get; set; }

    public int Fetter { get; set; }

    public int ActivedConstellationNumber { get; set; }

    public bool IsGettingSkillLevel { get; set; }

    public bool GotSkillLevel { get; set; }

    public new List<CharacterInfo_Talent> Talents { get; set; }

    public new List<CharacterInfo_Constellation> Constellations { get; set; }

    public CharacterInfo_Weapon Weapon { get; set; }

    public List<AvatarReliquary> Reliquaries { get; set; }

    public List<WishlogItemEx>? Wishlogs { get; set; }

}



[INotifyPropertyChanged]
internal partial class CharacterInfo_Talent : CharacterTalentInfo
{


    private int _CurrentLevel;
    public int CurrentLevel
    {
        get => _CurrentLevel;
        set => SetProperty(ref _CurrentLevel, value);
    }

    private bool _ShowSkillLevel;
    public bool ShowSkillLevel
    {
        get => _ShowSkillLevel;
        set => SetProperty(ref _ShowSkillLevel, value);
    }

}


internal class CharacterInfo_Constellation : CharacterConstellationInfo
{

    public bool IsActived { get; set; }

}


internal class CharacterInfo_Weapon : AvatarWeapon
{

    public string GachaIcon { get; set; }

}