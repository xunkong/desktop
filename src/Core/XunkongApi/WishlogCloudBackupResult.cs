using Xunkong.Core.Wish;

namespace Xunkong.Core.XunkongApi
{

    public class WishlogCloudBackupResult
    {

        public int Uid { get; set; }


        public int CurrentCount { get; set; }


        public int GetCount { get; set; }


        public int PutCount { get; set; }


        public int DeleteCount { get; set; }


        public List<WishlogItem>? List { get; set; }


        public WishlogCloudBackupResult(int uid, int currentCount, int getCount, int putCount, int deleteCount, List<WishlogItem>? list = null)
        {
            Uid = uid;
            CurrentCount = currentCount;
            GetCount = getCount;
            PutCount = putCount;
            DeleteCount = deleteCount;
            List = list;
        }



    }


}
