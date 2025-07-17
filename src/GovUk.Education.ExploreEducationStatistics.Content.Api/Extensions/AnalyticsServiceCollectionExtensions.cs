using GovUk.Education.ExploreEducationStatistics.Analytics.Common.Extensions;
using GovUk.Education.ExploreEducationStatistics.Content.Services;
using GovUk.Education.ExploreEducationStatistics.Content.Services.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Content.Services.Strategies;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace GovUk.Education.ExploreEducationStatistics.Content.Api.Extensions;

public static class AnalyticsServiceCollectionExtensions
{
    public static IServiceCollection AddAnalytics(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        var analyticsOptions = configuration
            .GetSection(AnalyticsOptions.Section)
            .Get<AnalyticsOptions>();

        return analyticsOptions is null or { Enabled: false }
            ? services
                .AddAnalyticsCommon(isAnalyticsEnabled: false).Services
            : services
                .AddAnalyticsCommon(isAnalyticsEnabled: true)
                .AddWriteStrategy<AnalyticsWritePublicZipDownloadStrategy>()
                .AddWriteStrategy<AnalyticsWritePublicCsvDownloadStrategy>().Services
                .AddSingleton<IAnalyticsPathResolver, AnalyticsPathResolver>();
    }
}
