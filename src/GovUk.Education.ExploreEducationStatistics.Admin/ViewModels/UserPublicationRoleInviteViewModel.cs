#nullable enable
using GovUk.Education.ExploreEducationStatistics.Content.Model;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace GovUk.Education.ExploreEducationStatistics.Admin.ViewModels;

public record UserPublicationRoleInviteViewModel
{
    public required Guid RoleId { get; init; }

    [JsonConverter(typeof(StringEnumConverter))]
    public required PublicationRole Role { get; init; }

    public required Guid UserId { get; init; }

    public required string Email { get; init; }
}
