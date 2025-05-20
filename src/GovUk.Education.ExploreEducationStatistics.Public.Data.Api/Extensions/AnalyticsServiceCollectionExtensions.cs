using GovUk.Education.ExploreEducationStatistics.Public.Data.Api.Options;
using GovUk.Education.ExploreEducationStatistics.Public.Data.Api.Services;
using GovUk.Education.ExploreEducationStatistics.Public.Data.Api.Services.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Public.Data.Api.Strategies;
using GovUk.Education.ExploreEducationStatistics.Public.Data.Api.Strategies.Interfaces;

namespace GovUk.Education.ExploreEducationStatistics.Public.Data.Api.Extensions;

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

        services.AddTransient<IAnalyticsService, AnalyticsService>();

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

        services.AddTransient<IAnalyticsWriteStrategy, AnalyticsWritePublicApiQueryStrategy>();
        services.AddTransient<IAnalyticsWriteStrategy, AnalyticsWriteDataSetVersionCallsStrategy>();

        return services;
    }
}
