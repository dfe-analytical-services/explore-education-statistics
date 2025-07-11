#nullable enable
using System;
using GovUk.Education.ExploreEducationStatistics.Analytics.Common.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Common.Model.Data.Query;

namespace GovUk.Education.ExploreEducationStatistics.Data.Services.Analytics.Dtos;

public record CaptureTableToolDownloadCall : IAnalyticsCaptureRequest
{
    public required Guid ReleaseVersionId { get; init; }
    public required string PublicationName { get; init; }
    public required string ReleasePeriodAndLabel { get; init; }
    public required Guid SubjectId { get; init; }
    public required string DataSetName { get; init; }
    public required TableDownloadFormat DownloadFormat { get; init; }
    public required FullTableQuery Query { get; init; }
}
