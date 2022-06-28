using Windows.ApplicationModel.Background;
using Windows.UI.StartScreen;

namespace Xunkong.Desktop.BackgroundTask
{
    public sealed class UpdatedTask : IBackgroundTask
    {
        public async void Run(IBackgroundTaskInstance taskInstance)
        {
            BackgroundTaskDeferral deferral = taskInstance.GetDeferral();
            var jumpList = await JumpList.LoadCurrentAsync();
            jumpList.Items.Clear();
            await jumpList.SaveAsync();
            deferral.Complete();
        }
    }
}
