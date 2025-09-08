using GovUk.Education.ExploreEducationStatistics.Analytics.Common.Interfaces;

namespace GovUk.Education.ExploreEducationStatistics.Analytics.Common;

public class AnalyticsWriter(IEnumerable<IAnalyticsWriteStrategy> strategies) : IAnalyticsWriter
{
    private readonly Dictionary<Type, IAnalyticsWriteStrategy> _strategyByRequestType = strategies.ToDictionary(
        strategy => strategy.RequestType
    );

    public async Task Report(IAnalyticsCaptureRequest request, CancellationToken cancellationToken)
    {
        if (!_strategyByRequestType.TryGetValue(request.GetType(), out var strategy))
        {
            throw new Exception($"No write strategy for request type {request.GetType()}");
        }

        await strategy.Report(request, cancellationToken);
    }
}
