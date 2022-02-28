namespace Xunkong.Core.XunkongApi
{
    public class NotificationWrapper<T> where T : NotificationModelBase
    {

        public PlatformType Platform { get; set; }

        public Version? Version { get; set; }

        public ChannelType Channel { get; set; }

        public List<T> List { get; set; }

    }
}
