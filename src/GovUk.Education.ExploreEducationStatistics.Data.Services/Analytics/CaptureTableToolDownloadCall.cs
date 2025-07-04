using System;
using GovUk.Education.ExploreEducationStatistics.Analytics.Common.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Common.Model.Data.Query;

namespace GovUk.Education.ExploreEducationStatistics.Data.Services.Analytics;

public record CaptureTableToolDownloadCall : IAnalyticsCaptureRequest
{
    public string ReleaseVersionId { get; init; }
    public string PublicationName { get; init; }
    public string ReleasePeriodAndLabel { get; init; }
    public Guid SubjectId { get; init; }
    public string DataSetName { get; init; }
    public TableDownloadFormat DownloadFormat { get; init; }
    public FullTableQuery Query { get; init; }
}
