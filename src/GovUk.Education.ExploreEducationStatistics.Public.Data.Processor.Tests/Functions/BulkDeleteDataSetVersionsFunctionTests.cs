using GovUk.Education.ExploreEducationStatistics.Common.Extensions;
using GovUk.Education.ExploreEducationStatistics.Common.IntegrationTests;
using GovUk.Education.ExploreEducationStatistics.Common.IntegrationTests.FunctionApp;
using GovUk.Education.ExploreEducationStatistics.Common.Model;
using GovUk.Education.ExploreEducationStatistics.Common.Tests.Extensions;
using GovUk.Education.ExploreEducationStatistics.Common.Tests.Fixtures;
using GovUk.Education.ExploreEducationStatistics.Content.Model;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Tests.Fixtures;
using GovUk.Education.ExploreEducationStatistics.Public.Data.Model;
using GovUk.Education.ExploreEducationStatistics.Public.Data.Model.Tests.Fixtures;
using GovUk.Education.ExploreEducationStatistics.Public.Data.Processor.Functions;
using GovUk.Education.ExploreEducationStatistics.Public.Data.Processor.Requests.Validators;
using GovUk.Education.ExploreEducationStatistics.Public.Data.Processor.Tests.Fixture;
using GovUk.Education.ExploreEducationStatistics.Public.Data.Processor.Tests.TheoryData;
using LinqToDB.Async;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Primitives;
using Moq;
using File = GovUk.Education.ExploreEducationStatistics.Content.Model.File;
using FileInfo = System.IO.FileInfo;
using Release = GovUk.Education.ExploreEducationStatistics.Content.Model.Release;

#pragma warning disable CS9107 // Parameter is captured into the state of the enclosing type and its value is also passed to the base constructor. The value might be captured by the base class as well.

namespace GovUk.Education.ExploreEducationStatistics.Public.Data.Processor.Tests.Functions;

// ReSharper disable once ClassNeverInstantiated.Global
public class BulkDeleteDataSetVersionsFunctionTestsFixture()
    : OptimisedPublicDataProcessorCollectionFixture(
        capabilities: [PublicDataProcessorIntegrationTestCapability.Postgres]
    )
{
    public BulkDeleteDataSetVersionsFunction Function = null!;

    protected override void ConfigureServicesAndConfiguration(
        OptimisedServiceAndConfigModifications serviceModifications
    )
    {
        base.ConfigureServicesAndConfiguration(serviceModifications);

        serviceModifications.AddInMemoryCollection([
            new KeyValuePair<string, string?>("App:EnableThemeDeletion", "false"),
        ]);
    }

    protected override async Task AfterFactoryConstructed(OptimisedServiceCollectionLookups lookups)
    {
        await base.AfterFactoryConstructed(lookups);

        Function = lookups.GetService<BulkDeleteDataSetVersionsFunction>();
    }
}

[CollectionDefinition(nameof(BulkDeleteDataSetVersionsFunctionTestsFixture))]
public class BulkDeleteDataSetVersionsFunctionTestsCollection
    : ICollectionFixture<BulkDeleteDataSetVersionsFunctionTestsFixture>;

[Collection(nameof(BulkDeleteDataSetVersionsFunctionTestsFixture))]
public abstract class BulkDeleteDataSetVersionsFunctionTests(BulkDeleteDataSetVersionsFunctionTestsFixture fixture)
    : OptimisedFunctionAppIntegrationTestBase(fixture)
{
    private static readonly DataFixture DataFixture = new();

    public class BulkDeleteDataSetVersionsTests(BulkDeleteDataSetVersionsFunctionTestsFixture fixture)
        : BulkDeleteDataSetVersionsFunctionTests(fixture)
    {
        [Theory]
        [MemberData(
            nameof(DataSetVersionStatusTheoryData.DeletableStatuses),
            MemberType = typeof(DataSetVersionStatusTheoryData)
        )]
        public async Task ReleaseVersionIsLinkedToOldFileWithApiDataSet(DataSetVersionStatus dataSetVersionStatus)
        {
            Publication publication = DataFixture.DefaultPublication();

            Release release = DataFixture.DefaultRelease();

            ReleaseVersion previousReleaseVersion = DataFixture
                .DefaultReleaseVersion()
                .WithPublication(publication)
                .WithRelease(release)
                .WithApprovalStatus(ReleaseApprovalStatus.Approved)
                .WithPublished(DateTimeOffset.UtcNow)
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

            await fixture
                .GetContentDbContext()
                .AddTestData(context =>
                {
                    context.ReleaseFiles.AddRange([
                        previousReleaseFile,
                        targetReleaseFileWithOldFile,
                        .. targetReleaseFilesWithNewFiles,
                    ]);
                });

            DataSet otherDataSet = DataFixture.DefaultDataSet().WithStatusPublished().WithPublicationId(publication.Id);

            var targetDataSets = DataFixture
                .DefaultDataSet()
                .WithStatusDraft()
                .WithPublicationId(publication.Id)
                .GenerateList(3);

            await fixture
                .GetPublicDataDbContext()
                .AddTestData(context => context.DataSets.AddRange([otherDataSet, .. targetDataSets]));

            DataSetVersion otherDataSetVersion = DataFixture
                .DefaultDataSetVersion(filters: 1, indicators: 1, locations: 1, timePeriods: 2)
                .WithVersionNumber(major: 1, minor: 0)
                .WithStatusPublished()
                .WithDataSet(otherDataSet)
                .WithRelease(DataFixture.DefaultDataSetVersionRelease().WithReleaseFileId(previousReleaseFile.Id))
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

            await fixture
                .GetPublicDataDbContext()
                .AddTestData(context =>
                {
                    context.DataSetVersions.AddRange([otherDataSetVersion, .. targetDataSetVersions]);
                    context.DataSets.UpdateRange([otherDataSet, .. targetDataSets]);
                });

            previousReleaseFile.PublicApiDataSetId = otherDataSet.Id;
            previousReleaseFile.PublicApiDataSetVersion = otherDataSetVersion.SemVersion();
            targetReleaseFileWithOldFile.PublicApiDataSetId = previousReleaseFile.PublicApiDataSetId;
            targetReleaseFileWithOldFile.PublicApiDataSetVersion = previousReleaseFile.PublicApiDataSetVersion;

            foreach (var (targetReleaseFileWithNewFile, index) in targetReleaseFilesWithNewFiles.WithIndex())
            {
                targetReleaseFileWithNewFile.PublicApiDataSetId = targetDataSets[index].Id;
                targetReleaseFileWithNewFile.PublicApiDataSetVersion = targetDataSetVersions[index].SemVersion();
            }

            await fixture
                .GetContentDbContext()
                .AddTestData(context =>
                    context.ReleaseFiles.UpdateRange([
                        previousReleaseFile,
                        targetReleaseFileWithOldFile,
                        .. targetReleaseFilesWithNewFiles,
                    ])
                );

            foreach (var targetDataSetVersion in targetDataSetVersions)
            {
                var targetDataSetVersionDirectory = fixture
                    .GetDataSetVersionPathResolver()
                    .DirectoryPath(targetDataSetVersion);
                Directory.CreateDirectory(targetDataSetVersionDirectory);
                await System.IO.File.Create(Path.Combine(targetDataSetVersionDirectory, "version1.txt")).DisposeAsync();
            }

            var otherDataSetVersionDirectory = fixture
                .GetDataSetVersionPathResolver()
                .DirectoryPath(otherDataSetVersion);
            Directory.CreateDirectory(otherDataSetVersionDirectory);
            await System.IO.File.Create(Path.Combine(otherDataSetVersionDirectory, "version1.txt")).DisposeAsync();

            var response = await BulkDeleteDataSetVersions(targetReleaseVersion.Id);

            response.AssertNoContent();

            // Assertions for the TARGET data sets linked to the TARGET release version being deleted
            for (var i = 0; i < 3; i++)
            {
                var targetDataSet = targetDataSets[i];
                var targetDataSetVersion = targetDataSetVersions[i];
                var targetReleaseFileWithNewFile = targetReleaseFilesWithNewFiles[i];

                // Assert that the TARGET parquet data set folder, linked to the release version being deleted, is now removed
                var targetDataSetDirectory = Directory.GetParent(
                    fixture.GetDataSetVersionPathResolver().DirectoryPath(targetDataSetVersion)
                );
                Assert.False(Directory.Exists(targetDataSetDirectory!.FullName));

                // Assert that the TARGET Data Set has been deleted
                Assert.Null(
                    await fixture
                        .GetPublicDataDbContext()
                        .DataSets.SingleOrDefaultAsync(ds => ds.Id == targetDataSet.Id)
                );

                // Assert that the TARGET Data Set Version has been deleted
                Assert.Null(
                    await fixture
                        .GetPublicDataDbContext()
                        .DataSetVersions.SingleOrDefaultAsync(dsv => dsv.Id == targetDataSetVersion.Id)
                );

                // Assert that the TARGET Data Set Version metadata is deleted
                await AssertMetadataIsDeleted(targetDataSetVersion);

                // Assert that the TARGET Data Set Version Import has been deleted
                Assert.Null(
                    await fixture
                        .GetPublicDataDbContext()
                        .DataSetVersionImports.SingleOrDefaultAsync(dsvi =>
                            dsvi.Id == targetDataSetVersion.Imports.Single().Id
                        )
                );

                // Assert that the TARGET Release File has been unassociated with its Public API DataSet
                var targetReleaseFile = await fixture
                    .GetContentDbContext()
                    .ReleaseFiles.SingleAsync(f => f.Id == targetReleaseFileWithNewFile.Id);
                Assert.Null(targetReleaseFile.PublicApiDataSetId);
                Assert.Null(targetReleaseFile.PublicApiDataSetVersion);
            }

            // Assert that the TARGET Release File, which points to the OLD File has been unassociated with its Public API DataSet
            var targetReleaseFileWithOldFilePostDelete = await fixture
                .GetContentDbContext()
                .ReleaseFiles.SingleAsync(f => f.Id == targetReleaseFileWithOldFile.Id);
            Assert.Null(targetReleaseFileWithOldFilePostDelete.PublicApiDataSetId);
            Assert.Null(targetReleaseFileWithOldFilePostDelete.PublicApiDataSetVersion);

            // Below are the Assertions for the OTHER data set linked to the NON-TARGET release version. NOTHING should be deleted

            // Assert that the OTHER data set folder, and its contents, linked to the NON-TARGET release version remains
            // as it was
            var otherDataSetFolder = Directory.GetParent(
                fixture.GetDataSetVersionPathResolver().DirectoryPath(otherDataSetVersion)
            );
            Assert.True(Directory.Exists(otherDataSetFolder!.FullName));

            var otherDataSetFolderEntries = Directory.GetFileSystemEntries(otherDataSetFolder.FullName);
            var otherDataSetVersionFolder = Assert.Single(
                otherDataSetFolderEntries,
                entry => new DirectoryInfo(entry).Name == $"v{otherDataSetVersion.SemVersion()}"
            );

            var otherDataSetVersionFolderEntries = Directory.GetFileSystemEntries(otherDataSetVersionFolder);
            Assert.Single(otherDataSetVersionFolderEntries, entry => new FileInfo(entry).Name == "version1.txt");

            // Assert that the OTHER Data Set has NOT been deleted
            Assert.NotNull(await fixture.GetPublicDataDbContext().DataSets.SingleAsync(ds => ds.Id == otherDataSet.Id));

            // Assert that the OTHER Data Set Version has NOT been deleted
            Assert.NotNull(
                await fixture
                    .GetPublicDataDbContext()
                    .DataSetVersions.SingleAsync(dsv => dsv.Id == otherDataSetVersion.Id)
            );

            // Assert that the OTHER Data Set Version metadata is NOT deleted
            await AssertMetadataIsNotDeleted(otherDataSetVersion);

            // Assert that the OTHER Data Set Version Import has NOT been deleted
            Assert.NotNull(
                await fixture
                    .GetPublicDataDbContext()
                    .DataSetVersionImports.SingleAsync(dsvi => dsvi.Id == otherDataSetVersion.Imports.Single().Id)
            );

            // Assert that the PREVIOUS Release File is still associated with its Public API DataSet
            var previousReleaseFilePostDelete = await fixture
                .GetContentDbContext()
                .ReleaseFiles.SingleAsync(f => f.Id == previousReleaseFile.Id);
            Assert.Equal(otherDataSet.Id, previousReleaseFilePostDelete.PublicApiDataSetId);
            Assert.Equal(otherDataSetVersion.SemVersion(), previousReleaseFilePostDelete.PublicApiDataSetVersion);
        }

        [Theory]
        [MemberData(
            nameof(DataSetVersionStatusTheoryData.DeletableStatuses),
            MemberType = typeof(DataSetVersionStatusTheoryData)
        )]
        public async Task ReleaseVersionIsLinkedToSubsequentDataSetVersion(DataSetVersionStatus dataSetVersionStatus)
        {
            Publication publication = DataFixture.DefaultPublication();

            Release release1 = DataFixture.DefaultRelease();

            Release release2 = DataFixture.DefaultRelease();

            ReleaseVersion release1Version1 = DataFixture
                .DefaultReleaseVersion()
                .WithPublication(publication)
                .WithRelease(release1)
                .WithApprovalStatus(ReleaseApprovalStatus.Approved)
                .WithPublished(DateTimeOffset.UtcNow)
                .WithVersion(0);

            ReleaseVersion release2Version1 = DataFixture
                .DefaultReleaseVersion()
                .WithPublication(publication)
                .WithRelease(release2)
                .WithApprovalStatus(ReleaseApprovalStatus.Approved)
                .WithPublished(DateTimeOffset.UtcNow)
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

            await fixture
                .GetContentDbContext()
                .AddTestData(context =>
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

            await fixture
                .GetPublicDataDbContext()
                .AddTestData(context => context.DataSets.AddRange(release1Version1DataSet, release2Version1DataSet));

            DataSetVersion release1Version1DataSetVersion = DataFixture
                .DefaultDataSetVersion(filters: 1, indicators: 1, locations: 1, timePeriods: 2)
                .WithVersionNumber(major: 1, minor: 0)
                .WithStatusPublished()
                .WithDataSet(release1Version1DataSet)
                .WithRelease(
                    DataFixture.DefaultDataSetVersionRelease().WithReleaseFileId(release1Version1ReleaseFile.Id)
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
                    DataFixture.DefaultDataSetVersionRelease().WithReleaseFileId(release2Version1ReleaseFile.Id)
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
                    DataFixture.DefaultDataSetVersionRelease().WithReleaseFileId(release2Version2ReleaseFile.Id)
                )
                .WithImports(() => DataFixture.DefaultDataSetVersionImport().Generate(1))
                .FinishWith(dsv => dsv.DataSet.LatestDraftVersion = dsv);

            await fixture
                .GetPublicDataDbContext()
                .AddTestData(context =>
                {
                    context.DataSetVersions.AddRange(
                        release1Version1DataSetVersion,
                        release2Version1DataSetVersion,
                        release2Version2DataSetVersion
                    );

                    context.DataSets.UpdateRange(release1Version1DataSet, release2Version1DataSet);
                });

            release1Version1ReleaseFile.PublicApiDataSetId = release1Version1DataSet.Id;
            release1Version1ReleaseFile.PublicApiDataSetVersion = release1Version1DataSetVersion.SemVersion();
            release2Version1ReleaseFile.PublicApiDataSetId = release2Version1DataSet.Id;
            release2Version1ReleaseFile.PublicApiDataSetVersion = release2Version1DataSetVersion.SemVersion();
            release2Version2ReleaseFile.PublicApiDataSetId = release1Version1DataSet.Id; // Same Data Set series as release1version1
            release2Version2ReleaseFile.PublicApiDataSetVersion = release2Version2DataSetVersion.SemVersion();

            await fixture
                .GetContentDbContext()
                .AddTestData(context =>
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
                var dataSetVersionDirectory = fixture.GetDataSetVersionPathResolver().DirectoryPath(dataSetVersion);
                Directory.CreateDirectory(dataSetVersionDirectory);
                await System
                    .IO.File.Create(Path.Combine(dataSetVersionDirectory, $"{dataSetVersion.SemVersion()}.txt"))
                    .DisposeAsync();
            }

            var response = await BulkDeleteDataSetVersions(release2Version2.Id);

            response.AssertNoContent();

            // Assert that the parquet folder for the new, DRAFT, API Data Set Version linked to the release version being deleted, is now deleted
            var release2Version2DataSetVersionDirectory = fixture
                .GetDataSetVersionPathResolver()
                .DirectoryPath(release2Version2DataSetVersion);
            Assert.False(Directory.Exists(release2Version2DataSetVersionDirectory));

            // Assert that the parent parquet folder for the API Data Set, linked to the release version being deleted, has NOT been deleted
            var release1Version1DataSetDirectory = Directory.GetParent(
                fixture.GetDataSetVersionPathResolver().DirectoryPath(release2Version2DataSetVersion)
            );
            Assert.True(Directory.Exists(release1Version1DataSetDirectory!.FullName));

            // Assert that the parquet folder, and its contents, for the API Data Set Version linked to Release 1 Version 1, has NOT been deleted
            var release1Version1DataSetVersionDirectory = fixture
                .GetDataSetVersionPathResolver()
                .DirectoryPath(release1Version1DataSetVersion);
            Assert.True(Directory.Exists(release1Version1DataSetVersionDirectory));

            var release1Version1DataSetVersionDirectoryEntries = Directory.GetFileSystemEntries(
                release1Version1DataSetVersionDirectory
            );
            Assert.Single(
                release1Version1DataSetVersionDirectoryEntries,
                entry => new FileInfo(entry).Name == $"{release1Version1DataSetVersion.SemVersion()}.txt"
            );

            // Assert that the parquet folder, and its contents, for the API Data Set Version linked to Release 2 Version 1, has NOT been deleted
            var release2Version1DataSetVersionDirectory = fixture
                .GetDataSetVersionPathResolver()
                .DirectoryPath(release2Version1DataSetVersion);
            Assert.True(Directory.Exists(release2Version1DataSetVersionDirectory));

            var release2Version1DataSetVersionDirectoryEntries = Directory.GetFileSystemEntries(
                release2Version1DataSetVersionDirectory
            );
            Assert.Single(
                release2Version1DataSetVersionDirectoryEntries,
                entry => new FileInfo(entry).Name == $"{release2Version1DataSetVersion.SemVersion()}.txt"
            );

            // Assert that the TARGET API Data Set, linked to the release version being deleted, has NOT been deleted
            Assert.NotNull(
                await fixture
                    .GetPublicDataDbContext()
                    .DataSets.SingleOrDefaultAsync(ds => ds.Id == release1Version1DataSet.Id)
            );

            // Assert that the OTHER Data Set has NOT been deleted
            Assert.NotNull(
                await fixture.GetPublicDataDbContext().DataSets.SingleAsync(ds => ds.Id == release2Version1DataSet.Id)
            );

            // Assert that the TARGET Data Set Version has been deleted
            Assert.Null(
                await fixture
                    .GetPublicDataDbContext()
                    .DataSetVersions.SingleOrDefaultAsync(dsv => dsv.Id == release2Version2DataSetVersion.Id)
            );

            // Assert that the OTHER Data Set Versions have NOT been deleted
            Assert.NotNull(
                await fixture
                    .GetPublicDataDbContext()
                    .DataSetVersions.SingleAsync(dsv => dsv.Id == release1Version1DataSetVersion.Id)
            );
            Assert.NotNull(
                await fixture
                    .GetPublicDataDbContext()
                    .DataSetVersions.SingleAsync(dsv => dsv.Id == release2Version1DataSetVersion.Id)
            );

            // Assert that the TARGET Data Set Version metadata is deleted
            await AssertMetadataIsDeleted(release2Version2DataSetVersion);

            // Assert that the OTHER Data Set Versions metadata has NOT been deleted
            await AssertMetadataIsNotDeleted(release1Version1DataSetVersion);
            await AssertMetadataIsNotDeleted(release2Version1DataSetVersion);

            // Assert that the TARGET Data Set Version Import has been deleted
            Assert.Null(
                await fixture
                    .GetPublicDataDbContext()
                    .DataSetVersionImports.SingleOrDefaultAsync(dsvi =>
                        dsvi.Id == release2Version2DataSetVersion.Imports.Single().Id
                    )
            );

            // Assert that the OTHER Data Set Version Imports have NOT been deleted
            Assert.NotNull(
                await fixture
                    .GetPublicDataDbContext()
                    .DataSetVersionImports.SingleAsync(dsvi =>
                        dsvi.Id == release1Version1DataSetVersion.Imports.Single().Id
                    )
            );
            Assert.NotNull(
                await fixture
                    .GetPublicDataDbContext()
                    .DataSetVersionImports.SingleAsync(dsvi =>
                        dsvi.Id == release2Version1DataSetVersion.Imports.Single().Id
                    )
            );

            // Assert that the TARGET Release File has been unassociated with its Public API DataSet
            var targetReleaseFile = await fixture
                .GetContentDbContext()
                .ReleaseFiles.SingleAsync(f => f.Id == release2Version2ReleaseFile.Id);

            Assert.Null(targetReleaseFile.PublicApiDataSetId);
            Assert.Null(targetReleaseFile.PublicApiDataSetVersion);

            // Assert that the OTHER Release Files are still associated with their Public API DataSets
            var release1Version1ReleaseFilePostDelete = await fixture
                .GetContentDbContext()
                .ReleaseFiles.SingleAsync(f => f.Id == release1Version1ReleaseFile.Id);
            var release2Version1ReleaseFilePostDelete = await fixture
                .GetContentDbContext()
                .ReleaseFiles.SingleAsync(f => f.Id == release2Version1ReleaseFile.Id);

            Assert.Equal(release1Version1DataSet.Id, release1Version1ReleaseFilePostDelete.PublicApiDataSetId);
            Assert.Equal(
                release1Version1DataSetVersion.SemVersion(),
                release1Version1ReleaseFilePostDelete.PublicApiDataSetVersion
            );
            Assert.Equal(release2Version1DataSet.Id, release2Version1ReleaseFilePostDelete.PublicApiDataSetId);
            Assert.Equal(
                release2Version1DataSetVersion.SemVersion(),
                release2Version1ReleaseFilePostDelete.PublicApiDataSetVersion
            );
        }

        private async Task AssertMetadataIsDeleted(DataSetVersion dataSetVersion)
        {
            Assert.False(
                await fixture
                    .GetPublicDataDbContext()
                    .FilterMetas.AnyAsync(fm => fm.DataSetVersionId == dataSetVersion.Id)
            );
            Assert.False(
                await fixture
                    .GetPublicDataDbContext()
                    .FilterOptionMetaLinks.AnyAsync(foml => dataSetVersion.FilterMetas.Contains(foml.Meta))
            );
            Assert.False(
                await fixture
                    .GetPublicDataDbContext()
                    .LocationMetas.AnyAsync(fm => fm.DataSetVersionId == dataSetVersion.Id)
            );
            Assert.False(
                await fixture
                    .GetPublicDataDbContext()
                    .LocationOptionMetaLinks.AnyAsync(loml => dataSetVersion.LocationMetas.Contains(loml.Meta))
            );
            Assert.False(
                await fixture
                    .GetPublicDataDbContext()
                    .IndicatorMetas.AnyAsync(fm => fm.DataSetVersionId == dataSetVersion.Id)
            );
            Assert.False(
                await fixture
                    .GetPublicDataDbContext()
                    .GeographicLevelMetas.AnyAsync(fm => fm.DataSetVersionId == dataSetVersion.Id)
            );
            Assert.False(
                await fixture
                    .GetPublicDataDbContext()
                    .TimePeriodMetas.AnyAsync(fm => fm.DataSetVersionId == dataSetVersion.Id)
            );
        }

        private async Task AssertMetadataIsNotDeleted(DataSetVersion dataSetVersion)
        {
            Assert.True(
                await fixture
                    .GetPublicDataDbContext()
                    .FilterMetas.AnyAsync(fm => fm.DataSetVersionId == dataSetVersion.Id)
            );
            Assert.True(
                await fixture
                    .GetPublicDataDbContext()
                    .FilterOptionMetaLinks.AnyAsync(foml => dataSetVersion.FilterMetas.Contains(foml.Meta))
            );
            Assert.True(
                await fixture
                    .GetPublicDataDbContext()
                    .LocationMetas.AnyAsync(fm => fm.DataSetVersionId == dataSetVersion.Id)
            );
            Assert.True(
                await fixture
                    .GetPublicDataDbContext()
                    .LocationOptionMetaLinks.AnyAsync(loml => dataSetVersion.LocationMetas.Contains(loml.Meta))
            );
            Assert.True(
                await fixture
                    .GetPublicDataDbContext()
                    .IndicatorMetas.AnyAsync(fm => fm.DataSetVersionId == dataSetVersion.Id)
            );
            Assert.True(
                await fixture
                    .GetPublicDataDbContext()
                    .GeographicLevelMetas.AnyAsync(fm => fm.DataSetVersionId == dataSetVersion.Id)
            );
            Assert.True(
                await fixture
                    .GetPublicDataDbContext()
                    .TimePeriodMetas.AnyAsync(fm => fm.DataSetVersionId == dataSetVersion.Id)
            );
        }

        [Theory]
        [MemberData(
            nameof(DataSetVersionStatusTheoryData.NonDeletableStatuses),
            MemberType = typeof(DataSetVersionStatusTheoryData)
        )]
        public async Task DataSetVersionCanNotBeDeleted_Returns400(DataSetVersionStatus dataSetVersionStatus)
        {
            ReleaseVersion releaseVersion = DataFixture
                .DefaultReleaseVersion()
                .WithPublication(DataFixture.DefaultPublication());

            var (releaseFile1, releaseFile2) = DataFixture
                .DefaultReleaseFile()
                .WithFile(DataFixture.DefaultFile(FileType.Data))
                .WithReleaseVersion(releaseVersion)
                .GenerateTuple2();

            await fixture
                .GetContentDbContext()
                .AddTestData(context =>
                {
                    context.ReleaseFiles.AddRange(releaseFile1, releaseFile2);
                });

            var (dataSet1, dataSet2) = DataFixture
                .DefaultDataSet()
                .WithPublicationId(releaseVersion.PublicationId)
                .WithStatusPublished()
                .GenerateTuple2();

            await fixture
                .GetPublicDataDbContext()
                .AddTestData(context => context.DataSets.AddRange(dataSet1, dataSet2));

            // Data set version that is not in a deletable state
            DataSetVersion dataSet1Version = DataFixture
                .DefaultDataSetVersion(filters: 1, indicators: 1, locations: 1, timePeriods: 2)
                .WithDataSet(dataSet1)
                .WithRelease(DataFixture.DefaultDataSetVersionRelease().WithReleaseFileId(releaseFile1.Id))
                .WithVersionNumber(major: 1, minor: 0)
                .WithStatus(dataSetVersionStatus)
                .FinishWith(dsv => dsv.DataSet.LatestLiveVersion = dsv);

            // Data set version that is in a deletable state
            DataSetVersion dataSet2Version = DataFixture
                .DefaultDataSetVersion(filters: 1, indicators: 1, locations: 1, timePeriods: 2)
                .WithDataSet(dataSet2)
                .WithRelease(DataFixture.DefaultDataSetVersionRelease().WithReleaseFileId(releaseFile2.Id))
                .WithVersionNumber(major: 1, minor: 0)
                .WithStatusDraft()
                .FinishWith(dsv => dsv.DataSet.LatestDraftVersion = dsv);

            await fixture
                .GetPublicDataDbContext()
                .AddTestData(context =>
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

        [Theory]
        [MemberData(
            nameof(DataSetVersionStatusTheoryData.NonDeletableStatuses),
            MemberType = typeof(DataSetVersionStatusTheoryData)
        )]
        public async Task TestTheme_ThemeDeletionNotAllowed_Returns400(DataSetVersionStatus dataSetVersionStatus)
        {
            ReleaseVersion releaseVersion = DataFixture
                .DefaultReleaseVersion()
                .WithPublication(
                    DataFixture
                        .DefaultPublication()
                        .WithTheme(DataFixture.DefaultTheme().WithTitle($"UI test theme {Guid.NewGuid()}"))
                );

            ReleaseFile releaseFile = DataFixture
                .DefaultReleaseFile()
                .WithFile(DataFixture.DefaultFile(FileType.Data))
                .WithReleaseVersion(releaseVersion);

            await fixture
                .GetContentDbContext()
                .AddTestData(context =>
                {
                    context.ReleaseFiles.AddRange(releaseFile);
                });

            var dataSet = DataFixture
                .DefaultDataSet()
                .WithPublicationId(releaseVersion.PublicationId)
                .WithStatusPublished()
                .Generate();

            await fixture.GetPublicDataDbContext().AddTestData(context => context.DataSets.AddRange(dataSet));

            // Data set version that is not in a deletable state without force delete
            DataSetVersion dataSetVersion = DataFixture
                .DefaultDataSetVersion(filters: 1, indicators: 1, locations: 1, timePeriods: 2)
                .WithDataSet(dataSet)
                .WithRelease(DataFixture.DefaultDataSetVersionRelease().WithReleaseFileId(releaseFile.Id))
                .WithVersionNumber(major: 1, minor: 0)
                .WithStatus(dataSetVersionStatus)
                .FinishWith(dsv => dsv.DataSet.LatestLiveVersion = dsv);

            await fixture
                .GetPublicDataDbContext()
                .AddTestData(context =>
                {
                    context.DataSetVersions.AddRange(dataSetVersion);
                    context.DataSets.Update(dataSet);
                });

            // Request force deletion but without delete support from the
            // environment configuration
            var response = await BulkDeleteDataSetVersions(releaseVersionId: releaseVersion.Id, forceDeleteAll: true);

            var validationProblem = response.AssertBadRequestWithValidationProblem();

            validationProblem.AssertHasError(
                expectedPath: "releaseVersionId",
                expectedCode: ValidationMessages.OneOrMoreDataSetVersionsCanNotBeDeleted.Code
            );
        }

        private async Task<IActionResult> BulkDeleteDataSetVersions(Guid releaseVersionId, bool? forceDeleteAll = false)
        {
            var request = new Mock<HttpRequest>(MockBehavior.Strict);

            var requestParams =
                forceDeleteAll != null
                    ? new Dictionary<string, StringValues> { { nameof(forceDeleteAll), forceDeleteAll + "" } }
                    : new Dictionary<string, StringValues>();

            request.Setup(r => r.Query).Returns(new QueryCollection(requestParams));

            return await fixture.Function.BulkDeleteDataSetVersions(
                httpRequest: request.Object,
                releaseVersionId: releaseVersionId,
                cancellationToken: CancellationToken.None
            );
        }
    }
}
