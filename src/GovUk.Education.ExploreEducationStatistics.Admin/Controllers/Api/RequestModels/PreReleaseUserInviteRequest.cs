using System.ComponentModel.DataAnnotations;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Controllers.Api.RequestModels;

public record PreReleaseUserInviteRequest
{
    [Required]
    [MinLength(1, ErrorMessage = "Must have at least one email.")]
    public List<string> Emails { get; init; }
}
