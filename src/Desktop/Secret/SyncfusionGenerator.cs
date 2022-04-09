using Microsoft.CodeAnalysis;
using System;

namespace Xunkong.Desktop.Secret
{

    [Generator]
    public class SyncfusionGenerator : ISourceGenerator
    {

        private string syncfusionKey;


        public void Execute(GeneratorExecutionContext context)
        {
            var source = $@"namespace Xunkong.Desktop;
                            public static class RegisterSyncfusion
                            {{
                                public static void Register()
                                {{
                                    Syncfusion.Licensing.SyncfusionLicenseProvider.RegisterLicense(""{syncfusionKey}"");
                                }}
                            }}";
            context.AddSource("RegisterSyncfusion.cs", source);
        }

        public void Initialize(GeneratorInitializationContext context)
        {
            syncfusionKey = Environment.GetEnvironmentVariable("SyncfusionKey");
        }

    }
}
