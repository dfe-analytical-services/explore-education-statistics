using GovUk.Education.ExploreEducationStatistics.Common.Tests.Fixtures;
using GovUk.Education.ExploreEducationStatistics.Content.Model;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Database;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Tests.Fixtures;
using GovUk.Education.ExploreEducationStatistics.Public.Data.Model;
using GovUk.Education.ExploreEducationStatistics.Public.Data.Model.Database;
using GovUk.Education.ExploreEducationStatistics.Public.Data.Model.Tests.Fixtures;
using Xunit;

namespace GovUk.Education.ExploreEducationStatistics.Publisher.Tests.Services;

public class EducationInNumbersServiceTests(PublisherFunctionsIntegrationTestFixture fixture)
    : PublisherFunctionsIntegrationTest(fixture)
{
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
            DataSetId = dataSet.Id,
            Version = "1.0.0",
            DataSetVersionId = oldDataSetVersionId,
            LatestDataSetVersionId = latestDataSetVersion.Id,
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

        Assert.Equal(dataSet.Id, apiQueryStatTile.DataSetId);
        Assert.Equal("1.0.0", apiQueryStatTile.Version);
        Assert.Equal(oldDataSetVersionId, apiQueryStatTile.DataSetVersionId);
        Assert.Equal(latestDataSetVersion.Id, apiQueryStatTile.LatestDataSetVersionId);
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
}
