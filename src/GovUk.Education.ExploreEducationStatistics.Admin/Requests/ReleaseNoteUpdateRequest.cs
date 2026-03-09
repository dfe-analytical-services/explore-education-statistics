#nullable enable
namespace GovUk.Education.ExploreEducationStatistics.Admin.Requests;

public record ReleaseNoteUpdateRequest
{
    public required DateTimeOffset On { get; init; }

    public required string Reason { get; init; }
}
