using Microsoft.UI.Xaml.Media.Animation;

namespace Xunkong.Desktop.Messages
{
    internal record NavigateMessage(Type Type, object? Parameter = null, NavigationTransitionInfo? TransitionInfo = null);
}
