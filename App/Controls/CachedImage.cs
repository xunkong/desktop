using CommunityToolkit.WinUI.UI;
using CommunityToolkit.WinUI.UI.Controls;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Media.Imaging;
using System.Threading;

namespace Xunkong.Desktop.Controls;

internal class CachedImage : ImageEx
{

    private int _id;


    static CachedImage()
    {
        ImageCache.Instance.CacheDuration = TimeSpan.FromDays(30);
        ImageCache.Instance.RetryCount = 3;
    }


    protected override async Task<ImageSource> ProvideCachedResourceAsync(Uri imageUri, CancellationToken token)
    {
        var id = ++_id;
        if (imageUri.Scheme is "file" or "ms-appx")
        {
            return new BitmapImage(imageUri);
        }
        var image = await ImageCache.Instance.GetFromCacheAsync(imageUri, false, token);
        if (image == null)
        {
            await ImageCache.Instance.RemoveAsync(new[] { imageUri });
            image = await ImageCache.Instance.GetFromCacheAsync(imageUri, false, token);
        }
        if (id == _id)
        {
            return image;
        }
        else
        {
            throw new TaskCanceledException("Image source has changed.");
        }
    }


}
