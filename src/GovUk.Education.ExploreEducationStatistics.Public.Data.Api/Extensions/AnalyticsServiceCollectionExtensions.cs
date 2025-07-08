using GovUk.Education.ExploreEducationStatistics.Analytics.Common.Extensions;
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
            services.AddAnalyticsCommon(isAnalyticsEnabled:false);
            return services;
        }

        services.AddAnalyticsCommon(isAnalyticsEnabled:true)
            .AddWriteStrategy<AnalyticsWriteTopLevelCallsStrategy>()
            .AddWriteStrategy<AnalyticsWritePublicationCallsStrategy>()
            .AddWriteStrategy<AnalyticsWriteDataSetCallsStrategy>()
            .AddWriteStrategy<AnalyticsWriteDataSetVersionCallsStrategy>()
            .AddWriteStrategy<AnalyticsWritePublicApiQueryStrategy>();

        services.AddTransient<IAnalyticsService, AnalyticsService>();

        if (hostEnvironment.IsDevelopment())
        {
            services.AddSingleton<IAnalyticsPathResolver, LocalAnalyticsPathResolver>();
        }
        else
        {
            services.AddSingleton<IAnalyticsPathResolver, AnalyticsPathResolver>();
        }
        
        return services;
    }
}
