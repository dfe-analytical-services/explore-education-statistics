#nullable enable
using System.Threading;
using System.Threading.Tasks;

namespace GovUk.Education.ExploreEducationStatistics.Content.Services.Interfaces;

public interface IAnalyticsManager
{
    Task AddReleaseVersionZipDownload(
        AnalyticsManager.CaptureReleaseVersionZipDownloadRequest request,
        CancellationToken cancellationToken);

    ValueTask<AnalyticsManager.CaptureReleaseVersionZipDownloadRequest>
        ReadReleaseVersionZipDownload(CancellationToken cancellationToken);
}
