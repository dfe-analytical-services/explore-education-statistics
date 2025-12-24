#nullable enable
using GovUk.Education.ExploreEducationStatistics.Admin.Services.Interfaces.Public.Data;
using GovUk.Education.ExploreEducationStatistics.Admin.Tests.Fixture.Optimised;
using GovUk.Education.ExploreEducationStatistics.Common.IntegrationTests;
using GovUk.Education.ExploreEducationStatistics.Common.IntegrationTests.WebApp;
using GovUk.Education.ExploreEducationStatistics.Common.Model;
using GovUk.Education.ExploreEducationStatistics.Common.Tests.Fixtures;
using GovUk.Education.ExploreEducationStatistics.Content.Model;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Tests.Fixtures;
using GovUk.Education.ExploreEducationStatistics.Public.Data.Model;
using GovUk.Education.ExploreEducationStatistics.Public.Data.Model.Tests.Fixtures;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore;
using static GovUk.Education.ExploreEducationStatistics.Common.Utils.EnumUtil;

#pragma warning disable CS9107 // Parameter is captured into the state of the enclosing type and its value is also passed to the base constructor. The value might be captured by the base class as well.

namespace GovUk.Education.ExploreEducationStatistics.Admin.Tests.Services;

// ReSharper disable once ClassNeverInstantiated.Global
public class DataSetVersionServiceTestsFixture()
    : OptimisedAdminCollectionFixture(
        capabilities: [AdminIntegrationTestCapability.UserAuth, AdminIntegrationTestCapability.Postgres]
    )
{
    public IDataSetVersionService DataSetVersionService = null!;

    protected override async Task AfterFactoryConstructed(OptimisedServiceCollectionLookups lookups)
    {
        await base.AfterFactoryConstructed(lookups);
        DataSetVersionService = lookups.GetService<IDataSetVersionService>();
    }
}

[CollectionDefinition(nameof(DataSetVersionServiceTestsFixture))]
public class DataSetVersionServiceTestsCollection : ICollectionFixture<DataSetVersionServiceTestsFixture>;

[Collection(nameof(DataSetVersionServiceTestsFixture))]
public abstract class DataSetVersionServiceTests(DataSetVersionServiceTestsFixture fixture)
    : OptimisedIntegrationTestBase<Startup>(fixture)
{
    private static readonly DataFixture DataFixture = new();

    public class GetStatusesForReleaseVersionTests(DataSetVersionServiceTestsFixture fixture)
        : DataSetVersionServiceTests(fixture)
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

            ReleaseVersion releaseVersion = DataFixture.DefaultReleaseVersion();

            ReleaseFile dataFile = DataFixture
                .DefaultReleaseFile()
                .WithFile(DataFixture.DefaultFile(FileType.Data))
                .WithReleaseVersion(releaseVersion);

            DataSetVersion dataSetVersion = DataFixture
                .DefaultDataSetVersion(filters: 1, indicators: 1, locations: 1, timePeriods: 2)
                .WithDataSet(DataFixture.DefaultDataSet())
                .WithRelease(DataFixture.DefaultDataSetVersionRelease().WithReleaseFileId(dataFile.Id))
                .WithStatus(DataSetVersionStatus.Processing);

            await fixture
                .GetContentDbContext()
                .AddTestData(context =>
                {
                    context.ReleaseFiles.Add(dataFile);
                });

            await fixture.GetPublicDataDbContext().AddTestData(context => context.DataSetVersions.Add(dataSetVersion));

            Assert.Empty(await fixture.DataSetVersionService.GetStatusesForReleaseVersion(unrelatedReleaseVersion));
        }

        private async Task AssertDataSetVersionStatusReturnedOk(DataSetVersionStatus status)
        {
            ReleaseVersion releaseVersion = DataFixture.DefaultReleaseVersion();

            ReleaseFile dataFile = DataFixture
                .DefaultReleaseFile()
                .WithFile(DataFixture.DefaultFile(FileType.Data))
                .WithReleaseVersion(releaseVersion);

            DataSetVersion dataSetVersion = DataFixture
                .DefaultDataSetVersion(filters: 1, indicators: 1, locations: 1, timePeriods: 2)
                .WithDataSet(DataFixture.DefaultDataSet())
                .WithRelease(DataFixture.DefaultDataSetVersionRelease().WithReleaseFileId(dataFile.Id))
                .WithStatus(status);

            await fixture
                .GetContentDbContext()
                .AddTestData(context =>
                {
                    context.ReleaseFiles.Add(dataFile);
                });

            await fixture.GetPublicDataDbContext().AddTestData(context => context.DataSetVersions.Add(dataSetVersion));

            var statusSummary = Assert.Single(
                await fixture.DataSetVersionService.GetStatusesForReleaseVersion(dataFile.ReleaseVersionId)
            );
            Assert.Equal(dataSetVersion.Id, statusSummary.Id);
            Assert.Equal(dataSetVersion.DataSet.Title, statusSummary.Title);
            Assert.Equal(status, statusSummary.Status);
        }
    }

    public class UpdateVersionsForReleaseVersionTests(DataSetVersionServiceTestsFixture fixture)
        : DataSetVersionServiceTests(fixture)
    {
        [Fact]
        public async Task Success()
        {
            ReleaseVersion releaseVersion = DataFixture
                .DefaultReleaseVersion()
                .WithRelease(DataFixture.DefaultRelease().WithPublication(DataFixture.DefaultPublication()));

            var (releaseDataFile1, releaseDataFile2) = DataFixture
                .DefaultReleaseFile()
                .WithFile(DataFixture.DefaultFile(FileType.Data))
                .WithReleaseVersion(releaseVersion)
                .GenerateTuple2();

            await fixture
                .GetContentDbContext()
                .AddTestData(context =>
                {
                    context.ReleaseVersions.Add(releaseVersion);
                    context.ReleaseFiles.AddRange(releaseDataFile1, releaseDataFile2);
                });

            DataSetVersion dataSetVersion1 = DataFixture
                .DefaultDataSetVersion()
                .WithDataSet(DataFixture.DefaultDataSet())
                .WithRelease(
                    DataFixture
                        .DefaultDataSetVersionRelease()
                        .WithReleaseFileId(releaseDataFile1.Id)
                        .WithSlug(releaseVersion.Release.Slug)
                        .WithTitle(releaseVersion.Release.Title)
                );

            DataSetVersion dataSetVersion2 = DataFixture
                .DefaultDataSetVersion()
                .WithDataSet(DataFixture.DefaultDataSet())
                .WithRelease(
                    DataFixture
                        .DefaultDataSetVersionRelease()
                        .WithReleaseFileId(releaseDataFile2.Id)
                        .WithSlug(releaseVersion.Release.Slug)
                        .WithTitle(releaseVersion.Release.Title)
                );

            await fixture
                .GetPublicDataDbContext()
                .AddTestData(context =>
                {
                    context.DataSetVersions.AddRange(dataSetVersion1, dataSetVersion2);
                });

            const string updatedReleaseSlug = "2024-25";
            const string updatedReleaseTitle = "Academic year 2024/25";

            await fixture.DataSetVersionService.UpdateVersionsForReleaseVersion(
                releaseVersion.Id,
                releaseSlug: updatedReleaseSlug,
                releaseTitle: updatedReleaseTitle
            );

            List<Guid> owningDataSetIds = [dataSetVersion1.DataSetId, dataSetVersion2.DataSetId];

            var actualDataSetVersions = await fixture
                .GetPublicDataDbContext()
                .DataSetVersions.AsNoTracking()
                .Where(dsv => owningDataSetIds.Contains(dsv.DataSetId))
                .ToListAsync();

            Assert.Equal(2, actualDataSetVersions.Count);

            Assert.All(
                actualDataSetVersions,
                [UsedImplicitly]
                (dataSetVersion) =>
                {
                    Assert.Equal(updatedReleaseSlug, dataSetVersion.Release.Slug);
                    Assert.Equal(updatedReleaseTitle, dataSetVersion.Release.Title);
                }
            );
        }

        [Fact]
        public async Task UnrelatedDataSetVersionsAreNotUpdated()
        {
            ReleaseVersion releaseVersion1 = DataFixture
                .DefaultReleaseVersion()
                .WithRelease(DataFixture.DefaultRelease().WithPublication(DataFixture.DefaultPublication()));

            ReleaseVersion releaseVersion2 = DataFixture
                .DefaultReleaseVersion()
                .WithRelease(DataFixture.DefaultRelease().WithPublication(DataFixture.DefaultPublication()));

            ReleaseFile releaseVersion1DataFile = DataFixture
                .DefaultReleaseFile()
                .WithFile(DataFixture.DefaultFile(FileType.Data))
                .WithReleaseVersion(releaseVersion1);

            ReleaseFile releaseVersion2DataFile = DataFixture
                .DefaultReleaseFile()
                .WithFile(DataFixture.DefaultFile(FileType.Data))
                .WithReleaseVersion(releaseVersion2);

            await fixture
                .GetContentDbContext()
                .AddTestData(context =>
                {
                    context.ReleaseVersions.AddRange(releaseVersion1, releaseVersion2);
                    context.ReleaseFiles.AddRange(releaseVersion1DataFile, releaseVersion2DataFile);
                });

            DataSetVersion dataSetVersion1 = DataFixture
                .DefaultDataSetVersion()
                .WithDataSet(DataFixture.DefaultDataSet())
                .WithRelease(
                    DataFixture
                        .DefaultDataSetVersionRelease()
                        .WithReleaseFileId(releaseVersion1DataFile.Id)
                        .WithSlug(releaseVersion1.Release.Slug)
                        .WithTitle(releaseVersion1.Release.Title)
                );

            DataSetVersion dataSetVersion2 = DataFixture
                .DefaultDataSetVersion()
                .WithDataSet(DataFixture.DefaultDataSet())
                .WithRelease(
                    DataFixture
                        .DefaultDataSetVersionRelease()
                        .WithReleaseFileId(releaseVersion2DataFile.Id)
                        .WithSlug(releaseVersion2.Release.Slug)
                        .WithTitle(releaseVersion2.Release.Title)
                );

            await fixture
                .GetPublicDataDbContext()
                .AddTestData(context =>
                {
                    context.DataSetVersions.AddRange(dataSetVersion1, dataSetVersion2);
                });

            const string updatedReleaseSlug = "2024-25";
            const string updatedReleaseTitle = "Academic year 2024/25";

            await fixture.DataSetVersionService.UpdateVersionsForReleaseVersion(
                releaseVersion1.Id,
                releaseSlug: updatedReleaseSlug,
                releaseTitle: updatedReleaseTitle
            );

            var actualDataSetVersion1 = await fixture
                .GetPublicDataDbContext()
                .DataSetVersions.AsNoTracking()
                .SingleAsync(dsv => dsv.Id == dataSetVersion1.Id);

            Assert.Equal(updatedReleaseSlug, actualDataSetVersion1.Release.Slug);
            Assert.Equal(updatedReleaseTitle, actualDataSetVersion1.Release.Title);

            // Assert that the data set version unrelated to the release version has not been updated
            var actualDataSetVersion2 = await fixture
                .GetPublicDataDbContext()
                .DataSetVersions.AsNoTracking()
                .SingleAsync(dsv => dsv.Id == dataSetVersion2.Id);

            Assert.Equal(releaseVersion2.Release.Slug, actualDataSetVersion2.Release.Slug);
            Assert.Equal(releaseVersion2.Release.Title, actualDataSetVersion2.Release.Title);
        }
    }
}
