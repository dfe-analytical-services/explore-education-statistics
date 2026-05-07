#nullable enable
using FluentValidation;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Requests.UserManagement;

public record UserPreReleaseRoleCreateRequest
{
    public Guid ReleaseId { get; init; }

    public class Validator : AbstractValidator<UserPreReleaseRoleCreateRequest>
    {
        public Validator()
        {
            RuleFor(x => x.ReleaseId).NotEmpty();
        }
    }
}
