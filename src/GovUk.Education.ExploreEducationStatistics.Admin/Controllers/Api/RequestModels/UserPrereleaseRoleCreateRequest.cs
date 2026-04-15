using System.ComponentModel.DataAnnotations;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Controllers.Api.RequestModels;

public record UserPrereleaseRoleCreateRequest
{
    [Required]
    public Guid ReleaseId { get; init; }
}
