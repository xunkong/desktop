using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Input;
using Syncfusion.Licensing;
using Xunkong.Hoyolab.TravelNotes;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace Xunkong.Desktop.Pages;

/// <summary>
/// An empty page that can be used on its own or navigated to within a Frame.
/// </summary>
[INotifyPropertyChanged]
public sealed partial class TravelNotesPage : Page
{


    static TravelNotesPage()
    {
        SyncfusionLicenseProvider.RegisterLicense("NjEwMjc4QDMyMzAyZTMxMmUzMFkyTEYzc3JuY2hOVXk1azE5d1p1K1NPaFQ4TFF4bU5zYXR2ZUdBQmorc2c9");
    }


    private readonly HoyolabService _hoyolabService;


    public TravelNotesPage()
    {
        this.InitializeComponent();
        _hoyolabService = ServiceProvider.GetService<HoyolabService>()!;
        Loaded += async (_, _) => await InitializeDataAsync();
    }




    private void SfCartesianChart_PointerWheelChanged(object sender, PointerRoutedEventArgs e)
    {
        e.Handled = true;
    }


    private bool _IsLoading;
    public bool IsLoading
    {
        get => _IsLoading;
        set => SetProperty(ref _IsLoading, value);
    }




    private TravelNotesPage_DayOrMonthStats? _DayStats;
    public TravelNotesPage_DayOrMonthStats? DayStats
    {
        get => _DayStats;
        set => SetProperty(ref _DayStats, value);
    }



    private TravelNotesPage_DayOrMonthStats? _MonthStats;
    public TravelNotesPage_DayOrMonthStats? MonthStats
    {
        get => _MonthStats;
        set => SetProperty(ref _MonthStats, value);
    }




    private List<TravelNotesPrimogemsMonthGroupStats>? _PrimogemsGroup;
    public List<TravelNotesPrimogemsMonthGroupStats>? PrimogemsGroup
    {
        get => _PrimogemsGroup;
        set => SetProperty(ref _PrimogemsGroup, value);
    }



    private List<TravelNotesPage_DayData> _RecentDayData;
    public List<TravelNotesPage_DayData> RecentDayData
    {
        get => _RecentDayData;
        set => SetProperty(ref _RecentDayData, value);
    }





    private List<TravelNotesPage_MonthData> _AllMonthData;
    public List<TravelNotesPage_MonthData> AllMonthData
    {
        get => _AllMonthData;
        set => SetProperty(ref _AllMonthData, value);
    }




    private List<TravelNotesAwardItem> _PrimogemsAwardItems;
    public List<TravelNotesAwardItem> PrimogemsAwardItems
    {
        get => _PrimogemsAwardItems;
        set => SetProperty(ref _PrimogemsAwardItems, value);
    }




    private List<TravelNotesAwardItem> _MoraAwardItems;
    public List<TravelNotesAwardItem> MoraAwardItems
    {
        get => _MoraAwardItems;
        set => SetProperty(ref _MoraAwardItems, value);
    }




    [RelayCommand]
    public async Task InitializeDataAsync()
    {
        IsLoading = true;
        await Task.Delay(100);
        try
        {
            var role = _hoyolabService.GetLastSelectedOrFirstGenshinRoleInfo();
            if (role is null)
            {
                NotificationProvider.Warning("没有找到已添加的账号");
                return;
            }
            else
            {
                var summary = await _hoyolabService.GetTravelNotesSummaryAsync(role, DateTime.UtcNow.AddHours(8).Month);
                DayStats = summary?.DayData?.Adapt<TravelNotesPage_DayOrMonthStats>();
                MonthStats = summary?.MonthData?.Adapt<TravelNotesPage_DayOrMonthStats>();
                PrimogemsGroup = summary?.MonthData?.PrimogemsGroupBy;
                var minTime = DateTime.UtcNow.AddDays(-30).AddHours(12);
                var recentDays = _hoyolabService.GetTravelNotesAwardItems(role.Uid, minTime, null).ToList();
                var group = recentDays.GroupBy(x => x.Time.Date).OrderBy(x => x.Key);
                var dayDatas = new List<TravelNotesPage_DayData>(30);
                foreach (var item in group)
                {
                    var primogems = item.Where(x => x.Type == TravelNotesAwardType.Primogems).Sum(x => x.Number);
                    var mora = item.Where(x => x.Type == TravelNotesAwardType.Mora).Sum(x => x.Number);
                    var dayData = new TravelNotesPage_DayData(item.Key.ToString("MM-dd"), primogems, mora);
                    dayDatas.Add(dayData);
                }
                RecentDayData = dayDatas;
                AllMonthData = _hoyolabService.GetTravelNotesMonthData(role.Uid).OrderBy(x => x.Year).ThenBy(x => x.Month).Adapt<List<TravelNotesPage_MonthData>>();
                PrimogemsAwardItems = recentDays.Where(x => x.Type == TravelNotesAwardType.Primogems).ToList();
                MoraAwardItems = recentDays.Where(x => x.Type == TravelNotesAwardType.Mora).ToList();
            }
        }
        catch (Exception ex)
        {
            NotificationProvider.Error(ex);
        }
        finally
        {
            IsLoading = false;
        }
    }




    [RelayCommand]
    private async Task GetTravelRecordDetailAsync(bool onlyCurrentMonth)
    {
        IsLoading = true;
        try
        {
            var role = _hoyolabService.GetLastSelectedOrFirstGenshinRoleInfo();
            if (role is null)
            {
                NotificationProvider.Warning("没有找到已添加的账号");
                return;
            }
            else
            {
                var now = DateTime.UtcNow.AddHours(8);
                var month = now.Month;
                await _hoyolabService.GetTravelNotesSummaryAsync(role, month);
                var primogemsAddCount = await _hoyolabService.GetTravelRecordDetailAsync(role, month, TravelNotesAwardType.Primogems);
                var moraAddCount = await _hoyolabService.GetTravelRecordDetailAsync(role, month, TravelNotesAwardType.Mora);
                NotificationProvider.Success($"{role.Nickname} 的 {month} 月旅行札记新增原石收入记录 {primogemsAddCount} 条、摩拉收入记录 {moraAddCount} 条", 10000);
                if (!onlyCurrentMonth)
                {
                    month = now.AddMonths(-1).Month;
                    await _hoyolabService.GetTravelNotesSummaryAsync(role, month);
                    primogemsAddCount = await _hoyolabService.GetTravelRecordDetailAsync(role, month, TravelNotesAwardType.Primogems);
                    moraAddCount = await _hoyolabService.GetTravelRecordDetailAsync(role, month, TravelNotesAwardType.Mora);
                    NotificationProvider.Success($"{role.Nickname} 的 {month} 月旅行札记新增原石收入记录 {primogemsAddCount} 条、摩拉收入记录 {moraAddCount} 条", 10000);
                    month = now.AddMonths(-2).Month;
                    await _hoyolabService.GetTravelNotesSummaryAsync(role, month);
                    primogemsAddCount = await _hoyolabService.GetTravelRecordDetailAsync(role, month, TravelNotesAwardType.Primogems);
                    moraAddCount = await _hoyolabService.GetTravelRecordDetailAsync(role, month, TravelNotesAwardType.Mora);
                    NotificationProvider.Success($"{role.Nickname} 的 {month} 月旅行札记新增原石收入记录 {primogemsAddCount} 条、摩拉收入记录 {moraAddCount} 条", 10000);
                }
            }
        }
        catch (Exception ex)
        {
            NotificationProvider.Error(ex);
        }
        finally
        {
            IsLoading = false;
        }
        await InitializeDataAsync();
    }

}


