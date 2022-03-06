using Xunkong.Core.TravelRecord;

namespace Xunkong.Desktop.ViewModels
{
    internal partial class TravelNotesViewModel : ObservableObject
    {

        private readonly ILogger<TravelNotesViewModel> _logger;

        private readonly IDbContextFactory<XunkongDbContext> _ctxFactory;

        private readonly HoyolabService _hoyolabService;

        private readonly UserSettingService _settingService;



        public TravelNotesViewModel(ILogger<TravelNotesViewModel> logger, IDbContextFactory<XunkongDbContext> ctxFactory, HoyolabService hoyolabService, UserSettingService settingService)
        {
            _logger = logger;
            _ctxFactory = ctxFactory;
            _hoyolabService = hoyolabService;
            _settingService = settingService;
        }


        private bool _IsLoading;
        public bool IsLoading
        {
            get => _IsLoading;
            set => SetProperty(ref _IsLoading, value);
        }




        private TravelNotes_DayOrMonthStats? _DayStats;
        public TravelNotes_DayOrMonthStats? DayStats
        {
            get => _DayStats;
            set => SetProperty(ref _DayStats, value);
        }



        private TravelNotes_DayOrMonthStats? _MonthStats;
        public TravelNotes_DayOrMonthStats? MonthStats
        {
            get => _MonthStats;
            set => SetProperty(ref _MonthStats, value);
        }




        private List<TravelRecordPrimogemsMonthGroupStats>? _PrimogemsGroup;
        public List<TravelRecordPrimogemsMonthGroupStats>? PrimogemsGroup
        {
            get => _PrimogemsGroup;
            set => SetProperty(ref _PrimogemsGroup, value);
        }



        private List<TravelNotes_DayData> _RecentDayData;
        public List<TravelNotes_DayData> RecentDayData
        {
            get => _RecentDayData;
            set => SetProperty(ref _RecentDayData, value);
        }





        private List<TravelNotes_MonthData> _AllMonthData;
        public List<TravelNotes_MonthData> AllMonthData
        {
            get => _AllMonthData;
            set => SetProperty(ref _AllMonthData, value);
        }




        private List<TravelRecordAwardItem> _PrimogemsAwardItems;
        public List<TravelRecordAwardItem> PrimogemsAwardItems
        {
            get => _PrimogemsAwardItems;
            set => SetProperty(ref _PrimogemsAwardItems, value);
        }




        private List<TravelRecordAwardItem> _MoraAwardItems;
        public List<TravelRecordAwardItem> MoraAwardItems
        {
            get => _MoraAwardItems;
            set => SetProperty(ref _MoraAwardItems, value);
        }




        [ICommand(AllowConcurrentExecutions = false)]
        public async Task InitializeDataAsync()
        {
            IsLoading = true;
            await Task.Delay(100);
            try
            {
                var role = await _hoyolabService.GetLastSelectedOrFirstUserGameRoleInfoAsync();
                if (role is null)
                {
                    InfoBarHelper.Warning("没有找到已添加的账号");
                    return;
                }
                else
                {
                    var summary = await _hoyolabService.GetTravelRecordSummaryAsync(role, DateTime.UtcNow.AddHours(8).Month);
                    DayStats = summary?.DayData?.Adapt<TravelNotes_DayOrMonthStats>();
                    MonthStats = summary?.MonthData?.Adapt<TravelNotes_DayOrMonthStats>();
                    PrimogemsGroup = summary?.MonthData?.PrimogemsGroupBy;
                    using var ctx = _ctxFactory.CreateDbContext();
                    var minTime = DateTime.UtcNow.AddDays(-30).AddHours(12);
                    var recentDays = await ctx.TravelRecordAwardItems.AsNoTracking().Where(x => x.Uid == role.Uid && x.Time >= minTime).ToListAsync();
                    var group = recentDays.GroupBy(x => x.Time.Date).OrderBy(x => x.Key);
                    var dayDatas = new List<TravelNotes_DayData>(30);
                    foreach (var item in group)
                    {
                        var primogems = item.Where(x => x.Type == TravelRecordAwardType.Primogems).Sum(x => x.Number);
                        var mora = item.Where(x => x.Type == TravelRecordAwardType.Mora).Sum(x => x.Number);
                        var dayData = new TravelNotes_DayData(item.Key.ToString("MM-dd"), primogems, mora);
                        dayDatas.Add(dayData);
                    }
                    RecentDayData = dayDatas;
                    AllMonthData = (await ctx.TravelRecordMonthDatas.AsNoTracking().Where(x => x.Uid == role.Uid).OrderBy(x => x.Year).ThenBy(x => x.Month).ToListAsync()).Adapt<List<TravelNotes_MonthData>>();
                    PrimogemsAwardItems = await ctx.TravelRecordAwardItems.AsNoTracking().Where(x => x.Uid == role.Uid && x.Time >= minTime && x.Type == TravelRecordAwardType.Primogems).ToListAsync();
                    MoraAwardItems = await ctx.TravelRecordAwardItems.AsNoTracking().Where(x => x.Uid == role.Uid && x.Time >= minTime && x.Type == TravelRecordAwardType.Mora).ToListAsync();
                }
            }
            catch (Exception ex)
            {
                InfoBarHelper.Error(ex);
            }
            finally
            {
                IsLoading = false;
            }
        }




        [ICommand(AllowConcurrentExecutions = false)]
        private async Task GetTravelRecordDetailAsync(bool onlyCurrentMonth)
        {
            IsLoading = true;
            try
            {
                var role = await _hoyolabService.GetLastSelectedOrFirstUserGameRoleInfoAsync();
                if (role is null)
                {
                    InfoBarHelper.Warning("没有找到已添加的账号");
                    return;
                }
                else
                {
                    var now = DateTime.UtcNow.AddHours(8);
                    var month = now.Month;
                    var primogemsAddCount = await _hoyolabService.GetTravelRecordDetailAsync(role, month, TravelRecordAwardType.Primogems);
                    var moraAddCount = await _hoyolabService.GetTravelRecordDetailAsync(role, month, TravelRecordAwardType.Mora);
                    InfoBarHelper.Success($"{role.Nickname} 的 {month} 月旅行札记新增原石收入记录 {primogemsAddCount} 条、摩拉收入记录 {moraAddCount} 条", 10000);
                    if (!onlyCurrentMonth)
                    {
                        month = now.AddMonths(-1).Month;
                        await _hoyolabService.GetTravelRecordDetailAsync(role, month, TravelRecordAwardType.Primogems);
                        await _hoyolabService.GetTravelRecordDetailAsync(role, month, TravelRecordAwardType.Mora);
                        InfoBarHelper.Success($"{role.Nickname} 的 {month} 月旅行札记新增原石收入记录 {primogemsAddCount} 条、摩拉收入记录 {moraAddCount} 条", 10000);
                        month = now.AddMonths(-2).Month;
                        await _hoyolabService.GetTravelRecordDetailAsync(role, month, TravelRecordAwardType.Primogems);
                        await _hoyolabService.GetTravelRecordDetailAsync(role, month, TravelRecordAwardType.Mora);
                        InfoBarHelper.Success($"{role.Nickname} 的 {month} 月旅行札记新增原石收入记录 {primogemsAddCount} 条、摩拉收入记录 {moraAddCount} 条", 10000);
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Get travel record detail.");
                InfoBarHelper.Error(ex);
            }
            finally
            {
                IsLoading = false;
            }
            await InitializeDataAsync();
        }






    }
}
