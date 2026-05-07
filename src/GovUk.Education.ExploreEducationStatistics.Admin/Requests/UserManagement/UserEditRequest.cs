#nullable enable
using FluentValidation;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Requests.UserManagement;

public record UserEditRequest
{
    public string RoleId { get; init; } = string.Empty;

    public class Validator : AbstractValidator<UserEditRequest>
    {
        public Validator()
        {
            RuleFor(x => x.RoleId).NotEmpty();
        }
    }
}
