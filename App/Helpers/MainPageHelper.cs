using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media.Animation;
using Xunkong.Desktop.Pages;

namespace Xunkong.Desktop.Helpers;

internal static class MainPageHelper
{

    private static MainPage _mainPage;

    private static Frame _frame;


    public static void Initialize(MainPage mainPage)
    {
        _mainPage = mainPage;
        _frame = mainPage.ContentFrame;
    }



    public static void Navigate(Type sourcePageType, object? param = null, NavigationTransitionInfo? infoOverride = null)
    {
        if (param is null)
        {
            _frame.Navigate(sourcePageType);
        }
        else if (infoOverride is null)
        {
            _frame.Navigate(sourcePageType, param);
        }
        else
        {
            _frame.Navigate(sourcePageType, param, infoOverride);
        }
    }

}
