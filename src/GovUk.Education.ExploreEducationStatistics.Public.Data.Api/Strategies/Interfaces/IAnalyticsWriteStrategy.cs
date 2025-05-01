using GovUk.Education.ExploreEducationStatistics.Public.Data.Api.Requests;

namespace GovUk.Education.ExploreEducationStatistics.Public.Data.Api.Strategies.Interfaces;

public interface IAnalyticsWriteStrategy
{
    Task Report(AnalyticsCaptureRequestBase request, CancellationToken cancellationToken);
}
