namespace Xunkong.Web.Api.Models
{
    public class ReadableModel
    {
        public int Id { get; set; }

        public string? Title { get; set; }

        public string? Content { get; set; }

        public long TitleTextMapHash { get; set; }

        public int ContentId { get; set; }

    }
}
