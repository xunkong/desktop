namespace Xunkong.Core.XunkongApi
{

    public class NotificationModelBase
    {

        public int Id { get; set; }

        public DateTimeOffset Time { get; set; }

        [MaxLength(255)]
        public string Title { get; set; }

        [MaxLength(255)]
        public string? Category { get; set; }

        public ContentType ContentType { get; set; }

        public string Content { get; set; }


    }



    public enum ContentType
    {

        Text,

        HtmlDialog,

        HtmlPage,

        Url,

    }


}
