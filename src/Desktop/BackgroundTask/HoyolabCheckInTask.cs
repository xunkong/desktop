using Windows.ApplicationModel;
using Windows.ApplicationModel.Background;

namespace Xunkong.Desktop.BackgroundTask
{
    public sealed class HoyolabCheckInTask : IBackgroundTask
    {

        public async void Run(IBackgroundTaskInstance taskInstance)
        {
            BackgroundTaskDeferral deferral = taskInstance.GetDeferral();
            await FullTrustProcessLauncher.LaunchFullTrustProcessForCurrentAppAsync("HoyolabCheckInTask");
            deferral.Complete();
        }
    }
}
