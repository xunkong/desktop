// See https://aka.ms/new-console-template for more information
using Serilog;
using System.Timers;

namespace Xunkong.Desktop.Extension;

class Program
{

    private static System.Timers.Timer _timer;

    public static string UserPath { get; private set; }

    public static string DbPath { get; private set; }

    public static string DbStr { get; private set; }


    private const string logTemplate = " {NewLine}[{Timestamp:HH:mm:ss.fff}] [{Level:u3}] {SourceContext}{NewLine}{Message}{NewLine}{Exception}";

    public static async Task Main(string[] args)
    {
        _timer = new(30000);
        _timer.Elapsed += BackgroundTaskTimeout;
        _timer.Start();
        CheckUserDataPath();
        if (string.IsNullOrWhiteSpace(UserPath) || !File.Exists(DbPath))
        {
            return;
        }
        if (!LocalSettingHelper.GetSetting<bool>(SettingKeys.DisableBackgroundTaskOutputLog))
        {
            var logPath = Path.Combine(UserPath, $@"Log\BackgroundTask\background_{DateTime.Now:yyyyMMdd}_{DateTime.Now:HHmmss}.txt");
            Log.Logger = new LoggerConfiguration().MinimumLevel.Verbose()
                                                  .WriteTo.File(path: logPath, outputTemplate: logTemplate, shared: true, retainedFileCountLimit: 1000)
                                                  .Enrich.FromLogContext()
                                                  .CreateLogger();
        }
        Log.Information(XunkongEnvironment.GetLogHeader());

        try
        {
            if (args.FirstOrDefault() == "/InvokerPRAID:")
            {
                Log.Information($"arg[2] is {args[2]}");
                switch (args[2])
                {
                    case "DailyNoteTask":
                        await DailyNoteTask.RefreshDailyNoteAsync();
                        break;
                    default:
                        break;
                }
            }
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Unhandled exception.");
        }

        Log.Information("Background task finished.");

    }

    private static void BackgroundTaskTimeout(object? sender, ElapsedEventArgs e)
    {
        Log.Fatal("Background task timeout.");
        Environment.Exit(-1);
    }

    private static void CheckUserDataPath()
    {
        UserPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), @"Xunkong");
        DbPath = Path.Combine(UserPath, @"Data\XunkongData.db");
        DbStr = $"Data Source={DbPath}";
    }


}