#nullable enable
namespace GovUk.Education.ExploreEducationStatistics.Admin.ViewModels.ManageContent;

public record ReleaseNoteViewModel
{
    public required Guid Id { get; init; }

    public required string Reason { get; init; }

    public required DateTime On { get; init; }
}
