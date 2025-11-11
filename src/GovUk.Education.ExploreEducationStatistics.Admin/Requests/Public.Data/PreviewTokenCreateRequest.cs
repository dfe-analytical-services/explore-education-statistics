#nullable enable
using FluentValidation;
using GovUk.Education.ExploreEducationStatistics.Common.Extensions;

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
            var nowUk = currentTime.GetUtcNow().ConvertToUkTimeZone();
            var sevenDaysFromNowUkLastTick = GetLocalUkLastTickXDaysFrom(nowUk, 7);

            When(
                r => r.Activates.HasValue,
                () =>
                {
                    RuleFor(r => r.Activates)
                        .Cascade(CascadeMode.Stop)
                        .Must(activates =>
                        {
                            var nowWithTolerance = nowUk.AddSeconds(-ToleranceSeconds);
                            var isValid = activates!.Value >= nowWithTolerance;
                            return isValid;
                        })
                        .WithMessage("Activates date must not be in the past.")
                        .Must(activates => activates!.Value <= sevenDaysFromNowUkLastTick)
                        .WithMessage("Activates date must be within the next 7 days.")
                        .Must(activates =>
                        {
                            var activatesMidnightUk = activates!.Value.AsStartOfDayForUkTimeZone();
                            var isMidnightInUk = activatesMidnightUk == activates;
                            var isValid = activates.Value.IsSameDay(nowUk) || isMidnightInUk;
                            return isValid;
                        })
                        .WithMessage(
                            "Activates time must be set to midnight UK local time when it's not today's date."
                        );
                }
            );

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
                                    .Must((r, expires) => expires!.Value != r.Activates!.Value)
                                    .WithMessage("Expires date must not be the same dates as the activates date.")
                                    .Must((r, expires) => expires!.Value > r.Activates!.Value)
                                    .WithMessage("Expires date must be after the activates date.")
                                    .Must(
                                        (r, expires) =>
                                        {
                                            var daysBetween = (expires!.Value.Date - r.Activates!.Value.Date).Days;
                                            return daysBetween < 8;
                                        }
                                    )
                                    .WithMessage("Expires date must be no more than 7 days after the activates date.")
                                    .Must(
                                        (r, expires) =>
                                        {
                                            var expiresUkLastTick = expires!.Value.AsEndOfDayForUkTimeZone();
                                            var isEndOfExpiresDay = expires == expiresUkLastTick;

                                            var isValid = r.Activates!.Value.IsSameDay(nowUk) || isEndOfExpiresDay;
                                            return isValid;
                                        }
                                    )
                                    .WithMessage("Expires time must be at 23:59:59 UK local time for that date.");
                            }
                        )
                        .Otherwise(() =>
                        {
                            RuleFor(r => r.Expires)
                                .Cascade(CascadeMode.Stop)
                                .Must(expires => expires!.Value >= nowUk)
                                .WithMessage("Expires date must not be in the past.")
                                .Must(expires =>
                                {
                                    var expiresUkLastTick = expires!.Value.AsEndOfDayForUkTimeZone();
                                    var isValid = expiresUkLastTick <= sevenDaysFromNowUkLastTick;
                                    return isValid;
                                })
                                .WithMessage("Expires date must be no more than 7 days from today.");
                        });
                }
            );

            RuleFor(r => r.DataSetVersionId).NotEmpty();
            RuleFor(r => r.Label).NotEmpty().MaximumLength(100);
        }

        private static DateTimeOffset GetLocalUkLastTickXDaysFrom(DateTimeOffset date, int days)
        {
            var xDaysFromGivenDate = date.AddDays(days);
            return xDaysFromGivenDate.AsEndOfDayForUkTimeZone();
        }
    }
}
