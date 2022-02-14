using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunkong.Core.Wish;

namespace Xunkong.Desktop.Models
{

    public class WishlogItemEditableModel : WishlogItem
    {

        public int Index { get; set; }


        public int GuaranteeIndex { get; set; }


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
