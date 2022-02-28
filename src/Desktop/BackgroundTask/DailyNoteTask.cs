using Windows.ApplicationModel;
using Windows.ApplicationModel.Background;

namespace Xunkong.Desktop.BackgroundTask
{
    public sealed class DailyNoteTask : IBackgroundTask
    {


        public async void Run(IBackgroundTaskInstance taskInstance)
        {
            BackgroundTaskDeferral deferral = taskInstance.GetDeferral();
            await FullTrustProcessLauncher.LaunchFullTrustProcessForCurrentAppAsync("DailyNoteTask");
            deferral.Complete();
        }


    }
}
