#nullable enable
using System.Text.Json.Serialization;
using GovUk.Education.ExploreEducationStatistics.Common.Model.Screener;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Responses.Screener;

public record DataSetScreenerCompletionReportResponse
{
    [JsonPropertyName("data_set_id")]
    public required Guid DataSetId { get; init; }

    [JsonPropertyName("completion_report")]
    public required DataSetScreenerResponse CompletionReport { get; init; }
}
