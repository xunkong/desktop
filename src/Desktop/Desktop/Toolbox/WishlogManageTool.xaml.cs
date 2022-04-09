using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using MiniExcelLibs;
using System.Text.Json.Nodes;
using Windows.ApplicationModel;
using Windows.ApplicationModel.DataTransfer;
using Windows.Storage.Pickers;
using WinRT.Interop;
using Xunkong.Core.Wish;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace Xunkong.Desktop.Toolbox
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    [Toolbox("祈愿记录管理", "ms-appx:///Assets/Images/Ui_SystemOpenIcon_Gacha.png", "显示表格时会有一些些些些些卡")]
    [INotifyPropertyChanged]
    public sealed partial class WishlogManageTool : Page
    {

        private readonly ILogger<WishlogManageTool> _logger;

        private readonly IDbContextFactory<XunkongDbContext> _ctxFactory;

        private readonly WishlogService _wishlogService;

        private readonly BackupService _backupService;

        private readonly JsonSerializerOptions _jsonOptions;


        static WishlogManageTool()
        {
            TypeAdapterConfig<WishlogItemEx, WishlogManage_ExcelExport_Origin>.NewConfig().Map(dest => dest.WishType, src => ((int)src.WishType).ToString());
            TypeAdapterConfig<WishlogItemEx, WishlogManage_ExcelExport_ByType>.NewConfig().Map(dest => dest.WishType, src => src.WishType.ToDescriptionOrString());
        }


        public WishlogManageTool()
        {
            this.InitializeComponent();
            var serviceProvider = App.Current.Services;
            _logger = ActivatorUtilities.GetServiceOrCreateInstance<ILogger<WishlogManageTool>>(serviceProvider);
            _ctxFactory = ActivatorUtilities.GetServiceOrCreateInstance<IDbContextFactory<XunkongDbContext>>(serviceProvider);
            _wishlogService = ActivatorUtilities.GetServiceOrCreateInstance<WishlogService>(serviceProvider);
            _backupService = ActivatorUtilities.GetServiceOrCreateInstance<BackupService>(serviceProvider);
            _jsonOptions = ActivatorUtilities.GetServiceOrCreateInstance<JsonSerializerOptions>(serviceProvider);
            Loaded += WishlogManageTool_Loaded;
        }


        [ObservableProperty]
        private string _InfoText;

        [ObservableProperty]
        private List<string> _UidList;


        [ObservableProperty]
        private string? _SelectedUid;


        [ObservableProperty]
        private ObservableCollection<WishlogItemEx> _WishlogList;

        /// <summary>
        /// 待删除的祈愿记录
        /// </summary>
        private List<WishlogItemEx> deletingWishlogItems = new();


        [ObservableProperty]
        private List<string> dataGridSelectionModes = new List<string> { "不选", "单选", "多选" };


        [ObservableProperty]
        private List<string> exportTemplates = new List<string> { "UIGF Excel v2.2", "UIGF Json v2.2" };


        [ObservableProperty]
        private int overwriteUid;


        [ObservableProperty]
        private string overwriteLang;


        [ObservableProperty]
        private bool overwriteExistedItems;


        private async void WishlogManageTool_Loaded(object sender, RoutedEventArgs e)
        {
            try
            {
                var uids = await _wishlogService.GetAllUidsAsync();
                UidList = uids.Select(x => x.ToString()).ToList();
            }
            catch (Exception ex)
            {
                InfoBarHelper.Error(ex);
            }
        }



        private async void _ComboBox_Uid_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            try
            {
                deletingWishlogItems.Clear();
                if (int.TryParse(SelectedUid, out int uid))
                {
                    var wishlogs = await _wishlogService.GetWishlogItemExByUidAsync(uid);
                    WishlogList = new(wishlogs.OrderByDescending(x => x.Id));
                    InfoText = $"祈愿记录总计 {wishlogs.Count} 条";
                }
            }
            catch (Exception ex)
            {
                InfoBarHelper.Error(ex);
            }
        }


        /// <summary>
        /// 显示表格
        /// </summary>
        [ICommand]
        private void ShowDataGrid()
        {
            _DataGrid_Wishlog.Visibility = Visibility.Visible;
            _StackPanel_GridCommand.Visibility = Visibility.Visible;
        }


        private void _ComboBox_DataGridSelectionMode_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            switch (_ComboBox_DataGridSelectionMode.SelectedIndex)
            {
                case 0:
                    _DataGrid_Wishlog.SelectionMode = Syncfusion.UI.Xaml.Grids.GridSelectionMode.None;
                    break;
                case 1:
                    _DataGrid_Wishlog.SelectionMode = Syncfusion.UI.Xaml.Grids.GridSelectionMode.Single;
                    break;
                case 2:
                    _DataGrid_Wishlog.SelectionMode = Syncfusion.UI.Xaml.Grids.GridSelectionMode.Multiple;
                    break;
                default:
                    _DataGrid_Wishlog.SelectionMode = Syncfusion.UI.Xaml.Grids.GridSelectionMode.None;
                    break;
            }
        }


        /// <summary>
        /// 删除选择的uid的所有记录
        /// </summary>
        /// <returns></returns>
        [ICommand(AllowConcurrentExecutions = false)]
        private async Task DeleteUidAsync()
        {
            try
            {
                if (int.TryParse(SelectedUid, out int uid))
                {
                    await _backupService.BackupWishlogItemsAsync(uid);
                    using var ctx = _ctxFactory.CreateDbContext();
                    var rows = await ctx.Database.ExecuteSqlRawAsync($"DELETE FROM Wishlog_Items WHERE Uid={uid};");
                    InfoText = $"已删除 {uid} 的祈愿记录 {rows} 条";
                    InfoBarHelper.Success("删除完成");
                }
                else
                {
                    InfoBarHelper.Error("未选择需要删除的账号");
                }
            }
            catch (Exception ex)
            {
                InfoBarHelper.Error(ex);
                _logger.LogError(ex, nameof(DeleteUidAsync));
            }
        }

        /// <summary>
        /// 表格全选
        /// </summary>
        [ICommand]
        private void SelectAll()
        {
            _ComboBox_DataGridSelectionMode.SelectedIndex = 2;
            _DataGrid_Wishlog.SelectionMode = Syncfusion.UI.Xaml.Grids.GridSelectionMode.Multiple;
            _DataGrid_Wishlog.SelectAll();
        }

        /// <summary>
        /// 表格取消选择
        /// </summary>
        [ICommand]
        private void ClearSelection()
        {
            _DataGrid_Wishlog.ClearSelections(false);
        }

        /// <summary>
        /// 把所选项纳入待删除
        /// </summary>
        [ICommand]
        private void DeleteSelectedWishlogItems()
        {
            try
            {
                var items = _DataGrid_Wishlog.SelectedItems.ToList();
                if (items.Any())
                {
                    foreach (WishlogItemEx item in items)
                    {
                        WishlogList?.Remove(item);
                        item.EditState = EditState.Delete;
                        deletingWishlogItems.Add(item);
                    }
                }
                InfoText = $"待删除记录 {deletingWishlogItems.Count} 条";
            }
            catch (Exception ex)
            {
                InfoBarHelper.Error(ex);
                _logger.LogError(ex, nameof(DeleteSelectedWishlogItems));
            }
        }

        /// <summary>
        /// 删除待删除项
        /// </summary>
        [ICommand(AllowConcurrentExecutions = false)]
        private async Task SaveDeletingOperationAsync()
        {
            try
            {
                if (!deletingWishlogItems.Any())
                {
                    InfoBarHelper.Information("没有需要删除的记录");
                    return;
                }
                int.TryParse(SelectedUid, out int uid);
                if (deletingWishlogItems.Any(x => x.Uid != uid))
                {
                    InfoBarHelper.Error("待删除的记录中存在与所选 uid 不匹配的项");
                    return;
                }
                _logger.LogInformation($"Start to delete wishlog items (count: {deletingWishlogItems.Count}) of uid {uid}.");
                InfoText = "正在备份现有记录";
                await Task.Delay(2000);
                await _backupService.BackupWishlogItemsAsync(uid);
                using var ctx = _ctxFactory.CreateDbContext();
                var rows = await ctx.Database.ExecuteSqlRawAsync($"DELETE FROM Wishlog_Items WHERE Uid={uid} AND Id IN ({string.Join(',', deletingWishlogItems.Select(x => x.Id))});");
                deletingWishlogItems.Clear();
                _logger.LogInformation($"Eventually deleted count of wishlog items is {rows}.");
                InfoText = $"已删除账号 {uid} 的祈愿记录 {rows} 条";
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, nameof(SaveDeletingOperationAsync));
                InfoBarHelper.Error(ex);
            }
        }

        /// <summary>
        /// 打开导入导出面板
        /// </summary>
        [ICommand]
        private void OpenImportExportGrid()
        {
            _Grid_ImportExport.Visibility = Visibility.Visible;
        }

        /// <summary>
        /// 关闭导入导出面板
        /// </summary>
        [ICommand]
        private void CloseImportExportGrid()
        {
            _Grid_ImportExport.Visibility = Visibility.Collapsed;
        }

        /// <summary>
        /// 选择导入文件
        /// </summary>
        [ICommand(AllowConcurrentExecutions = false)]
        private async Task PickFileAsync()
        {
            try
            {
                var dialog = new FileOpenPicker();
                dialog.SuggestedStartLocation = PickerLocationId.DocumentsLibrary;
                dialog.FileTypeFilter.Add("*");
                dialog.FileTypeFilter.Add(".xlsx");
                dialog.FileTypeFilter.Add(".json");
                InitializeWithWindow.Initialize(dialog, MainWindow.Hwnd);
                var files = await dialog.PickMultipleFilesAsync();
                await ImportWishlogAsync(files.FirstOrDefault()?.Path);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, nameof(PickFileAsync));
                InfoBarHelper.Error(ex);
            }
        }


        private void _Rect_DropFile_DragOver(object sender, DragEventArgs e)
        {
            e.AcceptedOperation = DataPackageOperation.Copy;
            e.DragUIOverride.IsCaptionVisible = false;
            e.DragUIOverride.IsGlyphVisible = false;
        }


        private async void _Rect_DropFile_Drop(object sender, DragEventArgs e)
        {
            if (e.DataView.Contains(StandardDataFormats.StorageItems))
            {
                var items = await e.DataView.GetStorageItemsAsync();
                if (items.Count > 0)
                {
                    await ImportWishlogAsync(items.FirstOrDefault()?.Path);
                }
            }
        }



        private List<WishlogManage_Import>? jsonImports;


        private async Task ImportWishlogAsync(string? file)
        {
            try
            {
                if (!File.Exists(file))
                {
                    InfoBarHelper.Error("文件不存在");
                    return;
                }
                List<WishlogManage_Import> importedItems = null;
                var exten = Path.GetExtension(file).ToLower();
                if (exten == ".xlsx")
                {
                    var items = await MiniExcel.QueryAsync<WishlogManage_Import>(file, "原始数据");
                    importedItems = items.ToList();
                }
                if (exten == ".json")
                {
                    var str = await File.ReadAllTextAsync(file);
                    jsonImports = new();
                    var baseNode = JsonNode.Parse(str);
                    ParseJson(baseNode);
                    importedItems = jsonImports;
                    jsonImports = null;
                }
                if (importedItems is null || !importedItems.Any())
                {
                    InfoBarHelper.Warning("无法解析导入的文件或文件内没有任何数据");
                    return;
                }
                var uids = importedItems.Select(x => x.Uid).Distinct().ToList();
                if (uids.Count(x => x != 0) > 1)
                {
                    InfoBarHelper.Error("导入的数据中存在多个uid");
                    return;
                }
                if (importedItems.Any(x => x.Id == 0))
                {
                    InfoBarHelper.Error("导入的数据中存在id为0的项");
                    return;
                }
                var langs = importedItems.Select(x => x.Language).Where(x => !string.IsNullOrWhiteSpace(x)).Distinct().ToList();
                if (langs.Count > 1)
                {
                    InfoBarHelper.Error("导入的数据中存在多个lang");
                    return;
                }
                if (OverwriteUid == 0)
                {
                    if (importedItems.Any(x => x.Uid == 0))
                    {
                        InfoBarHelper.Error("导入的数据中存在uid为0的项");
                        return;
                    }
                }
                else
                {
                    var existUid = importedItems.Where(x => x.Uid != 0).Select(x => x.Uid).FirstOrDefault();
                    if (existUid != 0 && existUid != OverwriteUid)
                    {
                        InfoBarHelper.Error("导入的数据中存在与指定uid不符的项");
                        return;
                    }
                    foreach (var item in importedItems)
                    {
                        item.Uid = OverwriteUid;
                    }
                }
                if (string.IsNullOrWhiteSpace(OverwriteLang))
                {
                    if (importedItems.Any(x => string.IsNullOrWhiteSpace(x.Language)))
                    {
                        InfoBarHelper.Error("导入的数据中存在lang为空的项");
                        return;
                    }
                }
                else
                {
                    var existLang = importedItems.Where(x => !string.IsNullOrWhiteSpace(x.Language)).Select(x => x.Language).FirstOrDefault();
                    if (existLang != null && existLang != OverwriteLang)
                    {
                        InfoBarHelper.Error("导入的数据中存在与指定lang不符的项");
                        return;
                    }
                    foreach (var item in importedItems)
                    {
                        item.Language = OverwriteLang;
                    }
                }
                var wishlogs = importedItems.Adapt<List<WishlogItem>>();
                if (!wishlogs.Any() || wishlogs.Any(x => x.Uid == 0) || wishlogs.Any(x => string.IsNullOrWhiteSpace(x.Language)))
                {
                    InfoBarHelper.Error("校验后仍存在不符合要求的数据");
                    return;
                }
                var uid = wishlogs.First().Uid;
                await _backupService.BackupWishlogItemsAsync(uid);
                using var ctx = _ctxFactory.CreateDbContext();
                var existIds = await ctx.WishlogItems.Where(x => x.Uid == uid).Select(x => x.Id).ToListAsync();
                var insertItems = wishlogs.ExceptBy(existIds, x => x.Id).ToList();
                ctx.AddRange(insertItems);
                if (OverwriteExistedItems)
                {
                    var updateItems = wishlogs.IntersectBy(existIds, x => x.Id).ToList();
                    ctx.UpdateRange(updateItems);
                }
                var rows = await ctx.SaveChangesAsync();
                var count = await ctx.WishlogItems.Where(x => x.Uid == uid).CountAsync();
                InfoText = $"此次导入新增祈愿记录 {insertItems.Count} 条，影响数据库 {rows} 行，导入后总计 {count} 条。";
                InfoBarHelper.Success("导入完成");
            }
            catch (Exception ex)
            {
                InfoBarHelper.Error(ex);
                _logger.LogError(ex, nameof(ImportWishlogAsync));
            }
        }


        private void ParseJson(JsonNode? node)
        {
            if (node is JsonValue value)
            {
                return;
            }
            if (node is JsonObject obj)
            {
                if (obj.ContainsKey("gacha_type")
                    && obj.ContainsKey("time")
                    && obj.ContainsKey("name")
                    && obj.ContainsKey("item_type")
                    && obj.ContainsKey("rank_type")
                    && obj.ContainsKey("id"))
                {
                    var data = obj.Deserialize<WishlogManage_Import>(_jsonOptions);
                    if (data != null)
                    {
                        jsonImports!.Add(data);
                    }
                    return;
                }
                else
                {
                    foreach (var property in obj)
                    {
                        ParseJson(property.Value);
                    }
                }
            }
            if (node is JsonArray array)
            {
                foreach (var item in array)
                {
                    ParseJson(item);
                }
            }
        }


        [ICommand(AllowConcurrentExecutions = false)]
        private async Task ExportWishlogAsync()
        {
            if (string.IsNullOrWhiteSpace(SelectedUid))
            {
                return;
            }
            if (!(WishlogList?.Any() ?? false))
            {
                return;
            }
            try
            {
                if (_ComboBox_Export.SelectedIndex == 0)
                {
                    var wm_excel_origin = WishlogList.Adapt<List<WishlogManage_ExcelExport_Origin>>().OrderBy(x => x.Id);
                    var wm_excel_novice = WishlogList.Where(x => x.QueryType == WishType.Novice).Adapt<List<WishlogManage_ExcelExport_ByType>>().OrderBy(x => x.Id);
                    int index = 0;
                    foreach (var item in wm_excel_novice)
                    {
                        item.Index = (++index).ToString();
                    }
                    var wm_excel_permanent = WishlogList.Where(x => x.QueryType == WishType.Permanent).Adapt<List<WishlogManage_ExcelExport_ByType>>().OrderBy(x => x.Id);
                    index = 0;
                    foreach (var item in wm_excel_permanent)
                    {
                        item.Index = (++index).ToString();
                    }
                    var wm_excel_character = WishlogList.Where(x => x.QueryType == WishType.CharacterEvent).Adapt<List<WishlogManage_ExcelExport_ByType>>().OrderBy(x => x.Id);
                    index = 0;
                    foreach (var item in wm_excel_character)
                    {
                        item.Index = (++index).ToString();
                    }
                    var wm_excel_weapon = WishlogList.Where(x => x.QueryType == WishType.WeaponEvent).Adapt<List<WishlogManage_ExcelExport_ByType>>().OrderBy(x => x.Id);
                    index = 0;
                    foreach (var item in wm_excel_weapon)
                    {
                        item.Index = (++index).ToString();
                    }
                    var exportObj = new
                    {
                        Novice = wm_excel_novice,
                        Permanent = wm_excel_permanent,
                        Character = wm_excel_character,
                        Weapon = wm_excel_weapon,
                        Origin = wm_excel_origin,
                    };
                    var filename = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), $@"Xunkong\Export\Wishlog\Wishlog_{SelectedUid}_{DateTimeOffset.Now:yyyyMMdd_HHmmss}.xlsx");
                    Directory.CreateDirectory(Path.GetDirectoryName(filename)!);
                    using var fs = File.Open(filename, FileMode.CreateNew);
                    var templatePath = Path.Combine(Package.Current.InstalledPath, @"Assets\Others\WishlogTemplate.xlsx");
                    await MiniExcel.SaveAsByTemplateAsync(fs, templatePath, exportObj);
                    Action action = () => Process.Start(new ProcessStartInfo { FileName = Path.GetDirectoryName(filename), UseShellExecute = true });
                    InfoBarHelper.ShowWithButton(InfoBarSeverity.Success, "导出完成", null, "打开文件夹", action);
                }
                if (_ComboBox_Export.SelectedIndex == 1)
                {
                    var wm_jsonitems = WishlogList.Adapt<List<WishlogManage_JsonExport_Item>>();
                    var wm_json = new WishlogManage_JsonExport { List = wm_jsonitems.OrderBy(x => x.Id).ToList() };
                    wm_json.InitializeInfo();
                    var str = JsonSerializer.Serialize(wm_json, _jsonOptions);
                    var filename = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), $@"Xunkong\Export\Wishlog\Wishlog_{SelectedUid}_{DateTimeOffset.Now:yyyyMMdd_HHmmss}.json");
                    Directory.CreateDirectory(Path.GetDirectoryName(filename)!);
                    await File.WriteAllTextAsync(filename, str);
                    Action action = () => Process.Start(new ProcessStartInfo { FileName = Path.GetDirectoryName(filename), UseShellExecute = true });
                    InfoBarHelper.ShowWithButton(InfoBarSeverity.Success, "导出完成", null, "打开文件夹", action);
                }
            }
            catch (Exception ex)
            {
                InfoBarHelper.Error(ex);
                _logger.LogError(ex, nameof(ExportWishlogAsync));
            }
        }



    }




}
