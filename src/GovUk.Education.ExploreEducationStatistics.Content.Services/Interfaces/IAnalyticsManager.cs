#nullable enable
using System.Threading;
using System.Threading.Tasks;
using static GovUk.Education.ExploreEducationStatistics.Content.Services.AnalyticsWriter;

namespace GovUk.Education.ExploreEducationStatistics.Content.Services.Interfaces;

public interface IAnalyticsManager
{
    Task Add(BaseCaptureRequest request, CancellationToken cancellationToken);

    ValueTask<BaseCaptureRequest> Read(CancellationToken cancellationToken);

    Task AddZipDownload(
        CaptureZipDownloadRequest request,
        CancellationToken cancellationToken);

    ValueTask<CaptureZipDownloadRequest> ReadZipDownload(CancellationToken cancellationToken);

    Task AddCsvDownload(
        CaptureCsvDownloadRequest request,
        CancellationToken cancellationToken);

    ValueTask<CaptureCsvDownloadRequest> ReadCsvDownload(CancellationToken cancellationToken);
}
