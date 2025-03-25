#nullable enable
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using GovUk.Education.ExploreEducationStatistics.Content.Services.Interfaces;

namespace GovUk.Education.ExploreEducationStatistics.Content.Services;

public class AnalyticsWriter : IAnalyticsWriter
{

    public Task ReportReleaseVersionZipDownload(CaptureReleaseVersionZipDownloadRequest request)
    {
        throw new NotImplementedException(); // @MarkFix
    }

    public record CaptureReleaseVersionZipDownloadRequest(
        Guid ReleaseVersionId,
        IList<Guid>? FileIds = null);
}
