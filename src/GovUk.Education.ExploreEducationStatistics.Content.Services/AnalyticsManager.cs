#nullable enable
using System.Threading;
using System.Threading.Channels;
using System.Threading.Tasks;
using GovUk.Education.ExploreEducationStatistics.Content.Services.Interfaces;
using static GovUk.Education.ExploreEducationStatistics.Content.Services.AnalyticsWriter;

namespace GovUk.Education.ExploreEducationStatistics.Content.Services;

public class AnalyticsManager : IAnalyticsManager
{
    private readonly Channel<BaseCaptureRequest> _channel =
        Channel.CreateUnbounded<BaseCaptureRequest>();

    public async Task Add(BaseCaptureRequest request, CancellationToken cancellationToken)
    {
        await _channel.Writer.WriteAsync(request, cancellationToken);
    }

    public ValueTask<BaseCaptureRequest> Read(CancellationToken cancellationToken)
    {
        return _channel.Reader.ReadAsync(cancellationToken);
    }

    private readonly Channel<CaptureZipDownloadRequest> _downloadZipChannel =
        Channel.CreateUnbounded<CaptureZipDownloadRequest>();

    public async Task AddZipDownload(
        CaptureZipDownloadRequest request,
        CancellationToken cancellationToken)
    {
        await _downloadZipChannel.Writer.WriteAsync(request, cancellationToken);
    }

    public ValueTask<CaptureZipDownloadRequest> ReadZipDownload(CancellationToken cancellationToken)
    {
        return _downloadZipChannel.Reader.ReadAsync(cancellationToken);
    }

    private readonly Channel<CaptureCsvDownloadRequest> _downloadCsvChannel =
        Channel.CreateUnbounded<CaptureCsvDownloadRequest>();

    public async Task AddCsvDownload(
        CaptureCsvDownloadRequest request,
        CancellationToken cancellationToken)
    {
        await _downloadCsvChannel.Writer.WriteAsync(request, cancellationToken);
    }

    public ValueTask<CaptureCsvDownloadRequest> ReadCsvDownload(CancellationToken cancellationToken)
    {
        return _downloadCsvChannel.Reader.ReadAsync(cancellationToken);
    }
}
