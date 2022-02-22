namespace Xunkong.Core.Metadata
{
    [Table("Info_Weapon")]
    public class WeaponInfo
    {

        public int Id { get; set; }

        [MaxLength(255)]
        public string? Name { get; set; }

        public bool Enable { get; set; }

        public string? Description { get; set; }

        public long NameTextMapHash { get; set; }

        public long? DescTextMapHash { get; set; }

        public int Rarity { get; set; }

        [JsonConverter(typeof(JsonStringEnumConverter))]
        public WeaponType WeaponType { get; set; }

        public string? Icon { get; set; }

        public string? AwakenIcon { get; set; }

        public string? GachaIcon { get; set; }

    }
}
