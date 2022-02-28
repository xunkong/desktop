#pragma warning disable CS8618


namespace Xunkong.Core.Wish
{
    [Table("Wishlog_Authkeys")]
    public class WishlogAuthkeyItem
    {
        [Key, MaxLength(4096)]
        public string Url { get; set; }

        public int Uid { get; set; }

        public DateTimeOffset DateTime { get; set; }

    }
}
