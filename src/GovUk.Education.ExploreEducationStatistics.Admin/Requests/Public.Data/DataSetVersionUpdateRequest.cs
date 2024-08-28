#nullable enable

namespace GovUk.Education.ExploreEducationStatistics.Admin.Requests.Public.Data;

public record DataSetVersionUpdateRequest
{
    public string? Notes { get; init; }
}
