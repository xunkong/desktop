using System.Security.Cryptography;
using System.Text;
using Windows.ApplicationModel;
using Xunkong.Core;

namespace Xunkong.Desktop.Models;

internal static class XunkongEnvironment
{


    public static readonly string AppName;

    public static readonly string PackageId;

    public static readonly bool IsStoreVersion;

    public static readonly PlatformType Platform;

    public static readonly ChannelType Channel;

    public static readonly Version AppVersion;

    public static readonly string DeviceId;

    public static readonly string UserDataPath;



    static XunkongEnvironment()
    {
        AppName = Package.Current.DisplayName;
        PackageId = Package.Current.Id.FullName;
        IsStoreVersion = PackageId.StartsWith("40418Scighost");
        var v = Package.Current.Id.Version;
        AppVersion = new Version(v.Major, v.Minor, v.Build, v.Revision);
        Platform = PlatformType.Desktop;
        Channel = IsStoreVersion ? ChannelType.Store : ChannelType.Sideload;

        var UserName = Environment.UserName;
        var MachineGuid = Microsoft.Win32.Registry.GetValue(@"HKEY_LOCAL_MACHINE\SOFTWARE\Microsoft\Cryptography\", "MachineGuid", UserName);
        var bytes = Encoding.UTF8.GetBytes(UserName + MachineGuid);
        var hash = MD5.HashData(bytes);
        DeviceId = Convert.ToHexString(hash);

        UserDataPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "Xunkong");
    }



}
