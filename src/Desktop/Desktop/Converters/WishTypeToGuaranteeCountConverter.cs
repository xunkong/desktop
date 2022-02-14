using Microsoft.UI.Xaml.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunkong.Core.Wish;

namespace Xunkong.Desktop.Converters
{

    internal class WishTypeToGuaranteeCountNumberConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            var type = (WishType)value;
            return type switch
            {
                WishType.WeaponEvent => 80.0,
                _ => 90.0,
            };
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            throw new NotImplementedException();
        }
    }


    internal class WishTypeToGuaranteeCountStringConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            var type = (WishType)value;
            return type switch
            {
                WishType.WeaponEvent => "80",
                _ => "90",
            };
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            throw new NotImplementedException();
        }
    }

}
