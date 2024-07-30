using System;
using FluentValidation;

namespace GovUk.Education.ExploreEducationStatistics.Notifier.Model;

public record ApiNotificationMessage
{
    public required Guid DataSetId { get; init; }
    public required string Version { get; init; }

    public class Validator : AbstractValidator<ApiNotificationMessage>
    {
        public Validator()
        {
            RuleFor(request => request.DataSetId)
                .NotEmpty();
            RuleFor(request => request.Version)
                .NotNull();
        }
    }
}
