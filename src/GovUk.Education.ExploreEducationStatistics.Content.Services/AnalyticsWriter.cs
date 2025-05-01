using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using GovUk.Education.ExploreEducationStatistics.Content.Services.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Content.Services.Requests;
using GovUk.Education.ExploreEducationStatistics.Content.Services.Strategies.Interfaces;

namespace GovUk.Education.ExploreEducationStatistics.Content.Services;

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

        await strategy.Report(request, cancellationToken);
    }
}
