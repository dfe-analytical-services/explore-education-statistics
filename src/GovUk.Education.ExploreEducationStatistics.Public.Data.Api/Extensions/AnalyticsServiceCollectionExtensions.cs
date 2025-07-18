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
        IConfiguration configuration)
    {
        var analyticsOptions = configuration
            .GetSection(AnalyticsOptions.Section)
            .Get<AnalyticsOptions>();

        return analyticsOptions is { Enabled: false }
            ? services
                .AddAnalyticsCommon(isAnalyticsEnabled: false).Services
                .AddTransient<IAnalyticsService, NoOpAnalyticsService>()
            : services
                .AddAnalyticsCommon(isAnalyticsEnabled: true)
                .AddWriteStrategy<AnalyticsWriteTopLevelCallsStrategy>()
                .AddWriteStrategy<AnalyticsWritePublicationCallsStrategy>()
                .AddWriteStrategy<AnalyticsWriteDataSetCallsStrategy>()
                .AddWriteStrategy<AnalyticsWriteDataSetVersionCallsStrategy>()
                .AddWriteStrategy<AnalyticsWritePublicApiQueryStrategy>().Services
                .AddTransient<IAnalyticsService, AnalyticsService>()
                .AddSingleton<IAnalyticsPathResolver, AnalyticsPathResolver>();
    }
}
