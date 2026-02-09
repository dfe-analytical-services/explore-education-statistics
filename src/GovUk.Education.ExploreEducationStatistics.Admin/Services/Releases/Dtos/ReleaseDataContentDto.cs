#nullable enable
using GovUk.Education.ExploreEducationStatistics.Common.Extensions;
using GovUk.Education.ExploreEducationStatistics.Common.Utils;
using GovUk.Education.ExploreEducationStatistics.Content.Model;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Extensions;
using GovUk.Education.ExploreEducationStatistics.Data.Services.Utils;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Services.Releases.Dtos;

public record ReleaseDataContentDto
{
    public required Guid ReleaseId { get; init; }
    public required Guid ReleaseVersionId { get; init; }
    public required string? DataDashboards { get; init; }
    public required string? DataGuidance { get; init; }
    public required ReleaseDataContentDataSetDto[] DataSets { get; init; }
    public required ReleaseDataContentFeaturedTableDto[] FeaturedTables { get; init; }
    public required ReleaseDataContentSupportingFileDto[] SupportingFiles { get; init; } = [];

    public static ReleaseDataContentDto FromReleaseVersion(
        ReleaseVersion releaseVersion,
        ReleaseDataContentDataSetDto[] dataSets,
        ReleaseDataContentFeaturedTableDto[] featuredTables,
        ReleaseDataContentSupportingFileDto[] supportingFiles
    ) =>
        new()
        {
            ReleaseId = releaseVersion.ReleaseId,
            ReleaseVersionId = releaseVersion.Id,
            DataDashboards = releaseVersion.RelatedDashboardsSection?.FindSingleContentBlockOfType<HtmlBlock>()?.Body,
            DataGuidance = releaseVersion.DataGuidance,
            DataSets = dataSets,
            FeaturedTables = featuredTables,
            SupportingFiles = supportingFiles,
        };
}

public record ReleaseDataContentDataSetDto
{
    public required Guid DataSetFileId { get; init; }
    public required Guid FileId { get; init; }
    public required Guid SubjectId { get; init; }
    public required ReleaseDataContentDataSetMetaDto Meta { get; init; }
    public required string Title { get; init; }
    public required string? Summary { get; init; }

    public static ReleaseDataContentDataSetDto FromReleaseFile(ReleaseFile releaseFile) =>
        new()
        {
            DataSetFileId =
                releaseFile.File.DataSetFileId ?? throw new ArgumentException("File must have DataSetFileId"),
            FileId = releaseFile.File.Id,
            SubjectId = releaseFile.File.SubjectId ?? throw new ArgumentException("File must have SubjectId"),
            Meta = ReleaseDataContentDataSetMetaDto.FromReleaseFile(releaseFile),
            // Summaries created before EES-4353 may contain HTML. Convert them to plain text here.
            // TODO: Remove HtmlToText after migrating all summaries to plain text.
            // Summary is only set when data guidance has been added, so a data set can initially have no summary.
            Summary = releaseFile.Summary != null ? HtmlToTextUtils.HtmlToText(releaseFile.Summary) : null,
            Title = releaseFile.Name ?? throw new ArgumentException("ReleaseFile must have Name"),
        };
}

public record ReleaseDataContentDataSetMetaDto
{
    public required string[] Filters { get; init; }
    public required string[] GeographicLevels { get; init; }
    public required string[] Indicators { get; init; }
    public required int NumDataFileRows { get; init; }
    public required ReleaseDataContentDataSetMetaTimePeriodRangeDto TimePeriodRange { get; init; }

    public static ReleaseDataContentDataSetMetaDto FromReleaseFile(ReleaseFile releaseFile)
    {
        var file = releaseFile.File;
        var meta = file.DataSetFileMeta ?? throw new ArgumentException("File must have DataSetFileMeta");
        return new ReleaseDataContentDataSetMetaDto
        {
            Filters = GetOrderedFilters(meta.Filters, releaseFile.FilterSequence),
            GeographicLevels = GetOrderedGeographicLevels(file.DataSetFileVersionGeographicLevels),
            Indicators = GetOrderedIndicators(meta.Indicators, releaseFile.IndicatorSequence),
            NumDataFileRows = meta.NumDataFileRows,
            TimePeriodRange = ReleaseDataContentDataSetMetaTimePeriodRangeDto.FromTimePeriodRangeMeta(
                meta.TimePeriodRange
            ),
        };
    }

    private static string[] GetOrderedGeographicLevels(
        IEnumerable<DataSetFileVersionGeographicLevel> dataSetFileVersionGeographicLevels
    ) => [.. dataSetFileVersionGeographicLevels.Select(level => level.GeographicLevel.GetEnumLabel()).Order()];

    private static string[] GetOrderedFilters(
        IEnumerable<FilterMeta> filters,
        IEnumerable<FilterSequenceEntry>? filterSequence
    ) =>
        MetaViewModelBuilderUtils
            .OrderBySequenceOrLabel(
                filters,
                idSelector: value => value.Id,
                labelSelector: value => value.Label,
                sequenceIdSelector: sequenceEntry => sequenceEntry.Id,
                resultSelector: value => value.Value.Label,
                filterSequence
            )
            .ToArray();

    private static string[] GetOrderedIndicators(
        IEnumerable<IndicatorMeta> indicators,
        IEnumerable<IndicatorGroupSequenceEntry>? indicatorGroupSequence
    )
    {
        var indicatorSequence = indicatorGroupSequence?.SelectMany(seq => seq.ChildSequence);
        return MetaViewModelBuilderUtils
            .OrderBySequenceOrLabel(
                indicators,
                idSelector: value => value.Id,
                labelSelector: value => value.Label,
                sequenceIdSelector: sequenceEntry => sequenceEntry,
                resultSelector: value => value.Value.Label,
                indicatorSequence
            )
            .ToArray();
    }
}

public record ReleaseDataContentDataSetMetaTimePeriodRangeDto
{
    public required string Start { get; init; }
    public required string End { get; init; }

    public static ReleaseDataContentDataSetMetaTimePeriodRangeDto FromTimePeriodRangeMeta(
        TimePeriodRangeMeta timePeriodRange
    )
    {
        var labels = timePeriodRange.ToLabels();
        return new ReleaseDataContentDataSetMetaTimePeriodRangeDto { Start = labels.Start, End = labels.End };
    }
}

public record ReleaseDataContentFeaturedTableDto
{
    public required Guid FeaturedTableId { get; init; }
    public required Guid DataBlockId { get; init; }
    public required Guid DataBlockParentId { get; init; }
    public required string Title { get; init; }
    public required string Summary { get; init; }

    public static ReleaseDataContentFeaturedTableDto FromFeaturedTable(FeaturedTable featuredTable) =>
        new()
        {
            FeaturedTableId = featuredTable.Id,
            DataBlockId = featuredTable.DataBlockId,
            DataBlockParentId = featuredTable.DataBlockParentId,
            Summary = featuredTable.Description ?? "",
            Title = featuredTable.Name,
        };
}

public record ReleaseDataContentSupportingFileDto
{
    public required Guid FileId { get; init; }
    public required string Extension { get; init; }
    public required string Filename { get; init; }
    public required string Title { get; init; }
    public required string Summary { get; init; }
    public required string Size { get; init; }

    public static ReleaseDataContentSupportingFileDto FromReleaseFile(ReleaseFile releaseFile) =>
        new()
        {
            FileId = releaseFile.File.Id,
            Extension = releaseFile.File.Extension,
            Filename = releaseFile.File.Filename,
            Size = releaseFile.File.DisplaySize(),
            // Default to an empty summary for older supporting files that predate the summary requirement.
            Summary = releaseFile.Summary ?? "",
            Title = releaseFile.Name ?? throw new ArgumentException("ReleaseFile must have Name"),
        };
}
