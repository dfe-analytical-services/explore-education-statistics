using GovUk.Education.ExploreEducationStatistics.Public.Data.Api.Requests;

namespace GovUk.Education.ExploreEducationStatistics.Public.Data.Api.Services.Interfaces;

public interface IAnalyticsWriter
{
    Task Report(AnalyticsCaptureRequestBase request, CancellationToken cancellationToken);
}
