using System.Diagnostics.CodeAnalysis;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.ApplicationInsights;

namespace GovUk.Education.ExploreEducationStatistics.Content.Api
{
    [ExcludeFromCodeCoverage]
    public static class Program
    {
        public static void Main(string[] args)
        {
            CreateHostBuilder(args).Build().Run();
        }

        public static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
                .ConfigureWebHostDefaults(webBuilder =>
                {
                    webBuilder.UseStartup<Startup>();
                    webBuilder.ConfigureKestrel(serverOptions => { serverOptions.AddServerHeader = false; });
                })
                .ConfigureLogging(
                    builder =>
                    {
                        // Capture logs from early in the application startup 
                        // pipeline from Startup.cs or Program.cs itself.
                        builder.AddApplicationInsights();

                        // Adding the filter below to ensure logs of all severity from Program.cs
                        // is sent to ApplicationInsights.
                        builder.AddFilter<ApplicationInsightsLoggerProvider>(typeof(Program).FullName, LogLevel.Debug);

                        // Adding the filter below to ensure logs of all severity from Startup.cs
                        // is sent to ApplicationInsights.
                        builder.AddFilter<ApplicationInsightsLoggerProvider>(typeof(Startup).FullName, LogLevel.Debug);

                        // Allow capturing logs in the App Service if turned on in the App Service logs settings page.
                        builder.AddAzureWebAppDiagnostics();
                    }
                );
    }
}