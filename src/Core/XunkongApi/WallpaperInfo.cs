namespace Xunkong.Core.XunkongApi
{
    [Table("Wallpapers")]
    [Index(nameof(Enable))]
    [Index(nameof(Recommend))]
    public class WallpaperInfo
    {

        public int Id { get; set; }

        public bool Enable { get; set; }

        public bool Recommend { get; set; }

        [MaxLength(255)]
        public string? Title { get; set; }

        [MaxLength(255)]
        public string? Author { get; set; }

        public string? Description { get; set; }

        public List<string> Tags { get; set; }

        public string Url { get; set; }

        public string? Source { get; set; }


        [MaxLength(255)]
        public string? FileName { get; set; }

        [JsonIgnore, NotMapped]
        public string SourceDomain => new Uri(Source ?? "").Host;


    }



    public record WallpaperInfoList(int Page, int TotalPage, int Count, List<WallpaperInfo> List);




}
