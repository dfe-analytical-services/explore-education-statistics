#nullable enable
using System.Threading;
using System.Threading.Channels;
using System.Threading.Tasks;
using GovUk.Education.ExploreEducationStatistics.Content.Services.Interfaces;
using static GovUk.Education.ExploreEducationStatistics.Content.Services.AnalyticsWriter;

namespace GovUk.Education.ExploreEducationStatistics.Content.Services;

public class AnalyticsManager : IAnalyticsManager
{
    private readonly Channel<CaptureReleaseVersionZipDownloadRequest> _releaseVersionZipChannel =
        Channel.CreateUnbounded<CaptureReleaseVersionZipDownloadRequest>();

    public async Task AddReleaseVersionZipDownload(
        CaptureReleaseVersionZipDownloadRequest request,
        CancellationToken cancellationToken)
    {
        await _releaseVersionZipChannel.Writer.WriteAsync(request, cancellationToken);
    }

    public ValueTask<CaptureReleaseVersionZipDownloadRequest> ReadReleaseVersionZipDownload(
        CancellationToken cancellationToken)
    {
        return _releaseVersionZipChannel.Reader.ReadAsync(cancellationToken);
    }
}
