using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Documents;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace Xunkong.Desktop.Controls
{
    public sealed class SettingCard : Control
    {
        public SettingCard()
        {
            this.DefaultStyleKey = typeof(SettingCard);
        }




        public object Icon
        {
            get { return (object)GetValue(IconProperty); }
            set { SetValue(IconProperty, value); }
        }

        // Using a DependencyProperty as the backing store for Icon.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty IconProperty =
            DependencyProperty.Register("Icon", typeof(object), typeof(SettingCard), new PropertyMetadata(null));


        public object Content
        {
            get { return (object)GetValue(ContentProperty); }
            set { SetValue(ContentProperty, value); }
        }

        // Using a DependencyProperty as the backing store for Content.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty ContentProperty =
            DependencyProperty.Register("Content", typeof(object), typeof(SettingCard), new PropertyMetadata(null));


        public object Selector
        {
            get { return (object)GetValue(SelectorProperty); }
            set { SetValue(SelectorProperty, value); }
        }

        // Using a DependencyProperty as the backing store for Selector.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty SelectorProperty =
            DependencyProperty.Register("Selector", typeof(object), typeof(SettingCard), new PropertyMetadata(null));




    }
}
