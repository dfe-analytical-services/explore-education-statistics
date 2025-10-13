#nullable enable
using FluentValidation;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Requests.Public.Data;

public record PreviewTokenCreateRequest
{
    public required Guid DataSetVersionId { get; init; }

    public required string Label { get; init; }

    /// <summary>
    /// Business rules:
    /// - Not in the past (date-only comparison if "today")
    /// - Within the next 7 days
    /// </summary>
    public DateTimeOffset? Activates { get; init; }

    /// <summary>
    /// Business rules:
    /// - If Activates is set: Expires must fall between Activates and Activates + 7 days
    /// - Else:                Expires must fall between now and now + 7 days
    /// </summary>
    public DateTimeOffset? Expires { get; init; }

    public class Validator : AbstractValidator<PreviewTokenCreateRequest>
    {
        private const int ToleranceSeconds = 10;
        public Validator(TimeProvider currentTime)
        {
            var utcNow = currentTime.GetUtcNow();

            // Activates
            When(r => r.Activates.HasValue, () =>
            {
                RuleFor(r => r.Activates)
                    .Cascade(CascadeMode.Stop)
                    .Must(activates => IsNotInPastWithTolerance(utcNow, activates!.Value, TimeSpan.FromSeconds(ToleranceSeconds)))
                    .WithMessage("Activates date must not be in the past.")
                    .Must(activates => activates <= utcNow.AddDays(7))
                    .WithMessage("Activates date must be within the next 7 days.");
            });

            // Expires
            When(r => r.Expires.HasValue, () =>
            {
                When(r => r.Activates.HasValue, () =>
                    {
                        RuleFor(r => r.Expires)
                            .Cascade(CascadeMode.Stop)
                            .Must((r, expires) => r.Activates!.Value < expires)
                            .WithMessage("Activates date must be before the expires date.")
                            .Must((r, expires) => expires!.Value.Date <= r.Activates!.Value.AddDays(7).Date) // ignore the time when comparing with the 7 days after 'Activates'
                            .WithMessage("Expires date must be no more than 7 days after the activates date.");
                    })
                    .Otherwise(() =>
                    {
                        RuleFor(r => r.Expires)
                            .Cascade(CascadeMode.Stop)
                            .Must(e => utcNow<= e!.Value)
                            .WithMessage("Expires date must not be in the past.")
                            .Must(expires => expires!.Value <=  utcNow.AddDays(7))
                            .WithMessage("Expires date must be no more than 7 days from today.");
                    });
            });

            RuleFor(request => request.DataSetVersionId)
                .NotEmpty();

            RuleFor(request => request.Label)
                .NotEmpty()
                .MaximumLength(100);
            return;

            static bool IsNotInPastWithTolerance(DateTimeOffset now, DateTimeOffset value, TimeSpan tolerance)
                => value >= now.Subtract(tolerance);

        }
    }
}
