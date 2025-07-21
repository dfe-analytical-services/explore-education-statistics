#nullable enable
using GovUk.Education.ExploreEducationStatistics.Analytics.Common.Extensions;
using GovUk.Education.ExploreEducationStatistics.Data.Services.Analytics.Writers;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace GovUk.Education.ExploreEducationStatistics.Data.Services.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddAnalytics(this IServiceCollection services, IConfiguration configuration) =>
        services
            .AddAnalyticsCommon(configuration)
                .WhenEnabled
                    .AddWriteStrategy<CaptureTableToolDownloadCallAnalyticsWriteStrategy>()
                    .AddWriteStrategy<CapturePermaLinkTableDownloadCallAnalyticsWriteStrategy>()
                    .Services
        ;
}
