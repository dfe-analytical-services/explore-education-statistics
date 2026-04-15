using System.ComponentModel.DataAnnotations;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Controllers.Api.RequestModels;

public record UserEditRequest
{
    [Required]
    public string RoleId { get; init; }
}
