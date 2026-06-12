#nullable enable
using FluentValidation;
using FluentValidation.Results;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Requests.UserManagement;

public record UserDrafterRoleCreateRequest
{
    public string Email { get; set; } = string.Empty;

    public Guid PublicationId { get; init; }

    public class Validator : AbstractValidator<UserDrafterRoleCreateRequest>
    {
        protected override bool PreValidate(
            ValidationContext<UserDrafterRoleCreateRequest> context,
            ValidationResult result
        )
        {
            var model = context.InstanceToValidate;

            model.Email = model.Email.Trim().ToLower();

            return base.PreValidate(context, result);
        }

        public Validator()
        {
            RuleFor(x => x.Email).NotEmpty().EmailAddress();

            RuleFor(x => x.PublicationId).NotEmpty();
        }
    }
}
