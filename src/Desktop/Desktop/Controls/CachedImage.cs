using CommunityToolkit.WinUI.UI;
using CommunityToolkit.WinUI.UI.Controls;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Media.Imaging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Xunkong.Desktop.Controls
{
    internal class CachedImage : ImageEx
    {




        public bool EnableMemoryCache
        {
            get { return (bool)GetValue(EnableMemoryCacheProperty); }
            set { SetValue(EnableMemoryCacheProperty, value); }
        }

        // Using a DependencyProperty as the backing store for EnableMemoryCache.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty EnableMemoryCacheProperty =
            DependencyProperty.Register("EnableMemoryCache", typeof(bool), typeof(CachedImage), new PropertyMetadata(false));




        static CachedImage()
        {
            ImageCache.Instance.CacheDuration = TimeSpan.FromDays(7);
            ImageCache.Instance.RetryCount = 3;
            ImageCache.Instance.MaxMemoryCacheCount = 100;
        }


        protected override async Task<ImageSource> ProvideCachedResourceAsync(Uri imageUri, CancellationToken token)
        {
            var image = await ImageCache.Instance.GetFromCacheAsync(imageUri, false, token);
            if (image == null)
            {
                await ImageCache.Instance.RemoveAsync(new[] { imageUri });
                image = await ImageCache.Instance.GetFromCacheAsync(imageUri, false, token);
            }
            return image;
        }


    }
}
