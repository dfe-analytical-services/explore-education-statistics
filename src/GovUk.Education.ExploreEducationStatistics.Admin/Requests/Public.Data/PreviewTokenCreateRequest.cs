#nullable enable
using FluentValidation;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Requests.Public.Data;

public record PreviewTokenCreateRequest
{
    public required Guid DataSetVersionId { get; init; }

    public required string Label { get; init; }
    
    public DateTimeOffset? Created { get; init; }
    
    public DateTimeOffset? Expires { get; init; }
    
    public class Validator : AbstractValidator<PreviewTokenCreateRequest>
    {
        public Validator()
        {
            bool IsCreatedToday(DateTimeOffset created)
            {
                return DateTimeOffset.UtcNow.Day == created.Day 
                       && DateTimeOffset.UtcNow.Month == created.Month 
                       && DateTimeOffset.UtcNow.Year == created.Year;
            }
            bool IsLessThanOrEqualDates(DateTimeOffset created, DateTimeOffset expires)
            {
                return (expires.Month < created.Month && expires.Year <= created.Year) ||
                (expires.Day < created.Day && expires.Month <= created.Month && expires.Year <= created.Year);
            }

            When(request => request.Created.HasValue && request.Expires.HasValue, () =>
            {
                RuleFor(request => request.Created!.Value)
                    .Must(created => 
                        (IsCreatedToday(created) || created >= DateTimeOffset.UtcNow.AddMinutes(-5))
                        && created <= DateTimeOffset.UtcNow.AddDays(7))
                    .WithMessage("Created date must be within the next 7 days.");
                RuleFor(request => request.Expires!.Value)
                    .Must((request, expires) => 
                        IsLessThanOrEqualDates(request.Created!.Value.AddDays(7), expires))
                    .WithMessage("Expires must be no more than 7 days after the created date.");
            });

            RuleFor(request => request.DataSetVersionId)
                .NotEmpty();

            RuleFor(request => request.Label)
                .NotEmpty()
                .MaximumLength(100);
        }
    }
}
