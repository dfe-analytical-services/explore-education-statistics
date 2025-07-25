using GovUk.Education.ExploreEducationStatistics.Analytics.Common.Extensions;
using GovUk.Education.ExploreEducationStatistics.Content.Services.Strategies;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace GovUk.Education.ExploreEducationStatistics.Content.Api.Extensions;

public static class AnalyticsServiceCollectionExtensions
{
    public static IServiceCollection AddAnalytics(
        this IServiceCollection services,
        IConfiguration configuration) =>
        services
            .AddAnalyticsCommon(configuration)
                .WhenEnabled
                    .AddWriteStrategy<AnalyticsWritePublicZipDownloadStrategy>()
                    .AddWriteStrategy<AnalyticsWritePublicCsvDownloadStrategy>()
                    .Services;
}
