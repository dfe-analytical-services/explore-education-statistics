#nullable enable
namespace GovUk.Education.ExploreEducationStatistics.Admin.Requests;

public record ReleaseNoteCreateRequest
{
    public required string Reason { get; init; }
}
