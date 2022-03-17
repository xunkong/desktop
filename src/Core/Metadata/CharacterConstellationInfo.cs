namespace Xunkong.Core.Metadata
{

    /// <summary>
    /// 命之座
    /// </summary>
    [Table("Info_Character_Constellation")]
    public class CharacterConstellationInfo
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

       

    }
}
