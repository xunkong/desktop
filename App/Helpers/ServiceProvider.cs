using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Http;
using Syncfusion.Licensing;
using System.Net.Http;
using System.Text.Encodings.Web;
using Xunkong.ApiClient;
using Xunkong.Desktop.ViewModels;
using Xunkong.Hoyolab;
using Xunkong.Hoyolab.Wishlog;

namespace Xunkong.Desktop.Helpers;

internal static class ServiceProvider
{


    private static IServiceProvider _serviceProvider;

    private static bool _isInitialized;


    private static void Initialize()
    {
        var sc = new ServiceCollection();
        ConfigureHttpClient(sc);
        ConfigureService(sc);
        _serviceProvider = sc.BuildServiceProvider();
        SyncfusionLicenseProvider.RegisterLicense("NjEwMjc4QDMyMzAyZTMxMmUzMFkyTEYzc3JuY2hOVXk1azE5d1p1K1NPaFQ4TFF4bU5zYXR2ZUdBQmorc2c9");
        _isInitialized = true;
    }


    private static void ConfigureHttpClient(ServiceCollection sc)
    {
        sc.Configure<HttpClientFactoryOptions>(options =>
        {
            options.HttpMessageHandlerBuilderActions.Add(builder =>
                builder.PrimaryHandler = new HttpClientHandler
                {
                    AutomaticDecompression = System.Net.DecompressionMethods.All,
                });
        });
        sc.AddHttpClient();
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


    private static void ConfigureService(ServiceCollection sc)
    {
        sc.AddSingleton<BackupService>();
        sc.AddSingleton<HoyolabService>();
        sc.AddSingleton<WishlogService>();
        sc.AddSingleton<XunkongApiService>();
        sc.AddSingleton<AlbumViewModel>();
        sc.AddSingleton(new JsonSerializerOptions { WriteIndented = true, Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping, PropertyNameCaseInsensitive = true });
    }



    public static T? GetService<T>()
    {
        if (!_isInitialized)
        {
            Initialize();
        }
        return ActivatorUtilities.GetServiceOrCreateInstance<T>(_serviceProvider);
    }



}
