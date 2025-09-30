using GovUk.Education.ExploreEducationStatistics.Common.Extensions;
using GovUk.Education.ExploreEducationStatistics.Common.Model;
using GovUk.Education.ExploreEducationStatistics.Common.Tests.Extensions;
using GovUk.Education.ExploreEducationStatistics.Content.Model;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Database;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Tests.Fixtures;
using GovUk.Education.ExploreEducationStatistics.Public.Data.Model;
using GovUk.Education.ExploreEducationStatistics.Public.Data.Model.Database;
using GovUk.Education.ExploreEducationStatistics.Public.Data.Model.Tests.Fixtures;
using GovUk.Education.ExploreEducationStatistics.Public.Data.Processor.Functions;
using GovUk.Education.ExploreEducationStatistics.Public.Data.Processor.Options;
using GovUk.Education.ExploreEducationStatistics.Public.Data.Processor.Requests.Validators;
using GovUk.Education.ExploreEducationStatistics.Public.Data.Processor.Tests.TheoryData;
using GovUk.Education.ExploreEducationStatistics.Public.Data.Services.Interfaces;
using LinqToDB;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.Primitives;
using Moq;
using File = GovUk.Education.ExploreEducationStatistics.Content.Model.File;
using FileInfo = System.IO.FileInfo;
using Release = GovUk.Education.ExploreEducationStatistics.Content.Model.Release;

namespace GovUk.Education.ExploreEducationStatistics.Public.Data.Processor.Tests.Functions;

public abstract class BulkDeleteDataSetVersionsFunctionTests(
    ProcessorFunctionsIntegrationTestFixture fixture
) : ProcessorFunctionsIntegrationTest(fixture)
{
    public class BulkDeleteDataSetVersionsTests : BulkDeleteDataSetVersionsFunctionTests
    {
        private readonly IDataSetVersionPathResolver _dataSetVersionPathResolver;

        public BulkDeleteDataSetVersionsTests(ProcessorFunctionsIntegrationTestFixture fixture)
            : base(fixture)
        {
            _dataSetVersionPathResolver = GetRequiredService<IDataSetVersionPathResolver>();
        }

        [Theory]
        [MemberData(
            nameof(DataSetVersionStatusTheoryData.DeletableStatuses),
            MemberType = typeof(DataSetVersionStatusTheoryData)
        )]
        public async Task ReleaseVersionIsLinkedToOldFileWithApiDataSet(
            DataSetVersionStatus dataSetVersionStatus
        )
        {
            Publication publication = DataFixture.DefaultPublication();

            Release release = DataFixture.DefaultRelease();

            ReleaseVersion previousReleaseVersion = DataFixture
                .DefaultReleaseVersion()
                .WithPublication(publication)
                .WithRelease(release)
                .WithApprovalStatus(ReleaseApprovalStatus.Approved)
                .WithPublished(DateTime.UtcNow)
                .WithVersion(0);

            ReleaseVersion targetReleaseVersion = DataFixture
                .DefaultReleaseVersion()
                .WithPublication(publication)
                .WithRelease(release)
                .WithVersion(1);

            File oldFile = DataFixture.DefaultFile(FileType.Data);

            ReleaseFile previousReleaseFile = DataFixture
                .DefaultReleaseFile()
                .WithReleaseVersion(previousReleaseVersion)
                .WithFile(oldFile);

            ReleaseFile targetReleaseFileWithOldFile = DataFixture
                .DefaultReleaseFile()
                .WithReleaseVersion(targetReleaseVersion)
                .WithFile(oldFile);

            var targetReleaseFilesWithNewFiles = DataFixture
                .DefaultReleaseFile()
                .WithReleaseVersion(targetReleaseVersion)
                .WithFile(() => DataFixture.DefaultFile(FileType.Data))
                .GenerateList(3);

            await AddTestData<ContentDbContext>(context =>
            {
                context.ReleaseFiles.AddRange(
                    [
                        previousReleaseFile,
                        targetReleaseFileWithOldFile,
                        .. targetReleaseFilesWithNewFiles,
                    ]
                );
            });

            DataSet otherDataSet = DataFixture
                .DefaultDataSet()
                .WithStatusPublished()
                .WithPublicationId(publication.Id);

            var targetDataSets = DataFixture
                .DefaultDataSet()
                .WithStatusDraft()
                .WithPublicationId(publication.Id)
                .GenerateList(3);

            await AddTestData<PublicDataDbContext>(context =>
                context.DataSets.AddRange([otherDataSet, .. targetDataSets])
            );

            DataSetVersion otherDataSetVersion = DataFixture
                .DefaultDataSetVersion(filters: 1, indicators: 1, locations: 1, timePeriods: 2)
                .WithVersionNumber(major: 1, minor: 0)
                .WithStatusPublished()
                .WithDataSet(otherDataSet)
                .WithRelease(
                    DataFixture
                        .DefaultDataSetVersionRelease()
                        .WithReleaseFileId(previousReleaseFile.Id)
                )
                .WithImports(() =>
                    DataFixture
                        .DefaultDataSetVersionImport()
                        .WithStage(DataSetVersionImportStage.Completing)
                        .Generate(1)
                )
                .FinishWith(dsv => dsv.DataSet.LatestLiveVersion = dsv);

            var targetDataSetVersions = DataFixture
                .DefaultDataSetVersion(filters: 1, indicators: 1, locations: 1, timePeriods: 2)
                .WithVersionNumber(major: 1, minor: 0)
                .WithStatus(dataSetVersionStatus)
                .WithImports(() => DataFixture.DefaultDataSetVersionImport().Generate(1))
                .ForIndex(
                    0,
                    dsv =>
                        dsv.SetDataSet(targetDataSets[0])
                            .SetRelease(
                                DataFixture
                                    .DefaultDataSetVersionRelease()
                                    .WithReleaseFileId(targetReleaseFilesWithNewFiles[0].Id)
                            )
                )
                .ForIndex(
                    1,
                    dsv =>
                        dsv.SetDataSet(targetDataSets[1])
                            .SetRelease(
                                DataFixture
                                    .DefaultDataSetVersionRelease()
                                    .WithReleaseFileId(targetReleaseFilesWithNewFiles[1].Id)
                            )
                )
                .ForIndex(
                    2,
                    dsv =>
                        dsv.SetDataSet(targetDataSets[2])
                            .SetRelease(
                                DataFixture
                                    .DefaultDataSetVersionRelease()
                                    .WithReleaseFileId(targetReleaseFilesWithNewFiles[2].Id)
                            )
                )
                .FinishWith(dsv => dsv.DataSet.LatestDraftVersion = dsv)
                .GenerateList();

            await AddTestData<PublicDataDbContext>(context =>
            {
                context.DataSetVersions.AddRange([otherDataSetVersion, .. targetDataSetVersions]);
                context.DataSets.UpdateRange([otherDataSet, .. targetDataSets]);
            });

            previousReleaseFile.PublicApiDataSetId = otherDataSet.Id;
            previousReleaseFile.PublicApiDataSetVersion = otherDataSetVersion.SemVersion();
            targetReleaseFileWithOldFile.PublicApiDataSetId =
                previousReleaseFile.PublicApiDataSetId;
            targetReleaseFileWithOldFile.PublicApiDataSetVersion =
                previousReleaseFile.PublicApiDataSetVersion;

            foreach (
                var (
                    targetReleaseFileWithNewFile,
                    index
                ) in targetReleaseFilesWithNewFiles.WithIndex()
            )
            {
                targetReleaseFileWithNewFile.PublicApiDataSetId = targetDataSets[index].Id;
                targetReleaseFileWithNewFile.PublicApiDataSetVersion = targetDataSetVersions[index]
                    .SemVersion();
            }

            await AddTestData<ContentDbContext>(context =>
                context.ReleaseFiles.UpdateRange(
                    [
                        previousReleaseFile,
                        targetReleaseFileWithOldFile,
                        .. targetReleaseFilesWithNewFiles,
                    ]
                )
            );

            foreach (var targetDataSetVersion in targetDataSetVersions)
            {
                var targetDataSetVersionDirectory = _dataSetVersionPathResolver.DirectoryPath(
                    targetDataSetVersion
                );
                Directory.CreateDirectory(targetDataSetVersionDirectory);
                await System
                    .IO.File.Create(Path.Combine(targetDataSetVersionDirectory, "version1.txt"))
                    .DisposeAsync();
            }

            var otherDataSetVersionDirectory = _dataSetVersionPathResolver.DirectoryPath(
                otherDataSetVersion
            );
            Directory.CreateDirectory(otherDataSetVersionDirectory);
            await System
                .IO.File.Create(Path.Combine(otherDataSetVersionDirectory, "version1.txt"))
                .DisposeAsync();

            var response = await BulkDeleteDataSetVersions(targetReleaseVersion.Id);

            response.AssertNoContent();

            await using var publicDataDbContext = GetDbContext<PublicDataDbContext>();
            await using var contentDataDbContext = GetDbContext<ContentDbContext>();

            // Assertions for the TARGET data sets linked to the TARGET release version being deleted
            for (var i = 0; i < 3; i++)
            {
                var targetDataSet = targetDataSets[i];
                var targetDataSetVersion = targetDataSetVersions[i];
                var targetReleaseFileWithNewFile = targetReleaseFilesWithNewFiles[i];

                // Assert that the TARGET parquet data set folder, linked to the release version being deleted, is now removed
                var targetDataSetDirectory = Directory.GetParent(
                    _dataSetVersionPathResolver.DirectoryPath(targetDataSetVersion)
                );
                Assert.False(Directory.Exists(targetDataSetDirectory!.FullName));

                // Assert that the TARGET Data Set has been deleted
                Assert.Null(
                    await publicDataDbContext.DataSets.SingleOrDefaultAsync(ds =>
                        ds.Id == targetDataSet.Id
                    )
                );

                // Assert that the TARGET Data Set Version has been deleted
                Assert.Null(
                    await publicDataDbContext.DataSetVersions.SingleOrDefaultAsync(dsv =>
                        dsv.Id == targetDataSetVersion.Id
                    )
                );

                // Assert that the TARGET Data Set Version metadata is deleted
                await AssertMetadataIsDeleted(publicDataDbContext, targetDataSetVersion);

                // Assert that the TARGET Data Set Version Import has been deleted
                Assert.Null(
                    await publicDataDbContext.DataSetVersionImports.SingleOrDefaultAsync(dsvi =>
                        dsvi.Id == targetDataSetVersion.Imports.Single().Id
                    )
                );

                // Assert that the TARGET Release File has been unassociated with its Public API DataSet
                var targetReleaseFile = await contentDataDbContext.ReleaseFiles.SingleAsync(f =>
                    f.Id == targetReleaseFileWithNewFile.Id
                );
                Assert.Null(targetReleaseFile.PublicApiDataSetId);
                Assert.Null(targetReleaseFile.PublicApiDataSetVersion);
            }

            // Assert that the TARGET Release File, which points to the OLD File has been unassociated with its Public API DataSet
            var targetReleaseFileWithOldFilePostDelete =
                await contentDataDbContext.ReleaseFiles.SingleAsync(f =>
                    f.Id == targetReleaseFileWithOldFile.Id
                );
            Assert.Null(targetReleaseFileWithOldFilePostDelete.PublicApiDataSetId);
            Assert.Null(targetReleaseFileWithOldFilePostDelete.PublicApiDataSetVersion);

            // Below are the Assertions for the OTHER data set linked to the NON-TARGET release version. NOTHING should be deleted

            // Assert that the OTHER data set folder, and its contents, linked to the NON-TARGET release version remains
            // as it was
            var otherDataSetFolder = Directory.GetParent(
                _dataSetVersionPathResolver.DirectoryPath(otherDataSetVersion)
            );
            Assert.True(Directory.Exists(otherDataSetFolder!.FullName));

            var otherDataSetFolderEntries = Directory.GetFileSystemEntries(
                otherDataSetFolder.FullName
            );
            var otherDataSetVersionFolder = Assert.Single(
                otherDataSetFolderEntries,
                entry => new DirectoryInfo(entry).Name == $"v{otherDataSetVersion.SemVersion()}"
            );

            var otherDataSetVersionFolderEntries = Directory.GetFileSystemEntries(
                otherDataSetVersionFolder
            );
            Assert.Single(
                otherDataSetVersionFolderEntries,
                entry => new FileInfo(entry).Name == "version1.txt"
            );

            // Assert that the OTHER Data Set has NOT been deleted
            Assert.NotNull(
                await publicDataDbContext.DataSets.SingleAsync(ds => ds.Id == otherDataSet.Id)
            );

            // Assert that the OTHER Data Set Version has NOT been deleted
            Assert.NotNull(
                await publicDataDbContext.DataSetVersions.SingleAsync(dsv =>
                    dsv.Id == otherDataSetVersion.Id
                )
            );

            // Assert that the OTHER Data Set Version metadata is NOT deleted
            await AssertMetadataIsNotDeleted(publicDataDbContext, otherDataSetVersion);

            // Assert that the OTHER Data Set Version Import has NOT been deleted
            Assert.NotNull(
                await publicDataDbContext.DataSetVersionImports.SingleAsync(dsvi =>
                    dsvi.Id == otherDataSetVersion.Imports.Single().Id
                )
            );

            // Assert that the PREVIOUS Release File is still associated with its Public API DataSet
            var previousReleaseFilePostDelete = await contentDataDbContext.ReleaseFiles.SingleAsync(
                f => f.Id == previousReleaseFile.Id
            );
            Assert.Equal(otherDataSet.Id, previousReleaseFilePostDelete.PublicApiDataSetId);
            Assert.Equal(
                otherDataSetVersion.SemVersion(),
                previousReleaseFilePostDelete.PublicApiDataSetVersion
            );
        }

        [Theory]
        [MemberData(
            nameof(DataSetVersionStatusTheoryData.DeletableStatuses),
            MemberType = typeof(DataSetVersionStatusTheoryData)
        )]
        public async Task ReleaseVersionIsLinkedToSubsequentDataSetVersion(
            DataSetVersionStatus dataSetVersionStatus
        )
        {
            Publication publication = DataFixture.DefaultPublication();

            Release release1 = DataFixture.DefaultRelease();

            Release release2 = DataFixture.DefaultRelease();

            ReleaseVersion release1Version1 = DataFixture
                .DefaultReleaseVersion()
                .WithPublication(publication)
                .WithRelease(release1)
                .WithApprovalStatus(ReleaseApprovalStatus.Approved)
                .WithPublished(DateTime.UtcNow)
                .WithVersion(0);

            ReleaseVersion release2Version1 = DataFixture
                .DefaultReleaseVersion()
                .WithPublication(publication)
                .WithRelease(release2)
                .WithApprovalStatus(ReleaseApprovalStatus.Approved)
                .WithPublished(DateTime.UtcNow)
                .WithVersion(0);

            ReleaseVersion release2Version2 = DataFixture
                .DefaultReleaseVersion()
                .WithPublication(publication)
                .WithRelease(release2)
                .WithVersion(1);

            ReleaseFile release1Version1ReleaseFile = DataFixture
                .DefaultReleaseFile()
                .WithReleaseVersion(release1Version1)
                .WithFile(DataFixture.DefaultFile(FileType.Data));

            ReleaseFile release2Version1ReleaseFile = DataFixture
                .DefaultReleaseFile()
                .WithReleaseVersion(release2Version1)
                .WithFile(DataFixture.DefaultFile(FileType.Data));

            ReleaseFile release2Version2ReleaseFile = DataFixture
                .DefaultReleaseFile()
                .WithReleaseVersion(release2Version2)
                .WithFile(DataFixture.DefaultFile(FileType.Data));

            await AddTestData<ContentDbContext>(context =>
            {
                context.ReleaseFiles.AddRange(
                    release1Version1ReleaseFile,
                    release2Version1ReleaseFile,
                    release2Version2ReleaseFile
                );
            });

            DataSet release1Version1DataSet = DataFixture
                .DefaultDataSet()
                .WithStatusPublished()
                .WithPublicationId(publication.Id);

            DataSet release2Version1DataSet = DataFixture
                .DefaultDataSet()
                .WithStatusPublished()
                .WithPublicationId(publication.Id);

            await AddTestData<PublicDataDbContext>(context =>
                context.DataSets.AddRange(release1Version1DataSet, release2Version1DataSet)
            );

            DataSetVersion release1Version1DataSetVersion = DataFixture
                .DefaultDataSetVersion(filters: 1, indicators: 1, locations: 1, timePeriods: 2)
                .WithVersionNumber(major: 1, minor: 0)
                .WithStatusPublished()
                .WithDataSet(release1Version1DataSet)
                .WithRelease(
                    DataFixture
                        .DefaultDataSetVersionRelease()
                        .WithReleaseFileId(release1Version1ReleaseFile.Id)
                )
                .WithImports(() =>
                    DataFixture
                        .DefaultDataSetVersionImport()
                        .WithStage(DataSetVersionImportStage.Completing)
                        .Generate(1)
                )
                .FinishWith(dsv => dsv.DataSet.LatestLiveVersion = dsv);

            DataSetVersion release2Version1DataSetVersion = DataFixture
                .DefaultDataSetVersion(filters: 1, indicators: 1, locations: 1, timePeriods: 2)
                .WithVersionNumber(major: 1, minor: 0)
                .WithStatusPublished()
                .WithDataSet(release2Version1DataSet)
                .WithRelease(
                    DataFixture
                        .DefaultDataSetVersionRelease()
                        .WithReleaseFileId(release2Version1ReleaseFile.Id)
                )
                .WithImports(() =>
                    DataFixture
                        .DefaultDataSetVersionImport()
                        .WithStage(DataSetVersionImportStage.Completing)
                        .Generate(1)
                )
                .FinishWith(dsv => dsv.DataSet.LatestLiveVersion = dsv);

            DataSetVersion release2Version2DataSetVersion = DataFixture
                .DefaultDataSetVersion(filters: 1, indicators: 1, locations: 1, timePeriods: 2)
                .WithVersionNumber(major: 2, minor: 0)
                .WithStatus(dataSetVersionStatus)
                .WithDataSet(release1Version1DataSet) // This is the 2nd Data Set Version of the series
                .WithRelease(
                    DataFixture
                        .DefaultDataSetVersionRelease()
                        .WithReleaseFileId(release2Version2ReleaseFile.Id)
                )
                .WithImports(() => DataFixture.DefaultDataSetVersionImport().Generate(1))
                .FinishWith(dsv => dsv.DataSet.LatestDraftVersion = dsv);

            await AddTestData<PublicDataDbContext>(context =>
            {
                context.DataSetVersions.AddRange(
                    release1Version1DataSetVersion,
                    release2Version1DataSetVersion,
                    release2Version2DataSetVersion
                );

                context.DataSets.UpdateRange(release1Version1DataSet, release2Version1DataSet);
            });

            release1Version1ReleaseFile.PublicApiDataSetId = release1Version1DataSet.Id;
            release1Version1ReleaseFile.PublicApiDataSetVersion =
                release1Version1DataSetVersion.SemVersion();
            release2Version1ReleaseFile.PublicApiDataSetId = release2Version1DataSet.Id;
            release2Version1ReleaseFile.PublicApiDataSetVersion =
                release2Version1DataSetVersion.SemVersion();
            release2Version2ReleaseFile.PublicApiDataSetId = release1Version1DataSet.Id; // Same Data Set series as release1version1
            release2Version2ReleaseFile.PublicApiDataSetVersion =
                release2Version2DataSetVersion.SemVersion();

            await AddTestData<ContentDbContext>(context =>
                context.ReleaseFiles.UpdateRange(
                    release1Version1ReleaseFile,
                    release2Version1ReleaseFile,
                    release2Version2ReleaseFile
                )
            );

            var allDataSetVersions = new List<DataSetVersion>
            {
                release1Version1DataSetVersion,
                release2Version1DataSetVersion,
                release2Version2DataSetVersion,
            };

            foreach (var dataSetVersion in allDataSetVersions)
            {
                var dataSetVersionDirectory = _dataSetVersionPathResolver.DirectoryPath(
                    dataSetVersion
                );
                Directory.CreateDirectory(dataSetVersionDirectory);
                await System
                    .IO.File.Create(
                        Path.Combine(dataSetVersionDirectory, $"{dataSetVersion.SemVersion()}.txt")
                    )
                    .DisposeAsync();
            }

            var response = await BulkDeleteDataSetVersions(release2Version2.Id);

            response.AssertNoContent();

            await using var publicDataDbContext = GetDbContext<PublicDataDbContext>();
            await using var contentDataDbContext = GetDbContext<ContentDbContext>();

            // Assert that the parquet folder for the new, DRAFT, API Data Set Version linked to the release version being deleted, is now deleted
            var release2Version2DataSetVersionDirectory = _dataSetVersionPathResolver.DirectoryPath(
                release2Version2DataSetVersion
            );
            Assert.False(Directory.Exists(release2Version2DataSetVersionDirectory));

            // Assert that the parent parquet folder for the API Data Set, linked to the release version being deleted, has NOT been deleted
            var release1Version1DataSetDirectory = Directory.GetParent(
                _dataSetVersionPathResolver.DirectoryPath(release2Version2DataSetVersion)
            );
            Assert.True(Directory.Exists(release1Version1DataSetDirectory!.FullName));

            // Assert that the parquet folder, and its contents, for the API Data Set Version linked to Release 1 Version 1, has NOT been deleted
            var release1Version1DataSetVersionDirectory = _dataSetVersionPathResolver.DirectoryPath(
                release1Version1DataSetVersion
            );
            Assert.True(Directory.Exists(release1Version1DataSetVersionDirectory));

            var release1Version1DataSetVersionDirectoryEntries = Directory.GetFileSystemEntries(
                release1Version1DataSetVersionDirectory
            );
            Assert.Single(
                release1Version1DataSetVersionDirectoryEntries,
                entry =>
                    new FileInfo(entry).Name == $"{release1Version1DataSetVersion.SemVersion()}.txt"
            );

            // Assert that the parquet folder, and its contents, for the API Data Set Version linked to Release 2 Version 1, has NOT been deleted
            var release2Version1DataSetVersionDirectory = _dataSetVersionPathResolver.DirectoryPath(
                release2Version1DataSetVersion
            );
            Assert.True(Directory.Exists(release2Version1DataSetVersionDirectory));

            var release2Version1DataSetVersionDirectoryEntries = Directory.GetFileSystemEntries(
                release2Version1DataSetVersionDirectory
            );
            Assert.Single(
                release2Version1DataSetVersionDirectoryEntries,
                entry =>
                    new FileInfo(entry).Name == $"{release2Version1DataSetVersion.SemVersion()}.txt"
            );

            // Assert that the TARGET API Data Set, linked to the release version being deleted, has NOT been deleted
            Assert.NotNull(
                await publicDataDbContext.DataSets.SingleOrDefaultAsync(ds =>
                    ds.Id == release1Version1DataSet.Id
                )
            );

            // Assert that the OTHER Data Set has NOT been deleted
            Assert.NotNull(
                await publicDataDbContext.DataSets.SingleAsync(ds =>
                    ds.Id == release2Version1DataSet.Id
                )
            );

            // Assert that the TARGET Data Set Version has been deleted
            Assert.Null(
                await publicDataDbContext.DataSetVersions.SingleOrDefaultAsync(dsv =>
                    dsv.Id == release2Version2DataSetVersion.Id
                )
            );

            // Assert that the OTHER Data Set Versions have NOT been deleted
            Assert.NotNull(
                await publicDataDbContext.DataSetVersions.SingleAsync(dsv =>
                    dsv.Id == release1Version1DataSetVersion.Id
                )
            );
            Assert.NotNull(
                await publicDataDbContext.DataSetVersions.SingleAsync(dsv =>
                    dsv.Id == release2Version1DataSetVersion.Id
                )
            );

            // Assert that the TARGET Data Set Version metadata is deleted
            await AssertMetadataIsDeleted(publicDataDbContext, release2Version2DataSetVersion);

            // Assert that the OTHER Data Set Versions metadata has NOT been deleted
            await AssertMetadataIsNotDeleted(publicDataDbContext, release1Version1DataSetVersion);
            await AssertMetadataIsNotDeleted(publicDataDbContext, release2Version1DataSetVersion);

            // Assert that the TARGET Data Set Version Import has been deleted
            Assert.Null(
                await publicDataDbContext.DataSetVersionImports.SingleOrDefaultAsync(dsvi =>
                    dsvi.Id == release2Version2DataSetVersion.Imports.Single().Id
                )
            );

            // Assert that the OTHER Data Set Version Imports have NOT been deleted
            Assert.NotNull(
                await publicDataDbContext.DataSetVersionImports.SingleAsync(dsvi =>
                    dsvi.Id == release1Version1DataSetVersion.Imports.Single().Id
                )
            );
            Assert.NotNull(
                await publicDataDbContext.DataSetVersionImports.SingleAsync(dsvi =>
                    dsvi.Id == release2Version1DataSetVersion.Imports.Single().Id
                )
            );

            // Assert that the TARGET Release File has been unassociated with its Public API DataSet
            var targetReleaseFile = await contentDataDbContext.ReleaseFiles.SingleAsync(f =>
                f.Id == release2Version2ReleaseFile.Id
            );

            Assert.Null(targetReleaseFile.PublicApiDataSetId);
            Assert.Null(targetReleaseFile.PublicApiDataSetVersion);

            // Assert that the OTHER Release Files are still associated with their Public API DataSets
            var release1Version1ReleaseFilePostDelete =
                await contentDataDbContext.ReleaseFiles.SingleAsync(f =>
                    f.Id == release1Version1ReleaseFile.Id
                );
            var release2Version1ReleaseFilePostDelete =
                await contentDataDbContext.ReleaseFiles.SingleAsync(f =>
                    f.Id == release2Version1ReleaseFile.Id
                );

            Assert.Equal(
                release1Version1DataSet.Id,
                release1Version1ReleaseFilePostDelete.PublicApiDataSetId
            );
            Assert.Equal(
                release1Version1DataSetVersion.SemVersion(),
                release1Version1ReleaseFilePostDelete.PublicApiDataSetVersion
            );
            Assert.Equal(
                release2Version1DataSet.Id,
                release2Version1ReleaseFilePostDelete.PublicApiDataSetId
            );
            Assert.Equal(
                release2Version1DataSetVersion.SemVersion(),
                release2Version1ReleaseFilePostDelete.PublicApiDataSetVersion
            );
        }

        private static async Task AssertMetadataIsDeleted(
            PublicDataDbContext publicDataDbContext,
            DataSetVersion dataSetVersion
        )
        {
            Assert.False(
                await publicDataDbContext.FilterMetas.AnyAsync(fm =>
                    fm.DataSetVersionId == dataSetVersion.Id
                )
            );
            Assert.False(
                await publicDataDbContext.FilterOptionMetaLinks.AnyAsync(foml =>
                    dataSetVersion.FilterMetas.Contains(foml.Meta)
                )
            );
            Assert.False(
                await publicDataDbContext.LocationMetas.AnyAsync(fm =>
                    fm.DataSetVersionId == dataSetVersion.Id
                )
            );
            Assert.False(
                await publicDataDbContext.LocationOptionMetaLinks.AnyAsync(loml =>
                    dataSetVersion.LocationMetas.Contains(loml.Meta)
                )
            );
            Assert.False(
                await publicDataDbContext.IndicatorMetas.AnyAsync(fm =>
                    fm.DataSetVersionId == dataSetVersion.Id
                )
            );
            Assert.False(
                await publicDataDbContext.GeographicLevelMetas.AnyAsync(fm =>
                    fm.DataSetVersionId == dataSetVersion.Id
                )
            );
            Assert.False(
                await publicDataDbContext.TimePeriodMetas.AnyAsync(fm =>
                    fm.DataSetVersionId == dataSetVersion.Id
                )
            );
        }

        private static async Task AssertMetadataIsNotDeleted(
            PublicDataDbContext publicDataDbContext,
            DataSetVersion dataSetVersion
        )
        {
            Assert.True(
                await publicDataDbContext.FilterMetas.AnyAsync(fm =>
                    fm.DataSetVersionId == dataSetVersion.Id
                )
            );
            Assert.True(
                await publicDataDbContext.FilterOptionMetaLinks.AnyAsync(foml =>
                    dataSetVersion.FilterMetas.Contains(foml.Meta)
                )
            );
            Assert.True(
                await publicDataDbContext.LocationMetas.AnyAsync(fm =>
                    fm.DataSetVersionId == dataSetVersion.Id
                )
            );
            Assert.True(
                await publicDataDbContext.LocationOptionMetaLinks.AnyAsync(loml =>
                    dataSetVersion.LocationMetas.Contains(loml.Meta)
                )
            );
            Assert.True(
                await publicDataDbContext.IndicatorMetas.AnyAsync(fm =>
                    fm.DataSetVersionId == dataSetVersion.Id
                )
            );
            Assert.True(
                await publicDataDbContext.GeographicLevelMetas.AnyAsync(fm =>
                    fm.DataSetVersionId == dataSetVersion.Id
                )
            );
            Assert.True(
                await publicDataDbContext.TimePeriodMetas.AnyAsync(fm =>
                    fm.DataSetVersionId == dataSetVersion.Id
                )
            );
        }

        [Theory]
        [MemberData(
            nameof(DataSetVersionStatusTheoryData.NonDeletableStatuses),
            MemberType = typeof(DataSetVersionStatusTheoryData)
        )]
        public async Task DataSetVersionCanNotBeDeleted_Returns400(
            DataSetVersionStatus dataSetVersionStatus
        )
        {
            ReleaseVersion releaseVersion = DataFixture
                .DefaultReleaseVersion()
                .WithPublication(DataFixture.DefaultPublication());

            var (releaseFile1, releaseFile2) = DataFixture
                .DefaultReleaseFile()
                .WithFile(DataFixture.DefaultFile(FileType.Data))
                .WithReleaseVersion(releaseVersion)
                .GenerateTuple2();

            await AddTestData<ContentDbContext>(context =>
            {
                context.ReleaseFiles.AddRange(releaseFile1, releaseFile2);
            });

            var (dataSet1, dataSet2) = DataFixture
                .DefaultDataSet()
                .WithPublicationId(releaseVersion.PublicationId)
                .WithStatusPublished()
                .GenerateTuple2();

            await AddTestData<PublicDataDbContext>(context =>
                context.DataSets.AddRange(dataSet1, dataSet2)
            );

            // Data set version that is not in a deletable state
            DataSetVersion dataSet1Version = DataFixture
                .DefaultDataSetVersion(filters: 1, indicators: 1, locations: 1, timePeriods: 2)
                .WithDataSet(dataSet1)
                .WithRelease(
                    DataFixture.DefaultDataSetVersionRelease().WithReleaseFileId(releaseFile1.Id)
                )
                .WithVersionNumber(major: 1, minor: 0)
                .WithStatus(dataSetVersionStatus)
                .FinishWith(dsv => dsv.DataSet.LatestLiveVersion = dsv);

            // Data set version that is in a deletable state
            DataSetVersion dataSet2Version = DataFixture
                .DefaultDataSetVersion(filters: 1, indicators: 1, locations: 1, timePeriods: 2)
                .WithDataSet(dataSet2)
                .WithRelease(
                    DataFixture.DefaultDataSetVersionRelease().WithReleaseFileId(releaseFile2.Id)
                )
                .WithVersionNumber(major: 1, minor: 0)
                .WithStatusDraft()
                .FinishWith(dsv => dsv.DataSet.LatestDraftVersion = dsv);

            await AddTestData<PublicDataDbContext>(context =>
            {
                context.DataSetVersions.AddRange(dataSet1Version, dataSet2Version);
                context.DataSets.Update(dataSet1);
                context.DataSets.Update(dataSet2);
            });

            var response = await BulkDeleteDataSetVersions(releaseVersion.Id);

            var validationProblem = response.AssertBadRequestWithValidationProblem();

            validationProblem.AssertHasError(
                expectedPath: "releaseVersionId",
                expectedCode: ValidationMessages.OneOrMoreDataSetVersionsCanNotBeDeleted.Code
            );
        }

        public class ForceDeleteTests(ProcessorFunctionsIntegrationTestFixture fixture)
            : BulkDeleteDataSetVersionsFunctionTests(fixture)
        {
            [Theory]
            [MemberData(
                nameof(DataSetVersionStatusTheoryData.NonDeletableStatuses),
                MemberType = typeof(DataSetVersionStatusTheoryData)
            )]
            public async Task TestTheme_ThemeDeletionAllowed_Success(
                DataSetVersionStatus dataSetVersionStatus
            )
            {
                ReleaseVersion releaseVersion = DataFixture
                    .DefaultReleaseVersion()
                    .WithPublication(
                        DataFixture
                            .DefaultPublication()
                            .WithTheme(DataFixture.DefaultTheme().WithTitle("UI test theme"))
                    );

                ReleaseFile releaseFile = DataFixture
                    .DefaultReleaseFile()
                    .WithFile(DataFixture.DefaultFile(FileType.Data))
                    .WithReleaseVersion(releaseVersion);

                await AddTestData<ContentDbContext>(context =>
                {
                    context.ReleaseFiles.AddRange(releaseFile);
                });

                var dataSet = DataFixture
                    .DefaultDataSet()
                    .WithPublicationId(releaseVersion.PublicationId)
                    .WithStatusPublished()
                    .Generate();

                await AddTestData<PublicDataDbContext>(context =>
                    context.DataSets.AddRange(dataSet)
                );

                // Data set version that is not in a deletable state without force delete
                DataSetVersion dataSetVersion = DataFixture
                    .DefaultDataSetVersion(filters: 1, indicators: 1, locations: 1, timePeriods: 2)
                    .WithDataSet(dataSet)
                    .WithRelease(
                        DataFixture.DefaultDataSetVersionRelease().WithReleaseFileId(releaseFile.Id)
                    )
                    .WithVersionNumber(major: 1, minor: 0)
                    .WithStatus(dataSetVersionStatus)
                    .FinishWith(dsv => dsv.DataSet.LatestLiveVersion = dsv);

                await AddTestData<PublicDataDbContext>(context =>
                {
                    context.DataSetVersions.AddRange(dataSetVersion);
                    context.DataSets.Update(dataSet);
                });

                // Request force deletion with support from the environment configuration
                var response = await BulkDeleteDataSetVersions(
                    releaseVersionId: releaseVersion.Id,
                    forceDeleteAll: true,
                    themeDeletionAllowed: true
                );

                response.AssertNoContent();
            }

            [Theory]
            [MemberData(
                nameof(DataSetVersionStatusTheoryData.NonDeletableStatuses),
                MemberType = typeof(DataSetVersionStatusTheoryData)
            )]
            public async Task ForceDeleteNotSpecified_Returns400(
                DataSetVersionStatus dataSetVersionStatus
            )
            {
                ReleaseVersion releaseVersion = DataFixture
                    .DefaultReleaseVersion()
                    .WithPublication(
                        DataFixture
                            .DefaultPublication()
                            .WithTheme(DataFixture.DefaultTheme().WithTitle("UI test theme"))
                    );

                ReleaseFile releaseFile = DataFixture
                    .DefaultReleaseFile()
                    .WithFile(DataFixture.DefaultFile(FileType.Data))
                    .WithReleaseVersion(releaseVersion);

                await AddTestData<ContentDbContext>(context =>
                {
                    context.ReleaseFiles.AddRange(releaseFile);
                });

                var dataSet = DataFixture
                    .DefaultDataSet()
                    .WithPublicationId(releaseVersion.PublicationId)
                    .WithStatusPublished()
                    .Generate();

                await AddTestData<PublicDataDbContext>(context =>
                    context.DataSets.AddRange(dataSet)
                );

                // Data set version that is not in a deletable state without force delete
                DataSetVersion dataSetVersion = DataFixture
                    .DefaultDataSetVersion(filters: 1, indicators: 1, locations: 1, timePeriods: 2)
                    .WithDataSet(dataSet)
                    .WithRelease(
                        DataFixture.DefaultDataSetVersionRelease().WithReleaseFileId(releaseFile.Id)
                    )
                    .WithVersionNumber(major: 1, minor: 0)
                    .WithStatus(dataSetVersionStatus)
                    .FinishWith(dsv => dsv.DataSet.LatestLiveVersion = dsv);

                await AddTestData<PublicDataDbContext>(context =>
                {
                    context.DataSetVersions.AddRange(dataSetVersion);
                    context.DataSets.Update(dataSet);
                });

                // Don't explicitly request force deletion.
                var response = await BulkDeleteDataSetVersions(
                    releaseVersionId: releaseVersion.Id,
                    themeDeletionAllowed: true,
                    forceDeleteAll: null
                );

                var validationProblem = response.AssertBadRequestWithValidationProblem();

                validationProblem.AssertHasError(
                    expectedPath: "releaseVersionId",
                    expectedCode: ValidationMessages.OneOrMoreDataSetVersionsCanNotBeDeleted.Code
                );
            }

            [Theory]
            [MemberData(
                nameof(DataSetVersionStatusTheoryData.NonDeletableStatuses),
                MemberType = typeof(DataSetVersionStatusTheoryData)
            )]
            public async Task TestTheme_ThemeDeletionNotAllowed_Returns400(
                DataSetVersionStatus dataSetVersionStatus
            )
            {
                ReleaseVersion releaseVersion = DataFixture
                    .DefaultReleaseVersion()
                    .WithPublication(
                        DataFixture
                            .DefaultPublication()
                            .WithTheme(DataFixture.DefaultTheme().WithTitle("UI test theme"))
                    );

                ReleaseFile releaseFile = DataFixture
                    .DefaultReleaseFile()
                    .WithFile(DataFixture.DefaultFile(FileType.Data))
                    .WithReleaseVersion(releaseVersion);

                await AddTestData<ContentDbContext>(context =>
                {
                    context.ReleaseFiles.AddRange(releaseFile);
                });

                var dataSet = DataFixture
                    .DefaultDataSet()
                    .WithPublicationId(releaseVersion.PublicationId)
                    .WithStatusPublished()
                    .Generate();

                await AddTestData<PublicDataDbContext>(context =>
                    context.DataSets.AddRange(dataSet)
                );

                // Data set version that is not in a deletable state without force delete
                DataSetVersion dataSetVersion = DataFixture
                    .DefaultDataSetVersion(filters: 1, indicators: 1, locations: 1, timePeriods: 2)
                    .WithDataSet(dataSet)
                    .WithRelease(
                        DataFixture.DefaultDataSetVersionRelease().WithReleaseFileId(releaseFile.Id)
                    )
                    .WithVersionNumber(major: 1, minor: 0)
                    .WithStatus(dataSetVersionStatus)
                    .FinishWith(dsv => dsv.DataSet.LatestLiveVersion = dsv);

                await AddTestData<PublicDataDbContext>(context =>
                {
                    context.DataSetVersions.AddRange(dataSetVersion);
                    context.DataSets.Update(dataSet);
                });

                // Request force deletion but without delete support from the
                // environment configuration
                var response = await BulkDeleteDataSetVersions(
                    releaseVersionId: releaseVersion.Id,
                    forceDeleteAll: true
                );

                var validationProblem = response.AssertBadRequestWithValidationProblem();

                validationProblem.AssertHasError(
                    expectedPath: "releaseVersionId",
                    expectedCode: ValidationMessages.OneOrMoreDataSetVersionsCanNotBeDeleted.Code
                );
            }

            [Theory]
            [MemberData(
                nameof(DataSetVersionStatusTheoryData.NonDeletableStatuses),
                MemberType = typeof(DataSetVersionStatusTheoryData)
            )]
            public async Task NonTestTheme_ThemeDeletionAllowed_Returns400(
                DataSetVersionStatus dataSetVersionStatus
            )
            {
                ReleaseVersion releaseVersion = DataFixture
                    .DefaultReleaseVersion()
                    .WithPublication(
                        DataFixture
                            .DefaultPublication()
                            .WithTheme(DataFixture.DefaultTheme().WithTitle("Non-test theme"))
                    );

                ReleaseFile releaseFile = DataFixture
                    .DefaultReleaseFile()
                    .WithFile(DataFixture.DefaultFile(FileType.Data))
                    .WithReleaseVersion(releaseVersion);

                await AddTestData<ContentDbContext>(context =>
                {
                    context.ReleaseFiles.AddRange(releaseFile);
                });

                var dataSet = DataFixture
                    .DefaultDataSet()
                    .WithPublicationId(releaseVersion.PublicationId)
                    .WithStatusPublished()
                    .Generate();

                await AddTestData<PublicDataDbContext>(context =>
                    context.DataSets.AddRange(dataSet)
                );

                // Data set version that is not in a deletable state without force delete
                DataSetVersion dataSetVersion = DataFixture
                    .DefaultDataSetVersion(filters: 1, indicators: 1, locations: 1, timePeriods: 2)
                    .WithDataSet(dataSet)
                    .WithRelease(
                        DataFixture.DefaultDataSetVersionRelease().WithReleaseFileId(releaseFile.Id)
                    )
                    .WithVersionNumber(major: 1, minor: 0)
                    .WithStatus(dataSetVersionStatus)
                    .FinishWith(dsv => dsv.DataSet.LatestLiveVersion = dsv);

                await AddTestData<PublicDataDbContext>(context =>
                {
                    context.DataSetVersions.AddRange(dataSetVersion);
                    context.DataSets.Update(dataSet);
                });

                // Request force deletion with support from the environment configuration
                // but targeting a non-Test DataSetVersion.
                var response = await BulkDeleteDataSetVersions(
                    releaseVersionId: releaseVersion.Id,
                    forceDeleteAll: true,
                    themeDeletionAllowed: true
                );

                var validationProblem = response.AssertBadRequestWithValidationProblem();

                validationProblem.AssertHasError(
                    expectedPath: "releaseVersionId",
                    expectedCode: ValidationMessages.OneOrMoreDataSetVersionsCanNotBeDeleted.Code
                );
            }
        }
    }

    private async Task<IActionResult> BulkDeleteDataSetVersions(
        Guid releaseVersionId,
        bool? forceDeleteAll = false,
        bool themeDeletionAllowed = false
    )
    {
        var function = GetRequiredService<BulkDeleteDataSetVersionsFunction>();

        var appOptions = GetRequiredService<IOptions<AppOptions>>();
        appOptions.Value.EnableThemeDeletion = themeDeletionAllowed;

        var request = new Mock<HttpRequest>(MockBehavior.Strict);

        var requestParams =
            forceDeleteAll != null
                ? new Dictionary<string, StringValues>
                {
                    { nameof(forceDeleteAll), forceDeleteAll + "" },
                }
                : new Dictionary<string, StringValues>();

        request.Setup(r => r.Query).Returns(new QueryCollection(requestParams));

        return await function.BulkDeleteDataSetVersions(
            httpRequest: request.Object,
            releaseVersionId: releaseVersionId,
            cancellationToken: CancellationToken.None
        );
    }
}
