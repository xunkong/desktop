using CommunityToolkit.WinUI.Notifications;
using Serilog;

namespace Xunkong.Desktop.Extension
{
    internal static class NotificationHelper
    {

        public static void SendNotification(string title, string? message = null)
        {
            try
            {
                var tb = new ToastContentBuilder();
                tb.AddText(title);
                if (!string.IsNullOrWhiteSpace(message))
                {
                    tb.AddText(message);
                }
                tb.AddAttributionText(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"));
                tb.Show();
            }
            catch (Exception ex)
            {
                Log.Information("The following notification message had not sent.");
                Log.Information($"{title}\n{message}");
                Log.Error(ex, "Send notification toast.");
            }

        }
    }
}
