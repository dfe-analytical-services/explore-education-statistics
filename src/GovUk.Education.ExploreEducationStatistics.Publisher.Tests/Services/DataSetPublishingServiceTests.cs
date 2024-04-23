using System;
using System.Threading.Tasks;
using GovUk.Education.ExploreEducationStatistics.Common.Model;
using GovUk.Education.ExploreEducationStatistics.Common.Tests.Extensions;
using GovUk.Education.ExploreEducationStatistics.Content.Model;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Database;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Tests.Fixtures;
using GovUk.Education.ExploreEducationStatistics.Public.Data.Model;
using GovUk.Education.ExploreEducationStatistics.Public.Data.Model.Database;
using GovUk.Education.ExploreEducationStatistics.Public.Data.Model.Tests.Fixtures;
using GovUk.Education.ExploreEducationStatistics.Publisher.Services.Interfaces;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace GovUk.Education.ExploreEducationStatistics.Publisher.Tests.Services;

public class DataSetPublishingServiceTests(PublisherFunctionsIntegrationTestFixture fixture) 
    : PublisherFunctionsIntegrationTest(fixture)
{
    public class PublishDataSets(PublisherFunctionsIntegrationTestFixture fixture)
        : DataSetPublishingServiceTests(fixture)
    {
        [Fact]
        public async Task FirstDataSetVersionPublished()
        {
            ReleaseVersion releaseVersion = DataFixture
                .DefaultReleaseVersion();

            ReleaseFile releaseDataFile = DataFixture
                .DefaultReleaseFile()
                .WithFile(DataFixture
                    .DefaultFile()
                    .WithType(FileType.Data))
                .WithReleaseVersion(releaseVersion);

            DataSet dataSet = DataFixture
                .DefaultDataSet()
                .WithStatusDraft();

            DataSetVersion dataSetVersion = DataFixture
                .DefaultDataSetVersion(
                    filters: 1,
                    indicators: 1,
                    locations: 1,
                    timePeriods: 2)
                .WithDataSetId(dataSet.Id)
                .WithCsvFileId(releaseDataFile.FileId);

            await AddTestData<ContentDbContext>(context => context.ReleaseFiles.Add(releaseDataFile));
            await AddTestData<PublicDataDbContext>(context =>
            {
                context.DataSets.Add(dataSet);
                context.SaveChanges();

                context.DataSetVersions.Add(dataSetVersion);
                context.SaveChanges();

                dataSet.LatestDraftVersionId = dataSetVersion.Id;
            });

            var service = GetRequiredService<IDataSetPublishingService>();
            await service.PublishDataSets([releaseVersion.Id]);

            await using var publicDataDbContext = GetDbContext<PublicDataDbContext>();

            var publishedDataSet = await publicDataDbContext
                .DataSets
                .Include(ds => ds.LatestLiveVersion)
                .SingleAsync(ds => ds.Id == dataSet.Id);

            publishedDataSet.Published.AssertUtcNow();
            Assert.Equal(DataSetStatus.Published, publishedDataSet.Status);
            Assert.Equal(dataSet.LatestDraftVersionId, publishedDataSet.LatestLiveVersionId);
            Assert.Null(publishedDataSet.LatestDraftVersionId);

            Assert.NotNull(publishedDataSet.LatestLiveVersion);
            Assert.Equal(DataSetVersionStatus.Published, publishedDataSet.LatestLiveVersion.Status);
            publishedDataSet.LatestLiveVersion.Published.AssertUtcNow();
        }

        [Fact]
        public async Task SecondDataSetVersionPublished()
        {
            ReleaseVersion releaseVersion = DataFixture
                .DefaultReleaseVersion();

            ReleaseFile releaseDataFile = DataFixture
                .DefaultReleaseFile()
                .WithFile(DataFixture
                    .DefaultFile()
                    .WithType(FileType.Data))
                .WithReleaseVersion(releaseVersion);

            DataSet dataSet = DataFixture
                .DefaultDataSet()
                .WithStatusPublished()
                .WithPublished(DateTimeOffset.UtcNow.AddDays(-1));

            var dataSetVersions = DataFixture
                .DefaultDataSetVersion(
                    filters: 1,
                    indicators: 1,
                    locations: 1,
                    timePeriods: 2)
                .WithDataSetId(dataSet.Id)
                .ForIndex(0, s => s
                    .SetCsvFileId(Guid.NewGuid())
                    .SetStatusPublished())
                .ForIndex(1, s => s
                    .SetCsvFileId(releaseDataFile.FileId)
                    .SetStatusDraft())
                .GenerateList(2);

            await AddTestData<ContentDbContext>(context => context.ReleaseFiles.Add(releaseDataFile));
            await AddTestData<PublicDataDbContext>(context =>
            {
                context.DataSets.Add(dataSet);
                context.SaveChanges();

                context.DataSetVersions.AddRange(dataSetVersions);
                context.SaveChanges();

                dataSet.LatestLiveVersionId = dataSetVersions[0].Id;
                dataSet.LatestDraftVersionId = dataSetVersions[1].Id;
            });

            var service = GetRequiredService<IDataSetPublishingService>();
            await service.PublishDataSets([releaseVersion.Id]);

            await using var publicDataDbContext = GetDbContext<PublicDataDbContext>();

            var publishedDataSet = await publicDataDbContext
                .DataSets
                .Include(ds => ds.LatestDraftVersion)
                .Include(ds => ds.LatestLiveVersion)
                .Include(ds => ds.Versions)
                .SingleAsync(ds => ds.Id == dataSet.Id);

            // Assert that the already-set Published date on the overarching DataSet is retained so that it indicates
            // the first date at which its first DataSetVersion was published.
            Assert.Equal(dataSet.Published.TruncateNanoseconds(), publishedDataSet.Published);
            
            Assert.Equal(DataSetStatus.Published, publishedDataSet.Status);
            Assert.Equal(dataSet.LatestDraftVersionId, publishedDataSet.LatestLiveVersionId);
            Assert.Null(publishedDataSet.LatestDraftVersion);
            
            Assert.NotNull(publishedDataSet.LatestLiveVersion);
            Assert.Equal(DataSetVersionStatus.Published, publishedDataSet.LatestLiveVersion.Status);
            publishedDataSet.LatestLiveVersion.Published.AssertUtcNow();
            
            Assert.Equal(2, publishedDataSet.Versions.Count);
            publishedDataSet.Versions.ForEach(version => 
                Assert.Equal(DataSetVersionStatus.Published, version.Status));
        }
        
        [Fact]
        public async Task NoDraftDataSetVersionsToPublish()
        {
            ReleaseVersion releaseVersion = DataFixture
                .DefaultReleaseVersion();

            ReleaseFile releaseDataFile = DataFixture
                .DefaultReleaseFile()
                .WithFile(DataFixture
                    .DefaultFile()
                    .WithType(FileType.Data))
                .WithReleaseVersion(releaseVersion);

            DataSet dataSet = DataFixture
                .DefaultDataSet()
                .WithStatusPublished()
                .WithPublished(DateTimeOffset.UtcNow.AddDays(-1).TruncateNanoseconds());

            DataSetVersion dataSetVersion = DataFixture
                .DefaultDataSetVersion(
                    filters: 1,
                    indicators: 1,
                    locations: 1,
                    timePeriods: 2)
                .WithDataSetId(dataSet.Id)
                .WithStatusPublished()
                .WithCsvFileId(releaseDataFile.FileId);

            await AddTestData<ContentDbContext>(context => context.ReleaseFiles.Add(releaseDataFile));
            await AddTestData<PublicDataDbContext>(context =>
            {
                context.DataSets.Add(dataSet);
                context.SaveChanges();

                context.DataSetVersions.AddRange(dataSetVersion);
                context.SaveChanges();

                dataSet.LatestLiveVersionId = dataSetVersion.Id;
            });

            var service = GetRequiredService<IDataSetPublishingService>();
            await service.PublishDataSets([releaseVersion.Id]);

            await using var publicDataDbContext = GetDbContext<PublicDataDbContext>();

            var retrievedDataSet = await publicDataDbContext
                .DataSets
                .Include(ds => ds.LatestDraftVersion)
                .Include(ds => ds.LatestLiveVersion)
                .Include(ds => ds.Versions)
                .SingleAsync(ds => ds.Id == dataSet.Id);

            // Assert that no publishing details have changed on the overarching DataSet.
            Assert.Equal(dataSet.Published, retrievedDataSet.Published);
            Assert.Equal(DataSetStatus.Published, retrievedDataSet.Status);
            
            // Assert that the LatestLiveVersion has not been changed.
            Assert.Equal(dataSet.LatestLiveVersionId, retrievedDataSet.LatestLiveVersionId);
            Assert.Null(retrievedDataSet.LatestDraftVersion);
            Assert.Equal(dataSet.LatestLiveVersion!.Status, retrievedDataSet.LatestLiveVersion!.Status);
            Assert.Equal(dataSet.LatestLiveVersion.Published.TruncateNanoseconds(), retrievedDataSet.LatestLiveVersion.Published);
        }
    }
}
