#nullable enable
using GovUk.Education.ExploreEducationStatistics.Admin.Models;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Requests.UserManagement;

public record UserGlobalRoleUpdateRequest
{
    [JsonConverter(typeof(StringEnumConverter))]
    public GlobalRoles.Role TargetGlobalRole { get; init; }
}
