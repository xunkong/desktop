using System.Text;
using Windows.Storage;

namespace Xunkong.Desktop.Helpers;

internal static class CacheHelper
{


    private static string _cacheBaseDir = ApplicationData.Current.TemporaryFolder.Path;


    public static string GetCacheFileName(string url)
    {
        byte[] utf8 = Encoding.UTF8.GetBytes(url);

        ulong value = (ulong)utf8.Length;
        for (int n = 0; n < utf8.Length; n++)
        {
            value += (ulong)utf8[n] << ((n * 5) % 56);
        }

        return value.ToString();
    }




    public static string GetCacheFilePath(string url, string cacheFolderName = "ImageCache")
    {
        return Path.Combine(_cacheBaseDir, cacheFolderName, GetCacheFileName(url));
    }


}
