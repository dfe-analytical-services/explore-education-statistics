using System.ComponentModel.DataAnnotations;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Controllers.Api.RequestModels;

public record UserPreReleaseRoleCreateRequest
{
    [Required]
    public Guid ReleaseId { get; init; }
}
