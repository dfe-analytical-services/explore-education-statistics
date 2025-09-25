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
            // Normalize comparisons to UK time to avoid "previous day" issues when converting from BST/GMT to UTC.
            var ukTz = TimeZoneInfo.FindSystemTimeZoneById("Europe/London");

            bool IsCreatedToday(DateTimeOffset created)
            {
                return DateTimeOffset.Now.Day == created.Day 
                       && DateTimeOffset.Now.Month == created.Month 
                       && DateTimeOffset.Now.Year == created.Year;
            }
            bool IsLessThanOrEqualDates(DateTimeOffset dateStart, DateTimeOffset dateEnd)
            {
                return (dateStart.Month < dateEnd.Month && dateStart.Year <= dateEnd.Year) ||
                (dateStart.Day <= dateEnd.Day && dateStart.Month <= dateEnd.Month && dateStart.Year <= dateEnd.Year);
            }

            When(request => request.Activates.HasValue && request.Expires.HasValue, () =>
            {
                var nowUk = TimeZoneInfo.ConvertTime(DateTimeOffset.UtcNow, ukTz);

                RuleFor(request => request.Activates!.Value)
                    .Must(created =>
                    {
                        var createdUk = TimeZoneInfo.ConvertTime(created, ukTz);
                        return (IsCreatedToday(createdUk) || created > nowUk)
                            && created <= nowUk.AddDays(7);
                    })
                    .WithMessage("Created date must be within the next 7 days.");
                RuleFor(request => request.Expires!.Value)
                    .Must((request, expires) =>
                    {
                        var createdUk = TimeZoneInfo.ConvertTime(request.Activates!.Value, ukTz);
                        var expiresUk = TimeZoneInfo.ConvertTime(expires, ukTz);
                        var sevenDaysFromCreated = createdUk.AddDays(7);

                        var expiresIsWithin7DaysFromCreated = IsLessThanOrEqualDates(expiresUk, sevenDaysFromCreated);
                        var createdIsBeforeExpires = IsLessThanOrEqualDates(createdUk, expiresUk);
                        return expiresIsWithin7DaysFromCreated && createdIsBeforeExpires;
                    })
                    .WithMessage("Expires date must be no more than 7 days after the created date.");
            });

            RuleFor(request => request.DataSetVersionId)
                .NotEmpty();

            RuleFor(request => request.Label)
                .NotEmpty()
                .MaximumLength(100);
        }
    }
}
