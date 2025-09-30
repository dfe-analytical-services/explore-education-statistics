using GovUk.Education.ExploreEducationStatistics.Common.Extensions;
using GovUk.Education.ExploreEducationStatistics.Common.Model;
using GovUk.Education.ExploreEducationStatistics.Common.Tests.Extensions;
using GovUk.Education.ExploreEducationStatistics.Content.Model;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Database;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Tests.Fixtures;
using GovUk.Education.ExploreEducationStatistics.Public.Data.Model;
using GovUk.Education.ExploreEducationStatistics.Public.Data.Model.Database;
using GovUk.Education.ExploreEducationStatistics.Public.Data.Model.Tests.Fixtures;
using GovUk.Education.ExploreEducationStatistics.Public.Data.Services.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Publisher.Services.Interfaces;
using Microsoft.EntityFrameworkCore;
using Xunit;
using File = GovUk.Education.ExploreEducationStatistics.Content.Model.File;

namespace GovUk.Education.ExploreEducationStatistics.Publisher.Tests.Services;

public class DataSetPublishingServiceTests(PublisherFunctionsIntegrationTestFixture fixture)
    : PublisherFunctionsIntegrationTest(fixture)
{
    public class PublishDataSetsTests(PublisherFunctionsIntegrationTestFixture fixture)
        : DataSetPublishingServiceTests(fixture)
    {
        [Fact]
        public async Task FirstDataSetVersionPublished()
        {
            ReleaseVersion releaseVersion = DataFixture
                .DefaultReleaseVersion();

            ReleaseFile releaseDataFile = DataFixture
                .DefaultReleaseFile()
                .WithFile(DataFixture.DefaultFile(FileType.Data))
                .WithReleaseVersion(releaseVersion);

            DataSet dataSet = DataFixture
                .DefaultDataSet()
                .WithStatusDraft();

            DataSetVersion dataSetVersion = DataFixture
                .DefaultDataSetVersion()
                .WithDataSetId(dataSet.Id)
                .WithRelease(DataFixture.DefaultDataSetVersionRelease()
                    .WithReleaseFileId(releaseDataFile.Id));

            await AddTestData<ContentDbContext>(context => context.ReleaseFiles.Add(releaseDataFile));
            await AddTestData<PublicDataDbContext>(context =>
            {
                context.DataSets.Add(dataSet);
                context.SaveChanges();

                context.DataSetVersions.Add(dataSetVersion);
                context.SaveChanges();

                dataSet.LatestDraftVersionId = dataSetVersion.Id;
            });

            var dataSetVersionPathResolver = GetRequiredService<IDataSetVersionPathResolver>();
            var draftFolderPath = dataSetVersionPathResolver.DirectoryPath(dataSetVersion);
            Directory.CreateDirectory(draftFolderPath);

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

            // Data set should be published at the same time as the latest live version
            Assert.Equal(publishedDataSet.LatestLiveVersion.Published, publishedDataSet.Published);

            // The Public API folder for the data set version should be updated to reflect its
            // version number at the time of publishing.
            var versionedFolderPath = dataSetVersionPathResolver.DirectoryPath(publishedDataSet.LatestLiveVersion);
            Assert.False(Directory.Exists(draftFolderPath));
            Assert.True(Directory.Exists(versionedFolderPath));
            Assert.EndsWith($"v{publishedDataSet.LatestLiveVersion.SemVersion()}", versionedFolderPath);
        }

        [Fact]
        public async Task SecondDataSetVersionPublished()
        {
            ReleaseVersion releaseVersion = DataFixture
                .DefaultReleaseVersion();

            ReleaseFile releaseDataFile = DataFixture
                .DefaultReleaseFile()
                .WithFile(DataFixture.DefaultFile(FileType.Data))
                .WithReleaseVersion(releaseVersion);

            DataSet dataSet = DataFixture
                .DefaultDataSet()
                .WithStatusPublished()
                .WithPublished(DateTimeOffset.UtcNow.AddDays(-1));

            var dataSetVersions = DataFixture
                .DefaultDataSetVersion()
                .WithDataSetId(dataSet.Id)
                .ForIndex(0, s => s.SetStatusPublished())
                .ForIndex(1,
                    s => s.SetRelease(DataFixture.DefaultDataSetVersionRelease()
                            .WithReleaseFileId(releaseDataFile.Id))
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

            var dataSetVersionPathResolver = GetRequiredService<IDataSetVersionPathResolver>();
            var draftFolderPath = dataSetVersionPathResolver.DirectoryPath(dataSetVersions[1]);
            Directory.CreateDirectory(draftFolderPath);

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

            // The Public API folder for the data set version should be updated to reflect its
            // version number at the time of publishing.
            var versionedFolderPath = dataSetVersionPathResolver.DirectoryPath(publishedDataSet.LatestLiveVersion);
            Assert.False(Directory.Exists(draftFolderPath));
            Assert.True(Directory.Exists(versionedFolderPath));
            Assert.EndsWith($"v{publishedDataSet.LatestLiveVersion.SemVersion()}", versionedFolderPath);
        }

        [Fact]
        public async Task NoDraftDataSetVersionsToPublish()
        {
            ReleaseVersion releaseVersion = DataFixture
                .DefaultReleaseVersion();

            ReleaseFile releaseDataFile = DataFixture
                .DefaultReleaseFile()
                .WithFile(DataFixture.DefaultFile(FileType.Data))
                .WithReleaseVersion(releaseVersion);

            DataSet dataSet = DataFixture
                .DefaultDataSet()
                .WithStatusPublished()
                .WithPublished(DateTimeOffset.UtcNow.AddDays(-1).TruncateNanoseconds());

            DataSetVersion dataSetVersion = DataFixture
                .DefaultDataSetVersion()
                .WithDataSetId(dataSet.Id)
                .WithStatusPublished()
                .WithRelease(DataFixture.DefaultDataSetVersionRelease()
                    .WithReleaseFileId(releaseDataFile.Id));

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
            Assert.Equal(
                dataSet.LatestLiveVersion.Published.TruncateNanoseconds(),
                retrievedDataSet.LatestLiveVersion.Published);
        }

        [Fact]
        public async Task ReleaseAmendmentPublished_UpdatesReleaseFileId()
        {
            ReleaseVersion originalReleaseVersion = DataFixture
                .DefaultReleaseVersion();

            ReleaseVersion amendmentReleaseVersion = DataFixture
                .DefaultReleaseVersion()
                .WithPreviousVersionId(originalReleaseVersion.Id);

            File dataFile = DataFixture
                .DefaultFile(FileType.Data);

            var (originalReleaseDataFile, amendmentReleaseDataFile) = DataFixture
                .DefaultReleaseFile()
                .WithFile(dataFile)
                .ForIndex(0, s => s.SetReleaseVersion(originalReleaseVersion))
                .ForIndex(1, s => s.SetReleaseVersion(amendmentReleaseVersion))
                .GenerateTuple2();

            DataSet dataSet = DataFixture
                .DefaultDataSet()
                .WithStatusPublished();

            DataSetVersion dataSetVersion = DataFixture
                .DefaultDataSetVersion()
                .WithDataSetId(dataSet.Id)
                .WithRelease(DataFixture.DefaultDataSetVersionRelease()
                    .WithReleaseFileId(originalReleaseDataFile.Id))
                .WithStatusPublished();

            await AddTestData<ContentDbContext>(context =>
                context.ReleaseFiles.AddRange(originalReleaseDataFile, amendmentReleaseDataFile));

            await AddTestData<PublicDataDbContext>(context =>
            {
                context.DataSets.Add(dataSet);
                context.SaveChanges();

                context.DataSetVersions.Add(dataSetVersion);
                context.SaveChanges();

                dataSet.LatestLiveVersionId = dataSetVersion.Id;
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

            Assert.NotNull(publishedDataSet.LatestLiveVersion);
            Assert.Equal(DataSetVersionStatus.Published, publishedDataSet.LatestLiveVersion.Status);
            Assert.Equal(
                dataSetVersion.Published.TruncateNanoseconds(),
                publishedDataSet.LatestLiveVersion.Published
            );

            // The version should have be updated to reference the amendment's ReleaseFile
            Assert.Equal(amendmentReleaseDataFile.Id, publishedDataSet.LatestLiveVersion.Release.ReleaseFileId);
        }

        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public async Task ReleaseAmendmentPublishedWithNewDataSetVersion_CorrectReleaseFileIds(bool amendmentResultedInPatchReplacement)
        {
            ReleaseVersion originalReleaseVersion = DataFixture
                .DefaultReleaseVersion();

            ReleaseVersion amendmentReleaseVersion = DataFixture
                .DefaultReleaseVersion()
                .WithPreviousVersionId(originalReleaseVersion.Id);

            var (dataFile, newDataFile) = DataFixture
                .DefaultFile(FileType.Data)
                .GenerateTuple2();

            var (originalReleaseDataFile, amendmentReleaseDataFile) = DataFixture
                .DefaultReleaseFile()
                .WithFile(dataFile)
                .ForIndex(0, s => s.SetReleaseVersion(originalReleaseVersion))
                .ForIndex(1, s => s.SetReleaseVersion(amendmentReleaseVersion))
                .GenerateTuple2();

            if (amendmentResultedInPatchReplacement)
            {
                amendmentReleaseDataFile = DataFixture
                    .DefaultReleaseFile()
                    .WithFile(newDataFile)
                    .WithReleaseVersion(amendmentReleaseVersion);
            }

            ReleaseFile newReleaseDataFile = DataFixture
                .DefaultReleaseFile()
                .WithFile(newDataFile)
                .WithReleaseVersion(amendmentReleaseVersion);

            DataSet dataSet = DataFixture
                .DefaultDataSet()
                .WithStatusPublished();

            DataSetVersion dataSetVersion1 = DataFixture
                .DefaultDataSetVersion()
                .WithDataSetId(dataSet.Id)
                .WithRelease(DataFixture.DefaultDataSetVersionRelease()
                    .WithReleaseFileId(originalReleaseDataFile.Id))
                .WithStatusPublished();

            DataSetVersion dataSetVersion2 = DataFixture
                .DefaultDataSetVersion()
                .WithDataSetId(dataSet.Id)
                .WithRelease(DataFixture.DefaultDataSetVersionRelease()
                    .WithReleaseFileId(newReleaseDataFile.Id))
                .WithVersionNumber(
                    major: amendmentResultedInPatchReplacement ? 1 : 2,
                    minor: 0,
                    patch: amendmentResultedInPatchReplacement ? 1 : 0)
                .WithStatusDraft();

            await AddTestData<ContentDbContext>(context =>
                context.ReleaseFiles.AddRange(originalReleaseDataFile, amendmentReleaseDataFile, newReleaseDataFile));

            await AddTestData<PublicDataDbContext>(context =>
            {
                context.DataSets.Add(dataSet);
                context.SaveChanges();

                context.DataSetVersions.AddRange(dataSetVersion1, dataSetVersion2);
                context.SaveChanges();

                dataSet.LatestLiveVersionId = dataSetVersion1.Id;
                dataSet.LatestDraftVersionId = dataSetVersion2.Id;
            });

            var dataSetVersionPathResolver = GetRequiredService<IDataSetVersionPathResolver>();
            var draftFolderPath = dataSetVersionPathResolver.DirectoryPath(dataSetVersion2);
            Directory.CreateDirectory(draftFolderPath);

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

            Assert.Equal(2, publishedDataSet.Versions.Count);

            var savedDataSetVersion2 = publishedDataSet.Versions[0];

            // The data set should now point to version 2 as the latest live version
            Assert.Equal(savedDataSetVersion2, publishedDataSet.LatestLiveVersion);

            Assert.Equal(savedDataSetVersion2.Id, publishedDataSet.LatestLiveVersionId);
            Assert.Equal(DataSetVersionStatus.Published, savedDataSetVersion2.Status);
            savedDataSetVersion2.Published.AssertUtcNow();

            Assert.Equal(newReleaseDataFile.Id, savedDataSetVersion2.Release.ReleaseFileId);

            var savedDataSetVersion1 = publishedDataSet.Versions[1];

            Assert.Equal(dataSetVersion1.Id, savedDataSetVersion1.Id);
            Assert.Equal(DataSetVersionStatus.Published, savedDataSetVersion1.Status);
            Assert.Equal(dataSetVersion1.Published.TruncateNanoseconds(), savedDataSetVersion1.Published);

            if (amendmentResultedInPatchReplacement)
            {
                // The ReleaseFileId shouldn't get overwritten with a new release file after a patch replacement. 
                Assert.Equal(originalReleaseDataFile.Id, savedDataSetVersion1.Release.ReleaseFileId);
            }
            else
            {
                // The ReleaseFileId should be updated to reference the amendment's ReleaseFile
                Assert.Equal(amendmentReleaseDataFile.Id, savedDataSetVersion1.Release.ReleaseFileId);
            }
        }
    }
}
