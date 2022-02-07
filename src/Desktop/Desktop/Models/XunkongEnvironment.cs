using System.Security.Cryptography;
using System.Text;
using Windows.ApplicationModel;
using Windows.Storage;
using Xunkong.Core.XunkongApi;

namespace Xunkong.Desktop.Models
{
    internal static class XunkongEnvironment
    {

        public static readonly string AppName;

        public static readonly PlatformType Platform;

        public static readonly ChannelType Channel;

        public static readonly Version AppVersion;

        public static readonly string DeviceId;

        public static readonly string UserDataPath;

        public static readonly string? WebView2Version;


        static XunkongEnvironment()
        {
            AppName = Package.Current.DisplayName;
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


        public static string GetLogHeader()
        {
            var sb = new StringBuilder();
            sb.AppendLine(DateTimeOffset.Now.ToString("O"));
            sb.AppendLine("Xunkong Desktop Log Message");
            sb.AppendLine($"App Name: {AppName}");
            sb.AppendLine($"App Version: {AppVersion}");
            sb.AppendLine($"OS Version: {Environment.OSVersion}");
            sb.AppendLine($"Device Id: {DeviceId}");
            sb.AppendLine($"Data Path: {UserDataPath}");
            sb.AppendLine($"WebView2 Version: {WebView2Version ?? "Null"}");
            sb.AppendLine($"Command Line: {Environment.CommandLine}");
            return sb.ToString();
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
}
