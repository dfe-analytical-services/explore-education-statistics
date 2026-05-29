#nullable enable
using FluentValidation;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Requests.UserManagement;

public record UserGlobalRoleUpdateRequest
{
    public string RoleId { get; init; } = string.Empty;

    public class Validator : AbstractValidator<UserGlobalRoleUpdateRequest>
    {
        public Validator()
        {
            RuleFor(x => x.RoleId).NotEmpty();
        }
    }
}
