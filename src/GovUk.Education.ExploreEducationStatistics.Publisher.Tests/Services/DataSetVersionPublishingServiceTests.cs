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

public class DataSetVersionPublishingServiceTests(PublisherFunctionsIntegrationTestFixture fixture) 
    : PublisherFunctionsIntegrationTest(fixture)
{
    public class PublishDataSetVersions(PublisherFunctionsIntegrationTestFixture fixture)
        : DataSetVersionPublishingServiceTests(fixture)
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
                .WithStatus(DataSetStatus.Draft);

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

            var service = GetRequiredService<IDataSetVersionPublishingService>();
            await service.PublishDataSetVersions([releaseVersion.Id]);

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
                .WithStatus(DataSetStatus.Published);

            var dataSetVersions = DataFixture
                .DefaultDataSetVersion(
                    filters: 1,
                    indicators: 1,
                    locations: 1,
                    timePeriods: 2)
                .WithDataSetId(dataSet.Id)
                .ForIndex(0, s => s.SetCsvFileId(Guid.NewGuid()))
                .ForIndex(1, s => s.SetCsvFileId(releaseDataFile.FileId))
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

            var service = GetRequiredService<IDataSetVersionPublishingService>();
            await service.PublishDataSetVersions([releaseVersion.Id]);

            await using var publicDataDbContext = GetDbContext<PublicDataDbContext>();

            var publishedDataSet = await publicDataDbContext
                .DataSets
                .Include(ds => ds.LatestDraftVersion)
                .Include(ds => ds.LatestLiveVersion)
                .SingleAsync(ds => ds.Id == dataSet.Id);

            publishedDataSet.Published.AssertUtcNow();
            Assert.Equal(DataSetStatus.Published, publishedDataSet.Status);
            Assert.Equal(dataSet.LatestDraftVersionId, publishedDataSet.LatestLiveVersionId);
            Assert.Null(publishedDataSet.LatestDraftVersion);

            Assert.NotNull(publishedDataSet.LatestLiveVersion);
            Assert.Equal(DataSetVersionStatus.Published, publishedDataSet.LatestLiveVersion.Status);
            publishedDataSet.LatestLiveVersion.Published.AssertUtcNow();
        }
    }
}
