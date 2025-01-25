using GovUk.Education.ExploreEducationStatistics.Public.Data.Model;
using GovUk.Education.ExploreEducationStatistics.Public.Data.Model.Database;
using GovUk.Education.ExploreEducationStatistics.Public.Data.Model.Utils;
using Microsoft.EntityFrameworkCore;
using Moq;
using Moq.EntityFrameworkCore;

namespace GovUk.Education.ExploreEducationStatistics.Public.Data.Api.Tests.UnitTests;

public class WildCardDatasetVersionTests
{
    [Theory]
    [InlineData("v*", 5)]
    [InlineData("*", 5)]
    [InlineData("2.*.*", 2, 1, 4)]
    [InlineData("2.*", 2, 1, 4)]
    [InlineData("2.1.*", 2, 1, 4)]
    [InlineData("1.*.*", 1, 3, 0)]
    [InlineData("1.2.*", 1, 2, 0)]
    [InlineData("1.1.*", 1, 1, 1)]
    [InlineData("1.*", 1, 3, 0)]
    [InlineData("v2.*.*", 2, 1, 4)]
    [InlineData("v2.*", 2, 1, 4)]
    [InlineData("v2.1.*", 2, 1, 4)]
    [InlineData("v1.*.*", 1, 3, 0)]
    [InlineData("v1.2.*", 1, 2, 0)]
    [InlineData("v1.1.*", 1, 1, 1)]
    [InlineData("v1.*", 1, 3, 0)]
    public async Task TestDataSetVersions(string versionString,
            int expectedMajor,
            int expectedMinor = default,
            int expectedPatch = default)
    {
        // Arrange
        var dataSetId = Guid.NewGuid();
        var queryable = SetupDataSetVersions(dataSetId);

        // Act 
        var actualResult = await new WildCardDataSetVersionQueryHelper().GetDatasetVersionUsingWildCard(queryable, dataSetId, versionString, CancellationToken.None);

        // Assert
        Assert.True(actualResult.IsRight);
        Assert.Equal(expectedMajor, actualResult.Right.VersionMajor);
        Assert.Equal(expectedMinor, actualResult.Right.VersionMinor);
        Assert.Equal(expectedPatch, actualResult.Right.VersionPatch);
    }

    private static IQueryable<DataSetVersion> SetupDataSetVersions(Guid dataSetId)
    {
        var publicDataDbContextMock = new Mock<PublicDataDbContext>();

        var release = new Release()
        {
            DataSetFileId = Guid.NewGuid(),
            ReleaseFileId = Guid.NewGuid(),
            Slug = "test",
            Title = "Test"
        };

        var versions = new List<DataSetVersion>
            {
                new() {
                    DataSetId = dataSetId, Notes = release.Slug, Release = release, Status = DataSetVersionStatus.Published,
                    VersionMajor = 2, VersionMinor = 0
                },
                new() {
                    DataSetId = dataSetId, Notes = release.Slug, Release = release, Status = DataSetVersionStatus.Published,
                    VersionMajor = 2, VersionMinor = 1, VersionPatch = 0
                },
                new() {
                    DataSetId = dataSetId, Notes = release.Slug, Release = release, Status = DataSetVersionStatus.Published,
                    VersionMajor = 2, VersionMinor = 1, VersionPatch = 1
                },
                new() {
                    DataSetId = dataSetId, Notes = release.Slug, Release = release, Status = DataSetVersionStatus.Published,
                    VersionMajor = 2, VersionMinor = 1, VersionPatch = 2
                },
                new() {
                    DataSetId = dataSetId, Notes = release.Slug, Release = release, Status = DataSetVersionStatus.Published,
                    VersionMajor = 2, VersionMinor = 1, VersionPatch = 3
                },
                new() {
                    DataSetId = dataSetId, Notes = release.Slug, Release = release, Status = DataSetVersionStatus.Published,
                    VersionMajor = 2, VersionMinor = 1, VersionPatch = 4
                },

                new() {
                    DataSetId = dataSetId , Notes = release.Slug, Release = release, Status = DataSetVersionStatus.Published,
                    VersionMajor = 1, VersionMinor = 0
                },
                new() {
                    DataSetId = dataSetId, Notes = release.Slug, Release = release, Status = DataSetVersionStatus.Published,
                    VersionMajor = 1, VersionMinor = 1
                },
                new() {
                    DataSetId = dataSetId, Notes = release.Slug, Release = release, Status = DataSetVersionStatus.Published,
                    VersionMajor = 1, VersionMinor = 1, VersionPatch = 1
                },
                new() {
                    DataSetId = dataSetId, Notes = release.Slug, Release = release, Status = DataSetVersionStatus.Published,
                    VersionMajor = 1, VersionMinor = 2
                },
                new() {
                    DataSetId = dataSetId, Notes = release.Slug, Release = release, Status = DataSetVersionStatus.Published,
                    VersionMajor = 1, VersionMinor = 3
                },
                 new() {
                    DataSetId = dataSetId, Notes = release.Slug, Release = release, Status = DataSetVersionStatus.Published,
                    VersionMajor = 3, VersionMinor = 0
                },
                  new() {
                    DataSetId = dataSetId, Notes = release.Slug, Release = release, Status = DataSetVersionStatus.Published,
                    VersionMajor = 4, VersionMinor = 0
                },
                  new() {
                    DataSetId = dataSetId, Notes = release.Slug, Release = release, Status = DataSetVersionStatus.Published,
                    VersionMajor = 5, VersionMinor = 0
                }
            };

        publicDataDbContextMock.SetupGet(dbContext => dbContext.DataSetVersions).ReturnsDbSet(versions);

        return publicDataDbContextMock.Object.DataSetVersions.AsNoTracking();
    }
}
