using GovUk.Education.ExploreEducationStatistics.Analytics.Model;
using GovUk.Education.ExploreEducationStatistics.Analytics.Requests.Consumer.Services.Interfaces;
using Microsoft.Extensions.Logging;

namespace GovUk.Education.ExploreEducationStatistics.Analytics.Requests.Consumer.Services;

public class PublicApiAnalyticsService(ILogger<PublicApiAnalyticsService> logger) : IPublicApiAnalyticsService
{
    public Task CaptureQuery(CaptureDataSetVersionQueryRequest request)
    {
        logger.LogInformation("Query for DataSetVersion {DataSetVersionName} captured", request.DataSetTitle);
        return Task.CompletedTask;
    }
}
