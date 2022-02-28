namespace Xunkong.Desktop.ViewModels
{
    internal class WishEventStatsViewModel : ObservableObject
    {


        private readonly ILogger<WishEventStatsViewModel> _logger;

        private readonly WishlogService _wishlogService;

        private readonly XunkongApiService _xunkongApiService;

        public WishEventStatsViewModel(ILogger<WishEventStatsViewModel> logger, WishlogService wishlogService, XunkongApiService xunkongApiService)
        {
            _logger = logger;
            _wishlogService = wishlogService;
            _xunkongApiService = xunkongApiService;
        }


        private ObservableCollection<WishEventStatsModel> _WishEventStatsList;
        public ObservableCollection<WishEventStatsModel> WishEventStatsList
        {
            get => _WishEventStatsList;
            set => SetProperty(ref _WishEventStatsList, value);
        }


        private bool _IsLoading;
        public bool IsLoading
        {
            get => _IsLoading;
            set => SetProperty(ref _IsLoading, value);
        }



        public async Task InitializeCharacterDataAsync(int uid = 0)
        {
            try
            {
                IsLoading = true;
                WishEventStatsList?.Clear();
                await Task.Delay(100);
                var stats = await _wishlogService.GetCharacterWishEventStatsModelsByUidAsync(uid);
                var collection = new ObservableCollection<WishEventStatsModel>(stats ?? new());
                WishEventStatsList = collection;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Init character wish event stats.");
                InfoBarHelper.Error(ex);
            }
            finally
            {
                IsLoading = false;
            }
        }


        public async Task InitializeWeaponDataAsync(int uid = 0)
        {
            try
            {
                IsLoading = true;
                WishEventStatsList?.Clear();
                await Task.Delay(100);
                var stats = await _wishlogService.GetWeaponWishEventStatsModelsByUidAsync(uid);
                var collection = new ObservableCollection<WishEventStatsModel>(stats ?? new());
                WishEventStatsList = collection;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Init weapon wish event stats.");
                InfoBarHelper.Error(ex);
            }
            finally
            {
                IsLoading = false;
            }
        }





    }
}
