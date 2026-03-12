using GovUk.Education.ExploreEducationStatistics.Common.Tests.Fixtures;
using GovUk.Education.ExploreEducationStatistics.Content.Model;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Database;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Tests.Fixtures;
using GovUk.Education.ExploreEducationStatistics.Public.Data.Model;
using GovUk.Education.ExploreEducationStatistics.Public.Data.Model.Database;
using GovUk.Education.ExploreEducationStatistics.Public.Data.Model.Tests.Fixtures;
using GovUk.Education.ExploreEducationStatistics.Publisher.Options;
using Microsoft.Extensions.Options;
using Moq;
using Xunit;

namespace GovUk.Education.ExploreEducationStatistics.Publisher.Tests.Services;

public class EducationInNumbersServiceTests(PublisherFunctionsIntegrationTestFixture fixture)
    : PublisherFunctionsIntegrationTest(fixture)
{
    private readonly PublisherFunctionsIntegrationTestFixture _fixture = fixture;
    private readonly DataFixture _dataFixture = new();

    [Fact]
    public async Task UpdateEinTiles_Success_UpdateEinTileLatestDataSetVersionId()
    {
        Publication publication = _dataFixture
            .DefaultPublication()
            .WithReleases([_dataFixture.DefaultRelease(publishedVersions: 1)]);

        var release = publication.Releases[0];
        var releaseVersion = release.Versions[0];

        ReleaseFile releaseDataFile = _dataFixture
            .DefaultReleaseFile()
            .WithFile(_dataFixture.DefaultFile(Common.Model.FileType.Data))
            .WithReleaseVersion(releaseVersion);

        DataSet dataSet = _dataFixture.DefaultDataSet().WithStatusPublished().WithPublicationId(publication.Id);

        var oldDataSetVersionId = Guid.NewGuid();

        DataSetVersion latestDataSetVersion = _dataFixture
            .DefaultDataSetVersion()
            .WithDataSetId(dataSet.Id)
            .WithRelease(_dataFixture.DefaultDataSetVersionRelease().WithReleaseFileId(releaseDataFile.Id))
            .WithVersionNumber(1, 0, 1);

        var einApiQueryStatTile = new EinApiQueryStatTile
        {
            Title = "ApiQueryStatTile title",
            DataSetId = dataSet.Id,
            Version = "1.0.0",
            DataSetVersionId = oldDataSetVersionId,
            LatestDataSetVersionId = oldDataSetVersionId,
            EinParentBlock = new EinTileGroupBlock
            {
                EinContentSection = new EinContentSection
                {
                    Heading = "Content section heading",
                    EducationInNumbersPage = new EducationInNumbersPage
                    {
                        Title = "Ein page title",
                        Slug = "ein-page",
                        Version = 2,
                    },
                },
            },
        };

        // when an EIN page amendment is made, all sections/blocks/tiles are cloned
        var previousPageEinApiQueryStatTile = new EinApiQueryStatTile
        {
            Title = "ApiQueryStatTile title",
            DataSetId = dataSet.Id,
            Version = "1.0.0",
            DataSetVersionId = oldDataSetVersionId,
            LatestDataSetVersionId = oldDataSetVersionId,
            EinParentBlock = new EinTileGroupBlock
            {
                EinContentSection = new EinContentSection
                {
                    Heading = "Content section heading",
                    EducationInNumbersPage = new EducationInNumbersPage
                    {
                        Title = "Ein page title",
                        Slug = "ein-page",
                        Version = 1,
                    },
                },
            },
        };

        await AddTestData<ContentDbContext>(contentDbContext =>
        {
            contentDbContext.Publications.Add(publication);
            contentDbContext.ReleaseFiles.Add(releaseDataFile);
            contentDbContext.EinTiles.AddRange(einApiQueryStatTile, previousPageEinApiQueryStatTile);
        });

        await AddTestData<PublicDataDbContext>(publicDataDbContext =>
        {
            publicDataDbContext.DataSets.Add(dataSet);
            publicDataDbContext.SaveChanges();

            publicDataDbContext.DataSetVersions.Add(latestDataSetVersion);
            publicDataDbContext.SaveChanges();

            dataSet.LatestLiveVersionId = latestDataSetVersion.Id; // cannot be set earlier due to db referential integration
        });

        var appOptions = GetRequiredService<IOptions<AppOptions>>().Value;

        // tiles from previous pages aren't included in the bau email
        var expectedMessage = $"""
            * [Ein page title]({appOptions.AdminAppUrl}/education-in-numbers/{einApiQueryStatTile.EinParentBlock.EinContentSection.EducationInNumbersPageId}/content)
              * Tile titled '{einApiQueryStatTile.Title}' in section '{einApiQueryStatTile.EinParentBlock.EinContentSection.Heading}', which uses [this data set]({appOptions.PublicAppUrl}/data-catalogue/data-set/{dataSet.LatestLiveVersion!.Release.DataSetFileId})

            """;
        _fixture.EmailService.Setup(mock =>
            mock.NotifyEinTilesRequireUpdate(
                "bau@example.com",
                It.Is<string>(message => AssertMessage(message, expectedMessage))
            )
        );

        var einService = GetRequiredService<Publisher.Services.Interfaces.IEducationInNumbersService>();
        await einService.UpdateEinTiles([releaseVersion.Id]);

        var contentDbContext = GetDbContext<ContentDbContext>();
        var einTiles = contentDbContext.EinTiles.ToList();

        Assert.Equal(2, einTiles.Count);

        var latestEinTile = einTiles.Single(tile => tile.Id == einApiQueryStatTile.Id);
        var apiQueryStatTile = Assert.IsType<EinApiQueryStatTile>(latestEinTile);
        Assert.Equal(dataSet.Id, apiQueryStatTile.DataSetId);
        Assert.Equal("1.0.0", apiQueryStatTile.Version);
        Assert.Equal(oldDataSetVersionId, apiQueryStatTile.DataSetVersionId);
        Assert.Equal(latestDataSetVersion.Id, apiQueryStatTile.LatestDataSetVersionId); // updated

        var previousEinTile = einTiles.Single(tile => tile.Id == previousPageEinApiQueryStatTile.Id);
        var previousApiQueryStatTile = Assert.IsType<EinApiQueryStatTile>(previousEinTile);
        Assert.Equal(dataSet.Id, previousApiQueryStatTile.DataSetId);
        Assert.Equal("1.0.0", previousApiQueryStatTile.Version);
        Assert.Equal(oldDataSetVersionId, previousApiQueryStatTile.DataSetVersionId);
        // This is updated on previous tiles, even if we don't include these tiles in the BAU email. BAU users
        // only need to be made aware of tiles from the latest EIN page version
        Assert.Equal(latestDataSetVersion.Id, previousApiQueryStatTile.LatestDataSetVersionId);
    }

    [Fact]
    public async Task UpdateEinTiles_Success_NoUpdate()
    {
        Publication publication = _dataFixture
            .DefaultPublication()
            .WithReleases([_dataFixture.DefaultRelease(publishedVersions: 1)]);

        var release = publication.Releases[0];
        var releaseVersion = release.Versions[0];

        ReleaseFile releaseDataFile = _dataFixture
            .DefaultReleaseFile()
            .WithFile(_dataFixture.DefaultFile(Common.Model.FileType.Data))
            .WithReleaseVersion(releaseVersion);

        DataSet dataSet = _dataFixture.DefaultDataSet().WithStatusPublished().WithPublicationId(publication.Id);

        var dataSetVersionId = Guid.NewGuid();

        DataSetVersion latestDataSetVersion = _dataFixture
            .DefaultDataSetVersion()
            .WithDataSetId(dataSet.Id)
            .WithRelease(_dataFixture.DefaultDataSetVersionRelease().WithReleaseFileId(releaseDataFile.Id))
            .WithVersionNumber(1, 0, 1);

        var einApiQueryStatTile = new EinApiQueryStatTile
        {
            DataSetId = Guid.NewGuid(), // for a different data set
            Version = "1.0.0",
            DataSetVersionId = dataSetVersionId,
            LatestDataSetVersionId = dataSetVersionId,
            EinParentBlock = new EinTileGroupBlock
            {
                EinContentSection = new EinContentSection
                {
                    Heading = "Content section heading",
                    EducationInNumbersPage = new EducationInNumbersPage { Title = "Ein page title" },
                },
            },
        };

        await AddTestData<ContentDbContext>(contentDbContext =>
        {
            contentDbContext.Publications.Add(publication);
            contentDbContext.ReleaseFiles.Add(releaseDataFile);
            contentDbContext.EinTiles.Add(einApiQueryStatTile);
        });

        await AddTestData<PublicDataDbContext>(publicDataDbContext =>
        {
            publicDataDbContext.DataSets.Add(dataSet);
            publicDataDbContext.SaveChanges();

            publicDataDbContext.DataSetVersions.Add(latestDataSetVersion);
            publicDataDbContext.SaveChanges();

            dataSet.LatestLiveVersionId = latestDataSetVersion.Id; // cannot be set earlier due to db referential integration
        });

        var einService = GetRequiredService<Publisher.Services.Interfaces.IEducationInNumbersService>();
        await einService.UpdateEinTiles([releaseVersion.Id]);

        var contentDbContext = GetDbContext<ContentDbContext>();
        var einTiles = contentDbContext.EinTiles.ToList();

        var einTile = einTiles.Single();

        var apiQueryStatTile = Assert.IsType<EinApiQueryStatTile>(einTile);

        Assert.Equal(einApiQueryStatTile.DataSetId, apiQueryStatTile.DataSetId);
        Assert.Equal("1.0.0", apiQueryStatTile.Version);
        Assert.Equal(dataSetVersionId, apiQueryStatTile.DataSetVersionId);
        Assert.Equal(dataSetVersionId, apiQueryStatTile.LatestDataSetVersionId); // no update
    }

    private static bool AssertMessage(string message, string expectedMessage)
    {
        Assert.Equal(message, expectedMessage);
        return true;
    }
}
