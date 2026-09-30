using GovUk.Education.ExploreEducationStatistics.Common.IntegrationTests;
using GovUk.Education.ExploreEducationStatistics.Common.Model;
using GovUk.Education.ExploreEducationStatistics.Content.Model;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Database;

namespace GovUk.Education.ExploreEducationStatistics.Public.Data.Processor.Tests.Fixture;

/// <summary>
/// Builds the Education in Numbers page structure needed to hold API query stat tiles, which have no
/// DataFixture generators of their own.
/// </summary>
public static class EinTestData
{
    /// <summary>
    /// Adds an Education in Numbers page holding a tile group block with one fully configured API query stat
    /// tile per supplied DataSet id, and returns those tiles in the same order.
    /// </summary>
    public static async Task<List<EinApiQueryStatTile>> AddApiQueryStatTiles(
        ContentDbContext contentDbContext,
        Guid releaseId,
        params Guid[] dataSetIds
    )
    {
        var page = new EinPage
        {
            Id = Guid.NewGuid(),
            Title = "Education in Numbers",
            Slug = $"education-in-numbers-{Guid.NewGuid()}",
            Description = "Key statistics",
            Order = 0,
        };

        var pageVersion = new EinPageVersion
        {
            Id = Guid.NewGuid(),
            EinPageId = page.Id,
            Version = 1,
            Created = DateTimeOffset.UtcNow,
            CreatedById = Guid.NewGuid(),
        };

        var contentSection = new EinContentSection
        {
            Id = Guid.NewGuid(),
            EinPageVersionId = pageVersion.Id,
            Heading = "Key statistics",
            Order = 0,
        };

        var tileGroupBlock = new EinTileGroupBlock
        {
            Id = Guid.NewGuid(),
            EinContentSectionId = contentSection.Id,
            Title = "Key statistics",
            Order = 0,
        };

        var tiles = dataSetIds
            .Select(
                (dataSetId, index) =>
                    new EinApiQueryStatTile
                    {
                        Id = Guid.NewGuid(),
                        EinParentBlockId = tileGroupBlock.Id,
                        Order = index,
                        Title = $"Tile {index}",
                        DataSetId = dataSetId,
                        Version = "1.0.0",
                        DataSetVersionId = Guid.NewGuid(),
                        LatestDataSetVersionId = Guid.NewGuid(),
                        Query = """{"indicators":["indicator-id"]}""",
                        Statistic = "1234",
                        IndicatorUnit = IndicatorUnit.Percent,
                        DecimalPlaces = 1,
                        QueryResult = """[{"values":{"indicator-id":"1234"}}]""",
                        ReleaseId = releaseId,
                    }
            )
            .ToList();

        await contentDbContext.AddTestData(context =>
        {
            context.EinPages.Add(page);
            context.EinPageVersions.Add(pageVersion);
            context.EinContentSections.Add(contentSection);
            context.EinContentBlocks.Add(tileGroupBlock);
            context.EinTiles.AddRange(tiles);
        });

        return tiles;
    }
}
