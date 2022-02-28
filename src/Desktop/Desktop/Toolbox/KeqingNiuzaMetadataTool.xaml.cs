using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Windows.Storage.Pickers;
using WinRT.Interop;
using Xunkong.Core.Metadata;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace Xunkong.Desktop.Toolbox
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    [Toolbox("刻记牛杂店元数据", "ms-appx:///Assets/Images/KeqingNiuza.png", "导出角色、武器、卡池信息以供刻记牛杂店使用")]
    public sealed partial class KeqingNiuzaMetadataTool : Page
    {


        private readonly IDbContextFactory<XunkongDbContext> _dbFactory;

        private readonly HttpClient _httpClient;

        private readonly JsonSerializerOptions _jsonSerializerOptions;



        public KeqingNiuzaMetadataTool()
        {
            this.InitializeComponent();
            _dbFactory = App.Current.Services.GetService<IDbContextFactory<XunkongDbContext>>()!;
            _httpClient = App.Current.Services.GetService<HttpClient>()!;
            _jsonSerializerOptions = App.Current.Services.GetService<JsonSerializerOptions>()!;
        }


        private string resourcePath;

        private bool isDownloadImage;


        private async void _Button_SelectFolder_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var dialog = new FolderPicker();
                dialog.SuggestedStartLocation = PickerLocationId.DocumentsLibrary;
                dialog.FileTypeFilter.Add("*");
                InitializeWithWindow.Initialize(dialog, MainWindow.Hwnd);
                var folder = await dialog.PickSingleFolderAsync();
                var path = folder.Path;
                if (File.Exists(Path.Combine(path, "KeqingNiuza Launcher.exe")))
                {
                    resourcePath = Path.Combine(path, @"bin\resource");
                    _TextBlock_SelectedFolder.Text = resourcePath;
                    return;
                }
                if (Directory.Exists(Path.Combine(path, "resource")))
                {
                    resourcePath = Path.Combine(path, "resource");
                    _TextBlock_SelectedFolder.Text = resourcePath;
                    return;
                }
                resourcePath = path;
                _TextBlock_SelectedFolder.Text = resourcePath;
            }
            catch (Exception ex)
            {
                InfoBarHelper.Error(ex);
            }
        }

        private async void _Button_ExportList_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(resourcePath))
            {
                InfoBarHelper.Warning("请选择导出位置", 3000);
                return;
            }
            try
            {
                var ctx = _dbFactory.CreateDbContext();
                var query_character = from c in ctx.CharacterInfos
                                      where !string.IsNullOrWhiteSpace(c.Birthday)
                                      join i in ctx.I18nModels
                                      on c.NameTextMapHash equals i.Id
                                      select new KeqingNiuzaCharacterInfo(c.Name, i.en_us, c.Rarity, "角色", (int)Math.Log2((int)c.Element));
                var str = JsonSerializer.Serialize(query_character, _jsonSerializerOptions);
                Directory.CreateDirectory(Path.Combine(resourcePath, "list"));
                await File.WriteAllTextAsync(Path.Combine(resourcePath, @"list\CharacterInfoList.json"), str);
                var query_weapon = from w in ctx.WeaponInfos
                                   join i in ctx.I18nModels
                                   on w.NameTextMapHash equals i.Id
                                   select new KeqingNiuzaWeaponInfo(w.Name, i.en_us, w.Rarity, "武器", WeaponTypeToInt(w.WeaponType));
                str = JsonSerializer.Serialize(query_weapon.ToList(), _jsonSerializerOptions);
                Directory.CreateDirectory(Path.Combine(resourcePath, "list"));
                await File.WriteAllTextAsync(Path.Combine(resourcePath, @"list\WeaponInfoList.json"), str);
                var query_wishevent = from w in ctx.WishEventInfos.AsNoTracking().ToList()
                                      group w by new { w.QueryType, w._StartTimeString } into g
                                      select new KeqingWishEventInfo((int)g.First().QueryType,
                                                                     string.Join(" ", g.Select(x => x.Name)),
                                                                     double.Parse(g.First().Version),
                                                                     StartTimeToString(g.First()._StartTimeString),
                                                                     g.First()._EndTimeString,
                                                                     g.SelectMany(x => x.Rank5UpItems).ToList(),
                                                                     g.First().Rank4UpItems);
                str = JsonSerializer.Serialize(query_wishevent.ToList(), _jsonSerializerOptions);
                Directory.CreateDirectory(Path.Combine(resourcePath, "list"));
                await File.WriteAllTextAsync(Path.Combine(resourcePath, @"list\WishEventList.json"), str);
                InfoBarHelper.Success("完成");
            }
            catch (Exception ex)
            {
                InfoBarHelper.Error(ex);
            }
        }

        private void _Button_DownloadImage_Click(object sender, RoutedEventArgs e)
        {

        }

        private record KeqingNiuzaCharacterInfo(string Name, string NameEn, int Rank, string ItemType, int ElementType);
        private record KeqingNiuzaWeaponInfo(string Name, string NameEn, int Rank, string ItemType, int WeaponType);
        private record KeqingWishEventInfo(int WishType, string Name, double Version, string StartTime, string EndTime, List<string> UpStar5, List<string> UpStar4);


        private static int WeaponTypeToInt(WeaponType weaponType)
        {
            return weaponType switch
            {
                WeaponType.Sword => 1,
                WeaponType.Claymore => 2,
                WeaponType.Bow => 3,
                WeaponType.Polearm => 4,
                WeaponType.Catalyst => 5,
                _ => 0,
            };
        }


        private static string StartTimeToString(string time)
        {
            if (time.Contains("+"))
            {
                var t = DateTime.Parse(time).AddDays(-1);
                return new DateTime(t.Year, t.Month, t.Day, 20, 0, 0).ToString("yyyy-MM-ddTHH:mm:ss");
            }
            else
            {
                return time;
            }
        }

    }
}
