#nullable enable
using System.ComponentModel.DataAnnotations;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Controllers.Api.RequestModels;

public record UserInviteCreateRequest
{
    [Required]
    [EmailAddress]
    public string Email { get; init; } = null!;

    [Required(AllowEmptyStrings = false)]
    public string RoleId { get; init; } = null!;

    public DateTimeOffset? CreatedDate { get; init; }

    public IReadOnlyList<UserPreReleaseRoleCreateRequest> UserPreReleaseRoles { get; init; } = [];

    public IReadOnlyList<UserPublicationRoleCreateRequest> UserPublicationRoles { get; init; } = [];
}
