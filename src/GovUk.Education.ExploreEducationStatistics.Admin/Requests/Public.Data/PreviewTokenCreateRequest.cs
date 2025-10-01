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
            var utcNow = DateTimeOffset.UtcNow;

            RuleFor(request => request.Activates!.Value)
                .Must(activates => IsLessThanOrEqualToEndDate(activates, utcNow.AddDays(7)))
                .When(request => request.Activates.HasValue)
                .WithMessage("Activates date must be within the next 7 days.");

            RuleFor(request => request.Activates!.Value)
                .Must(activates => IsLessThanOrEqualToEndDate(utcNow, activates) )
                .When(request => request.Activates.HasValue)
                .WithMessage("Activates date must not be in the past.");

            RuleFor(request => request.Expires!.Value)
                .Must((request, expires) =>
                {
                    var activates = request.Activates ?? utcNow;
                    var sevenDaysFromCreated = activates.AddDays(7);
                    return IsLessThanOrEqualToEndDate(expires, sevenDaysFromCreated);
                })
                .When(request => request.Expires.HasValue)
                .WithMessage("Expires date must be no more than 7 days after the activates date.");

            RuleFor(request => request.Expires!.Value)
                .Must((request, expires) =>
                {
                    var activates = request.Activates ?? utcNow;
                    return IsLessThanOrEqualToEndDate(activates, expires);
                })
                .When(request => request.Expires.HasValue)
                .WithMessage("Activates date must be before or equal to the expires date.");

            RuleFor(request => request.DataSetVersionId)
                .NotEmpty();

            RuleFor(request => request.Label)
                .NotEmpty()
                .MaximumLength(100);
            return;

            static bool IsLessThanOrEqualToEndDate(DateTimeOffset startDate, DateTimeOffset endDate)
            {
                var startUtc = startDate.ToUniversalTime();
                var endUtc = endDate.ToUniversalTime();

                return startUtc.Date <= endUtc.Date;
            }
        }
    }
}
