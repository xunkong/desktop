using Microsoft.CodeAnalysis;
using System;

namespace Xunkong.Desktop.Secret
{

    [Generator]
    public class AppCenterGenerator : ISourceGenerator
    {

        private string secret;


        public void Execute(GeneratorExecutionContext context)
        {
            string source = $$"""
                using Microsoft.AppCenter;
                using Microsoft.AppCenter.Analytics;
                using Microsoft.AppCenter.Crashes;
                namespace Xunkong.Desktop;
                public static class InitializeAppCenter
                {
                    public static void Initialize()
                    {
                        AppCenter.Start("{{secret}}", typeof(Analytics), typeof(Crashes));
                        AppCenter.SetUserId(XunkongEnvironment.DeviceId);
                    }
                }
                """;
            context.AddSource("InitializeAppCenter.cs", source);
        }


        public void Initialize(GeneratorInitializationContext context)
        {
            secret = Environment.GetEnvironmentVariable("XunkongAppCenterSecret");
        }

    }
}
