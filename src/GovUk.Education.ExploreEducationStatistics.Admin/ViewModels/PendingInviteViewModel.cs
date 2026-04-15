#nullable enable
namespace GovUk.Education.ExploreEducationStatistics.Admin.ViewModels;

public record PendingInviteViewModel
{
    public required string Email { get; init; }

    public required string Role { get; init; }

    public List<UserPublicationRoleViewModel> UserPublicationRoles { get; init; } = [];

    public List<UserPrereleaseRoleViewModel> UserPrereleaseRoles { get; init; } = [];
}
