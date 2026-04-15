#nullable enable
namespace GovUk.Education.ExploreEducationStatistics.Admin.ViewModels;

public record PreReleaseUserSummaryViewModel(string Email);

public record PreReleaseUserViewModel
{
    public required Guid UserId { get; init; }

    public required string Name { get; init; }

    public required string Email { get; init; }
}

public record PreReleaseUserInvitePlan
{
    public required List<string> AlreadyInvited { get; init; }

    public required List<string> AlreadyAccepted { get; init; }

    public required List<string> Invitable { get; init; }
}
