#nullable enable
using FluentValidation;
using GovUk.Education.ExploreEducationStatistics.Common.Converters;
using GovUk.Education.ExploreEducationStatistics.Common.Model;
using GovUk.Education.ExploreEducationStatistics.Content.Model;
using Newtonsoft.Json;
using static GovUk.Education.ExploreEducationStatistics.Content.Model.NamingUtils;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Requests;

public record ReleaseVersionUpdateRequest
{
    public ReleaseType? Type { get; init; }

    [JsonConverter(typeof(TimeIdentifierJsonConverter))]
    public TimeIdentifier TimePeriodCoverage { get; init; }

    public string Slug => CreateReleaseSlug(year: Year, timePeriodCoverage: TimePeriodCoverage, label: Label);

    public int Year { get; init; }

    public string? Label { get; init; }

    public Guid[] PublishingOrganisations { get; init; } = [];

    public class Validator : AbstractValidator<ReleaseVersionUpdateRequest>
    {
        public Validator()
        {
            RuleFor(request => request.Type).NotNull().NotEqual(ReleaseType.ExperimentalStatistics);

            RuleFor(request => request.Year).InclusiveBetween(1000, 9999);

            RuleFor(request => request.Label).MaximumLength(50);

            RuleFor(request => request.PublishingOrganisations.Length).LessThanOrEqualTo(3);
        }
    }
}
