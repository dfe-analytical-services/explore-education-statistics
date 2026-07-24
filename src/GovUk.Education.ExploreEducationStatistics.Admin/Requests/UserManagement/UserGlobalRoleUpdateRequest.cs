#nullable enable
using FluentValidation;
using GovUk.Education.ExploreEducationStatistics.Admin.Models;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Requests.UserManagement;

public record UserGlobalRoleUpdateRequest
{
    [JsonConverter(typeof(StringEnumConverter))]
    public GlobalRoles.Role? TargetGlobalRole { get; init; }

    public class Validator : AbstractValidator<UserGlobalRoleUpdateRequest>
    {
        public Validator()
        {
            RuleFor(x => x.TargetGlobalRole).NotNull();
        }
    }
}
