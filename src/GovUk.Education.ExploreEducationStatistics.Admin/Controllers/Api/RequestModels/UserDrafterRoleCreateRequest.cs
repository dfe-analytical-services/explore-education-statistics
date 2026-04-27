using System.ComponentModel.DataAnnotations;
using Newtonsoft.Json;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Controllers.Api.RequestModels;

public record UserDrafterRoleCreateRequest
{
    [Required]
    [EmailAddress]
    public string Email { get; init; }

    [Required]
    public Guid PublicationId { get; init; }
}
