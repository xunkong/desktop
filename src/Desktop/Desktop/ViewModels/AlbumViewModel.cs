using Microsoft.Win32;
using System.Text.RegularExpressions;

namespace Xunkong.Desktop.ViewModels
{
    internal partial class AlbumViewModel : ObservableObject
    {

        private const string REG_PATH = @"HKEY_LOCAL_MACHINE\SOFTWARE\Microsoft\Windows\CurrentVersion\Uninstall\原神\";
        private const string REG_KEY = "InstallPath";


        private readonly ILogger<AlbumViewModel> _logger;

        private string? albumFolder;

        private FileSystemWatcher _watcher;


        public AlbumViewModel(ILogger<AlbumViewModel> logger)
        {
            _logger = logger;
            _watcher = new();
            _watcher.Filter = "*.png";
            _watcher.Created += FileSystemWatcher_Created;
        }

        private void FileSystemWatcher_Created(object sender, FileSystemEventArgs e)
        {
            if (e.ChangeType == WatcherChangeTypes.Created)
            {
                _logger.LogInformation($"New image: {e.FullPath}");
                var fileInfo = new FileInfo(e.FullPath);
                if (fileInfo.Exists && fileInfo.Extension.ToLower() == ".png")
                {
                    var dq = MainWindow.Current.DispatcherQueue;
                    if (latestImages is null)
                    {
                        latestImages = new("新增", new[] { fileInfo });
                        dq.TryEnqueue(() => ImageGroupList?.Insert(0, latestImages));
                    }
                    else
                    {
                        dq.TryEnqueue(() => latestImages.Add(fileInfo));
                    }
                    dq.TryEnqueue(() => ImageList.Insert(latestImages.Count - 1, fileInfo));
                }
            }
        }


        private ImageGroupCollection latestImages;


        private ObservableCollection<ImageGroupCollection> _ImageGroupList;
        public ObservableCollection<ImageGroupCollection> ImageGroupList
        {
            get => _ImageGroupList;
            set => SetProperty(ref _ImageGroupList, value);
        }


        private ObservableCollection<FileInfo> _ImageList;
        public ObservableCollection<FileInfo> ImageList
        {
            get => _ImageList;
            set => SetProperty(ref _ImageList, value);
        }



        public async Task InitializeDataAsync()
        {
            await Task.Delay(100);
            if (!string.IsNullOrWhiteSpace(albumFolder))
            {
                return;
            }
            try
            {
                var exePath = LocalSettingHelper.GetSetting<string>(SettingKeys.GameExePath);
                if (!string.IsNullOrWhiteSpace(exePath))
                {
                    _logger.LogInformation($"Exe path: {exePath}.");
                    var path = Path.GetDirectoryName(exePath);
                    var screenPath = Path.Combine(path!, "ScreenShot");
                    if (Directory.Exists(screenPath))
                    {
                        albumFolder = screenPath;
                        _logger.LogInformation($"Album folder: {albumFolder}");
                    }
                }
                else
                {
                    var launcherPath = Registry.GetValue(REG_PATH, REG_KEY, null) as string;
                    if (string.IsNullOrWhiteSpace(launcherPath))
                    {
                        InfoBarHelper.Warning("Cannot find genshin launcher folder.");
                        return;
                    }
                    _logger.LogTrace($"Launcher path: {launcherPath}");
                    var configPath = Path.Combine(launcherPath, "config.ini");
                    if (!File.Exists(configPath))
                    {
                        InfoBarHelper.Warning("Cannot find config file.");
                        return;
                    }
                    var str = await File.ReadAllTextAsync(configPath);
                    var gamePath = Regex.Match(str, @"game_install_path=(.+)").Groups[1].Value.Trim();
                    if (string.IsNullOrWhiteSpace(gamePath))
                    {
                        InfoBarHelper.Warning("Cannot find game path.");
                        return;
                    }
                    _logger.LogTrace($"Game path: {gamePath}");
                    albumFolder = Path.Combine(gamePath, "ScreenShot");
                    if (!Directory.Exists(albumFolder))
                    {
                        InfoBarHelper.Warning("Cannot find screenshot foler.");
                        return;
                    }
                    _logger.LogInformation($"Album folder: {albumFolder}");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Get genshin screenshot path.");
                InfoBarHelper.Error(ex);
            }
            if (Directory.Exists(albumFolder))
            {
                _watcher.Path = albumFolder;
                _watcher.EnableRaisingEvents = true;
                GetAllImages();
            }
        }



        private void GetAllImages()
        {
            if (Directory.Exists(albumFolder))
            {
                try
                {
                    var folderInfo = new DirectoryInfo(albumFolder);
                    var fileInfos = folderInfo.GetFiles();
                    var queryList = fileInfos.Where(x => x.Extension.ToLower() == ".png").OrderByDescending(x => x.CreationTime);
                    ImageList = new(queryList);
                    var queryGroup = ImageList.GroupBy(x => x.CreationTime.ToString("yyyy-MM")).Select(x => new ImageGroupCollection(x.Key, x)).OrderByDescending(x => x.Header);
                    ImageGroupList = new(queryGroup);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Get genshin screenshot images.");
                    InfoBarHelper.Error(ex);
                }
            }
        }



        [ICommand]
        private void OpenImageFolder()
        {
            if (!Directory.Exists(albumFolder))
            {
                InfoBarHelper.Warning("Cannot find screenshot folder.");
                return;
            }
            try
            {
                Process.Start(new ProcessStartInfo
                {
                    FileName = albumFolder,
                    UseShellExecute = true,
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Open genshin screenshot folder.");
                InfoBarHelper.Error(ex);
            }
        }






        internal class ImageGroupCollection : ObservableCollection<FileInfo>
        {

            public string Header { get; set; }


            public ImageGroupCollection(string header, IEnumerable<FileInfo> list) : base(list)
            {
                Header = header;
            }

        }


    }
}
