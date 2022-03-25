namespace Xunkong.Core.Metadata
{
    [Table("Info_Weapon_Skill")]
    public class WeaponSkill
    {
        public int Id { get; set; }

        public int WeaponInfoId { get; set; }

        public int Level { get; set; }

        public string? Name { get; set; }

        public string? Description { get; set; }
    }
}
