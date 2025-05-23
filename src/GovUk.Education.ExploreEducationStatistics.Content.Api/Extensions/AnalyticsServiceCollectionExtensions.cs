using GovUk.Education.ExploreEducationStatistics.Analytics.Common;
using GovUk.Education.ExploreEducationStatistics.Analytics.Common.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Content.Services;
using GovUk.Education.ExploreEducationStatistics.Content.Services.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Content.Services.Strategies;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace GovUk.Education.ExploreEducationStatistics.Content.Api.Extensions;

public static class AnalyticsServiceCollectionExtensions
{
    public static IServiceCollection AddAnalytics(
        this IServiceCollection services,
        IHostEnvironment hostEnvironment,
        IConfiguration configuration)
    {
        var analyticsOptions = configuration
            .GetSection(AnalyticsOptions.Section)
            .Get<AnalyticsOptions>();

        if (analyticsOptions is { Enabled: false })
        {
            services.AddSingleton<IAnalyticsManager, NoOpAnalyticsManager>();
            return services;
        }
        
        services.AddSingleton<IAnalyticsManager, AnalyticsManager>();
        services.AddSingleton<IAnalyticsWriter, AnalyticsWriter>();
        services.AddHostedService<AnalyticsConsumer>();

        if (hostEnvironment.IsDevelopment())
        {
            services.AddSingleton<IAnalyticsPathResolver, LocalAnalyticsPathResolver>();
        }
        else
        {
            services.AddSingleton<IAnalyticsPathResolver, AnalyticsPathResolver>();
        }

        services.AddTransient<IAnalyticsWriteStrategy, AnalyticsWritePublicZipDownloadStrategy>();
        services.AddTransient<IAnalyticsWriteStrategy, AnalyticsWritePublicDataSetFileDownloadStrategy>();

        return services;
    }
}
