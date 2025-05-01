using GovUk.Education.ExploreEducationStatistics.Public.Data.Api.Requests;

namespace GovUk.Education.ExploreEducationStatistics.Public.Data.Api.Services.Interfaces;

public interface IAnalyticsManager
{
    Task Add(AnalyticsCaptureRequestBase request, CancellationToken cancellationToken);
    
    ValueTask<AnalyticsCaptureRequestBase> Read(CancellationToken cancellationToken);
}
