using Xunkong.Core.Hoyolab;

namespace Xunkong.Desktop.ViewModels
{
    internal partial class CharacterInfoViewModel : ObservableObject
    {

        private readonly ILogger<CharacterInfoViewModel> _logger;

        private readonly IDbContextFactory<XunkongDbContext> _ctxFactory;

        private readonly HoyolabService _houselabService;

        private readonly WishlogService _wishlogService;



        static CharacterInfoViewModel()
        {
            TypeAdapterConfig<AvatarConstellation, CharacterInfo_Constellation>.NewConfig().Map(dest => dest.Description, src => src.Effect);
        }


        public CharacterInfoViewModel(ILogger<CharacterInfoViewModel> logger, IDbContextFactory<XunkongDbContext> ctxFactory, HoyolabService houselabService, WishlogService wishlogService)
        {
            _logger = logger;
            _ctxFactory = ctxFactory;
            _houselabService = houselabService;
            _wishlogService = wishlogService;
        }


        private UserGameRoleInfo? _role;


        private List<CharacterInfo_Character> _Characters;
        public List<CharacterInfo_Character> Characters
        {
            get => _Characters;
            set => SetProperty(ref _Characters, value);
        }



        private CharacterInfo_Character? _SelectedCharacter;
        public CharacterInfo_Character? SelectedCharacter
        {
            get => _SelectedCharacter;
            set
            {
                SetProperty(ref _SelectedCharacter, value);
                GetCharacterSkillLevelAsync(value);
            }
        }



        public async Task InitializeDataAsync()
        {
            try
            {
                using var ctx = _ctxFactory.CreateDbContext();
                var role = await _houselabService.GetLastSelectedOrFirstUserGameRoleInfoAsync();
                _role = role;
                var avatars = await _houselabService.GetAvatarDetailsAsync(role);
                var info_characters = await ctx.CharacterInfos.AsNoTracking().Include(x => x.Talents).Include(x => x.Constellations).ToListAsync();
                var characters = info_characters.Adapt<List<CharacterInfo_Character>>();
                var weaponDic = await ctx.WeaponInfos.AsNoTracking().ToDictionaryAsync(x => x.Id, x => x.GachaIcon);
                var wishlogs = await _wishlogService.GetWishlogItemExByUidAsync(role?.Uid ?? 0);
                var matches = from a in avatars join c in characters on a.Id equals c.Id select (a, c);
                int exceptId = 0;
                foreach (var item in matches)
                {
                    item.c.IsOwn = true;
                    item.c.Level = item.a.Level;
                    item.c.Fetter = item.a.Fetter;
                    item.c.ActivedConstellationNumber = item.a.ActivedConstellationNumber;
                    item.c.Weapon = item.a.Weapon.Adapt<CharacterInfo_Weapon>();
                    weaponDic.TryGetValue(item.c.Weapon.Id, out var gachaIcon);
                    item.c.Weapon.GachaIcon = gachaIcon!;
                    item.c.Reliquaries = item.a.Reliquaries;
                    var cons = item.c.Constellations.OrderBy(x => x.Id).ToList();
                    cons.Take(item.a.ActivedConstellationNumber).ToList().ForEach(x => x.IsActived = true);
                    item.c.Constellations = cons;
                    var thisWishlog = wishlogs.Where(x => x.Name == item.c.Name).OrderByDescending(x => x.Id).ToList();
                    item.c.Wishlogs = thisWishlog.Any() ? thisWishlog : null;
                    if (item.c.Id is 10000005 or 10000007)
                    {
                        item.c.Element = item.a.Element;
                        item.c.Talents = new();
                        item.c.Constellations = item.a.Constellations.Adapt<List<CharacterInfo_Constellation>>();
                        if (item.c.Id is 10000005)
                        {
                            exceptId = 10000007;
                        }
                        else
                        {
                            exceptId = 10000005;
                        }
                    }
                }
                Characters = characters.Where(x => x.Id != exceptId)
                                       .OrderByDescending(x => x.Level)
                                       .ThenByDescending(x => x.Fetter)
                                       .ThenByDescending(x => x.ActivedConstellationNumber)
                                       .ThenByDescending(x => x.Rarity)
                                       .ToList();
                SelectedCharacter = Characters.FirstOrDefault();
            }
            catch (Exception ex)
            {
                InfoBarHelper.Error(ex);
                _logger.LogError(ex, "Initialize data.");
            }
        }



        private async void GetCharacterSkillLevelAsync(CharacterInfo_Character? character)
        {
            if (_role is null || !character.IsOwn || character.GotSkillLevel || character.IsGettingSkillLevel || character is null)
            {
                return;
            }
            lock (character)
            {
                if (character.IsGettingSkillLevel)
                {
                    return;
                }
                else
                {
                    character.IsGettingSkillLevel = true;
                }
            }
            try
            {
                var skills = await _houselabService.GetAvatarSkillLevelAsync(_role, character.Id);
                var group = from t in character.Talents
                            join s in skills on t.TalentId equals s.Id
                            where s.MaxLevel > 1
                            select (t, s);
                foreach (var item in group)
                {
                    item.t.ShowSkillLevel = true;
                    item.t.CurrentLevel = item.s.CurrentLevel;
                }
                character.GotSkillLevel = true;
            }
            catch (Exception ex)
            {
                InfoBarHelper.Error(ex);
                _logger.LogError(ex, $"Get skill level with {character.Name} ({character.Id}).");
            }
            finally
            {
                character.IsGettingSkillLevel = false;
            }

        }




    }
}
