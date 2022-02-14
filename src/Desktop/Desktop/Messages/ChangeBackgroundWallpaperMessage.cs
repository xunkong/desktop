using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Xunkong.Desktop.Messages
{
    internal class ChangeBackgroundWallpaperMessage
    {
        public int RandomOrNext { get; set; }


        /// <summary>
        /// random is 0, next is 1
        /// </summary>
        /// <param name="randomOrNext"></param>
        public ChangeBackgroundWallpaperMessage(int randomOrNext)
        {
            RandomOrNext = randomOrNext;
        }


    }
}
