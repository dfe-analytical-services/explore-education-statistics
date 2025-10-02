#nullable enable
using FluentValidation;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Requests;

public record BoundaryLevelUpdateRequest
{
    public required long Id { get; init; }

    public required string Label { get; init; }

    public class Validator : AbstractValidator<BoundaryLevelUpdateRequest>
    {
        public Validator()
        {
            RuleFor(request => request.Id).GreaterThan(0);

            RuleFor(request => request.Label).NotEmpty();
        }
    }
}
