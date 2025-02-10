using GovUk.Education.ExploreEducationStatistics.Analytics.Model;

namespace GovUk.Education.ExploreEducationStatistics.Analytics.Requests.Consumer.Services.Interfaces;

public interface IPublicApiAnalyticsService
{
    Task CaptureQuery(CaptureDataSetVersionQueryRequest request);
}
