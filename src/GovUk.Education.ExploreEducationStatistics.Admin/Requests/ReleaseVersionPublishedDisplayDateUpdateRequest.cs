#nullable enable
using FluentValidation;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Requests;

public record ReleaseVersionPublishedDisplayDateUpdateRequest
{
    public DateTimeOffset? PublishedDisplayDate { get; init; }

    public class Validator : AbstractValidator<ReleaseVersionPublishedDisplayDateUpdateRequest>
    {
        private static readonly DateTimeOffset MinDate = new(2000, 1, 1, 0, 0, 0, TimeSpan.Zero);

        public Validator(TimeProvider currentTime)
        {
            RuleFor(r => r.PublishedDisplayDate).InclusiveBetween(MinDate, currentTime.GetUtcNow());
        }
    }
}
