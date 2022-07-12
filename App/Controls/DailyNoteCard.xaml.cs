using Microsoft.UI.Xaml.Controls;
using Xunkong.Hoyolab;
using Xunkong.Hoyolab.Account;
using Xunkong.Hoyolab.DailyNote;
using Xunkong.Hoyolab.TravelNotes;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace Xunkong.Desktop.Controls;

[INotifyPropertyChanged]
public sealed partial class DailyNoteCard : UserControl
{

    private readonly HoyolabClient _client;




    public DailyNoteCard()
    {
        this.InitializeComponent();
        _client = ServiceProvider.GetService<HoyolabClient>()!;
    }




    [ObservableProperty]
    private HoyolabUserInfo hoyolabUserInfo;


    [ObservableProperty]
    private GenshinRoleInfo genshinRoleInfo;


    [ObservableProperty]
    private DailyNoteInfo? dailyNoteInfo;

    [ObservableProperty]
    private TravelNotesDayData? travelNotesDayData;

    [ObservableProperty]
    private bool error = true;

    [ObservableProperty]
    private string? errorMessage;



    partial void OnGenshinRoleInfoChanged(GenshinRoleInfo value)
    {
        if (value is null)
        {
            return;
        }
        DispatcherQueue.TryEnqueue(async () =>
        {
            try
            {
                DailyNoteInfo = await _client.GetDailyNoteAsync(value);
                TravelNotesDayData = (await _client.GetTravelNotesSummaryAsync(value, 0)).DayData;
                Error = false;
            }
            catch (Exception ex)
            {
                await Task.Delay(100);
                Error = true;
                errorMessage = ex.Message;
            }
        });
    }


    [RelayCommand(AllowConcurrentExecutions = false)]
    private async Task RefreshAsync()
    {
        if (GenshinRoleInfo is null)
        {
            return;
        }
        try
        {
            DailyNoteInfo = await _client.GetDailyNoteAsync(GenshinRoleInfo);
            TravelNotesDayData = (await _client.GetTravelNotesSummaryAsync(GenshinRoleInfo, 0)).DayData;
            Error = false;
        }
        catch (Exception ex)
        {
            Error = true;
            errorMessage = ex.Message;
        }
    }




}
