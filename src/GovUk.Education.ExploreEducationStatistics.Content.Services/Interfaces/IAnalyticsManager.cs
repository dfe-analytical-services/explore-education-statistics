#nullable enable
using System.Threading;
using System.Threading.Tasks;
using static GovUk.Education.ExploreEducationStatistics.Content.Services.AnalyticsWriter;

namespace GovUk.Education.ExploreEducationStatistics.Content.Services.Interfaces;

public interface IAnalyticsManager
{
    Task AddZipDownload(
        CaptureZipDownloadRequest request,
        CancellationToken cancellationToken);

    ValueTask<CaptureZipDownloadRequest>
        ReadZipDownload(CancellationToken cancellationToken);
}
