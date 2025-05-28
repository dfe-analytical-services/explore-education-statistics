using GovUk.Education.ExploreEducationStatistics.Content.Search.FunctionApp.Functions;
using Microsoft.ApplicationInsights;
using Microsoft.Azure.Functions.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Serilog;
using Serilog.Extensions.Logging;

// Copied from Serilog documentation. See bottom of:
// https://github.com/serilog-contrib/serilog-sinks-applicationinsights?tab=readme-ov-file
[assembly: FunctionsStartup(typeof(Startup))]
namespace GovUk.Education.ExploreEducationStatistics.Content.Search.FunctionApp.Functions;

public class Startup : FunctionsStartup
{
    public override void Configure(IFunctionsHostBuilder builder)
    {
        builder.Services.AddSingleton<ILoggerProvider>((sp) => 
        {
            Log.Logger = new LoggerConfiguration()
                .Enrich.FromLogContext()
                .WriteTo.ApplicationInsights(
                    sp.GetRequiredService<TelemetryClient>(), 
                    TelemetryConverter.Traces)
                .CreateLogger();
            return new SerilogLoggerProvider(Log.Logger, true);
        });
    }
}
