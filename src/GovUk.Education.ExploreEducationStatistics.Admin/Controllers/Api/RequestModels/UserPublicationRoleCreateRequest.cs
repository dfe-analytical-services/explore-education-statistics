using System.ComponentModel.DataAnnotations;
using GovUk.Education.ExploreEducationStatistics.Content.Model;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Controllers.Api.RequestModels;

public record UserPublicationRoleCreateRequest
{
    [Required]
    public Guid PublicationId { get; init; }

    [Required]
    [JsonConverter(typeof(StringEnumConverter))]
    public PublicationRole PublicationRole { get; init; }
}
