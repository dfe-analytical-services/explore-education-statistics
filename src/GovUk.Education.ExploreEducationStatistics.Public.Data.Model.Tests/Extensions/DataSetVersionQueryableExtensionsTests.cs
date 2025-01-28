using GovUk.Education.ExploreEducationStatistics.Common.Tests.Fixtures;
using GovUk.Education.ExploreEducationStatistics.Public.Data.Model.Database;
using GovUk.Education.ExploreEducationStatistics.Public.Data.Model.Extensions;
using GovUk.Education.ExploreEducationStatistics.Public.Data.Model.Tests.Fixtures;
using Microsoft.EntityFrameworkCore;
using Moq;
using Moq.EntityFrameworkCore;

namespace GovUk.Education.ExploreEducationStatistics.Public.Data.Model.Tests.Extensions;

public class DataSetVersionQueryableExtensionsTests
{
    public class FindByWildcardVersionTests
    {
        private readonly DataFixture _dataFixture = new();

        [Theory]
        [InlineData("v*", 5)]
        [InlineData("*", 5)]
        [InlineData("0.*", 0, 2, 1)]
        [InlineData("0.*.*", 0, 2, 1)]
        [InlineData("0.0.*", 0, 0, 3)]
        [InlineData("0.2.*", 0, 2, 1)]
        [InlineData("0.1.*", 0, 1, 3)]
        [InlineData("2.*.*", 2, 1, 4)]
        [InlineData("2.*", 2, 1, 4)]
        [InlineData("2.1.*", 2, 1, 4)]
        [InlineData("1.*.*", 1, 3, 0)]
        [InlineData("1.2.*", 1, 2, 0)]
        [InlineData("1.1.*", 1, 1, 1)]
        [InlineData("1.*", 1, 3, 0)]
        [InlineData("v0.*", 0, 2, 1)]
        [InlineData("v0.*.*", 0, 2, 1)]
        [InlineData("v0.0.*", 0, 0, 3)]
        [InlineData("v0.2.*", 0, 2, 1)]
        [InlineData("v0.1.*", 0, 1, 3)]
        [InlineData("v2.*.*", 2, 1, 4)]
        [InlineData("v2.*", 2, 1, 4)]
        [InlineData("v2.1.*", 2, 1, 4)]
        [InlineData("v1.*.*", 1, 3, 0)]
        [InlineData("v1.2.*", 1, 2, 0)]
        [InlineData("v1.1.*", 1, 1, 1)]
        [InlineData("v1.*", 1, 3, 0)]
        public async Task TestDataSetVersions_ReturnsExpectedVersion(string versionString,
                int expectedMajor,
                int expectedMinor = default,
                int expectedPatch = default)
        {
            // Arrange
            var dataSetId = Guid.NewGuid();
            var queryable = SetupDataSetVersions(out dataSetId);

            // Act 
            var actualResult = await queryable.FindByWildcardVersion(dataSetId, versionString, CancellationToken.None);

            // Assert
            Assert.True(actualResult.IsRight);
            Assert.Equal(expectedMajor, actualResult.Right.VersionMajor);
            Assert.Equal(expectedMinor, actualResult.Right.VersionMinor);
            Assert.Equal(expectedPatch, actualResult.Right.VersionPatch);
        }

        [Theory]
        [InlineData("2.1*.0")]
        [InlineData("2.**.*")]
        [InlineData("2*.*.0")]
        [InlineData("2*.1.0")]
        [InlineData("*.1.0")]
        [InlineData("1.*.4")]
        [InlineData("2.4.*")]
        [InlineData("7.*")]
        [InlineData("0.3.*")]
        public async Task TestDataSetVersions_ReturnsNotFound(string versionString)
        {
            // Arrange
            Guid dataSetId;
            var queryable = SetupDataSetVersions(out dataSetId);

            // Act 
            var actualResult = await queryable.FindByWildcardVersion(dataSetId, versionString, CancellationToken.None);

            // Assert
            Assert.True(actualResult.IsLeft);
        }

        private IQueryable<DataSetVersion> SetupDataSetVersions(out Guid dataSetGuid)
        {
            DataSet dataSet = _dataFixture
                        .DefaultDataSet()
                        .WithStatusPublished();
            dataSetGuid = dataSet.Id;

            var dataSetVersions = _dataFixture
                .DefaultDataSetVersion(filters: 1, indicators: 1, locations: 1, timePeriods: 3)
                .WithStatusPublished()
                .WithDataSetId(dataSet.Id)
                .WithPreviewTokens(() => [_dataFixture.DefaultPreviewToken()])
                .ForIndex(0, dsv => dsv.SetVersionNumber(0, 0, 1))
                .ForIndex(1, dsv => dsv.SetVersionNumber(0, 0, 2))
                .ForIndex(2, dsv => dsv.SetVersionNumber(0, 0, 3))
                .ForIndex(3, dsv => dsv.SetVersionNumber(0, 1, 1))
                .ForIndex(4, dsv => dsv.SetVersionNumber(0, 1, 2))
                .ForIndex(5, dsv => dsv.SetVersionNumber(0, 1, 3))
                .ForIndex(6, dsv => dsv.SetVersionNumber(0, 2, 0))
                .ForIndex(7, dsv => dsv.SetVersionNumber(0, 2, 1))
                .ForIndex(8, dsv => dsv.SetVersionNumber(1, 0, 0))
                .ForIndex(9, dsv => dsv.SetVersionNumber(1, 0, 1))
                .ForIndex(10, dsv => dsv.SetVersionNumber(1, 0, 2))
                .ForIndex(11, dsv => dsv.SetVersionNumber(1, 0, 3))
                .ForIndex(12, dsv => dsv.SetVersionNumber(1, 1, 0))
                .ForIndex(13, dsv => dsv.SetVersionNumber(1, 1, 1))
                .ForIndex(14, dsv => dsv.SetVersionNumber(1, 2, 0))
                .ForIndex(15, dsv => dsv.SetVersionNumber(1, 3, 0))
                .ForIndex(16, dsv => dsv.SetVersionNumber(2, 0, 0))
                .ForIndex(17, dsv => dsv.SetVersionNumber(2, 1, 0))
                .ForIndex(18, dsv => dsv.SetVersionNumber(2, 1, 1))
                .ForIndex(19, dsv => dsv.SetVersionNumber(2, 1, 2))
                .ForIndex(20, dsv => dsv.SetVersionNumber(2, 1, 3))
                .ForIndex(21, dsv => dsv.SetVersionNumber(2, 1, 4))
                .ForIndex(22, dsv => dsv.SetVersionNumber(3, 0, 0))
                .ForIndex(23, dsv => dsv.SetVersionNumber(4, 0, 0))
                .ForIndex(24, dsv => dsv.SetVersionNumber(5, 0, 0))
                .GenerateList();

            var publicDataDbContextMock = new Mock<PublicDataDbContext>();
            publicDataDbContextMock.Setup(dbContext => dbContext.DataSetVersions).ReturnsDbSet(dataSetVersions);

            return publicDataDbContextMock.Object.DataSetVersions.AsNoTracking();
        }
    }
}
