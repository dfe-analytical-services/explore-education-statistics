using GovUk.Education.ExploreEducationStatistics.Analytics.Common;
using GovUk.Education.ExploreEducationStatistics.Analytics.Common.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Public.Data.Api.Options;
using GovUk.Education.ExploreEducationStatistics.Public.Data.Api.Services;
using GovUk.Education.ExploreEducationStatistics.Public.Data.Api.Services.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Public.Data.Api.Strategies;

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

        if (analyticsOptions is { Enabled: false })
        {
            services.AddTransient<IAnalyticsService, NoOpAnalyticsService>();
            return services;
        }

        services.AddTransient<IAnalyticsService, AnalyticsService>();
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

        services
            .AddTransient<IAnalyticsWriteStrategy, AnalyticsWriteTopLevelCallsStrategy>()
            .AddTransient<IAnalyticsWriteStrategy, AnalyticsWriteDataSetCallsStrategy>()
            .AddTransient<IAnalyticsWriteStrategy, AnalyticsWriteDataSetVersionCallsStrategy>()
            .AddTransient<IAnalyticsWriteStrategy, AnalyticsWritePublicApiQueryStrategy>();
        
        services.AddTransient(
            typeof(ICommonAnalyticsWriteStrategyWorkflow<>),
            typeof(CommonAnalyticsWriteStrategyWorkflow<>));
        
        return services;
    }
}
