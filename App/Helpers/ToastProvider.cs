using CommunityToolkit.WinUI.Notifications;

namespace Xunkong.Desktop.Helpers;

internal static class ToastProvider
{

    public static void Send(string title, string? message = null)
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
        finally
        {
            Task.Delay(100);
        }
    }

    public static async Task SendAsync(string title, string? message = null)
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
        finally
        {
            await Task.Delay(100);
        }
    }

}
