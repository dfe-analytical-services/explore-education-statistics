#nullable enable
using System.ComponentModel.DataAnnotations;

namespace GovUk.Education.ExploreEducationStatistics.Admin.ViewModels;

public class UserInviteCreateRequest
{
    [EmailAddress]
    public string Email { get; init; } = string.Empty;

    [Required(AllowEmptyStrings = false)]
    public string RoleId { get; init; } = string.Empty;

    public DateTimeOffset? CreatedDate { get; init; }

    public List<UserPrereleaseRoleCreateRequest> UserPrereleaseRoles { get; init; } = [];

    public List<UserPublicationRoleCreateRequest> UserPublicationRoles { get; init; } = [];
}
