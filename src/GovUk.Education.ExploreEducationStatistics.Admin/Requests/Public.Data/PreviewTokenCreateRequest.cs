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
            When(
                r => r.Activates.HasValue,
                () =>
                {
                    RuleFor(r => r.Activates)
                        .Cascade(CascadeMode.Stop)
                        .Must(activates => activates >= utcNow.Subtract(TimeSpan.FromSeconds(ToleranceSeconds)))
                        .WithMessage("Activates date must not be in the past.")
                        .Must(activates => activates <= utcNow.AddDays(7))
                        .WithMessage("Activates date must be within the next 7 days.")
                        .Must(activates =>
                        {
                            if (activates!.Value.Date != utcNow.Date)
                            {
                                return StartOfDayUk(activates.Value.Date) == activates.Value.ToUniversalTime();
                            }
                            return true;
                        })
                        .WithMessage("Activates time must be at midnight GMT/BST when it's not today's date.");
                }
            );

            // Expires
            When(
                r => r.Expires.HasValue,
                () =>
                {
                    When(
                            r => r.Activates.HasValue,
                            () =>
                            {
                                RuleFor(r => r.Expires)
                                    .Cascade(CascadeMode.Stop)
                                    .Must((r, expires) => expires > r.Activates)
                                    .WithMessage("Expires date must be after the activates date.")
                                    .Must(
                                        (r, expires) =>
                                        {
                                            // Ensure expires is not 8 or more days after activates
                                            var daysBetween = (expires!.Value.Date - r.Activates!.Value.Date).Days;
                                            return daysBetween < 8;
                                        }
                                    )
                                    .WithMessage("Expires date must be no more than 7 days after the activates date.")
                                    .Must(
                                        (r, expires) =>
                                        {
                                            if (r.Activates!.Value.Date == utcNow.Date)
                                            {
                                                return true;
                                            }
                                            return EndOfDayUk(expires!.Value.Date) == expires.Value.ToUniversalTime();
                                        }
                                    )
                                    .WithMessage("Expires time must end at midnight GMT/BST for a given date.");
                            }
                        )
                        .Otherwise(() =>
                        {
                            RuleFor(r => r.Expires)
                                .Cascade(CascadeMode.Stop)
                                .Must(expires => utcNow <= expires)
                                .WithMessage("Expires date must not be in the past.")
                                .Must(expires => expires <= utcNow.AddDays(7))
                                .WithMessage("Expires date must be no more than 7 days from today.");
                        });
                }
            );

            RuleFor(request => request.DataSetVersionId).NotEmpty();

            RuleFor(request => request.Label).NotEmpty().MaximumLength(100);
        }

        private static DateTimeOffset StartOfDayUk(DateTime date) => TimeOfDayUk(date, 0, 0, 0);

        private static DateTimeOffset EndOfDayUk(DateTime date) => TimeOfDayUk(date, 23, 59, 59);

        private static DateTimeOffset TimeOfDayUk(DateTime date, int hour, int minute, int second)
        {
            var ukTz = TimeZoneInfo.FindSystemTimeZoneById("Europe/London");
            var unspecified = new DateTime(
                date.Year,
                date.Month,
                date.Day,
                hour,
                minute,
                second,
                DateTimeKind.Unspecified
            );
            var offset = ukTz.GetUtcOffset(unspecified);
            return new DateTimeOffset(unspecified, offset).ToUniversalTime();
        }
    }
}
