using GovUk.Education.ExploreEducationStatistics.Public.Data.Api.Requests;
using GovUk.Education.ExploreEducationStatistics.Public.Data.Api.Services.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Public.Data.Api.Strategies.Interfaces;

namespace GovUk.Education.ExploreEducationStatistics.Public.Data.Api.Services;

public class AnalyticsWriter(
    IDictionary<Type, IAnalyticsWriteStrategy> strategyByRequestType) : IAnalyticsWriter
{
    public async Task Report(AnalyticsCaptureRequestBase request, CancellationToken cancellationToken)
    {
        var success = strategyByRequestType.TryGetValue(request.GetType(), out var strategy);
        if (!success)
        {
            throw new Exception($"No write strategy for request type {request.GetType()}");
        }

        await strategy!.Report(request, cancellationToken);
    }
}
