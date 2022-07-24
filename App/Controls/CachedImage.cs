using CommunityToolkit.WinUI.UI;
using CommunityToolkit.WinUI.UI.Controls;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Media.Imaging;
using System.Threading;

namespace Xunkong.Desktop.Controls;

internal class CachedImage : ImageEx
{



    static CachedImage()
    {
        FileCache.Instance.CacheDuration = TimeSpan.FromDays(30);
        FileCache.Instance.RetryCount = 3;
    }


    protected override async Task<ImageSource> ProvideCachedResourceAsync(Uri imageUri, CancellationToken token)
    {
        try
        {
            if (imageUri.Scheme is "file" or "ms-appx")
            {
                return new BitmapImage(imageUri);
            }
            var file = await FileCache.Instance.GetFromCacheAsync(imageUri, false, token);
            if (token.IsCancellationRequested)
            {
                throw new TaskCanceledException("Image source has changed.");
            }
            return new BitmapImage(new Uri(file.Path));
        }
        catch (TaskCanceledException)
        {
            throw;
        }
        catch
        {
            await FileCache.Instance.RemoveAsync(new[] { imageUri });
            throw;
        }

    }


}
