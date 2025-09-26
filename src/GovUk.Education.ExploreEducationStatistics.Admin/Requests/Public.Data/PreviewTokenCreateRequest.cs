#nullable enable
using FluentValidation;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Requests.Public.Data;

public record PreviewTokenCreateRequest
{
    public required Guid DataSetVersionId { get; init; }

    public required string Label { get; init; }
    
    public DateTimeOffset? Activates { get; init; }
    
    public DateTimeOffset? Expires { get; init; }
    
    public class Validator : AbstractValidator<PreviewTokenCreateRequest>
    {
        public Validator()
        {
            When(request => request.Activates.HasValue && request.Expires.HasValue, () =>
            {
                var utcNow = DateTimeOffset.UtcNow;

                RuleFor(request => request.Activates!.Value)
                    .Must(created => IsLessThanOrEqualEndDate(utcNow, created)
                                     && created <= utcNow.AddDays(7))
                    .WithMessage("Created date must be within the next 7 days.");
                RuleFor(request => request.Expires!.Value)
                    .Must((request, expires) =>
                    {
                        var activates = request.Activates!.Value;
                        var sevenDaysFromCreated = activates.AddDays(7);

                        var expiresIsWithin7DaysFromCreated = IsLessThanOrEqualEndDate(expires, sevenDaysFromCreated);
                        var createdIsBeforeExpires = IsLessThanOrEqualEndDate(activates, expires);
                        return expiresIsWithin7DaysFromCreated && createdIsBeforeExpires;
                    })
                    .WithMessage("Expires date must be no more than 7 days after the created date.");
            });

            RuleFor(request => request.DataSetVersionId)
                .NotEmpty();

            RuleFor(request => request.Label)
                .NotEmpty()
                .MaximumLength(100);
            return;

            static bool IsLessThanOrEqualEndDate(DateTimeOffset startDate, DateTimeOffset endDate)
            {
                return (startDate.Month < endDate.Month && startDate.Year <= endDate.Year) ||
                       (startDate.Day <= endDate.Day && startDate.Month <= endDate.Month && startDate.Year <= endDate.Year);
            }
        }
    }
}
