using GovUk.Education.ExploreEducationStatistics.Content.Search.FunctionApp.Domain;

namespace GovUk.Education.ExploreEducationStatistics.Content.Search.FunctionApp.Clients.ContentApi.Dtos;

internal record DataSetSearchableDocumentDto
{
    public required Guid FileId { get; init; }
    public required Guid DataSetFileId { get; init; }
    public required Guid SubjectId { get; init; }
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
    public required DataSetSearchableDocumentPublishingOrganisationDto[] PublishingOrganisations { get; init; }

    public DataSetDocument ToModel() =>
        new()
        {
            FileId = FileId,
            DataSetFileId = DataSetFileId,
            SubjectId = SubjectId,
            Filename = Filename,
            FileExtension = FileExtension,
            FileSize = FileSize,
            Title = Title,
            Content = Content,
            ThemeId = ThemeId,
            ThemeTitle = ThemeTitle,
            PublicationId = PublicationId,
            PublicationTitle = PublicationTitle,
            PublicationSlug = PublicationSlug,
            ReleaseId = ReleaseId,
            ReleaseVersionId = ReleaseVersionId,
            ReleaseTitle = ReleaseTitle,
            ReleaseSlug = ReleaseSlug,
            ReleaseType = ReleaseType,
            LatestData = LatestData,
            IsSuperseded = IsSuperseded,
            Published = Published,
            LastUpdated = LastUpdated,
            Api = Api is null ? null : new DataSetDocumentApi { Id = Api.Id, Version = Api.Version },
            NumDataFileRows = NumDataFileRows,
            GeographicLevelDetails =
            [
                .. GeographicLevels.Select(geographicLevel => new DataSetDocumentGeographicLevel
                {
                    Code = geographicLevel.Code,
                    Label = geographicLevel.Label,
                    CsvOnly = geographicLevel.CsvOnly,
                }),
            ],
            TimePeriodRange = new DataSetDocumentTimePeriodRange
            {
                From = TimePeriodRange.From,
                To = TimePeriodRange.To,
            },
            Filters = Filters,
            Indicators = Indicators,
            PublishingOrganisationIds =
            [
                .. PublishingOrganisations.Select(publishingOrganisation => publishingOrganisation.Id),
            ],
            PublishingOrganisationTitles =
            [
                .. PublishingOrganisations.Select(publishingOrganisation => publishingOrganisation.Title),
            ],
        };
}

internal record DataSetSearchableDocumentApiDto
{
    public required Guid Id { get; init; }
    public required string Version { get; init; }
}

internal record DataSetSearchableDocumentGeographicLevelDto
{
    public required string Code { get; init; }
    public required string Label { get; init; }
    public required bool CsvOnly { get; init; }
}

internal record DataSetSearchableDocumentTimePeriodRangeDto
{
    public required string From { get; init; }
    public required string To { get; init; }
}

internal record DataSetSearchableDocumentPublishingOrganisationDto
{
    public required Guid Id { get; init; }
    public required string Title { get; init; }
}
