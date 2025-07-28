using GovUk.Education.ExploreEducationStatistics.Analytics.Common.Extensions;
using GovUk.Education.ExploreEducationStatistics.Public.Data.Api.Services;
using GovUk.Education.ExploreEducationStatistics.Public.Data.Api.Services.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Public.Data.Api.Strategies;

namespace GovUk.Education.ExploreEducationStatistics.Public.Data.Api.Extensions;

public static class AnalyticsServiceCollectionExtensions
{
    public static IServiceCollection AddAnalytics(
        this IServiceCollection services,
        IConfiguration configuration) =>
        services
            .AddAnalyticsCommon(configuration)
                .WhenEnabled
                    .AddWriteStrategy<AnalyticsWriteTopLevelCallsStrategy>()
                    .AddWriteStrategy<AnalyticsWritePublicationCallsStrategy>()
                    .AddWriteStrategy<AnalyticsWriteDataSetCallsStrategy>()
                    .AddWriteStrategy<AnalyticsWriteDataSetVersionCallsStrategy>()
                    .AddWriteStrategy<AnalyticsWritePublicApiQueryStrategy>()
                    .WithService(s => s.AddTransient<IAnalyticsService, AnalyticsService>())
                .WhenDisabled
                    .WithService(s => s.AddTransient<IAnalyticsService, NoOpAnalyticsService>())
                .Services;
}
