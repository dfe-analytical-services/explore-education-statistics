#nullable enable
using System;
using System.Threading;
using System.Threading.Tasks;
using GovUk.Education.ExploreEducationStatistics.Content.Services.Requests;

namespace GovUk.Education.ExploreEducationStatistics.Content.Services.Strategies.Interfaces;

public interface IAnalyticsWriteStrategy
{
    Type RequestType { get; }

    Task Report(AnalyticsCaptureRequestBase request, CancellationToken cancellationToken);
}
