#nullable enable
using FluentValidation;
using FluentValidation.Results;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Requests.UserManagement;

public record PreReleaseUserRemoveRequest
{
    public string Email { get; set; } = string.Empty;

    public class Validator : AbstractValidator<PreReleaseUserRemoveRequest>
    {
        protected override bool PreValidate(
            ValidationContext<PreReleaseUserRemoveRequest> context,
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
        }
    }
}
