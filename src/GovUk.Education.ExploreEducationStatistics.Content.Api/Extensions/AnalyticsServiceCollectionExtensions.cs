using GovUk.Education.ExploreEducationStatistics.Analytics.Common.Extensions;
using GovUk.Education.ExploreEducationStatistics.Content.Services.Strategies;

namespace GovUk.Education.ExploreEducationStatistics.Content.Api.Extensions;

public static class AnalyticsServiceCollectionExtensions
{
    public static IServiceCollection AddContentAnalytics(
        this IServiceCollection services,
        IConfiguration configuration
    ) =>
        services
            .AddAnalyticsCommon(configuration)
            .WhenEnabled.AddWriteStrategy<AnalyticsWritePublicZipDownloadStrategy>()
            .AddWriteStrategy<AnalyticsWritePublicCsvDownloadStrategy>()
            .Services;
}
