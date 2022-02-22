using System.Xml.Linq;

namespace Xunkong.Core.Metadata
{
    [Table("Info_Character")]
    [Index(nameof(Name))]
    [Index(nameof(Rarity))]
    [Index(nameof(Gender))]
    [Index(nameof(Element))]
    [Index(nameof(WeaponType))]
    [Index(nameof(NameTextMapHash))]
    public class CharacterInfo
    {
        public int Id { get; set; }

        [MaxLength(255)]
        public string? Name { get; set; }

        public bool Enable { get; set; }

        public long? NameTextMapHash { get; set; }

        public long? DescTextMapHash { get; set; }

        [MaxLength(255)]
        public string? Title { get; set; }

        public string? Description { get; set; }

        public int Rarity { get; set; }

        [MaxLength(255)]
        public string? Gender { get; set; }

        /// <summary>
        /// 所属势力
        /// </summary>
        [MaxLength(255)]
        public string? Affiliation { get; set; }

        [JsonConverter(typeof(JsonStringEnumConverter))]
        public ElementType Element { get; set; }

        [JsonConverter(typeof(JsonStringEnumConverter))]
        public WeaponType WeaponType { get; set; }

        /// <summary>
        /// MM/dd
        /// </summary>
        [MaxLength(255)]
        public string? Birthday { get; set; }

        public string? Card { get; set; }

        public string? Portrait { get; set; }

        public string? FaceIcon { get; set; }

        public string? SideIcon { get; set; }

        public string? GachaCard { get; set; }

        public string? GachaSplash { get; set; }

        public string? AvatarIcon { get; set; }

        [MaxLength(255)]
        public string? ConstllationName { get; set; }


        /// <summary>
        /// 命之座
        /// </summary>
        public List<ConstellationInfo>? Constellations { get; set; }



    }
}
