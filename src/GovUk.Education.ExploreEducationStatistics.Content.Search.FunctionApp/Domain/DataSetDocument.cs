using Generator.Equals;

namespace GovUk.Education.ExploreEducationStatistics.Content.Search.FunctionApp.Domain;

/// <summary>
/// A data set document, describing one data file in the latest published version of a release. Its properties match
/// the fields of the data set search index, so that it can be stored as JSON and indexed without any field mappings.
/// </summary>
[Equatable]
public partial record DataSetDocument
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

    public required DataSetDocumentApi? Api { get; init; }

    public required int NumDataFileRows { get; init; }

    [OrderedEquality]
    public required DataSetDocumentGeographicLevel[] GeographicLevelDetails { get; init; }

    public required DataSetDocumentTimePeriodRange TimePeriodRange { get; init; }

    [OrderedEquality]
    public required string[] Filters { get; init; }

    [OrderedEquality]
    public required string[] Indicators { get; init; }

    [OrderedEquality]
    public required Guid[] PublishingOrganisationIds { get; init; }

    [OrderedEquality]
    public required string[] PublishingOrganisationTitles { get; init; }

    public DataSetDocumentKey GetKey() =>
        new()
        {
            PublicationId = PublicationId,
            ReleaseId = ReleaseId,
            FileId = FileId,
        };
}

public record DataSetDocumentApi
{
    public required Guid Id { get; init; }

    public required string Version { get; init; }
}

public record DataSetDocumentGeographicLevel
{
    public required string Code { get; init; }

    public required string Label { get; init; }

    public required bool CsvOnly { get; init; }
}

public record DataSetDocumentTimePeriodRange
{
    public required string From { get; init; }

    public required string To { get; init; }
}
