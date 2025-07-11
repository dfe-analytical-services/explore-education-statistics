using GovUk.Education.ExploreEducationStatistics.Analytics.Common.Extensions;
using GovUk.Education.ExploreEducationStatistics.Data.Services.Analytics;
using GovUk.Education.ExploreEducationStatistics.Data.Services.Analytics.Config;
using GovUk.Education.ExploreEducationStatistics.Data.Services.Analytics.Utils;
using GovUk.Education.ExploreEducationStatistics.Data.Services.Analytics.Writers;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace GovUk.Education.ExploreEducationStatistics.Data.Services.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddAnalytics(this IServiceCollection services, IConfiguration configuration) =>
        services
            .AddOptions<AnalyticsOptions>().Bind(configuration.GetSection(AnalyticsOptions.Section)).Services
            .AddAnalyticsCommon(
                isAnalyticsEnabled:configuration
                    .GetSection(AnalyticsOptions.Section)
                    .Get<AnalyticsOptions>()?
                    .Enabled == true)
                .AddWriteStrategy<CaptureTableToolDownloadCallAnalyticsWriteStrategy>()
                .AddWriteStrategy<CapturePermaLinkTableDownloadCallAnalyticsWriteStrategy>()
                .Services
            .AddTransient<IAnalyticsPathResolver, AnalyticsPathResolver>()
        ;
}
