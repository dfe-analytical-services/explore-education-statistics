using GovUk.Education.ExploreEducationStatistics.Common.Tests.Fixtures;
using GovUk.Education.ExploreEducationStatistics.Content.Model;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Database;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Tests.Fixtures;
using GovUk.Education.ExploreEducationStatistics.Notifier.Model;
using GovUk.Education.ExploreEducationStatistics.Public.Data.Model;
using GovUk.Education.ExploreEducationStatistics.Public.Data.Model.Database;
using GovUk.Education.ExploreEducationStatistics.Public.Data.Model.Tests.Fixtures;
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

        // tiles from previous pages aren't included in the bau email
        var expectedMessage = new EinTilesRequireUpdateMessage
        {
            Pages =
            [
                new EinPageRequiresUpdate
                {
                    Id = einApiQueryStatTile.EinParentBlock.EinContentSection.EducationInNumbersPageId,
                    Title = einApiQueryStatTile.EinParentBlock.EinContentSection.EducationInNumbersPage.Title,
                    Tiles =
                    [
                        new EinTileRequiresUpdate
                        {
                            Title = einApiQueryStatTile.Title,
                            ContentSectionTitle = einApiQueryStatTile.EinParentBlock.EinContentSection.Heading,
                            DataSetFileId = dataSet.LatestLiveVersion!.Release.DataSetFileId,
                        },
                    ],
                },
            ],
        };
        _fixture
            .NotifierClient.Setup(mock =>
                mock.NotifyEinTilesRequireUpdate(
                    It.Is<List<EinTilesRequireUpdateMessage>>(messages => AssertMessage(messages, expectedMessage)),
                    CancellationToken.None
                )
            )
            .Returns(Task.CompletedTask);

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

    private static bool AssertMessage(
        List<EinTilesRequireUpdateMessage> messages,
        EinTilesRequireUpdateMessage expectedMessage
    )
    {
        var message = Assert.Single(messages);
        Assert.Equal(expectedMessage.Pages.Count, message.Pages.Count);
        var actualPages = message.Pages;
        for (var i = 0; i < message.Pages.Count; i++)
        {
            var expectedPage = expectedMessage.Pages[i];
            var actualPage = actualPages[i];
            Assert.Equal(expectedPage.Tiles.Count, actualPage.Tiles.Count);
            Assert.Equal(expectedPage.Id, actualPage.Id);
            Assert.Equal(expectedPage.Title, actualPage.Title);
            for (var j = 0; j < actualPage.Tiles.Count; j++)
            {
                var expectedTile = expectedPage.Tiles[j];
                var actualTile = actualPage.Tiles[j];
                Assert.Equal(expectedTile.ContentSectionTitle, actualTile.ContentSectionTitle);
                Assert.Equal(expectedTile.Title, actualTile.Title);
                Assert.Equal(expectedTile.DataSetFileId, actualTile.DataSetFileId);
            }
        }

        return true;
    }
}
