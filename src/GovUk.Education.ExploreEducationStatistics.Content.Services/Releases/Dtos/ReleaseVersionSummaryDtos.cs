using GovUk.Education.ExploreEducationStatistics.Common.Extensions;
using GovUk.Education.ExploreEducationStatistics.Content.Model;
using GovUk.Education.ExploreEducationStatistics.Content.Services.Organisations.Dtos;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace GovUk.Education.ExploreEducationStatistics.Content.Services.Releases.Dtos;

public record ReleaseVersionSummaryDto
{
    public required Guid Id { get; init; }

    public required Guid ReleaseId { get; init; }

    public required bool IsLatestRelease { get; init; }

    public required string? Label { get; init; }

    public required DateTimeOffset LastUpdated { get; init; }

    public required DateTimeOffset Published { get; init; }

    public required OrganisationDto[] PublishingOrganisations { get; init; }

    public required string Slug { get; init; }

    public required string Title { get; init; }

    public required string CoverageTitle { get; init; }

    public required string YearTitle { get; init; }

    [JsonConverter(typeof(StringEnumConverter))]
    public required ReleaseType Type { get; init; }

    public required string? PreReleaseAccessList { get; init; }

    public required int UpdateCount { get; init; }

    public required ReleaseVersionSummaryPublicationDto Publication { get; init; }

    public static ReleaseVersionSummaryDto FromReleaseVersion(
        ReleaseVersion releaseVersion,
        bool isLatestRelease,
        OrganisationDto[] publishingOrganisations,
        int updateCount
    ) =>
        new()
        {
            Id = releaseVersion.Id,
            ReleaseId = releaseVersion.ReleaseId,
            IsLatestRelease = isLatestRelease,
            Label = releaseVersion.Release.Label,
            LastUpdated = releaseVersion.Published ?? throw new ArgumentException("ReleaseVersion must be published"),
            Published =
                releaseVersion.PublishedDisplayDate ?? throw new ArgumentException("ReleaseVersion must be published"),
            PublishingOrganisations = publishingOrganisations,
            Slug = releaseVersion.Release.Slug,
            Title = releaseVersion.Release.Title,
            CoverageTitle = releaseVersion.Release.TimePeriodCoverage.GetEnumLabel(),
            YearTitle = releaseVersion.Release.YearTitle,
            Type = releaseVersion.Type,
            PreReleaseAccessList = releaseVersion.PreReleaseAccessList,
            UpdateCount = updateCount,
            Publication = ReleaseVersionSummaryPublicationDto.FromPublication(releaseVersion.Release.Publication),
        };
}

public record ReleaseVersionSummaryPublicationDto
{
    public required Guid Id { get; init; }
    public required string Slug { get; init; }
    public required string Title { get; init; }

    public static ReleaseVersionSummaryPublicationDto FromPublication(Publication publication) =>
        new()
        {
            Id = publication.Id,
            Slug = publication.Slug,
            Title = publication.Title,
        };
}
