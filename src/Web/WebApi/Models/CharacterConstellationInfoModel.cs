using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Xunkong.Core.Metadata;

namespace Xunkong.Web.Api.Models
{

    [Table("Info_Character_Constellation")]
    public class CharacterConstellationInfoModel
    {
        public int Id { get; set; }

        public int ConstellationId { get; set; }

        public int CharacterInfoId { get; set; }

        [MaxLength(255)]
        public string? Name { get; set; }

        public string? Description { get; set; }

        public string? Icon { get; set; }

        /// <summary>
        /// 上一个命星的id
        /// </summary>
        public int PreviewConstellationId { get; set; }

        public long NameTextMapHash { get; set; }

        public long DescTextMapHash { get; set; }

    }
}
