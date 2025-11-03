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
        private static readonly TimeZoneInfo UkTimeZone = TimeZoneInfo.FindSystemTimeZoneById("Europe/London");

        public Validator(TimeProvider currentTime)
        {
            var nowUtc = currentTime.GetUtcNow();
            var nowUk = TimeZoneInfo.ConvertTime(nowUtc, UkTimeZone);

            // Activates
            var eightDaysFromNow = nowUk.AddDays(8);
            var eightDaysFromNowMidnight = new DateTimeOffset(
                eightDaysFromNow.Year,
                eightDaysFromNow.Month,
                eightDaysFromNow.Day,
                0,
                0,
                0,
                eightDaysFromNow.Offset
            );
            ;
            When(
                r => r.Activates.HasValue,
                () =>
                {
                    RuleFor(r => r.Activates)
                        .Cascade(CascadeMode.Stop)
                        .Must(activates =>
                        {
                            var activatesUk = TimeZoneInfo.ConvertTime(activates!.Value, UkTimeZone);
                            var nowWithTolerance = nowUk.AddSeconds(-ToleranceSeconds);
                            return activatesUk >= nowWithTolerance;
                        })
                        .WithMessage("Activates date must not be in the past.")
                        .Must(activates =>
                        {
                            var activatesUk = TimeZoneInfo.ConvertTime(activates!.Value, UkTimeZone);
                            return activatesUk < eightDaysFromNowMidnight;
                        })
                        .WithMessage("Activates date must be within the next 7 days.")
                        .Must(activates =>
                        {
                            var activatesUk = TimeZoneInfo.ConvertTime(activates!.Value, UkTimeZone);
                            var isLocalMidnight = IsLocalMidnight(activatesUk);
                            return activatesUk.Date == nowUk.Date || isLocalMidnight;
                        })
                        .WithMessage(
                            "Activates time must be set to midnight UK local time when it's not today's date."
                        );
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
                                    .Must(
                                        (r, expires) =>
                                        {
                                            var expiresUk = TimeZoneInfo.ConvertTime(expires!.Value, UkTimeZone);
                                            var activatesUk = TimeZoneInfo.ConvertTime(r.Activates!.Value, UkTimeZone);
                                            return expiresUk > activatesUk;
                                        }
                                    )
                                    .WithMessage("Expires date must be after the activates date.")
                                    .Must(
                                        (r, expires) =>
                                        {
                                            var expiresUk = TimeZoneInfo.ConvertTime(expires!.Value, UkTimeZone);
                                            var activatesUk = TimeZoneInfo.ConvertTime(r.Activates!.Value, UkTimeZone);
                                            var daysBetween = (expiresUk.Date - activatesUk.Date).Days;
                                            return daysBetween < 8;
                                        }
                                    )
                                    .WithMessage("Expires date must be no more than 7 days after the activates date.")
                                    .Must(
                                        (r, expires) =>
                                        {
                                            var expiresUk = TimeZoneInfo.ConvertTime(expires!.Value, UkTimeZone);
                                            var activatesUk = TimeZoneInfo.ConvertTime(r.Activates!.Value, UkTimeZone);
                                            var isEndOfExpiresDay = IsEndOfDay(expiresUk);
                                            return activatesUk.Date == nowUk.Date || isEndOfExpiresDay;
                                        }
                                    )
                                    .WithMessage("Expires time must be at 23:59:59 UK local time for that date.");
                            }
                        )
                        .Otherwise(() =>
                        {
                            RuleFor(r => r.Expires)
                                .Cascade(CascadeMode.Stop)
                                .Must(expires =>
                                {
                                    var expiresUk = TimeZoneInfo.ConvertTime(expires!.Value, UkTimeZone);
                                    return expiresUk >= nowUk;
                                })
                                .WithMessage("Expires date must not be in the past.")
                                .Must(expires =>
                                {
                                    var expiresUk = TimeZoneInfo.ConvertTime(expires!.Value, UkTimeZone);
                                    return expiresUk < eightDaysFromNowMidnight;
                                })
                                .WithMessage("Expires date must be no more than 7 days from today.");
                        });
                }
            );

            RuleFor(r => r.DataSetVersionId).NotEmpty();
            RuleFor(r => r.Label).NotEmpty().MaximumLength(100);
        }

        private static bool IsLocalMidnight(DateTimeOffset dtUk)
        {
            var local = dtUk.ToOffset(UkTimeZone.GetUtcOffset(dtUk));
            return local is { Hour: 0, Minute: 0, Second: 0 };
        }

        private static bool IsEndOfDay(DateTimeOffset dtUk)
        {
            var local = dtUk.ToOffset(UkTimeZone.GetUtcOffset(dtUk));
            return local is { Hour: 23, Minute: 59, Second: 59 };
        }
    }
}
