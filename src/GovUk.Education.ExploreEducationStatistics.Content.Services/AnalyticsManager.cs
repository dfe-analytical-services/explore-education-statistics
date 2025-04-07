#nullable enable
using System.Threading;
using System.Threading.Channels;
using System.Threading.Tasks;
using GovUk.Education.ExploreEducationStatistics.Content.Services.Interfaces;
using static GovUk.Education.ExploreEducationStatistics.Content.Services.AnalyticsWriter;

namespace GovUk.Education.ExploreEducationStatistics.Content.Services;

public class AnalyticsManager : IAnalyticsManager
{
    private readonly Channel<CaptureZipDownloadRequest> _downloadZipChannel =
        Channel.CreateUnbounded<CaptureZipDownloadRequest>();

    public async Task AddZipDownload(
        CaptureZipDownloadRequest request,
        CancellationToken cancellationToken)
    {
        await _downloadZipChannel.Writer.WriteAsync(request, cancellationToken);
    }

    public ValueTask<CaptureZipDownloadRequest> ReadZipDownload(
        CancellationToken cancellationToken)
    {
        return _downloadZipChannel.Reader.ReadAsync(cancellationToken);
    }
}
