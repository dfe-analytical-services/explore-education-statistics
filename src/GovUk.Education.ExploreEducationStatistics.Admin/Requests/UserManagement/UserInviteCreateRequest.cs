#nullable enable
using FluentValidation;
using FluentValidation.Results;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Requests.UserManagement;

public record UserInviteCreateRequest
{
    public string Email { get; set; } = string.Empty;

    public string RoleId { get; init; } = string.Empty;

    public DateTimeOffset? CreatedDate { get; init; }

    public IReadOnlyList<UserPreReleaseRoleCreateRequest> UserPreReleaseRoles { get; init; } = [];

    public IReadOnlyList<UserPublicationRoleCreateRequest> UserPublicationRoles { get; init; } = [];

    public class Validator : AbstractValidator<UserInviteCreateRequest>
    {
        protected override bool PreValidate(ValidationContext<UserInviteCreateRequest> context, ValidationResult result)
        {
            var model = context.InstanceToValidate;

            model.Email = model.Email.Trim().ToLower();

            return base.PreValidate(context, result);
        }

        public Validator()
        {
            RuleFor(x => x.Email).NotEmpty().EmailAddress();

            RuleFor(x => x.RoleId).NotEmpty();
        }
    }
}
