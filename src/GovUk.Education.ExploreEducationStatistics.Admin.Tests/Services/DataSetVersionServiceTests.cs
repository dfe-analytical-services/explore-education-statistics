#nullable enable
using GovUk.Education.ExploreEducationStatistics.Admin.Services.Interfaces.Public.Data;
using GovUk.Education.ExploreEducationStatistics.Admin.Tests.Fixture;
using GovUk.Education.ExploreEducationStatistics.Common.Model;
using GovUk.Education.ExploreEducationStatistics.Content.Model;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Database;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Tests.Fixtures;
using GovUk.Education.ExploreEducationStatistics.Public.Data.Model;
using GovUk.Education.ExploreEducationStatistics.Public.Data.Model.Database;
using GovUk.Education.ExploreEducationStatistics.Public.Data.Model.Tests.Fixtures;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using static GovUk.Education.ExploreEducationStatistics.Common.Utils.EnumUtil;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Tests.Services;

public abstract class DataSetVersionServiceTests(TestApplicationFactory testApp) : IntegrationTestFixture(testApp)
{
    public class GetStatusesForReleaseVersionTests(TestApplicationFactory testApp) : DataSetVersionServiceTests(testApp)
    {
        /// <summary>
        /// Test that each DataSetVersion status is reported correctly.
        /// </summary>
        [Fact]
        public async Task AllStatuses()
        {
            await GetEnums<DataSetVersionStatus>()
                .ToAsyncEnumerable()
                .ForEachAwaitAsync(AssertDataSetVersionStatusReturnedOk);
        }

        /// <summary>
        /// Test the scenario when data set versions exist, but for an unrelated ReleaseVersion.
        /// </summary>
        [Fact]
        public async Task DataSetVersionForDifferentReleaseVersion()
        {
            var unrelatedReleaseVersion = Guid.NewGuid();

            ReleaseVersion releaseVersion = DataFixture
                .DefaultReleaseVersion();

            ReleaseFile dataFile = DataFixture
                .DefaultReleaseFile()
                .WithFile(DataFixture.DefaultFile(FileType.Data))
                .WithReleaseVersion(releaseVersion);

            DataSetVersion dataSetVersion = DataFixture
                .DefaultDataSetVersion(filters: 1,
                    indicators: 1,
                    locations: 1,
                    timePeriods: 2)
                .WithDataSet(DataFixture.DefaultDataSet())
                .WithRelease(DataFixture.DefaultDataSetVersionRelease()
                    .WithReleaseFileId(dataFile.Id))
                .WithStatus(DataSetVersionStatus.Processing);

            await TestApp.AddTestData<ContentDbContext>(context =>
            {
                context.ReleaseFiles.Add(dataFile);
            });

            await TestApp.AddTestData<PublicDataDbContext>(context =>
                context.DataSetVersions.Add(dataSetVersion));

            var service = TestApp.Services.GetRequiredService<IDataSetVersionService>();

            Assert.Empty(await service.GetStatusesForReleaseVersion(unrelatedReleaseVersion));
        }

        private async Task AssertDataSetVersionStatusReturnedOk(DataSetVersionStatus status)
        {
            ReleaseVersion releaseVersion = DataFixture
                .DefaultReleaseVersion();

            ReleaseFile dataFile = DataFixture
                .DefaultReleaseFile()
                .WithFile(DataFixture.DefaultFile(FileType.Data))
                .WithReleaseVersion(releaseVersion);

            DataSetVersion dataSetVersion = DataFixture
                .DefaultDataSetVersion(filters: 1,
                    indicators: 1,
                    locations: 1,
                    timePeriods: 2)
                .WithDataSet(DataFixture.DefaultDataSet())
                .WithRelease(DataFixture.DefaultDataSetVersionRelease()
                    .WithReleaseFileId(dataFile.Id))
                .WithStatus(status);

            await TestApp.AddTestData<ContentDbContext>(context =>
            {
                context.ReleaseFiles.Add(dataFile);
            });

            await TestApp.AddTestData<PublicDataDbContext>(context =>
                context.DataSetVersions.Add(dataSetVersion));

            var service = TestApp.Services.GetRequiredService<IDataSetVersionService>();

            var statusSummary = Assert.Single(await service.GetStatusesForReleaseVersion(dataFile.ReleaseVersionId));
            Assert.Equal(dataSetVersion.Id, statusSummary.Id);
            Assert.Equal(dataSetVersion.DataSet.Title, statusSummary.Title);
            Assert.Equal(status, statusSummary.Status);
        }
    }

    public class UpdateVersionsForReleaseVersionTests(TestApplicationFactory testApp)
        : DataSetVersionServiceTests(testApp)
    {
        [Fact]
        public async Task Success()
        {
            ReleaseVersion releaseVersion = DataFixture.DefaultReleaseVersion()
                .WithRelease(DataFixture.DefaultRelease()
                    .WithPublication(DataFixture.DefaultPublication()));

            var (releaseDataFile1, releaseDataFile2) = DataFixture
                .DefaultReleaseFile()
                .WithFile(DataFixture.DefaultFile(FileType.Data))
                .WithReleaseVersion(releaseVersion)
                .GenerateTuple2();

            await TestApp.AddTestData<ContentDbContext>(context =>
            {
                context.ReleaseVersions.Add(releaseVersion);
                context.ReleaseFiles.AddRange(releaseDataFile1, releaseDataFile2);
            });

            DataSetVersion dataSetVersion1 = DataFixture
                .DefaultDataSetVersion()
                .WithDataSet(DataFixture.DefaultDataSet())
                .WithRelease(DataFixture.DefaultDataSetVersionRelease()
                    .WithReleaseFileId(releaseDataFile1.Id)
                    .WithSlug(releaseVersion.Release.Slug)
                    .WithTitle(releaseVersion.Release.Title));

            DataSetVersion dataSetVersion2 = DataFixture
                .DefaultDataSetVersion()
                .WithDataSet(DataFixture.DefaultDataSet())
                .WithRelease(DataFixture.DefaultDataSetVersionRelease()
                    .WithReleaseFileId(releaseDataFile2.Id)
                    .WithSlug(releaseVersion.Release.Slug)
                    .WithTitle(releaseVersion.Release.Title));

            await TestApp.AddTestData<PublicDataDbContext>(context =>
            {
                context.DataSetVersions.AddRange(dataSetVersion1, dataSetVersion2);
            });

            var service = TestApp.Services.GetRequiredService<IDataSetVersionService>();

            const string updatedReleaseSlug = "2024-25";
            const string updatedReleaseTitle = "Academic year 2024/25";

            await service.UpdateVersionsForReleaseVersion(
                releaseVersion.Id,
                releaseSlug: updatedReleaseSlug,
                releaseTitle: updatedReleaseTitle);

            await using var publicDataDbContext = TestApp.GetDbContext<PublicDataDbContext>();

            var actualDataSetVersions = await publicDataDbContext.DataSetVersions
                .AsNoTracking()
                .ToListAsync();

            Assert.Equal(2, actualDataSetVersions.Count);

            Assert.All(actualDataSetVersions,
                dataSetVersion =>
                {
                    Assert.Equal(updatedReleaseSlug, dataSetVersion.Release.Slug);
                    Assert.Equal(updatedReleaseTitle, dataSetVersion.Release.Title);
                });
        }

        [Fact]
        public async Task UnrelatedDataSetVersionsAreNotUpdated()
        {
            ReleaseVersion releaseVersion1 = DataFixture.DefaultReleaseVersion()
                .WithRelease(DataFixture.DefaultRelease()
                    .WithPublication(DataFixture.DefaultPublication()));

            ReleaseVersion releaseVersion2 = DataFixture.DefaultReleaseVersion()
                .WithRelease(DataFixture.DefaultRelease()
                    .WithPublication(DataFixture.DefaultPublication()));

            ReleaseFile releaseVersion1DataFile = DataFixture
                .DefaultReleaseFile()
                .WithFile(DataFixture.DefaultFile(FileType.Data))
                .WithReleaseVersion(releaseVersion1);

            ReleaseFile releaseVersion2DataFile = DataFixture
                .DefaultReleaseFile()
                .WithFile(DataFixture.DefaultFile(FileType.Data))
                .WithReleaseVersion(releaseVersion2);

            await TestApp.AddTestData<ContentDbContext>(context =>
            {
                context.ReleaseVersions.AddRange(releaseVersion1, releaseVersion2);
                context.ReleaseFiles.AddRange(releaseVersion1DataFile, releaseVersion2DataFile);
            });

            DataSetVersion dataSetVersion1 = DataFixture
                .DefaultDataSetVersion()
                .WithDataSet(DataFixture.DefaultDataSet())
                .WithRelease(DataFixture.DefaultDataSetVersionRelease()
                    .WithReleaseFileId(releaseVersion1DataFile.Id)
                    .WithSlug(releaseVersion1.Release.Slug)
                    .WithTitle(releaseVersion1.Release.Title));

            DataSetVersion dataSetVersion2 = DataFixture
                .DefaultDataSetVersion()
                .WithDataSet(DataFixture.DefaultDataSet())
                .WithRelease(DataFixture.DefaultDataSetVersionRelease()
                    .WithReleaseFileId(releaseVersion2DataFile.Id)
                    .WithSlug(releaseVersion2.Release.Slug)
                    .WithTitle(releaseVersion2.Release.Title));

            await TestApp.AddTestData<PublicDataDbContext>(context =>
            {
                context.DataSetVersions.AddRange(dataSetVersion1, dataSetVersion2);
            });

            var service = TestApp.Services.GetRequiredService<IDataSetVersionService>();

            const string updatedReleaseSlug = "2024-25";
            const string updatedReleaseTitle = "Academic year 2024/25";

            await service.UpdateVersionsForReleaseVersion(
                releaseVersion1.Id,
                releaseSlug: updatedReleaseSlug,
                releaseTitle: updatedReleaseTitle);

            await using var publicDataDbContext = TestApp.GetDbContext<PublicDataDbContext>();

            var actualDataSetVersion1 = await publicDataDbContext.DataSetVersions
                .AsNoTracking()
                .SingleAsync(dsv => dsv.Id == dataSetVersion1.Id);

            Assert.Equal(updatedReleaseSlug, actualDataSetVersion1.Release.Slug);
            Assert.Equal(updatedReleaseTitle, actualDataSetVersion1.Release.Title);

            // Assert that the data set version unrelated to the release version has not been updated
            var actualDataSetVersion2 = await publicDataDbContext.DataSetVersions
                .AsNoTracking()
                .SingleAsync(dsv => dsv.Id == dataSetVersion2.Id);

            Assert.Equal(releaseVersion2.Release.Slug, actualDataSetVersion2.Release.Slug);
            Assert.Equal(releaseVersion2.Release.Title, actualDataSetVersion2.Release.Title);
        }
    }
}
