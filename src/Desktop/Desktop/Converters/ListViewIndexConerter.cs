using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Controls.Primitives;
using Microsoft.UI.Xaml.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Xunkong.Desktop.Converters
{
    internal class ListViewIndexConerter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            //var item = (value as ListViewItemPresenter)!;
            //var container = ItemsControl.GetItemsOwner(item);
            //ListView listView = (ItemsControl.ItemsControlFromItemContainer(item) as ListView)!;
            //int index = listView.ItemContainerGenerator.IndexFromContainer(item);
            //return index.ToString();
            return 1;
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            throw new NotImplementedException();
        }
    }
}
