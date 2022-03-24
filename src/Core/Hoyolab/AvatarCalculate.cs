namespace Xunkong.Core.Hoyolab;

public class AvatarCalculate
{

    [JsonPropertyName("skill_list")]
    public List<AvatarSkill> Skills { get; set; }


    //[JsonPropertyName("weapon")]
    //public Weapon? Weapon { get; set; }


    //[JsonPropertyName("reliquary_list")]
    //public List<Reliquary>? ReliquaryList { get; set; }

}


public class AvatarSkill
{
    [JsonPropertyName("id")]
    public int Id { get; set; }

    [JsonPropertyName("group_id")]
    public int GroupId { get; set; }

    [JsonPropertyName("name")]
    public string Name { get; set; }

    [JsonPropertyName("icon")]
    public string Icon { get; set; }

    [JsonPropertyName("max_level")]
    public int MaxLevel { get; set; }

    [JsonPropertyName("level_current")]
    public int CurrentLevel { get; set; }

}