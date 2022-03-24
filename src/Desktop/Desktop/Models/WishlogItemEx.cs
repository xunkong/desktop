using Xunkong.Core.Wish;

namespace Xunkong.Desktop.Models
{

    public class WishlogItemEx : WishlogItem
    {

        public int Index { get; set; }


        public int GuaranteeIndex { get; set; }


        public string Version { get; set; }


        public string WishEventName { get; set; }


        public string VersionAndName => string.IsNullOrWhiteSpace(Version) ? WishEventName : $"{Version} {WishEventName}";


        public bool IsUp { get; set; }


        public EditState EditState { get; set; }

    }



    public enum EditState
    {

        None,

        Add,

        Update,

        Delete,

    }


}
