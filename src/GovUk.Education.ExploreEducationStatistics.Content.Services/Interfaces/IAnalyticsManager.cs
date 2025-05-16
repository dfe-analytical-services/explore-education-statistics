#nullable enable
using System.Threading;
using System.Threading.Tasks;
using GovUk.Education.ExploreEducationStatistics.Content.Services.Requests;

namespace GovUk.Education.ExploreEducationStatistics.Content.Services.Interfaces;

public interface IAnalyticsManager
{
    Task Add(IAnalyticsCaptureRequestBase request, CancellationToken cancellationToken);

    ValueTask<IAnalyticsCaptureRequestBase> Read(CancellationToken cancellationToken);
}
