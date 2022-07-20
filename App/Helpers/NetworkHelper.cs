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
        var cost = profile.GetConnectionCost();
        return cost.NetworkCostType switch
        {
            NetworkCostType.Unknown => false,
            NetworkCostType.Unrestricted => false,
            NetworkCostType.Fixed => cost.OverDataLimit,
            NetworkCostType.Variable => true,
            _ => false,
        };
    }


}
