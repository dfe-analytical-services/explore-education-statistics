using GovUk.Education.ExploreEducationStatistics.Notifier.Configuration;
using GovUk.Education.ExploreEducationStatistics.Notifier.Services;
using GovUk.Education.ExploreEducationStatistics.Notifier.Services.Interfaces;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

var host = new HostBuilder()
    .ConfigureFunctionsWebApplication()
    .ConfigureAppConfiguration(builder =>
    {
        builder
            .AddJsonFile("appsettings.json", optional: true, reloadOnChange: false)
            .AddJsonFile("appsettings.Local.json", optional: true, reloadOnChange: false)
            .AddEnvironmentVariables();
    })
    .ConfigureServices((hostContext, services) =>
    {
        services
            .AddApplicationInsightsTelemetryWorkerService()
            .ConfigureFunctionsApplicationInsights()
            .AddTransient<IEmailService, EmailService>()
            .AddTransient<INotificationClientProvider, NotificationClientProvider>()
            .AddTransient<IStorageTableService, StorageTableService>()
            .AddTransient<ITokenService, TokenService>()
            .Configure<AppSettingOptions>(hostContext.Configuration.GetSection(AppSettingOptions.AppSettings))
            .Configure<GovUkNotifyOptions>(hostContext.Configuration.GetSection(GovUkNotifyOptions.GovUkNotify));
    })
    .Build();

await host.RunAsync();
