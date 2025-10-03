using System;
using FluentValidation;

namespace GovUk.Education.ExploreEducationStatistics.Notifier.Requests;

public record PendingApiSubscriptionCreateRequest
{
    public required string Email { get; init; }
    public required Guid DataSetId { get; init; }
    public required string DataSetTitle { get; init; }

    public class Validator : AbstractValidator<PendingApiSubscriptionCreateRequest>
    {
        public Validator()
        {
            RuleFor(request => request.Email).NotEmpty().EmailAddress();
            RuleFor(request => request.DataSetId).NotEmpty();
            RuleFor(request => request.DataSetTitle).NotEmpty();
        }
    }
}
