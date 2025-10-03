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
        public Validator(TimeProvider currentTime)
        {
            var utcNow = currentTime.GetUtcNow();

            // Activates
            When(r => r.Activates.HasValue, () =>
            {
                RuleFor(r => r.Activates)
                    .Cascade(CascadeMode.Stop)
                    .Must(a => IsNotInPastWithTolerance(utcNow, a!.Value, TimeSpan.FromSeconds(10)))
                    .WithMessage("Activates date must not be in the past.")
                    .Must(a => a!.Value.UtcDateTime<= utcNow.AddDays(7))
                    .WithMessage("Activates date must be within the next 7 days.");
            });

            // Expires
            When(r => r.Expires.HasValue, () =>
            {
                When(r => r.Activates.HasValue, () =>
                    {
                        RuleFor(r => r)
                            .Cascade(CascadeMode.Stop)
                            .Must(r => r.Activates!.Value < r.Expires!.Value)
                            .WithMessage("Activates date must be before the expires date.")
                            .Must(r => r.Expires!.Value <= r.Activates!.Value.AddDays(7))
                            .WithMessage("Expires date must be no more than 7 days after the activates date.");
                    })
                    .Otherwise(() =>
                    {
                        RuleFor(r => r.Expires)
                            .Cascade(CascadeMode.Stop)
                            .Must(e => utcNow<= e!.Value)
                            .WithMessage("Expires date must not be in the past.")
                            .Must(e => e!.Value <=  utcNow.AddDays(7))
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
                => value.UtcDateTime >= now.UtcDateTime.Subtract(tolerance);

        }
    }
}
