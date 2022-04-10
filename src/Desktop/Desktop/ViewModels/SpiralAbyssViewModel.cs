using Xunkong.Core.SpiralAbyss;

namespace Xunkong.Desktop.ViewModels
{
    [ObservableObject]
    internal partial class SpiralAbyssViewModel
    {


        private readonly ILogger<SpiralAbyssViewModel> _logger;


        private readonly IDbContextFactory<XunkongDbContext> _ctxFactory;


        private readonly HoyolabService _hoyolabService;



        static SpiralAbyssViewModel()
        {
            TypeAdapterConfig<SpiralAbyssInfo, SpiralAbyss_AbyssInfo>.NewConfig()
                                                                     .Map(dest => dest.DamageRank, src => src.DamageRank.FirstOrDefault())
                                                                     .Map(dest => dest.DefeatRank, src => src.DamageRank.FirstOrDefault())
                                                                     .Map(dest => dest.EnergySkillRank, src => src.EnergySkillRank.FirstOrDefault())
                                                                     .Map(dest => dest.NormalSkillRank, src => src.NormalSkillRank.FirstOrDefault())
                                                                     .Map(dest => dest.TakeDamageRank, src => src.TakeDamageRank.FirstOrDefault());
        }


        public SpiralAbyssViewModel(ILogger<SpiralAbyssViewModel> logger, IDbContextFactory<XunkongDbContext> ctxFactory, HoyolabService hoyolabService)
        {
            _logger = logger;
            _ctxFactory = ctxFactory;
            _hoyolabService = hoyolabService;
        }


        [ObservableProperty]
        private List<SpiralAbyss_LeftPanel> leftPanels;

        [ObservableProperty]
        private SpiralAbyss_AbyssInfo? selectedAbyssInfo;



        public async Task InitializeDataAsync()
        {
            try
            {
                var role = await _hoyolabService.GetLastSelectedOrFirstUserGameRoleInfoAsync();
                if (role == null)
                {
                    return;
                }
                using var ctx = _ctxFactory.CreateDbContext();
                var abyssInfos = await ctx.SpiralAbyssInfos.AsNoTracking().Where(x => x.Uid == role.Uid && x.TotalBattleCount > 0).OrderByDescending(x => x.ScheduleId).ToListAsync();
                LeftPanels = abyssInfos.Adapt<List<SpiralAbyss_LeftPanel>>();
            }
            catch (Exception ex)
            {
                InfoBarHelper.Error(ex);
                _logger.LogError(ex, nameof(InitializeDataAsync));
            }
        }


        public async Task SelectedAbyssInfoChangedAsync(int id)
        {
            try
            {
                using var ctx = _ctxFactory.CreateDbContext();
                var info = await ctx.SpiralAbyssInfos.AsNoTracking()
                                                    .Where(x => x.Id == id)
                                                    .Include(x => x.DamageRank)
                                                    .Include(x => x.DefeatRank)
                                                    .Include(x => x.EnergySkillRank)
                                                    .Include(x => x.NormalSkillRank)
                                                    .Include(x => x.RevealRank)
                                                    .Include(x => x.TakeDamageRank)
                                                    .Include(x => x.Floors)
                                                    .ThenInclude(x => x.Levels)
                                                    .ThenInclude(x => x.Battles)
                                                    .ThenInclude(x => x.Avatars)
                                                    .FirstOrDefaultAsync();
                if (info is null)
                {
                    return;
                }
                SelectedAbyssInfo = info.Adapt<SpiralAbyss_AbyssInfo>();


            }
            catch (Exception ex)
            {
                InfoBarHelper.Error(ex);
                _logger.LogError(ex, nameof(SelectedAbyssInfoChangedAsync));
            }
        }



        [ICommand(AllowConcurrentExecutions = false)]
        public async Task GetSpiralAbyssDataAsync()
        {
            try
            {
                var role = await _hoyolabService.GetLastSelectedOrFirstUserGameRoleInfoAsync();
                if (role is null)
                {
                    InfoBarHelper.Warning("没有找到账号信息");
                    return;
                }
                var l = await _hoyolabService.GetSpiralAbyssInfoAsync(role, 2);
                var c = await _hoyolabService.GetSpiralAbyssInfoAsync(role, 1);
                SelectedAbyssInfo = null;
                await InitializeDataAsync();
                InfoBarHelper.Success($"已获取 {role.Nickname} 最新的深渊记录");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, nameof(GetSpiralAbyssDataAsync));
                InfoBarHelper.Error(ex);
            }
        }




    }
}
