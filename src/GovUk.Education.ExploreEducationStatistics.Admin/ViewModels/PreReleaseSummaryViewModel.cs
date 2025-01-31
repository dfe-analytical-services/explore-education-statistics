#nullable enable
namespace GovUk.Education.ExploreEducationStatistics.Admin.ViewModels;

public record PreReleaseSummaryViewModel
{
    public required string PublicationSlug { get; init; }

    public required string ReleaseSlug { get; init; }

    public required string PublicationTitle { get; init; }

    public required string ReleaseTitle { get; init; }

    public required string ContactEmail { get; init; }

    public required string ContactTeam { get; init; }
}
