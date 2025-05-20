using GovUk.Education.ExploreEducationStatistics.Public.Data.Api.Requests;
using GovUk.Education.ExploreEducationStatistics.Public.Data.Api.Services.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Public.Data.Api.Strategies.Interfaces;

namespace GovUk.Education.ExploreEducationStatistics.Public.Data.Api.Services;

public class AnalyticsWriter(IEnumerable<IAnalyticsWriteStrategy> strategies) : IAnalyticsWriter
{
    private readonly Dictionary<Type, IAnalyticsWriteStrategy> _strategyByRequestType =
        strategies.ToDictionary(strategy => strategy.RequestType);

    public async Task Report(IAnalyticsCaptureRequestBase request, CancellationToken cancellationToken)
    {
        var success = _strategyByRequestType.TryGetValue(request.GetType(), out var strategy);
        if (!success)
        {
            throw new Exception($"No write strategy for request type {request.GetType()}");
        }

        await strategy!.Report(request, cancellationToken);
    }
}
