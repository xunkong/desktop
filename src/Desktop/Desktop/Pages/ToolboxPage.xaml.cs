using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media.Animation;
using Xunkong.Desktop.Toolbox;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace Xunkong.Desktop.Pages
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    [INotifyPropertyChanged]
    public sealed partial class ToolboxPage : Page
    {


        public ToolboxPage()
        {
            this.InitializeComponent();
            Loaded += ToolboxPage_Loaded;
        }




        private void ToolboxPage_Loaded(object sender, RoutedEventArgs e)
        {
            var nameSpaces = typeof(ToolboxPage).Assembly.GetTypes().Where(x => x.FullName?.Contains("Xunkong.Desktop.Toolbox") ?? false);
            var tools = nameSpaces.Where(x => x.GetCustomAttributes(typeof(ToolboxAttribute), false).Any())
                                  .Select(x => new ToolItem(x.GetCustomAttributes(typeof(ToolboxAttribute), false).FirstOrDefault() as ToolboxAttribute, x));
            Tools = new(tools);
        }



        private ObservableCollection<ToolItem> _Tools;
        internal ObservableCollection<ToolItem> Tools
        {
            get => _Tools;
            set => SetProperty(ref _Tools, value);
        }

        private void _GridView_ToolItem_ItemClick(object sender, ItemClickEventArgs e)
        {
            if (e.ClickedItem is ToolItem tool)
            {
                if (tool.PageType is not null)
                {
                    NavigationHelper.NavigateTo(tool.PageType, null, new SlideNavigationTransitionInfo { Effect = SlideNavigationTransitionEffect.FromRight });
                }
            }
        }

    }



    internal class ToolItem
    {

        public ToolboxAttribute? Attribute { get; set; }


        public Type PageType { get; set; }


        public ToolItem(ToolboxAttribute? attribute, Type type)
        {
            Attribute = attribute;
            PageType = type;
        }

    }
}
