#nullable enable
namespace GovUk.Education.ExploreEducationStatistics.Admin.Requests;

public record BoundaryLevelUpdateRequest
{
    public required long Id { get; init; }

    public required string Label { get; init; }
}
