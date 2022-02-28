using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using Xunkong.Desktop.Pages;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace Xunkong.Desktop.Controls
{
    public sealed partial class NotificationControl : UserControl, INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler? PropertyChanged;
        private void OnPropertyChanged([CallerMemberName] string propertyName = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }


        private readonly ILogger<NotificationControl> _logger;


        private readonly IDbContextFactory<XunkongDbContext> _dbContextFactory;



        public NotificationControl()
        {
            this.InitializeComponent();
            DataContext = this;
            _logger = App.Current.Services.GetService<ILogger<NotificationControl>>()!;
            _dbContextFactory = App.Current.Services.GetService<IDbContextFactory<XunkongDbContext>>()!;
            Loaded += NotificationControl_Loaded;
        }



        private async void NotificationControl_Loaded(object sender, RoutedEventArgs e)
        {
            try
            {
                _Text_ListEnd.Text = "到此为止了";
                using var ctx = _dbContextFactory.CreateDbContext();
                var list = await ctx.NotificationItems.AsNoTracking().OrderByDescending(x => x.Id).Take(20).ToListAsync();
                TotalCount = list.Count();
                UnreadCount = list.Where(x => !x.HasRead).Count();
                NotificationList = new(list);
            }
            catch (Exception ex)
            {
                _Text_ListEnd.Text = "出现了错误";
                _logger.LogError(ex, "Error in {MethodName}.", nameof(NotificationControl_Loaded));
            }

        }



        private int _UnreadCount;
        public int UnreadCount
        {
            get { return _UnreadCount; }
            set
            {
                _UnreadCount = value;
                OnPropertyChanged();
            }
        }


        private int _TotalCount;
        public int TotalCount
        {
            get { return _TotalCount; }
            set
            {
                _TotalCount = value;
                OnPropertyChanged();
            }
        }



        private ObservableCollection<NotificationDesktopModel> _NotificationList;
        internal ObservableCollection<NotificationDesktopModel> NotificationList
        {
            get { return _NotificationList; }
            set
            {
                _NotificationList = value;
                OnPropertyChanged();
            }
        }

        private async void _Button_ReadAll_Click(object sender, RoutedEventArgs e)
        {
            if (NotificationList is null)
            {
                return;
            }
            var list = NotificationList.Where(x => !x.HasRead).ToList();
            if (!list.Any())
            {
                return;
            }
            foreach (var item in list)
            {
                item.HasRead = true;
            }
            UnreadCount = 0;
            try
            {
                using var ctx = _dbContextFactory.CreateDbContext();
                ctx.UpdateRange(list);
                await ctx.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error when clear all unread notifications.");
            }
        }



        private async void _Button_HasRead_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button button)
            {
                if (button.DataContext is NotificationDesktopModel model)
                {
                    model.HasRead = true;
                    UnreadCount -= 1;
                    try
                    {
                        using var ctx = _dbContextFactory.CreateDbContext();
                        ctx.Update(model);
                        await ctx.SaveChangesAsync();
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "Error when clear an unread notifications");
                    }
                }
            }
        }




        private async void _Button_NotificationTitle_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button button)
            {
                if (button.DataContext is NotificationDesktopModel model)
                {
                    model.HasRead = true;
                    try
                    {
                        using var ctx = _dbContextFactory.CreateDbContext();
                        ctx.Update(model);
                        await ctx.SaveChangesAsync();
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "Error when clear an unread notifications");
                    }
                    var dialog = new ContentDialog
                    {
                        PrimaryButtonText = "关闭",
                        XamlRoot = MainWindow.XamlRoot,
                    };
                    switch (model.ContentType)
                    {
                        case Core.XunkongApi.ContentType.Text:
                            dialog.Title = model.Title;
                            dialog.Content = model.Content;
                            break;
                        case Core.XunkongApi.ContentType.HtmlDialog:
                            var webview = new WebView2 { Width = 720, Height = 580 };
                            webview.Loading += async (_, _) => await webview.EnsureCoreWebView2Async();
                            webview.CoreWebView2Initialized += (_, _) => webview.CoreWebView2.NavigateToString(model.Content);
                            dialog.Content = webview;
                            break;
                        case Core.XunkongApi.ContentType.HtmlPage:
                            NavigationHelper.NavigateTo(typeof(WebViewPage), (0, model.Content));
                            return;
                        case Core.XunkongApi.ContentType.Url:
                            NavigationHelper.NavigateTo(typeof(WebViewPage), model.Content);
                            return;
                        default:
                            break;
                    }
                    await dialog.ShowAsync();
                }
            }
        }


    }
}
