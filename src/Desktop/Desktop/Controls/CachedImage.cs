using CommunityToolkit.WinUI.UI;
using CommunityToolkit.WinUI.UI.Controls;
using Microsoft.UI.Xaml.Media;

namespace Xunkong.Desktop.Controls
{
    internal class CachedImage : ImageEx
    {

        [ThreadStatic]
        private int randomId;


        static CachedImage()
        {
            ImageCache.Instance.CacheDuration = TimeSpan.FromDays(7);
            ImageCache.Instance.RetryCount = 3;
        }


        protected override async Task<ImageSource> ProvideCachedResourceAsync(Uri imageUri, CancellationToken token)
        {
            var id = Random.Shared.Next();
            randomId = id;
            var image = await ImageCache.Instance.GetFromCacheAsync(imageUri, false, token);
            if (image == null)
            {
                await ImageCache.Instance.RemoveAsync(new[] { imageUri });
                image = await ImageCache.Instance.GetFromCacheAsync(imageUri, false, token);
            }
            if (randomId == id)
            {
                return image;
            }
            else
            {
                throw new TaskCanceledException("Image source has changed.");
            }
        }


    }
}
