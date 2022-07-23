using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Controls.Primitives;
using Microsoft.UI.Xaml.Documents;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using System.Numerics;
using System.Text.RegularExpressions;
using Windows.Foundation;
using Windows.UI;
using Xunkong.GenshinData.Character;
using Xunkong.GenshinData.Weapon;
using Xunkong.Hoyolab.Account;
using Xunkong.Hoyolab.Avatar;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace Xunkong.Desktop.Pages;

/// <summary>
/// An empty page that can be used on its own or navigated to within a Frame.
/// </summary>
[INotifyPropertyChanged]
public sealed partial class CharacterInfoPage : Page
{


    static CharacterInfoPage()
    {
        TypeAdapterConfig<AvatarConstellation, CharacterInfoPage_Constellation>.NewConfig().Map(dest => dest.Description, src => src.Effect);
    }


    // todo 天赋命座背景装饰图
    // UI_Img_TalentActive.png
    // UI_Img_TalentUnactive.png


    private readonly HoyolabService _houselabService;

    private readonly WishlogService _wishlogService;

    private readonly XunkongApiService _xunkongApiService;


    /// <summary>
    /// 上方滚动条内的 ScrollViewer 控件
    /// </summary>
    private ScrollViewer? _ScrollViewer_SideIcon;


    public CharacterInfoPage()
    {
        this.InitializeComponent();
        _houselabService = ServiceProvider.GetService<HoyolabService>()!;
        _wishlogService = ServiceProvider.GetService<WishlogService>()!;
        _xunkongApiService = ServiceProvider.GetService<XunkongApiService>()!;
        Loaded += CharacterInfoPage_Loaded;
    }


    private async void CharacterInfoPage_Loaded(object sender, RoutedEventArgs e)
    {
        _ScrollViewer_SideIcon = (VisualTreeHelper.GetChild(_ListBox_SideIcon, 0) as Border)?.Child as ScrollViewer;
        await InitializeDataAsync();
    }

    /// <summary>
    /// 上方滚动条的鼠标操作
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private void _ListBox_SideIcon_PointerWheelChanged(object sender, PointerRoutedEventArgs e)
    {
        var pointer = e.GetCurrentPoint(_ListBox_SideIcon);
        var delta = pointer.Properties.MouseWheelDelta;
        _ScrollViewer_SideIcon?.ScrollToHorizontalOffset(_ScrollViewer_SideIcon.HorizontalOffset + delta);
    }


    /// <summary>
    /// 弧形化天赋和命之座列表
    /// </summary>
    private void ArcList()
    {
        var width = _Grid_GachaSplash.ActualWidth;
        var height = _Grid_GachaSplash.ActualHeight;
        for (int i = 0; i < _ItemsControl_Talents.Items.Count; i++)
        {
            var element = _ItemsControl_Talents.ContainerFromIndex(i) as UIElement;
            if (element is null)
            {
                break;
            }
            if (double.IsNaN(element.Translation.X))
            {
                element.Translation = new Vector3();
            }
            var pos = element.TransformToVisual(_Grid_GachaSplash).TransformPoint(new Point());
            var x = pos.X - 400 - element.Translation.X;
            var y = height / 2 - pos.Y;
            var deltaX = Math.Sqrt(Math.Pow(x, 2) - Math.Pow(y, 2)) - x;
            element.Translation = new Vector3((float)deltaX, 0, 0);
        }
        for (int i = 0; i < _ItemsControl_Constellations.Items.Count; i++)
        {
            var element = _ItemsControl_Constellations.ContainerFromIndex(i) as UIElement;
            if (element is null)
            {
                break;
            }
            if (double.IsNaN(element.Translation.X))
            {
                element.Translation = new Vector3();
            }
            var pos = element.TransformToVisual(_Grid_GachaSplash).TransformPoint(new Point());
            var x = pos.X - 550 - element.Translation.X;
            var y = pos.Y - height / 2;
            var deltaX = Math.Sqrt(Math.Pow(x, 2) - Math.Pow(y, 2)) - x;
            element.Translation = new Vector3((float)deltaX, 0, 0);
        }
    }


    /// <summary>
    /// 切换角色时重新弧形化天赋和命之座列表
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private void _Character_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        ArcList();
    }


    /// <summary>
    /// 点击天赋和命之座时弹出相关介绍
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private void ShowAttachedFlyout(object sender, TappedRoutedEventArgs e)
    {
        var stack = sender as StackPanel;
        var flyout = FlyoutBase.GetAttachedFlyout(sender as FrameworkElement);
        var desc = (stack?.DataContext as CharacterInfoPage_Constellation)?.Description;
        if (desc is null)
        {
            desc = (stack?.DataContext as CharacterTalentInfo)?.Description;
        }
        if (desc is not null)
        {
            // 解析文字颜色
            try
            {
                var text = new TextBlock { TextWrapping = TextWrapping.Wrap, MaxWidth = 400 };
                var subs = desc.Split("\\n");
                foreach (var sub in subs)
                {
                    var matches = Regex.Matches(sub, @"<color=([^>]+)>([^<]+)</color>");
                    string head, remain = sub;
                    foreach (Match match in matches)
                    {
                        var origin = match.Groups[0].Value;
                        head = remain.Substring(0, remain.IndexOf(origin));
                        remain = remain.Substring(remain.IndexOf(origin) + origin.Length);
                        if (!string.IsNullOrWhiteSpace(head))
                        {
                            text.Inlines.Add(new Run { Text = head });
                        }
                        var colorStr = match.Groups[1].Value;
                        var content = match.Groups[2].Value;
                        var color = System.Drawing.ColorTranslator.FromHtml(colorStr.Substring(0, 7));
                        text.Inlines.Add(new Run
                        {
                            Text = content,
                            Foreground = new SolidColorBrush(Color.FromArgb(color.A, color.R, color.G, color.B)),
                        });
                    }
                    if (!string.IsNullOrWhiteSpace(remain))
                    {
                        if (remain.Contains("<i>"))
                        {
                            var quote = remain.Replace("<i>", "").Replace("</i>", "");
                            text.Inlines.Add(new Run { Text = quote, FontFamily = new FontFamily("楷体") });
                        }
                        else
                        {
                            text.Inlines.Add(new Run { Text = remain });
                        }
                    }
                    text.Inlines.Add(new LineBreak());
                }
                if (text.Inlines.LastOrDefault() is LineBreak @break)
                {
                    text.Inlines.Remove(@break);
                }
                if (flyout is Flyout f)
                {
                    f.Content = text;
                }
            }
            catch (Exception ex)
            {

            }
        }
        FlyoutBase.ShowAttachedFlyout(sender as FrameworkElement);
    }




    private GenshinRoleInfo? _role;


    private List<CharacterInfoPage_Character> _Characters;
    public List<CharacterInfoPage_Character> Characters
    {
        get => _Characters;
        set => SetProperty(ref _Characters, value);
    }



    private CharacterInfoPage_Character? _SelectedCharacter;
    public CharacterInfoPage_Character? SelectedCharacter
    {
        get => _SelectedCharacter;
        set
        {
            SetProperty(ref _SelectedCharacter, value);
            GetCharacterSkillLevelAsync(value);
        }
    }


    private bool _IsRefreshPageTeachingTipOpen;
    public bool IsRefreshPageTeachingTipOpen
    {
        get => _IsRefreshPageTeachingTipOpen;
        set => SetProperty(ref _IsRefreshPageTeachingTipOpen, value);
    }



    [RelayCommand]
    public async Task InitializeDataAsync()
    {
        try
        {
            using var liteDb = DatabaseProvider.CreateLiteDB();
            var role = _houselabService.GetLastSelectedOrFirstGenshinRoleInfo();
            List<AvatarDetail> avatars;
            if (role == null)
            {
                avatars = new();
            }
            else
            {
                _role = role;
                avatars = await _houselabService.GetAvatarDetailsAsync(role!);
            }
            var info_characters = liteDb.GetCollection<CharacterInfo>().FindAll().ToList();
            var characters = info_characters.Adapt<List<CharacterInfoPage_Character>>();
            var weaponDic = liteDb.GetCollection<WeaponInfo>().FindAll().ToDictionary(x => x.Id, x => x.AwakenIcon);
            var wishlogs = _wishlogService.GetWishlogItemExByUid(role?.Uid ?? 0);
            var matches = from a in avatars join c in characters on a.Id equals c.Id select (a, c);
            int exceptId = 0;
            foreach (var item in matches)
            {
                item.c.IsOwn = true;
                item.c.Level = item.a.Level;
                item.c.Fetter = item.a.Fetter;
                item.c.ActivedConstellationNumber = item.a.ActivedConstellationNumber;
                item.c.Weapon = item.a.Weapon.Adapt<CharacterInfoPage_Weapon>();
                item.c.Weapon.Description = item.c.Weapon.Description.Replace("\\n", "\n");
                weaponDic.TryGetValue(item.c.Weapon.Id, out var gachaIcon);
                item.c.Weapon.AwakenIcon = gachaIcon!;
                item.c.Reliquaries = item.a.Reliquaries;
                var cons = item.c.Constellations.OrderBy(x => x.Id).ToList();
                cons.Take(item.a.ActivedConstellationNumber).ToList().ForEach(x => x.IsActived = true);
                item.c.Constellations = cons;
                var thisWishlog = wishlogs.Where(x => x.Name == item.c.Name).OrderByDescending(x => x.Id).ToList();
                item.c.Wishlogs = thisWishlog.Any() ? thisWishlog : null;
                // 哥哥或妹妹，只显示一个
                if (item.c.Id is 10000005 or 10000007)
                {
                    item.c.Element = item.a.Element;
                    item.c.Talents = new();
                    item.c.Constellations = item.a.Constellations.Adapt<List<CharacterInfoPage_Constellation>>();
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
            NotificationProvider.Error(ex);
        }
        finally
        {
            await Task.Delay(100);
            ArcList();
        }
    }


    /// <summary>
    /// 获取角色天赋等级
    /// </summary>
    /// <param name="character"></param>
    private async void GetCharacterSkillLevelAsync(CharacterInfoPage_Character? character)
    {
        if (_role is null || character is null || !character.IsOwn || character.GotSkillLevel || character.IsGettingSkillLevel)
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
            NotificationProvider.Error(ex);
        }
        finally
        {
            character.IsGettingSkillLevel = false;
        }

    }


    /// <summary>
    /// 获取角色武器卡池数据
    /// </summary>
    /// <returns></returns>
    [RelayCommand]
    private async Task GetGenshinDataAsync()
    {
        try
        {
            await _xunkongApiService.GetAllGenshinDataFromServerAsync();
            IsRefreshPageTeachingTipOpen = true;
        }
        catch (Exception ex)
        {
            NotificationProvider.Error(ex);
        }
    }




}
