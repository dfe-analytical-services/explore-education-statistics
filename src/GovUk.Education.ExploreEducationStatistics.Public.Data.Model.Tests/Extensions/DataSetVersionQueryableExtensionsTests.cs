using GovUk.Education.ExploreEducationStatistics.Common.Tests.Extensions;
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
    public class FindByVersionTests
    {
        private readonly DataFixture _dataFixture = new();

        [Theory]
        [MemberData(nameof(ValidVersions))]
        public async Task TestValidDataSetVersions_ReturnsExpectedVersion(string versionString,
                int expectedMajor,
                int expectedMinor = default,
                int expectedPatch = default)
        {
            // Arrange
            var dataSetId = Guid.NewGuid();
            var queryable = SetupDataSetVersions(out dataSetId);

            // Act 
            var actualResult = await queryable.FindByVersion(dataSetId, versionString, false, CancellationToken.None);

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
        [InlineData("1.   2  .0")]
        [InlineData("1.   2  .*")]
        [InlineData("1.   *  .0")]
        [InlineData("1.*.4")]
        [InlineData("2.4.*")]
        [InlineData("7.*")]
        [InlineData("0.0.*")]
        [InlineData("0.3.*")]
        [InlineData("")]
        [InlineData(" ")]
        [InlineData("v")]
        [InlineData(".")]
        [InlineData("..")]
        public async Task TestInvalidDataSetVersions_ReturnsNotFound(string versionString)
        {
            // Arrange
            Guid dataSetId;
            var queryable = SetupDataSetVersions(out dataSetId);

            // Act 
            var actualResult = await queryable.FindByVersion(dataSetId, versionString, false, CancellationToken.None);

            // Assert
            actualResult.AssertNotFound();
        }

        [Theory]
        [InlineData("2.*")]
        [InlineData("2.1.*")]
        [InlineData("2.1")]
        [InlineData("2.0")]

        public async Task TestPublicOnlyDataSetVersions_SpecifyDraftVersion_ReturnsNotFound(
            string versionString)
        {
            // Arrange
            DataSet dataSet = _dataFixture
                .DefaultDataSet()
                .WithStatusPublished();

            var dataSetVersions = _dataFixture
                .DefaultDataSetVersion(filters: 1, indicators: 1, locations: 1, timePeriods: 3)
                .WithStatusPublished()
                .WithDataSetId(dataSet.Id)
                .WithPreviewTokens(() => [_dataFixture.DefaultPreviewToken()])
                .ForIndex(1, dsv => dsv.SetVersionNumber(1, 1))
                .ForIndex(2, dsv => dsv.SetVersionNumber(1, 2))
                .ForIndex(3, dsv =>
                {
                    dsv.SetStatusDraft();
                    dsv.SetVersionNumber(2, 0);
                })
                .ForIndex(4, dsv =>
                {
                    dsv.SetStatusDraft();
                    dsv.SetVersionNumber(2, 1);
                })
                .GenerateList();

            var publicDataDbContextMock = new Mock<PublicDataDbContext>();
            publicDataDbContextMock.Setup(dbContext => dbContext.DataSetVersions).ReturnsDbSet(dataSetVersions);

            var queryable = publicDataDbContextMock.Object.DataSetVersions.AsNoTracking();

            // Act 
            var actualResult = await queryable.FindByVersion(dataSet.Id, versionString, true, CancellationToken.None);

            // Assert
            actualResult.AssertNotFound();
        }
        
        [Theory]
        [InlineData("1.*")]
        [InlineData("*")]
        [InlineData("v1.*")]
        [InlineData("v*")]
        public async Task TestPublicOnlyDataSetVersions_SpecifyWildCard_DoesNotReturnDraftVersion(
            string versionString)
        {
            // Arrange
            DataSet dataSet = _dataFixture
                .DefaultDataSet()
                .WithStatusPublished();

            var dataSetVersions = _dataFixture
                .DefaultDataSetVersion(filters: 1, indicators: 1, locations: 1, timePeriods: 3)
                .WithStatusPublished()
                .WithDataSetId(dataSet.Id)
                .WithPreviewTokens(() => [_dataFixture.DefaultPreviewToken()])
                .ForIndex(1, dsv => dsv.SetVersionNumber(1, 1))
                .ForIndex(2, dsv => dsv.SetVersionNumber(1, 2))
                .ForIndex(3, dsv =>
                {
                    dsv.SetStatusDraft();
                    dsv.SetVersionNumber(1, 3);
                })
                .ForIndex(4, dsv =>
                {
                    dsv.SetStatusDraft();
                    dsv.SetVersionNumber(2, 0);
                })
                .ForIndex(5, dsv =>
                {
                    dsv.SetStatusDraft();
                    dsv.SetVersionNumber(2, 1);
                })                
                .GenerateList();

            var publicDataDbContextMock = new Mock<PublicDataDbContext>();
            publicDataDbContextMock.Setup(dbContext => dbContext.DataSetVersions).ReturnsDbSet(dataSetVersions);

            var queryable = publicDataDbContextMock.Object.DataSetVersions.AsNoTracking();

            // Act 
            var actualResult = await queryable.FindByVersion(dataSet.Id, versionString, true, CancellationToken.None);

            // Assert
            Assert.Equal(1, actualResult.Right.VersionMajor);
            Assert.Equal(2, actualResult.Right.VersionMinor);
            Assert.Equal(0, actualResult.Right.VersionPatch);
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
                .ForIndex(1, dsv => dsv.SetVersionNumber(0, 1, 1))
                .ForIndex(2, dsv => dsv.SetVersionNumber(0, 1, 2))
                .ForIndex(3, dsv => dsv.SetVersionNumber(0, 1, 3))
                .ForIndex(4, dsv => dsv.SetVersionNumber(0, 2, 0))
                .ForIndex(5, dsv => dsv.SetVersionNumber(0, 2, 1))
                .ForIndex(6, dsv => dsv.SetVersionNumber(1, 0, 0))
                .ForIndex(7, dsv => dsv.SetVersionNumber(1, 0, 1))
                .ForIndex(8, dsv => dsv.SetVersionNumber(1, 0, 2))
                .ForIndex(9, dsv => dsv.SetVersionNumber(1, 0, 3))
                .ForIndex(10, dsv => dsv.SetVersionNumber(1, 1, 0))
                .ForIndex(11, dsv => dsv.SetVersionNumber(1, 1, 1))
                .ForIndex(12, dsv => dsv.SetVersionNumber(1, 2, 0))
                .ForIndex(13, dsv => dsv.SetVersionNumber(1, 3, 0))
                .ForIndex(14, dsv => dsv.SetVersionNumber(2, 0, 0))
                .ForIndex(15, dsv => dsv.SetVersionNumber(2, 1, 0))
                .ForIndex(16, dsv => dsv.SetVersionNumber(2, 1, 1))
                .ForIndex(17, dsv => dsv.SetVersionNumber(2, 1, 2))
                .ForIndex(18, dsv => dsv.SetVersionNumber(2, 1, 3))
                .ForIndex(19, dsv => dsv.SetVersionNumber(2, 1, 4))
                .ForIndex(20, dsv => dsv.SetVersionNumber(3, 0, 0))
                .ForIndex(21, dsv => dsv.SetVersionNumber(4, 0, 0))
                .ForIndex(22, dsv => dsv.SetVersionNumber(5, 0, 0))
                .GenerateList();

            var publicDataDbContextMock = new Mock<PublicDataDbContext>();
            publicDataDbContextMock.Setup(dbContext => dbContext.DataSetVersions).ReturnsDbSet(dataSetVersions);

            return publicDataDbContextMock.Object.DataSetVersions.AsNoTracking();
        }

        public static TheoryData<string, int, int, int> ValidVersions => new()
        {
            {"0.1.*", 0, 1, 3},
            {"0.1.3", 0, 1, 3},
            {"0.2", 0, 2, 0},
            {"0.2.*", 0, 2, 1},
            {"0.2.1", 0, 2, 1},
            {"0.*", 0, 2, 1},
            {"0.*.*", 0, 2, 1},
            {"1.1.*", 1, 1, 1},
            {"1.1.0", 1, 1, 0},
            {"1.2.*", 1, 2, 0},
            {"1.2.0", 1, 2, 0},
            {"1.3", 1, 3, 0},
            {"1.3.0", 1, 3, 0},
            {"1.*", 1, 3, 0},
            {"1.*.*", 1, 3, 0},
            {"2.1", 2, 1, 0},
            {"2.1.*", 2, 1, 4},
            {"2.1.4", 2, 1, 4},
            {"2.*", 2, 1, 4},
            {"2.*.*", 2, 1, 4},
            {"*", 5, 0, 0},
            {"v0.1.*", 0, 1, 3},
            {"v0.1.3", 0, 1, 3},
            {"v0.2", 0, 2, 0},
            {"v0.2.*", 0, 2, 1},
            {"v0.2.1", 0, 2, 1},
            {"v0.*", 0, 2, 1},
            {"v0.*.*", 0, 2, 1},
            {"v1.1.*", 1, 1, 1},
            {"v1.1.0", 1, 1, 0},
            {"v1.2.*", 1, 2, 0},
            {"v1.2.0", 1, 2, 0},
            {"v1.3", 1, 3, 0},
            {"v1.3.0", 1, 3, 0},
            {"v1.*", 1, 3, 0},
            {"v1.*.*", 1, 3, 0},
            {"v2.1", 2, 1, 0},
            {"v2.1.*", 2, 1, 4},
            {"v2.1.4", 2, 1, 4},
            {"v2.*", 2, 1, 4},
            {"v2.*.*", 2, 1, 4},
            {"v*", 5, 0, 0}
        };
    }
}
