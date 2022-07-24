using Windows.Networking.Connectivity;

namespace Xunkong.Desktop.Helpers;

internal static class NetworkHelper
{

    /// <summary>
    /// 网络是否为按流量计费
    /// </summary>
    public static bool IsInternetOnMeteredConnection => GetIsInternetOnMeteredConnection();



    private static bool GetIsInternetOnMeteredConnection()
    {
        var profile = NetworkInformation.GetInternetConnectionProfile();
        if (profile is null)
        {
            return true;
        }
        var cost = profile.GetConnectionCost();
        return cost.NetworkCostType != NetworkCostType.Unrestricted;
    }


}
