#nullable enable
using System;
using GovUk.Education.ExploreEducationStatistics.Analytics.Common.Interfaces;

namespace GovUk.Education.ExploreEducationStatistics.Data.Services.Analytics.Dtos;

public record CapturePermaLinkTableDownloadCall : IAnalyticsCaptureRequest
{
    public required string PermalinkTitle { get; init; }
    public required Guid PermalinkId { get; init; }
    public required TableDownloadFormat DownloadFormat { get; init; }
}
