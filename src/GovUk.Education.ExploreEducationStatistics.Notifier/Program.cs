using Azure.Data.Tables;
using GovUk.Education.ExploreEducationStatistics.Common.Extensions;
using GovUk.Education.ExploreEducationStatistics.Notifier.Configuration;
using GovUk.Education.ExploreEducationStatistics.Notifier.Repositories;
using GovUk.Education.ExploreEducationStatistics.Notifier.Repositories.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Notifier.Services;
using GovUk.Education.ExploreEducationStatistics.Notifier.Services.Interfaces;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.Logging;

var host = new HostBuilder()
    .ConfigureFunctionsWebApplication()
    .ConfigureAppConfiguration(builder =>
    {
        builder
            .AddJsonFile("appsettings.json", optional: true, reloadOnChange: false)
            .AddJsonFile("appsettings.Local.json", optional: true, reloadOnChange: false)
            .AddEnvironmentVariables();
    })
    .ConfigureLogging(logging =>
    {
        // TODO EES-5013 Why can't this be controlled through application settings?
        logging.AddFilter("Microsoft.EntityFrameworkCore.Database.Command", LogLevel.Warning);
    })
    .ConfigureServices((hostContext, services) =>
    {
        services
            .AddApplicationInsightsTelemetryWorkerService()
            .ConfigureFunctionsApplicationInsights()
            .AddFluentValidation()
            .Configure<AppSettingsOptions>(hostContext.Configuration.GetSection(AppSettingsOptions.AppSettings))
            .Configure<GovUkNotifyOptions>(hostContext.Configuration.GetSection(GovUkNotifyOptions.GovUkNotify))
            //.AddSingleton(serviceProvider =>
            //{
            //    var options = serviceProvider.GetRequiredService<IOptions<AppSettingsOptions>>();
            //    return new TableServiceClient(options.Value.TableStorageConnectionString);
            //})
            .AddTransient<IApiSubscriptionTableStorageService, ApiSubscriptionTableStorageService>()
            .AddTransient<IEmailService, EmailService>()
            .AddTransient<INotificationClientProvider, NotificationClientProvider>()
            .AddTransient<IPublicationSubscriptionRepository, PublicationSubscriptionRepository>()
            .AddTransient<IApiSubscriptionRepository, ApiSubscriptionRepository>()
            .AddTransient<IApiSubscriptionService, ApiSubscriptionService>()
            .AddTransient<ITokenService, TokenService>();
    })
    .Build();

await host.RunAsync();
