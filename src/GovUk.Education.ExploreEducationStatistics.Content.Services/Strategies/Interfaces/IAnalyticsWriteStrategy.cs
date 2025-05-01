#nullable enable
using System.Threading;
using System.Threading.Tasks;
using GovUk.Education.ExploreEducationStatistics.Content.Services.Requests;

namespace GovUk.Education.ExploreEducationStatistics.Content.Services.Strategies.Interfaces;

public interface IAnalyticsWriteStrategy
{
    Task Report(AnalyticsCaptureRequestBase request, CancellationToken cancellationToken);
}
