using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;
using Xunkong.Core.Metadata;

namespace Xunkong.Web.Api.Models
{
    [Table("Info_Weapon")]
    public class WeaponInfoModel
    {
        public int Id { get; set; }

        public bool Enable { get; set; }

        public string? Name { get; set; }

        public string? Description { get; set; }

        public int Rarity { get; set; }

        [JsonConverter(typeof(JsonStringEnumConverter))]
        public WeaponType WeaponType { get; set; }

        public string? Icon { get; set; }

        public string? AwakenIcon { get; set; }

        public string? GachaIcon { get; set; }

        public string? Story { get; set; }

        [ForeignKey("WeaponInfoId")]
        public List<WeaponSkillModel> Skills { get; set; }

        public long NameTextMapHash { get; set; }

        public long DescTextMapHash { get; set; }

        public int StoryId { get; set; }

    }
}
