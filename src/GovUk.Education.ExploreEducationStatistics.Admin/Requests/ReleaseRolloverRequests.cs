#nullable enable
using FluentValidation;
using GovUk.Education.ExploreEducationStatistics.Common.Converters;
using GovUk.Education.ExploreEducationStatistics.Common.Model;
using GovUk.Education.ExploreEducationStatistics.Content.Model;
using Newtonsoft.Json;
using static GovUk.Education.ExploreEducationStatistics.Content.Model.NamingUtils;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Requests;

public enum RolloverMode
{
    WithData,
    StructureOnly,
}

public record ReleaseRolloverCreateRequest
{
    public required Guid PublicationId { get; init; }

    public required Guid RolloverSourceReleaseId { get; init; }

    public required RolloverMode Mode { get; init; }

    public ReleaseType? Type { get; init; }

    [JsonConverter(typeof(TimeIdentifierJsonConverter))]
    public TimeIdentifier TimePeriodCoverage { get; init; }

    public string Slug => CreateReleaseSlug(year: Year, timePeriodCoverage: TimePeriodCoverage, label: Label);

    public int Year { get; init; }

    public string? Label { get; init; }

    public Guid[] PublishingOrganisations { get; init; } = [];

    public class Validator : AbstractValidator<ReleaseRolloverCreateRequest>
    {
        public Validator()
        {
            RuleFor(request => request.PublicationId).NotEmpty();

            RuleFor(request => request.RolloverSourceReleaseId).NotEmpty();

            RuleFor(request => request.Mode).IsInEnum();

            RuleFor(request => request.Type).NotNull().NotEqual(ReleaseType.ExperimentalStatistics);

            RuleFor(request => request.Year).InclusiveBetween(1000, 9999);

            RuleFor(request => request.Label).MaximumLength(20);

            RuleFor(request => request.PublishingOrganisations).NotEmpty();

            RuleFor(request => request.PublishingOrganisations.Length).LessThanOrEqualTo(3);
        }
    }
}
