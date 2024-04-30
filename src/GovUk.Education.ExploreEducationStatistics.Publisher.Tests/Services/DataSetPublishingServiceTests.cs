using System;
using System.Linq;
using System.Threading.Tasks;
using GovUk.Education.ExploreEducationStatistics.Common.Extensions;
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
                .WithReleaseFileId(releaseDataFile.Id);

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
                    .SetReleaseFileId(Guid.NewGuid())
                    .SetStatusPublished())
                .ForIndex(1, s => s
                    .SetReleaseFileId(releaseDataFile.Id)
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
                .WithReleaseFileId(releaseDataFile.Id);

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
        
        /// <summary>
        /// In this test, a previous ReleaseVersion has a ReleaseFile that is being used as the basis of a Public API
        /// DataSet. There is therefore a published DataSetVersion that references a ReleaseFile that is connected to
        /// that previous ReleaseVersion.
        ///
        /// A ReleaseVersion amendment is now being published that still contains the Data File that is a basis of
        /// the Public API DataSet. This means that the ReleaseVersion amendment has its own ReleaseFile link table
        /// entry linking it to the Data File.  We now test that the DataSetVersion that referenced the ReleaseFile
        /// link table entry from the previous ReleaseVersion has been updated to reference the equivalent ReleaseFile
        /// from the ReleaseVersion amendment. 
        /// </summary>
        [Fact]
        public async Task ReleaseAmendmentPublished_ContainsPreviousReleaseVersionDataFile()
        {
            ReleaseVersion originalReleaseVersion = DataFixture
                .DefaultReleaseVersion();

            ReleaseVersion amendmentReleaseVersion = DataFixture
                .DefaultReleaseVersion()
                .WithPreviousVersionId(originalReleaseVersion.Id);
            
            File dataFile = DataFixture
                .DefaultFile()
                .WithType(FileType.Data);

            var (originalReleaseDataFile, amendmentReleaseDataFile) = DataFixture
                .DefaultReleaseFile()
                .WithFile(dataFile)
                .ForIndex(0, s => s.SetReleaseVersion(originalReleaseVersion))
                .ForIndex(1, s => s.SetReleaseVersion(amendmentReleaseVersion))
                .Generate(2)
                .ToTuple2();

            DataSet dataSet = DataFixture
                .DefaultDataSet()
                .WithStatusPublished();

            DataSetVersion originalReleaseVersionDataSetVersion = DataFixture
                .DefaultDataSetVersion(
                    filters: 1,
                    indicators: 1,
                    locations: 1,
                    timePeriods: 2)
                .WithDataSetId(dataSet.Id)
                .WithReleaseFileId(originalReleaseDataFile.Id)
                .WithStatusPublished();

            await AddTestData<ContentDbContext>(context => 
                context.ReleaseFiles.AddRange(originalReleaseDataFile, amendmentReleaseDataFile));
            
            await AddTestData<PublicDataDbContext>(context =>
            {
                context.DataSets.Add(dataSet);
                context.SaveChanges();

                context.DataSetVersions.Add(originalReleaseVersionDataSetVersion);
                context.SaveChanges();

                dataSet.LatestLiveVersionId = originalReleaseVersionDataSetVersion.Id;
            });

            var service = GetRequiredService<IDataSetPublishingService>();
            await service.PublishDataSets([amendmentReleaseVersion.Id]);

            await using var publicDataDbContext = GetDbContext<PublicDataDbContext>();

            var publishedDataSet = await publicDataDbContext
                .DataSets
                .Include(ds => ds.LatestLiveVersion)
                .SingleAsync(ds => ds.Id == dataSet.Id);

            Assert.Equal(dataSet.Published.TruncateNanoseconds(), publishedDataSet.Published);
            Assert.Equal(DataSetStatus.Published, publishedDataSet.Status);
            Assert.Equal(dataSet.LatestLiveVersionId, publishedDataSet.LatestLiveVersionId);
            Assert.Null(publishedDataSet.LatestDraftVersionId);

            Assert.Equal(DataSetVersionStatus.Published, publishedDataSet.LatestLiveVersion.Status);
            Assert.Equal(originalReleaseVersionDataSetVersion.Published.TruncateNanoseconds(), publishedDataSet.LatestLiveVersion.Published);
            
            // Assert that the DataSetVersion that was created for the previous ReleaseVersion has been 
            // updated to be linked to the ReleaseFile that is linked to the now-published amendment version
            // of this Release.
            Assert.Equal(amendmentReleaseDataFile.Id, publishedDataSet.LatestLiveVersion.ReleaseFileId);
        }
        
        /// <summary>
        /// In this test, a previous ReleaseVersion has a ReleaseFile that is being used as the basis of a Public API
        /// DataSet. There is therefore a published DataSetVersion that references a ReleaseFile that is connected to
        /// that previous ReleaseVersion.
        ///
        /// A ReleaseVersion amendment is now being published that still references the Data File referenced by the
        /// original DataSetVersion.  At the same time, this amendment *also* contains a brand new Data File that has
        /// become the basis for a new version of the same DataSet that was in the previous ReleaseVersion.  We
        /// therefore add a new "2.0" DataSetVersion entry that is the next version up from the existing "1.0"
        /// DataSetVersion and is backed by the new Data File on the ReleaseVersion amendment.
        ///
        /// We test that the first DataSetVersion from the previous ReleaseVersion has its ReleaseFile value updated to
        /// point at the equivalent ReleaseFile in the ReleaseVersion amendment.
        ///
        /// We also test that the new DataSetVersion is promoted correctly alongside the new ReleaseVersion.
        /// </summary>
        [Fact]
        public async Task ReleaseAmendmentPublishedWithNewDataSetVersion_PreviousDataSetVersionReferencesPreviousReleaseVersion()
        {
            ReleaseVersion originalReleaseVersion = DataFixture
                .DefaultReleaseVersion();

            ReleaseVersion amendmentReleaseVersion = DataFixture
                .DefaultReleaseVersion()
                .WithPreviousVersionId(originalReleaseVersion.Id);
            
            var (dataFile, newDataFile) = DataFixture
                .DefaultFile()
                .WithType(FileType.Data)
                .Generate(2)
                .ToTuple2();

            var (originalReleaseDataFile, amendmentReleaseDataFile) = DataFixture
                .DefaultReleaseFile()
                .WithFile(dataFile)
                .ForIndex(0, s => s.SetReleaseVersion(originalReleaseVersion))
                .ForIndex(1, s => s.SetReleaseVersion(amendmentReleaseVersion))
                .Generate(2)
                .ToTuple2();
            
            ReleaseFile newReleaseDataFile = DataFixture
                .DefaultReleaseFile()
                .WithFile(newDataFile)
                .WithReleaseVersion(amendmentReleaseVersion);

            DataSet dataSet = DataFixture
                .DefaultDataSet()
                .WithStatusPublished();

            DataSetVersion originalReleaseVersionDataSetVersion = DataFixture
                .DefaultDataSetVersion(
                    filters: 1,
                    indicators: 1,
                    locations: 1,
                    timePeriods: 2)
                .WithDataSetId(dataSet.Id)
                .WithReleaseFileId(originalReleaseDataFile.Id)
                .WithStatusPublished();
            
            DataSetVersion amendmentReleaseVersionDataSetVersion = DataFixture
                .DefaultDataSetVersion(
                    filters: 1,
                    indicators: 1,
                    locations: 1,
                    timePeriods: 2)
                .WithDataSetId(dataSet.Id)
                .WithReleaseFileId(newReleaseDataFile.Id)
                .WithVersionNumber(major: originalReleaseVersionDataSetVersion.VersionMajor + 1, minor: 0)
                .WithStatusDraft();

            await AddTestData<ContentDbContext>(context => 
                context.ReleaseFiles.AddRange(originalReleaseDataFile, amendmentReleaseDataFile, newReleaseDataFile));
            
            await AddTestData<PublicDataDbContext>(context =>
            {
                context.DataSets.Add(dataSet);
                context.SaveChanges();

                context.DataSetVersions.AddRange(
                    originalReleaseVersionDataSetVersion, amendmentReleaseVersionDataSetVersion);
                
                context.SaveChanges();

                dataSet.LatestLiveVersionId = originalReleaseVersionDataSetVersion.Id;
                dataSet.LatestDraftVersionId = amendmentReleaseVersionDataSetVersion.Id;
            });

            var service = GetRequiredService<IDataSetPublishingService>();
            await service.PublishDataSets([amendmentReleaseVersion.Id]);

            await using var publicDataDbContext = GetDbContext<PublicDataDbContext>();

            var publishedDataSet = await publicDataDbContext
                .DataSets
                .Include(ds => ds.LatestLiveVersion)
                .Include(ds => ds.Versions)
                .SingleAsync(ds => ds.Id == dataSet.Id);

            Assert.Equal(dataSet.Published.TruncateNanoseconds(), publishedDataSet.Published);
            Assert.Equal(DataSetStatus.Published, publishedDataSet.Status);
            Assert.Null(publishedDataSet.LatestDraftVersionId);

            Assert.NotNull(publishedDataSet.LatestLiveVersion);
            Assert.Equal(amendmentReleaseVersionDataSetVersion.Id, publishedDataSet.LatestLiveVersionId);
            Assert.Equal(DataSetVersionStatus.Published, publishedDataSet.LatestLiveVersion.Status);
            publishedDataSet.LatestLiveVersion.Published.AssertUtcNow();
            
            // Assert that the DataSetVersion that was created for the previous ReleaseVersion has been 
            // updated to be linked to the ReleaseFile that is linked to the now-published amendment version
            // of this Release.
            var firstDataSetVersion = publishedDataSet.Versions.Last();
            Assert.Equal(amendmentReleaseDataFile.Id, firstDataSetVersion.ReleaseFileId);
            
            // Assert that the new version for the same DataSet is referencing the ReleaseFile for the brand new File
            // that is the basis for the new version of the DataSet.
            Assert.Equal(newReleaseDataFile.Id, publishedDataSet.LatestLiveVersion.ReleaseFileId);
        }
    }
}
