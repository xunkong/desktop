using System;
using Windows.ApplicationModel;
using Windows.ApplicationModel.Background;

namespace Xunkong.Desktop.Background;

public sealed class DailyNoteTask : IBackgroundTask
{


    public async void Run(IBackgroundTaskInstance taskInstance)
    {
        BackgroundTaskDeferral deferral = taskInstance.GetDeferral();
        await FullTrustProcessLauncher.LaunchFullTrustProcessForCurrentAppAsync("DailyNoteTask");
        deferral.Complete();
    }


}
