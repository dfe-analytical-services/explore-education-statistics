using GovUk.Education.ExploreEducationStatistics.Common.Extensions;
using GovUk.Education.ExploreEducationStatistics.Content.Model;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Extensions;
using GovUk.Education.ExploreEducationStatistics.Content.Services.Releases.Dtos;

namespace GovUk.Education.ExploreEducationStatistics.Content.Services.DataSets.Dtos;

public record DataSetSearchableDocumentDto
{
    public required Guid FileId { get; init; }
    public required Guid DataSetFileId { get; init; }
    public required Guid? SubjectId { get; init; }
    public required string Filename { get; init; }
    public required string FileExtension { get; init; }
    public required string FileSize { get; init; }
    public required string Title { get; init; }
    public required string Content { get; init; }
    public required Guid ThemeId { get; init; }
    public required string ThemeTitle { get; init; }
    public required Guid PublicationId { get; init; }
    public required string PublicationTitle { get; init; }
    public required string PublicationSlug { get; init; }
    public required Guid ReleaseId { get; init; }
    public required Guid ReleaseVersionId { get; init; }
    public required string ReleaseTitle { get; init; }
    public required string ReleaseSlug { get; init; }
    public required string ReleaseType { get; init; }
    public required bool LatestData { get; init; }
    public required bool IsSuperseded { get; init; }
    public required DateTimeOffset Published { get; init; }
    public required DateTimeOffset LastUpdated { get; init; }
    public required DataSetSearchableDocumentApiDto? Api { get; init; }
    public required int NumDataFileRows { get; init; }
    public required DataSetSearchableDocumentGeographicLevelDto[] GeographicLevels { get; init; }
    public required DataSetSearchableDocumentTimePeriodRangeDto TimePeriodRange { get; init; }
    public required string[] Filters { get; init; }
    public required string[] Indicators { get; init; }
    public required SearchableDocumentPublishingOrganisationDto[] PublishingOrganisations { get; init; }

    public static DataSetSearchableDocumentDto FromReleaseFile(ReleaseFile releaseFile)
    {
        var file = releaseFile.File;
        var releaseVersion = releaseFile.ReleaseVersion;
        var release = releaseVersion.Release;
        var publication = release.Publication;
        var meta =
            file.DataSetFileMeta ?? throw new ArgumentException("Data set file must have meta", nameof(releaseFile));

        return new DataSetSearchableDocumentDto
        {
            FileId = releaseFile.FileId,
            DataSetFileId =
                file.DataSetFileId
                ?? throw new ArgumentException("Data set file must have a data set file id", nameof(releaseFile)),
            SubjectId = file.SubjectId,
            Filename = file.Filename,
            FileExtension = Path.GetExtension(file.Filename).TrimStart('.'),
            FileSize = file.DisplaySize(),
            Title = releaseFile.Name ?? "",
            Content = releaseFile.Summary ?? "",
            ThemeId = publication.ThemeId,
            ThemeTitle = publication.Theme.Title,
            PublicationId = publication.Id,
            PublicationTitle = publication.Title,
            PublicationSlug = publication.Slug,
            ReleaseId = release.Id,
            ReleaseVersionId = releaseVersion.Id,
            ReleaseTitle = release.Title,
            ReleaseSlug = release.Slug,
            ReleaseType = releaseVersion.Type.ToString(),
            LatestData = releaseVersion.Id == publication.LatestPublishedReleaseVersionId,
            IsSuperseded = publication.SupersededBy?.LatestPublishedReleaseVersionId != null,
            Published =
                releaseVersion.PublishedDisplayDate
                ?? throw new ArgumentException("Release must have a published date", nameof(releaseFile)),
            LastUpdated = DateTime.SpecifyKind(
                releaseFile.Published
                    ?? throw new ArgumentException("Data set file must have a published date", nameof(releaseFile)),
                DateTimeKind.Utc
            ),
            Api = DataSetSearchableDocumentApiDto.FromReleaseFile(releaseFile),
            NumDataFileRows = meta.NumDataFileRows,
            GeographicLevels =
            [
                .. file
                    .DataSetFileVersionGeographicLevels.Select(
                        DataSetSearchableDocumentGeographicLevelDto.FromGeographicLevel
                    )
                    .OrderBy(level => level.Label),
            ],
            TimePeriodRange = new DataSetSearchableDocumentTimePeriodRangeDto
            {
                From = meta.TimePeriodRange.Start.ToLabel(),
                To = meta.TimePeriodRange.End.ToLabel(),
            },
            Filters = [.. DataSetFileMetaUtils.GetOrderedFilters(meta.Filters, releaseFile.FilterSequence)],
            Indicators = [.. DataSetFileMetaUtils.GetOrderedIndicators(meta.Indicators, releaseFile.IndicatorSequence)],
            PublishingOrganisations =
            [
                .. releaseVersion
                    .PublishingOrganisations.OrderByTitleWithDepartmentForEducationFirst()
                    .Select(SearchableDocumentPublishingOrganisationDto.FromOrganisation),
            ],
        };
    }
}

public record DataSetSearchableDocumentApiDto
{
    public required Guid Id { get; init; }
    public required string Version { get; init; }

    public static DataSetSearchableDocumentApiDto? FromReleaseFile(ReleaseFile releaseFile) =>
        releaseFile.PublicApiDataSetId is null || releaseFile.PublicApiDataSetVersion is null
            ? null
            : new DataSetSearchableDocumentApiDto
            {
                Id = releaseFile.PublicApiDataSetId.Value,
                Version = releaseFile.PublicApiDataSetVersionString!,
            };
}

public record DataSetSearchableDocumentGeographicLevelDto
{
    public required string Code { get; init; }
    public required string Label { get; init; }
    public required bool CsvOnly { get; init; }

    public static DataSetSearchableDocumentGeographicLevelDto FromGeographicLevel(
        DataSetFileVersionGeographicLevel geographicLevel
    ) =>
        new()
        {
            Code = geographicLevel.GeographicLevel.GetEnumValue(),
            Label = geographicLevel.GeographicLevel.GetEnumLabel(),
            CsvOnly = geographicLevel.CsvOnly,
        };
}

public record DataSetSearchableDocumentTimePeriodRangeDto
{
    public required string From { get; init; }
    public required string To { get; init; }
}
