using CommunityToolkit.WinUI.UI;
using CommunityToolkit.WinUI.UI.Controls;
using Microsoft.UI.Xaml.Media;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Xunkong.Desktop.Controls
{
    internal class CachedImage : ImageEx
    {


        static CachedImage()
        {
            ImageCache.Instance.CacheDuration = TimeSpan.FromDays(7);
        }


        protected override async Task<ImageSource> ProvideCachedResourceAsync(Uri imageUri, CancellationToken token)
        {
            return await ImageCache.Instance.GetFromCacheAsync(imageUri, false, token);
        }
    }
}
