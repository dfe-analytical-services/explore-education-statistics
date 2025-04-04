#nullable enable
using System.Threading;
using System.Threading.Tasks;
using static GovUk.Education.ExploreEducationStatistics.Content.Services.AnalyticsWriter;

namespace GovUk.Education.ExploreEducationStatistics.Content.Services.Interfaces;

public interface IAnalyticsManager
{
    Task AddReleaseVersionZipDownload(
        CaptureReleaseVersionZipDownloadRequest request,
        CancellationToken cancellationToken);

    ValueTask<CaptureReleaseVersionZipDownloadRequest>
        ReadReleaseVersionZipDownload(CancellationToken cancellationToken);
}
