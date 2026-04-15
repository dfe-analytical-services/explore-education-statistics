using System.ComponentModel.DataAnnotations;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Controllers.Api.RequestModels;

public record PreReleaseUserRemoveRequest
{
    [Required]
    [EmailAddress]
    public string Email { get; init; }
}
