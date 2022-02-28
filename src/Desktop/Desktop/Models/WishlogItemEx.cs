using Xunkong.Core.Wish;

namespace Xunkong.Desktop.Models
{

    public class WishlogItemEx : WishlogItem
    {

        public int Index { get; set; }


        public int GuaranteeIndex { get; set; }


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
