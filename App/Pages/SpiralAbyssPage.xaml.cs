using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Xunkong.Hoyolab.SpiralAbyss;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace Xunkong.Desktop.Pages;

/// <summary>
/// An empty page that can be used on its own or navigated to within a Frame.
/// </summary>
[INotifyPropertyChanged]
public sealed partial class SpiralAbyssPage : Page
{

    static SpiralAbyssPage()
    {
        TypeAdapterConfig<SpiralAbyssInfo, SpiralAbyssPage_AbyssInfo>.NewConfig()
                                                                     .Map(dest => dest.DamageRank, src => src.DamageRank.FirstOrDefault())
                                                                     .Map(dest => dest.DefeatRank, src => src.DefeatRank.FirstOrDefault())
                                                                     .Map(dest => dest.EnergySkillRank, src => src.EnergySkillRank.FirstOrDefault())
                                                                     .Map(dest => dest.NormalSkillRank, src => src.NormalSkillRank.FirstOrDefault())
                                                                     .Map(dest => dest.TakeDamageRank, src => src.TakeDamageRank.FirstOrDefault())
                                                                     .Map(dest => dest.Floors, src => src.Floors.Where(x => x.Index > 8).ToList());
    }


    private readonly HoyolabService _hoyolabService;


    public SpiralAbyssPage()
    {
        this.InitializeComponent();
        _hoyolabService = ServiceProvider.GetService<HoyolabService>()!;
        Loaded += SpiralAbyssPage_Loaded;
    }



    [ObservableProperty]
    private List<SpiralAbyssPage_LeftPanel> leftPanels;

    [ObservableProperty]
    private SpiralAbyssPage_AbyssInfo? selectedAbyssInfo;

    private Dictionary<int, SpiralAbyssPage_AbyssInfo> abyssDic = new();


    private void SpiralAbyssPage_Loaded(object sender, RoutedEventArgs e)
    {
        InitializeData();
    }


    public void InitializeData()
    {
        try
        {
            var role = _hoyolabService.GetLastSelectedOrFirstGenshinRoleInfo();
            if (role == null)
            {
                return;
            }
            var abyssInfos = _hoyolabService.GetSpiralAbyssInfos(role.Uid);
            LeftPanels = abyssInfos.Adapt<List<SpiralAbyssPage_LeftPanel>>();
        }
        catch (Exception ex)
        {
            NotificationProvider.Error(ex);
        }
    }


    [RelayCommand]
    public async Task GetSpiralAbyssDataAsync()
    {
        try
        {
            var role = _hoyolabService.GetLastSelectedOrFirstGenshinRoleInfo();
            if (role is null)
            {
                NotificationProvider.Warning("没有找到账号信息");
                return;
            }
            var last = await _hoyolabService.GetSpiralAbyssInfoAsync(role, 2);
            var current = await _hoyolabService.GetSpiralAbyssInfoAsync(role, 1);
            SelectedAbyssInfo = null;
            abyssDic.Clear();
            InitializeData();
            NotificationProvider.Success($"已获取 {role.Nickname} 最新的深渊记录");
        }
        catch (Exception ex)
        {
            NotificationProvider.Error(ex);
        }
    }



    private void _GridView_LeftPanel_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (_GridView_LeftPanel.SelectedItem is SpiralAbyssPage_LeftPanel leftInfo)
        {
            try
            {
                var id = leftInfo.Id;
                if (abyssDic.TryGetValue(id, out var info))
                {
                    SelectedAbyssInfo = info;
                }
                else
                {

                    var originInfo = _hoyolabService.GetSpiralAbyssInfo(id);
                    if (originInfo is not null)
                    {
                        SelectedAbyssInfo = originInfo.Adapt<SpiralAbyssPage_AbyssInfo>();
                        abyssDic.TryAdd(id, SelectedAbyssInfo);
                    }
                }
            }
            catch (Exception ex)
            {
                NotificationProvider.Error(ex);
            }
        }
    }



}
