using System.Threading;
using System.Threading.Tasks;
using GovUk.Education.ExploreEducationStatistics.Content.Services.Requests;

namespace GovUk.Education.ExploreEducationStatistics.Content.Services.Interfaces;

public interface IAnalyticsManager
{
    Task Add(BaseCaptureRequest request, CancellationToken cancellationToken);

    ValueTask<BaseCaptureRequest> Read(CancellationToken cancellationToken);
}
