using Xunkong.Core.Metadata;

namespace Xunkong.Desktop.Converters
{
    internal class ElementToElementImageConverter : IValueConverter
    {

        private const string Element_Pyro = "ms-appx:///Assets/Images/Element_Pyro.png";
        private const string Element_Hydro = "ms-appx:///Assets/Images/Element_Hydro.png";
        private const string Element_Anemo = "ms-appx:///Assets/Images/Element_Anemo.png";
        private const string Element_Electro = "ms-appx:///Assets/Images/Element_Electro.png";
        private const string Element_Dendro = "ms-appx:///Assets/Images/Element_Dendro.png";
        private const string Element_Cryo = "ms-appx:///Assets/Images/Element_Cryo.png";
        private const string Element_Geo = "ms-appx:///Assets/Images/Element_Geo.png";

        public object? Convert(object value, Type targetType, object parameter, string language)
        {
            var element = (ElementType)value;
            return element switch
            {
                ElementType.Pyro => Element_Pyro,
                ElementType.Hydro => Element_Hydro,
                ElementType.Anemo => Element_Anemo,
                ElementType.Electro => Element_Electro,
                ElementType.Dendro => Element_Dendro,
                ElementType.Cryo => Element_Cryo,
                ElementType.Geo => Element_Geo,
                _ => null,
            };
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            throw new NotImplementedException();
        }
    }
}
