using GovUk.Education.ExploreEducationStatistics.Public.Data.Api.Requests;

namespace GovUk.Education.ExploreEducationStatistics.Public.Data.Api.Services.Interfaces;

public interface IAnalyticsManager
{
    Task Add(IAnalyticsCaptureRequestBase request, CancellationToken cancellationToken);
    
    ValueTask<IAnalyticsCaptureRequestBase> Read(CancellationToken cancellationToken);
}
