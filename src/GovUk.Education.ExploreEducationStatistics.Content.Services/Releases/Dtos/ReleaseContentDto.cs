using GovUk.Education.ExploreEducationStatistics.Common.Model.Chart;
using GovUk.Education.ExploreEducationStatistics.Common.Model.Data;
using GovUk.Education.ExploreEducationStatistics.Common.Model.Data.Query;
using GovUk.Education.ExploreEducationStatistics.Content.Model;
using JsonKnownTypes;
using Newtonsoft.Json;

namespace GovUk.Education.ExploreEducationStatistics.Content.Services.Releases.Dtos;

public record ReleaseContentDto
{
    public required ContentSectionDto[] Content { get; init; }

    public required ContentSectionDto HeadlinesSection { get; init; }

    public required KeyStatisticBaseDto[] KeyStatistics { get; init; }

    public required ContentSectionDto KeyStatisticsSecondarySection { get; init; }

    public required ContentSectionDto SummarySection { get; init; }

    public ContentSectionDto[] GetAllSections() =>
        [HeadlinesSection, KeyStatisticsSecondarySection, SummarySection, .. Content];

    public static ReleaseContentDto FromReleaseVersion(ReleaseVersion releaseVersion) =>
        new()
        {
            Content = releaseVersion
                .GenericContent.OrderBy(cs => cs.Order)
                .Select(ContentSectionDto.FromContentSection)
                .ToArray(),
            HeadlinesSection = ContentSectionDto.FromContentSection(releaseVersion.HeadlinesSection),
            KeyStatistics = releaseVersion
                .KeyStatistics.OrderBy(ks => ks.Order)
                .Select(KeyStatisticBaseDto.FromKeyStatistic)
                .ToArray(),
            KeyStatisticsSecondarySection = ContentSectionDto.FromContentSection(
                releaseVersion.KeyStatisticsSecondarySection
            ),
            SummarySection = ContentSectionDto.FromContentSection(releaseVersion.SummarySection),
        };
}

public record ContentSectionDto
{
    public required Guid Id { get; init; }

    public required string Heading { get; init; }

    public required ContentBlockBaseDto[] Content { get; init; }

    public static ContentSectionDto FromContentSection(ContentSection contentSection) =>
        new()
        {
            Id = contentSection.Id,
            Heading = contentSection.Heading,
            Content = contentSection
                .Content.OrderBy(cb => cb.Order)
                .Select(ContentBlockBaseDto.FromContentBlock)
                .ToArray(),
        };
}

[JsonConverter(typeof(JsonKnownTypesConverter<ContentBlockBaseDto>))]
[JsonDiscriminator(Name = "type")]
public abstract record ContentBlockBaseDto
{
    public required Guid Id { get; init; }

    public static ContentBlockBaseDto FromContentBlock(ContentBlock contentBlock) =>
        contentBlock switch
        {
            DataBlock dataBlock => DataBlockDto.FromDataBlock(dataBlock),
            EmbedBlockLink embedBlockLink => EmbedBlockLinkDto.FromEmbedBlockLink(embedBlockLink),
            HtmlBlock htmlBlock => HtmlBlockDto.FromHtmlBlock(htmlBlock),
            _ => throw new ArgumentOutOfRangeException(nameof(contentBlock)),
        };
}

[JsonKnownThisType("DataBlock")]
public record DataBlockDto : ContentBlockBaseDto
{
    public required DataBlockVersionDto DataBlockVersion { get; init; }

    public static DataBlockDto FromDataBlock(DataBlock dataBlock) =>
        new()
        {
            Id = dataBlock.Id,
            DataBlockVersion = DataBlockVersionDto.FromDataBlockVersion(dataBlock.DataBlockVersion),
        };
}

public record DataBlockVersionDto
{
    public required Guid DataBlockVersionId { get; init; }

    public required Guid DataBlockParentId { get; init; }

    public required List<IChart> Charts { get; init; }

    public required string Heading { get; init; }

    public required string Name { get; init; }

    public required FullTableQuery Query { get; init; }

    public required string Source { get; init; }

    public required TableBuilderConfiguration Table { get; init; }

    public static DataBlockVersionDto FromDataBlockVersion(DataBlockVersion dataBlockVersion) =>
        new()
        {
            DataBlockVersionId = dataBlockVersion.Id,
            DataBlockParentId = dataBlockVersion.DataBlockParentId,
            Charts = dataBlockVersion.Charts,
            Heading = dataBlockVersion.Heading,
            Name = dataBlockVersion.Name,
            Query = dataBlockVersion.Query,
            Source = dataBlockVersion.Source,
            Table = dataBlockVersion.Table,
        };
}

[JsonKnownThisType("EmbedBlock")]
public record EmbedBlockLinkDto : ContentBlockBaseDto
{
    public required EmbedBlockDto EmbedBlock { get; init; }

    public static EmbedBlockLinkDto FromEmbedBlockLink(EmbedBlockLink embedBlockLink) =>
        new() { Id = embedBlockLink.Id, EmbedBlock = EmbedBlockDto.FromEmbedBlock(embedBlockLink.EmbedBlock) };
}

public record EmbedBlockDto
{
    public required Guid EmbedBlockId { get; init; }

    public required string Title { get; init; }

    public required string Url { get; init; }

    public static EmbedBlockDto FromEmbedBlock(EmbedBlock embedBlock) =>
        new()
        {
            EmbedBlockId = embedBlock.Id,
            Title = embedBlock.Title,
            Url = embedBlock.Url,
        };
}

[JsonKnownThisType("HtmlBlock")]
public record HtmlBlockDto : ContentBlockBaseDto
{
    public required string Body { get; set; }

    public static HtmlBlockDto FromHtmlBlock(HtmlBlock htmlBlock) => new() { Id = htmlBlock.Id, Body = htmlBlock.Body };
}

[JsonConverter(typeof(JsonKnownTypesConverter<KeyStatisticBaseDto>))]
[JsonDiscriminator(Name = "type")]
public abstract record KeyStatisticBaseDto
{
    public required Guid Id { get; init; }

    public required string? GuidanceText { get; init; }

    public required string? GuidanceTitle { get; init; }

    public required string? Trend { get; init; }

    public static KeyStatisticBaseDto FromKeyStatistic(KeyStatistic keyStatistic) =>
        keyStatistic switch
        {
            KeyStatisticDataBlock dataBlock => KeyStatisticDataBlockDto.FromKeyStatisticDataBlock(dataBlock),
            KeyStatisticText text => KeyStatisticTextDto.FromKeyStatisticText(text),
            _ => throw new ArgumentOutOfRangeException(nameof(keyStatistic)),
        };
}

[JsonKnownThisType("KeyStatisticDataBlock")]
public record KeyStatisticDataBlockDto : KeyStatisticBaseDto
{
    public required Guid DataBlockVersionId { get; init; }

    public required Guid DataBlockParentId { get; init; }

    public static KeyStatisticDataBlockDto FromKeyStatisticDataBlock(KeyStatisticDataBlock keyStatisticDataBlock) =>
        new()
        {
            Id = keyStatisticDataBlock.Id,
            DataBlockVersionId = keyStatisticDataBlock.DataBlockId,
            DataBlockParentId = keyStatisticDataBlock.DataBlockParentId,
            GuidanceText = keyStatisticDataBlock.GuidanceText,
            GuidanceTitle = keyStatisticDataBlock.GuidanceTitle,
            Trend = keyStatisticDataBlock.Trend,
        };
}

[JsonKnownThisType("KeyStatisticText")]
public record KeyStatisticTextDto : KeyStatisticBaseDto
{
    public required string Statistic { get; init; }

    public required string Title { get; init; }

    public static KeyStatisticTextDto FromKeyStatisticText(KeyStatisticText keyStatisticText) =>
        new()
        {
            Id = keyStatisticText.Id,
            GuidanceText = keyStatisticText.GuidanceText,
            GuidanceTitle = keyStatisticText.GuidanceTitle,
            Statistic = keyStatisticText.Statistic,
            Title = keyStatisticText.Title,
            Trend = keyStatisticText.Trend,
        };
}
