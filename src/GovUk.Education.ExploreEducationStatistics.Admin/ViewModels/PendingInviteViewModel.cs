#nullable enable
namespace GovUk.Education.ExploreEducationStatistics.Admin.ViewModels;

public class PendingInviteViewModel
{
    public string Email { get; init; } = string.Empty;

    public string Role { get; init; } = string.Empty;

    public List<UserPublicationRoleViewModel> UserPublicationRoles { get; init; } = [];

    public List<UserPrereleaseRoleViewModel> UserPrereleaseRoles { get; init; } = [];
}
