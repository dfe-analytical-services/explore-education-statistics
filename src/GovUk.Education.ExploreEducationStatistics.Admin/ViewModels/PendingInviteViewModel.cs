#nullable enable
namespace GovUk.Education.ExploreEducationStatistics.Admin.ViewModels;

public class PendingInviteViewModel
{
    public string Email { get; set; } = string.Empty;
    
    public string? Role { get; set; }

    public List<UserPublicationRoleViewModel> UserPublicationRoles { get; set; } = new();
    
    public List<UserReleaseRoleViewModel> UserReleaseRoles { get; set; } = new();
}
