using System.Runtime.Serialization;
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

    public static ReleaseContentDto FromReleaseVersion(ReleaseVersion releaseVersion) =>
        new()
        {
            Content = releaseVersion.GenericContent
                .OrderBy(cs => cs.Order)
                .Select(ContentSectionDto.FromContentSection)
                .ToArray(),
            HeadlinesSection = ContentSectionDto.FromContentSection(releaseVersion.HeadlinesSection),
            KeyStatistics = releaseVersion.KeyStatistics
                .OrderBy(ks => ks.Order)
                .Select(KeyStatisticBaseDto.FromKeyStatistic)
                .ToArray(),
            KeyStatisticsSecondarySection =
                ContentSectionDto.FromContentSection(releaseVersion.KeyStatisticsSecondarySection),
            SummarySection = ContentSectionDto.FromContentSection(releaseVersion.SummarySection)
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
            Content = contentSection.Content
                .OrderBy(cb => cb.Order)
                .Select(ContentBlockBaseDto.FromContentBlock)
                .ToArray()
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
            DataBlock dataBlock => DataBlockBaseDto.FromDataBlock(dataBlock),
            EmbedBlockLink embedBlockLink => EmbedBlockBaseLinkDto.FromEmbedBlockLink(embedBlockLink),
            HtmlBlock htmlBlock => HtmlBlockBaseDto.FromHtmlBlock(htmlBlock),
            _ => throw new ArgumentException($"Unknown ContentBlock type: {contentBlock.GetType()}")
        };
}

[JsonKnownThisType("DataBlock")]
public record DataBlockBaseDto : ContentBlockBaseDto
{
    public required Guid DataBlockParentId { get; init; }

    public required string Heading { get; init; }

    public required string Name { get; init; }

    public required string Source { get; init; }

    public required FullTableQuery Query { get; init; }

    public required List<IChart> Charts { get; init; }

    public required TableBuilderConfiguration Table { get; init; }

    public static DataBlockBaseDto FromDataBlock(DataBlock dataBlock) =>
        new()
        {
            Id = dataBlock.Id,
            DataBlockParentId = Guid.Empty, // TODO EES-6406
            Heading = dataBlock.Heading,
            Name = dataBlock.Name,
            Source = dataBlock.Source,
            Query = dataBlock.Query,
            Charts = dataBlock.Charts,
            Table = dataBlock.Table
        };
}

[JsonKnownThisType("EmbedBlockLink")]
public record EmbedBlockBaseLinkDto : ContentBlockBaseDto
{
    public required string Title { get; init; }

    public required string Url { get; init; }

    public static EmbedBlockBaseLinkDto FromEmbedBlockLink(EmbedBlockLink embedBlockLink) =>
        new()
        {
            Id = embedBlockLink.Id,
            Title = embedBlockLink.EmbedBlock.Title,
            Url = embedBlockLink.EmbedBlock.Url
        };
}

[JsonKnownThisType("HtmlBlock")]
public record HtmlBlockBaseDto : ContentBlockBaseDto
{
    public required string Body { get; init; }

    public static HtmlBlockBaseDto FromHtmlBlock(HtmlBlock htmlBlock) =>
        new()
        {
            Id = htmlBlock.Id,
            Body = htmlBlock.Body
        };
}

[JsonConverter(typeof(JsonKnownTypesConverter<KeyStatisticBaseDto>))]
[JsonDiscriminator(Name = "type")]
[KnownType(typeof(KeyStatisticBaseTextDto))]
[KnownType(typeof(KeyStatisticBaseDataBlockDto))]
public abstract record KeyStatisticBaseDto
{
    public required Guid Id { get; init; }

    public required string? GuidanceText { get; init; }

    public required string? GuidanceTitle { get; init; }

    public required string? Trend { get; init; }

    public static KeyStatisticBaseDto FromKeyStatistic(KeyStatistic keyStatistic) =>
        keyStatistic switch
        {
            KeyStatisticDataBlock dataBlock => KeyStatisticBaseDataBlockDto.FromKeyStatisticDataBlock(dataBlock),
            KeyStatisticText text => KeyStatisticBaseTextDto.FromKeyStatisticText(text),
            // TODO EES-6406 Or use ArgumentOutOfRangeException?
            _ => throw new ArgumentException($"Unknown KeyStatistic type: {keyStatistic.GetType()}")
        };
}

[JsonKnownThisType(nameof(KeyStatisticDataBlock))]
public record KeyStatisticBaseDataBlockDto : KeyStatisticBaseDto
{
    public required Guid DataBlockId { get; init; }

    public required Guid DataBlockParentId { get; init; }

    public static KeyStatisticBaseDataBlockDto FromKeyStatisticDataBlock(KeyStatisticDataBlock keyStatisticDataBlock) =>
        new()
        {
            Id = keyStatisticDataBlock.Id,
            GuidanceText = keyStatisticDataBlock.GuidanceText,
            GuidanceTitle = keyStatisticDataBlock.GuidanceTitle,
            Trend = keyStatisticDataBlock.Trend,
            DataBlockId = keyStatisticDataBlock.DataBlockId,
            DataBlockParentId = keyStatisticDataBlock.DataBlockParentId
        };
}

[JsonKnownThisType(nameof(KeyStatisticText))]
public record KeyStatisticBaseTextDto : KeyStatisticBaseDto
{
    public required string Statistic { get; init; }

    public required string Title { get; init; }

    public static KeyStatisticBaseTextDto FromKeyStatisticText(KeyStatisticText keyStatisticText) =>
        new()
        {
            Id = keyStatisticText.Id,
            GuidanceText = keyStatisticText.GuidanceText,
            GuidanceTitle = keyStatisticText.GuidanceTitle,
            Trend = keyStatisticText.Trend,
            Statistic = keyStatisticText.Statistic,
            Title = keyStatisticText.Title
        };
}
