using System.Security.Cryptography;
using System.Text;
using Windows.ApplicationModel;
using Xunkong.Core;

namespace Xunkong.Desktop.Models;

internal static class XunkongEnvironment
{


    public static readonly string AppName;

    public static readonly string PackageId;

    public static readonly PlatformType Platform;

    public static readonly ChannelType Channel;

    public static readonly Version AppVersion;

    public static readonly string DeviceId;

    public static readonly string UserDataPath;

    public static readonly string? WebView2Version;


    static XunkongEnvironment()
    {
        AppName = Package.Current.DisplayName;
        PackageId = Package.Current.Id.FullName;
        var v = Package.Current.Id.Version;
        AppVersion = new Version(v.Major, v.Minor, v.Build, v.Revision);
        Platform = PlatformType.Desktop;
        Channel = GetChannel();

        var UserName = Environment.UserName;
        var MachineGuid = Microsoft.Win32.Registry.GetValue(@"HKEY_LOCAL_MACHINE\SOFTWARE\Microsoft\Cryptography\", "MachineGuid", UserName);
        var bytes = Encoding.UTF8.GetBytes(UserName + MachineGuid);
        var hash = MD5.HashData(bytes);
        DeviceId = Convert.ToHexString(hash);

        UserDataPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "Xunkong");
        try
        {
            WebView2Version = Microsoft.Web.WebView2.Core.CoreWebView2Environment.GetAvailableBrowserVersionString();
        }
        catch { }
    }


    public static ChannelType GetChannel()
    {
        if (AppName.Contains("Dev") || AppName.Contains("开发"))
        {
            return ChannelType.Development;
        }
        if (AppName.Contains("Pre") || AppName.Contains("预览"))
        {
            return ChannelType.Preview;
        }
        return ChannelType.Stable;
    }


}
