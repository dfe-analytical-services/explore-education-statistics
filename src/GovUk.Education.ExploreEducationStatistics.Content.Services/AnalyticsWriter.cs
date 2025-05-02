#nullable enable
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using GovUk.Education.ExploreEducationStatistics.Content.Services.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Content.Services.Requests;
using GovUk.Education.ExploreEducationStatistics.Content.Services.Strategies.Interfaces;

namespace GovUk.Education.ExploreEducationStatistics.Content.Services;

public class AnalyticsWriter(IEnumerable<IAnalyticsWriteStrategy> strategies) : IAnalyticsWriter
{
    private readonly Dictionary<Type, IAnalyticsWriteStrategy> _strategyByRequestType =
        strategies.ToDictionary(strategy => strategy.RequestType);

    public async Task Report(AnalyticsCaptureRequestBase request, CancellationToken cancellationToken)
    {
        if (!_strategyByRequestType.TryGetValue(request.GetType(), out var strategy))
        {
            throw new Exception($"No write strategy for request type {request.GetType()}");
        }

        await strategy.Report(request, cancellationToken);
    }
}
