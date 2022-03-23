using System.ComponentModel.DataAnnotations.Schema;

namespace Xunkong.Web.Api.Models
{
    [Table("Info_Weapon_Skill")]
    public class WeaponSkillModel
    {
        public int Id { get; set; }

        public int WeaponInfoId { get; set; }

        public int Level { get; set; }

        public string? Name { get; set; }

        public string? Description { get; set; }

        public long NameTextMapHash { get; set; }

        public long DescTextMapHash { get; set; }

    }
}
