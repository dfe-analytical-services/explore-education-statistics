using Microsoft.Extensions.Configuration;
using Serilog;
using Serilog.Events;
using Serilog.Exceptions;

namespace GovUk.Education.ExploreEducationStatistics.Content.Search.FunctionApp.Extensions;

public static class LoggerConfigurationExtensions
{
    /// <summary>
    /// Initializes the Serilog <paramref name="logger"/> with default configuration.
    /// </summary>
    public static LoggerConfiguration ConfigureBootstrapLogger(
        this LoggerConfiguration logger) =>
        logger
            // default log level settings from ASP.NET Core Visual Studio template (appsettings.json)
            .MinimumLevel.Debug()
            .MinimumLevel.Override("Microsoft", LogEventLevel.Warning)
            .MinimumLevel.Override("Microsoft.Hosting.Lifetime", LogEventLevel.Information)
            // remove noisy HttpClient logs
            .MinimumLevel.Override("System", LogEventLevel.Warning)
            .AddEnrichers()
            .WriteTo.Console();

    public static LoggerConfiguration ConfigureSerilogLogger(
        this LoggerConfiguration logger,
        IConfiguration configuration) =>
        logger
            .AddEnrichers()
            .ReadFrom.Configuration(configuration);

    private static LoggerConfiguration AddEnrichers(this LoggerConfiguration loggerConfiguration) =>
        // To simply the config, specify the common enrichers here.
        // "Enrich": [ "FromLogContext", "WithExceptionDetails", "WithThreadId" ]
        loggerConfiguration
            .Enrich.FromLogContext()
            .Enrich.WithExceptionDetails()
            .Enrich.WithThreadId();
}
