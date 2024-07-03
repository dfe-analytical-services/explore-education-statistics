using FluentValidation;
using GovUk.Education.ExploreEducationStatistics.Common.Extensions;
using GovUk.Education.ExploreEducationStatistics.Notifier.Configuration;
using GovUk.Education.ExploreEducationStatistics.Notifier.Repositories;
using GovUk.Education.ExploreEducationStatistics.Notifier.Repositories.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Notifier.Requests;
using GovUk.Education.ExploreEducationStatistics.Notifier.Services;
using GovUk.Education.ExploreEducationStatistics.Notifier.Services.Interfaces;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
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
            .AddTransient<IEmailService, EmailService>()
            .AddTransient<INotificationClientProvider, NotificationClientProvider>()
            .AddTransient<IPublicationSubscriptionRepository, PublicationSubscriptionRepository>()
            .AddTransient<ITokenService, TokenService>()
            .AddScoped<IValidator<NewPendingSubscriptionRequest>,
                NewPendingSubscriptionRequest.Validator>()
            .Configure<AppSettingsOptions>(hostContext.Configuration.GetSection(AppSettingsOptions.AppSettings))
            .Configure<GovUkNotifyOptions>(hostContext.Configuration.GetSection(GovUkNotifyOptions.GovUkNotify));
    })
    .Build();

await host.RunAsync();
