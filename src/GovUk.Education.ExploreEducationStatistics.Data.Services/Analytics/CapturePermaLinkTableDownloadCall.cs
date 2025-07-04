using System;
using GovUk.Education.ExploreEducationStatistics.Analytics.Common.Interfaces;

namespace GovUk.Education.ExploreEducationStatistics.Data.Services.Analytics;

public record CapturePermaLinkTableDownloadCall : IAnalyticsCaptureRequest
{
    public string PermalinkTitle { get; init; }
    public Guid PermalinkId { get; init; }
    public TableDownloadFormat DownloadFormat { get; init; }
}
