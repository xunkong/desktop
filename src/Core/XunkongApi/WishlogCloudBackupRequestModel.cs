using Xunkong.Core.Wish;

namespace Xunkong.Core.XunkongApi
{
    public class WishlogCloudBackupRequestModel
    {

        public int Uid { get; set; }


        public string Url { get; set; }


        /// <summary>
        /// 获取记录时本地最后一项的Id
        /// </summary>
        public long LastId { get; set; }

        /// <summary>
        /// 上传时使用，其他操作为空
        /// </summary>
        public IEnumerable<WishlogItem>? List { get; set; }

    }
}
