#nullable enable
using GovUk.Education.ExploreEducationStatistics.Admin.Models;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace GovUk.Education.ExploreEducationStatistics.Admin.ViewModels;

public record PendingInviteViewModel
{
    public required string Email { get; init; }

    [JsonConverter(typeof(StringEnumConverter))]
    public required GlobalRoles.Role GlobalRole { get; init; }

    public List<UserPublicationRoleViewModel> UserPublicationRoles { get; init; } = [];

    public List<UserPreReleaseRoleViewModel> UserPreReleaseRoles { get; init; } = [];
}
