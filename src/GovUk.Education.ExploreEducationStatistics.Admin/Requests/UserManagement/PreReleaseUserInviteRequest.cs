#nullable enable
using FluentValidation;
using FluentValidation.Results;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Requests.UserManagement;

public record PreReleaseUserInviteRequest
{
    public List<string> Emails { get; set; } = [];

    public class Validator : AbstractValidator<PreReleaseUserInviteRequest>
    {
        protected override bool PreValidate(
            ValidationContext<PreReleaseUserInviteRequest> context,
            ValidationResult result
        )
        {
            var model = context.InstanceToValidate;

            model.Emails = [.. model.Emails.Select(x => x.Trim().ToLower())];

            return base.PreValidate(context, result);
        }

        public Validator()
        {
            RuleFor(x => x.Emails).NotEmpty().WithMessage("Must have at least one email.");

            RuleForEach(x => x.Emails)
                .NotEmpty()
                .EmailAddress()
                .WithMessage("'{PropertyValue}' is not a valid email address.");
            ;
        }
    }
}
