using Microsoft.UI.Xaml.Media.Animation;

namespace Xunkong.Desktop.Helpers
{
    internal static class NavigationHelper
    {


        public static void NavigateTo(Type type, object? parameter = null, NavigationTransitionInfo? transitionInfo = null)
        {
            var message = new NavigateMessage(type, parameter, transitionInfo);
            WeakReferenceMessenger.Default.Send(message);
        }


    }
}
