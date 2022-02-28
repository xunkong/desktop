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
