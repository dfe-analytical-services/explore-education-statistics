using System.Diagnostics.CodeAnalysis;
using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.ApplicationInsights;

namespace GovUk.Education.ExploreEducationStatistics.Data.Api
{
    [ExcludeFromCodeCoverage]
    public class Program
    {
        public static void Main(string[] args)
        {
            CreateWebHostBuilder(args).Build().Run();
        }

        private static IWebHostBuilder CreateWebHostBuilder(string[] args) =>
            WebHost.CreateDefaultBuilder(args)
                .ConfigureLogging(logging => logging.AddAzureWebAppDiagnostics())
                .UseStartup<Startup>()
                .ConfigureLogging(builder =>
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
                });
    }
}