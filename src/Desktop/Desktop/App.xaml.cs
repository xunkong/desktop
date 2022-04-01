using Microsoft.Extensions.Http;
using Microsoft.UI.Xaml;
using Microsoft.Windows.AppLifecycle;
using Serilog;
using Serilog.Events;
using System.Text.Encodings.Web;
using Windows.Storage;
using Windows.UI.StartScreen;
using Xunkong.Core.Hoyolab;
using Xunkong.Core.Wish;
using Xunkong.Core.XunkongApi;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace Xunkong.Desktop
{
    /// <summary>
    /// Provides application-specific behavior to supplement the default Application class.
    /// </summary>
    public partial class App : Application
    {


        /// <summary>
        /// Initializes the singleton application object.  This is the first line of authored code
        /// executed, and as such is the logical equivalent of main() or WinMain().
        /// </summary>
        public App()
        {
            UnhandledException += App_UnhandledException;
            AppDomain.CurrentDomain.UnhandledException += CurrentDomain_UnhandledException;
            Services = ConfigureServiceProvider();
            InitializeApplicationTheme();
            InitializeJumpList();
            this.InitializeComponent();
        }


        private void App_UnhandledException(object sender, Microsoft.UI.Xaml.UnhandledExceptionEventArgs e)
        {
            Log.Fatal(e.Exception, "App crash.");
            Log.CloseAndFlush();
        }

        private void CurrentDomain_UnhandledException(object sender, System.UnhandledExceptionEventArgs e)
        {
            Log.Fatal(e.ExceptionObject as Exception, "App crash.");
            Log.CloseAndFlush();
        }

        /// <summary>
        /// Invoked when the application is launched normally by the end user.  Other entry points
        /// will be used such as when the application is launched to open a specific file.
        /// </summary>
        /// <param name="args">Details about the launch request and process.</param>
        protected override void OnLaunched(LaunchActivatedEventArgs _)
        {
            var args = AppInstance.GetCurrent().GetActivatedEventArgs();
            var data = args.Data as Windows.ApplicationModel.Activation.LaunchActivatedEventArgs;
            if (data != null)
            {
                if (!string.IsNullOrWhiteSpace(data.Arguments))
                {
                    LauncheWithArguement(data.Arguments);
                    return;
                }
            }
#if !DEBUG
            InitializeAppCenter.Initialize();
#endif
            m_window = new MainWindow();
            m_window.Activate();
        }



        private MainWindow m_window;


        public static new App Current => (App)Application.Current;


        public IServiceProvider Services { get; private set; }



        #region Configure Services


        private IServiceProvider ConfigureServiceProvider()
        {
            var sc = new ServiceCollection();

            ConfigureLogging(sc);
            ConfigureHttpClient(sc);
            ConfigureDatabase(sc);
            ConfigureServices(sc);

            sc.AddSingleton(_ => new JsonSerializerOptions { PropertyNameCaseInsensitive = true, Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping, WriteIndented = true });
            return sc.BuildServiceProvider();
        }



        private const string logTemplate = " {NewLine}[{Timestamp:HH:mm:ss.fff}] [{Level:u3}] {SourceContext}{NewLine}{Message}{NewLine}{Exception}";


        private void ConfigureLogging(ServiceCollection sc)
        {
            var now = DateTime.Now;
            var fxLogPath = Path.Combine(ApplicationData.Current.LocalCacheFolder.Path, $@"Xunkong\Log\Trace\trace_{now:yyyyMMdd}_{now:HHmmss}.txt");
            var fxLogger = new LoggerConfiguration().MinimumLevel.Verbose()
                                                    .MinimumLevel.Override("Microsoft.EntityFrameworkCore", LogEventLevel.Warning)
                                                    .WriteTo.Async(x => x.File(path: fxLogPath, outputTemplate: logTemplate, shared: true, retainedFileCountLimit: 1000))
                                                    .Enrich.FromLogContext()
                                                    .CreateLogger();
            var myLogPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), $@"Xunkong\Log\Log\log_{now:yyyyMMdd}_{now:HHmmss}.txt");
            var myLogger = new LoggerConfiguration().MinimumLevel.Verbose()
                                                    .MinimumLevel.Override("System", LogEventLevel.Warning)
                                                    .MinimumLevel.Override("Microsoft", LogEventLevel.Warning)
                                                    //.Filter.ByIncludingOnly("StartsWith(SourceContext, 'Xunkong')")
                                                    .WriteTo.Async(x => x.File(path: myLogPath, outputTemplate: logTemplate, shared: true, retainedFileCountLimit: 1000))
                                                    .Enrich.FromLogContext()
                                                    .CreateLogger();
            Log.Logger = myLogger;
            sc.AddLogging(logBuilder =>
            {
                logBuilder.AddSerilog(fxLogger, true);
                logBuilder.AddSerilog(myLogger, true);
            });
            Log.Information(XunkongEnvironment.GetLogHeader());
        }



        private void ConfigureHttpClient(ServiceCollection sc)
        {
            sc.Configure<HttpClientFactoryOptions>(options =>
            {
                options.HttpMessageHandlerBuilderActions.Add(builder =>
                    builder.PrimaryHandler = new HttpClientHandler
                    {
                        AutomaticDecompression = System.Net.DecompressionMethods.All,
                    });
            });
            sc.AddHttpClient<HoyolabClient>()
              .ConfigurePrimaryHttpMessageHandler(() => new HttpClientHandler { AutomaticDecompression = System.Net.DecompressionMethods.All });
            sc.AddHttpClient<WishlogClient>()
              .ConfigurePrimaryHttpMessageHandler(() => new HttpClientHandler { AutomaticDecompression = System.Net.DecompressionMethods.All });
            sc.AddHttpClient<XunkongApiClient>()
              .ConfigureHttpClient(client =>
              {
                  client.DefaultRequestHeaders.Add("User-Agent", $"XunkongDesktop/{XunkongEnvironment.AppVersion}");
                  client.DefaultRequestHeaders.Add("X-Platform", "Desktop");
                  client.DefaultRequestHeaders.Add("X-Channel", XunkongEnvironment.Channel.ToString());
                  client.DefaultRequestHeaders.Add("X-Version", XunkongEnvironment.AppVersion.ToString());
                  client.DefaultRequestHeaders.Add("X-Device-Id", XunkongEnvironment.DeviceId);
              })
              .ConfigurePrimaryHttpMessageHandler(() => new HttpClientHandler { AutomaticDecompression = System.Net.DecompressionMethods.All });
        }



        private void ConfigureDatabase(ServiceCollection sc)
        {
            var dbPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), @"Xunkong\Data\XunkongData.db");
            var sqlConStr = $"Data Source={dbPath};";
            sc.AddPooledDbContextFactory<XunkongDbContext>(options => options.UseSqlite(sqlConStr));
            sc.AddTransient(_ => new DbConnectionFactory<SqliteConnection>(sqlConStr));
            if (File.Exists(dbPath))
            {
                var vs = LocalSettingHelper.GetSetting<string>(SettingKeys.LastVersion);
                if (Version.TryParse(vs, out var lastVersion))
                {
                    Log.Information($"Last version is {lastVersion}.");
                    if (lastVersion >= XunkongEnvironment.AppVersion)
                    {
                        Log.Information("Last version is equal with or bigger than current version, don't need to migrate database.");
                        return;
                    }
                }
            }
            else
            {
                Log.Information("Database is not existed.");
                Directory.CreateDirectory(Path.GetDirectoryName(dbPath)!);
            }
            Log.Information("Start migrating database.");
            using var ctx = new XunkongDbContext(new DbContextOptionsBuilder<XunkongDbContext>().UseSqlite(sqlConStr).Options);
            ctx.Database.Migrate();
            LocalSettingHelper.SaveSetting(SettingKeys.LastVersion, XunkongEnvironment.AppVersion.ToString());
            Log.Information($"Migrate finished.");
        }



        private void ConfigureServices(ServiceCollection sc)
        {
            sc.AddSingleton<HoyolabService>();
            sc.AddSingleton<UserSettingService>();
            sc.AddSingleton<WishlogService>();
            sc.AddSingleton<XunkongApiService>();
            sc.AddSingleton<BackupService>();
            sc.AddSingleton<InvokeService>();

            sc.AddSingleton<AlbumViewModel>();
        }


        #endregion



        private void InitializeApplicationTheme()
        {
            var themeIndex = LocalSettingHelper.GetSetting<int>(SettingKeys.ApplicationTheme);
            if (themeIndex == 1)
            {
                RequestedTheme = ApplicationTheme.Light;
            }
            if (themeIndex == 2)
            {
                RequestedTheme = ApplicationTheme.Dark;
            }
        }


        private async void InitializeJumpList()
        {
            try
            {
                var jumpList = await JumpList.LoadCurrentAsync();
                var item1 = jumpList.Items.FirstOrDefault(x => x.Arguments == "startgame");
                if (item1?.GroupName != "启动游戏")
                {
                    jumpList.Items.Remove(item1);
                    item1 = JumpListItem.CreateWithArguments("startgame", "启动游戏");
                    item1.GroupName = "启动游戏";
                    item1.Logo = new Uri("ms-appx:///Assets/Logos/StoreLogo.png");
                    jumpList.Items.Add(item1);
                }
                var item2 = jumpList.Items.FirstOrDefault(x => x.Arguments == "maptool");
                if (item2 is null)
                {
                    item2 = JumpListItem.CreateWithArguments("maptool", "启动小地图");
                    item2.Logo = new Uri("ms-appx:///Assets/Images/mapicon.png");
                    jumpList.Items.Add(item2);
                }
                await jumpList.SaveAsync();

            }
            catch (Exception ex)
            {
                Log.Error(ex, "Initialize jump list.");
            }
        }




        private System.Timers.Timer _timer;




        private async void LauncheWithArguement(string arg)
        {
            _timer = new(30000);
            _timer.Elapsed += BackgroundTaskTimeout;
            _timer.Start();
            Log.Information($"Launcher argurement: {arg}");
            var service = ActivatorUtilities.GetServiceOrCreateInstance<InvokeService>(Services);
            if (arg == "dailynote")
            {
                await service.RefreshDailyNoteTilesAsync();
            }
            Log.Information("Task finished, exit.");
            Exit();
        }



        private void BackgroundTaskTimeout(object? sender, System.Timers.ElapsedEventArgs e)
        {
            Log.Fatal("Background task timeout.");
            Environment.Exit(-1);
        }



    }
}
