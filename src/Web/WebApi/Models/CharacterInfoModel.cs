using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;
using Xunkong.Core.Metadata;

namespace Xunkong.Web.Api.Models
{
    [Table("Info_Character")]
    [Index(nameof(Name))]
    [Index(nameof(Rarity))]
    [Index(nameof(Gender2))]
    [Index(nameof(Element))]
    [Index(nameof(WeaponType))]
    public class CharacterInfoModel
    {

        public int Id { get; set; }

        [MaxLength(255)]
        public string? Name { get; set; }

        public bool Enable { get; set; }

        [MaxLength(255)]
        public string? Title { get; set; }

        public string? Description { get; set; }

        public int Rarity { get; set; }

        [MaxLength(255)]
        public string? Gender { get; set; }

        public int Gender2 { get; set; }

        /// <summary>
        /// 所属势力
        /// </summary>
        [MaxLength(255)]
        public string? Affiliation { get; set; }

        [MaxLength(255)]
        public string? ConstllationName { get; set; }

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


        [MaxLength(255)]
        public string? CvChinese { get; set; }

        [MaxLength(255)]
        public string? CvJapanese { get; set; }
        
        [MaxLength(255)]
        public string? CvEnglish { get; set; }

        [MaxLength(255)]
        public string? CvKorean { get; set; }

        /// <summary>
        /// 天赋
        /// </summary>
        [ForeignKey("CharacterInfoId")]
        public List<CharacterTalentInfoModel>? Talents { get; set; }


        /// <summary>
        /// 命之座
        /// </summary>
        [ForeignKey("CharacterInfoId")]
        public List<CharacterConstellationInfoModel>? Constellations { get; set; }


        public long NameTextMapHash { get; set; }

        public long TitleTextMapHash { get; set; }

        public long DescTextMapHash { get; set; }

        public long AffiliationTextMapHash { get; set; }

        public long ConstllationTextMapHash { get; set; }

        public long CvChineseTextMapHash { get; set; }

        public long CvJapaneseTextMapHash { get; set; }

        public long CvEnglishTextMapHash { get; set; }

        public long CvKoreanTextMapHash { get; set; }



    }
}
