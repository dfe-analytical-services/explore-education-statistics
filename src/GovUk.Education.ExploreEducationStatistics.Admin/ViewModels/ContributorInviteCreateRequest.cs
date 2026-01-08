#nullable enable
using System.ComponentModel.DataAnnotations;

namespace GovUk.Education.ExploreEducationStatistics.Admin.ViewModels;

public class ContributorInviteCreateRequest
{
    [EmailAddress]
    public string Email { get; set; } = string.Empty;

    [MinLength(1, ErrorMessage = "Must have at least one release.")]
    public HashSet<Guid> ReleaseIds { get; set; } = [];
}
