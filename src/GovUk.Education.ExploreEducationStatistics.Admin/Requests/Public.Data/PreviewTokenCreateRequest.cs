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
    /// - Must always be set to 23:59:59 in UK time
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
            var endOfDay7DaysFromNowUk = nowUk.GetUkEndOfDayOn(7);

            When(
                r => r.Activates.HasValue,
                () =>
                {
                    RuleFor(r => r.Activates)
                        .Cascade(CascadeMode.Stop)
                        .Must(activates =>
                        {
                            var nowUkWithTolerance = nowUk.AddSeconds(-ToleranceSeconds);

                            return activates >= nowUkWithTolerance;
                        })
                        .WithMessage("Activates date must not be in the past.")
                        .Must(activates => activates < endOfDay7DaysFromNowUk)
                        .WithMessage("Activates date must be within the next 7 days.")
                        .Must(activates =>
                        {
                            var startOfDayOnActivatesDateUk = activates!.Value.GetUkStartOfDayOn();
                            var activatesIsStartOfDayUk = startOfDayOnActivatesDateUk == activates;

                            return activates.Value.ConvertToUkTimeZone().Date == nowUk.Date || activatesIsStartOfDayUk;
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
                                    .Must((r, expires) => expires > r.Activates)
                                    .WithMessage("Expires date must be after the activates date.")
                                    .Must(
                                        (r, expires) =>
                                        {
                                            var endOfDay7DaysFromActivatesUk = r.Activates!.Value.GetUkEndOfDayOn(7);

                                            return expires <= endOfDay7DaysFromActivatesUk;
                                        }
                                    )
                                    .WithMessage("Expires date must be no more than 7 days after the activates date.");
                            }
                        )
                        .Otherwise(() =>
                        {
                            RuleFor(r => r.Expires)
                                .Cascade(CascadeMode.Stop)
                                .Must(expires => expires > nowUk)
                                .WithMessage("Expires date must not be in the past.")
                                .Must(expires => expires <= endOfDay7DaysFromNowUk)
                                .WithMessage("Expires date must be no more than 7 days from today.");
                        });
                    RuleFor(r => r.Expires)
                        .Must(
                            (r, expires) =>
                            {
                                var endOfDayOnExpiryDateUk = expires!.Value.GetUkEndOfDayOn();

                                return expires == endOfDayOnExpiryDateUk;
                            }
                        )
                        .WithMessage("Expires time must always be at 23:59:59 UK local time.");
                }
            );

            RuleFor(r => r.DataSetVersionId).NotEmpty();
            RuleFor(r => r.Label).NotEmpty().MaximumLength(100);
        }
    }
}
