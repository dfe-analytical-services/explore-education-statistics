using System;
using FluentValidation;
using Semver;

namespace GovUk.Education.ExploreEducationStatistics.Notifier.Model;

public record ApiNotificationMessage
{
    public required Guid DataSetId { get; init; }
    public required SemVersion Version { get; init; }

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
