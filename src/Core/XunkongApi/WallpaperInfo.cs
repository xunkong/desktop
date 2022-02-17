using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Xunkong.Core.XunkongApi
{
    [Table("Wallpapers")]
    public class WallpaperInfo
    {

        public int Id { get; set; }

        public bool Enable { get; set; }

        [MaxLength(255)]
        public string? Title { get; set; }

        [MaxLength(255)]
        public string? Author { get; set; }

        public string? Description { get; set; }

        public string Url { get; set; }

        public string? Source { get; set; }

        [MaxLength(255)]
        public string? FileName { get; set; }

        [JsonIgnore, NotMapped]
        public string SourceDomain => new Uri(Source ?? "").Host;


    }
}
