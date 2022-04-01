
using Windows.ApplicationModel;

namespace Xunkong.Desktop
{
    public static class Program
    {
        [global::System.Runtime.InteropServices.DllImport("Microsoft.ui.xaml.dll")]
        private static extern void XamlCheckProcessRequirements();

        [global::System.CodeDom.Compiler.GeneratedCodeAttribute("Microsoft.UI.Xaml.Markup.Compiler", " 1.0.0.0")]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.STAThreadAttribute]
        static void Main(string[] args)
        {
            if (args.Any() && args[0].Contains("startgame"))
            {
                bool result = false;
                if (args[0] == "startgame")
                {
                    result = InvokeService.StartGameWishoutLogAsync().Result;
                }
                else
                {
                    var userName = args[0].Split('_')[1];
                    result = InvokeService.StartGameWishAccountAsync(userName).Result;
                }
                if (result)
                {
                    InvokeService.CheckTransformerReachedAsync().Wait();
                }
                return;
            }
            if (args.Any() && args[0] == "maptool")
            {
                var maptoolPath = Path.Combine(Package.Current.InstalledPath, @"Xunkong.Desktop.MapTool\Xunkong.Desktop.MapTool.exe");
                Process.Start(maptoolPath);
                return;
            }

            XamlCheckProcessRequirements();

            global::WinRT.ComWrappersSupport.InitializeComWrappers();
            global::Microsoft.UI.Xaml.Application.Start((p) =>
            {
                var context = new global::Microsoft.UI.Dispatching.DispatcherQueueSynchronizationContext(global::Microsoft.UI.Dispatching.DispatcherQueue.GetForCurrentThread());
                global::System.Threading.SynchronizationContext.SetSynchronizationContext(context);
                new App();
            });
        }
    }


}
