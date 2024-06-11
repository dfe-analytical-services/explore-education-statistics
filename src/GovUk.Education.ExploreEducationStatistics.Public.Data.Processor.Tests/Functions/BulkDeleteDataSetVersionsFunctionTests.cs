using GovUk.Education.ExploreEducationStatistics.Common.Extensions;
using GovUk.Education.ExploreEducationStatistics.Common.Tests.Extensions;
using GovUk.Education.ExploreEducationStatistics.Common.Tests.Fixtures;
using GovUk.Education.ExploreEducationStatistics.Content.Model;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Database;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Tests.Fixtures;
using GovUk.Education.ExploreEducationStatistics.Public.Data.Model;
using GovUk.Education.ExploreEducationStatistics.Public.Data.Model.Database;
using GovUk.Education.ExploreEducationStatistics.Public.Data.Model.Tests.Fixtures;
using GovUk.Education.ExploreEducationStatistics.Public.Data.Processor.Functions;
using GovUk.Education.ExploreEducationStatistics.Public.Data.Processor.Requests.Validators;
using GovUk.Education.ExploreEducationStatistics.Public.Data.Services.Interfaces;
using LinqToDB;
using Microsoft.AspNetCore.Mvc;
using File = GovUk.Education.ExploreEducationStatistics.Content.Model.File;

namespace GovUk.Education.ExploreEducationStatistics.Public.Data.Processor.Tests.Functions;

public abstract class BulkDeleteDataSetVersionsFunctionTests(ProcessorFunctionsIntegrationTestFixture fixture)
    : ProcessorFunctionsIntegrationTest(fixture)
{
    public class BulkDeleteDataSetVersionsTests : BulkDeleteDataSetVersionsFunctionTests
    {
        private readonly IDataSetVersionPathResolver _dataSetVersionPathResolver;

        public BulkDeleteDataSetVersionsTests(ProcessorFunctionsIntegrationTestFixture fixture) : base(fixture)
        {
            _dataSetVersionPathResolver = GetRequiredService<IDataSetVersionPathResolver>();
        }

        [Theory]
        [InlineData(DataSetVersionStatus.Failed)]
        [InlineData(DataSetVersionStatus.Mapping)]
        [InlineData(DataSetVersionStatus.Draft)]
        [InlineData(DataSetVersionStatus.Cancelled)]
        public async Task Success(DataSetVersionStatus dataSetVersionStatus)
        {
            ReleaseVersion targetReleaseVersion = DataFixture.DefaultReleaseVersion()
                .WithPublication(DataFixture.DefaultPublication());

            ReleaseVersion otherReleaseVersion = DataFixture.DefaultReleaseVersion()
                .WithPublication(DataFixture.DefaultPublication());

            var targetFiles = DataFixture.DefaultFile()
                .GenerateList(3);

            File otherFile = DataFixture.DefaultFile();

            var targetReleaseFiles = DataFixture.DefaultReleaseFile()
                .WithReleaseVersion(targetReleaseVersion)
                .ForIndex(0, rf => rf.SetFile(targetFiles[0]))
                .ForIndex(1, rf => rf.SetFile(targetFiles[1]))
                .ForIndex(2, rf => rf.SetFile(targetFiles[2]))
                .GenerateList();

            ReleaseFile otherReleaseFile = DataFixture.DefaultReleaseFile()
                .WithReleaseVersion(otherReleaseVersion)
                .WithFile(otherFile);

            await AddTestData<ContentDbContext>(context =>
            {
                context.ReleaseFiles.AddRange([..targetReleaseFiles, otherReleaseFile]);
            });

            var targetDataSets = DataFixture
                .DefaultDataSet()
                .WithStatusDraft()
                .WithPublicationId(targetReleaseVersion.PublicationId)
                .GenerateList(3);

            DataSet otherDataSet = DataFixture
                .DefaultDataSet()
                .WithStatusDraft()
                .WithPublicationId(otherReleaseVersion.PublicationId);

            await AddTestData<PublicDataDbContext>(context => context.DataSets.AddRange([..targetDataSets, otherDataSet]));

            var targetDataSetVersions = DataFixture
                .DefaultDataSetVersion(filters: 1, indicators: 1, locations: 1, timePeriods: 2)
                .WithVersionNumber(1, 0, 0)
                .WithStatus(dataSetVersionStatus)
                .WithImports(() => DataFixture
                    .DefaultDataSetVersionImport()
                    .Generate(1))
                .ForIndex(0, dsv => dsv
                    .SetReleaseFileId(targetReleaseFiles[0].Id)
                    .SetDataSet(targetDataSets[0]))
                .ForIndex(1, dsv => dsv
                    .SetReleaseFileId(targetReleaseFiles[1].Id)
                    .SetDataSet(targetDataSets[1]))
                .ForIndex(2, dsv => dsv
                    .SetReleaseFileId(targetReleaseFiles[2].Id)
                    .SetDataSet(targetDataSets[2]))
                .FinishWith(dsv => dsv.DataSet.LatestDraftVersion = dsv)
                .GenerateList();

            DataSetVersion otherDataSetVersion = DataFixture
                .DefaultDataSetVersion(filters: 1, indicators: 1, locations: 1, timePeriods: 2)
                .WithVersionNumber(1, 0, 0)
                .WithStatusDraft()
                .WithDataSet(otherDataSet)
                .WithImports(() => DataFixture
                    .DefaultDataSetVersionImport()
                    .Generate(1))
                .FinishWith(dsv => dsv.DataSet.LatestDraftVersion = dsv);

            await AddTestData<PublicDataDbContext>(context =>
            {
                context.DataSetVersions.AddRange([..targetDataSetVersions, otherDataSetVersion]);
                context.DataSets.UpdateRange([..targetDataSets, otherDataSet]);
            });

            foreach (var (targetFile, index) in targetFiles.WithIndex())
            {
                targetFile.PublicApiDataSetId = targetDataSets[index].Id;
                targetFile.PublicApiDataSetVersion = targetDataSetVersions[index].FullSemanticVersion();
            }

            otherFile.PublicApiDataSetId = otherDataSet.Id;
            otherFile.PublicApiDataSetVersion = otherDataSetVersion.FullSemanticVersion();

            await AddTestData<ContentDbContext>(context => context.Files.UpdateRange([..targetFiles, otherFile]));

            foreach (var targetDataSetVersion in targetDataSetVersions)
            {
                var targetDataSetVersionDirectory = _dataSetVersionPathResolver.DirectoryPath(targetDataSetVersion);
                Directory.CreateDirectory(targetDataSetVersionDirectory);
                await System.IO.File.Create(Path.Combine(targetDataSetVersionDirectory, "version1.txt")).DisposeAsync();
            }

            var otherDataSetVersionDirectory = _dataSetVersionPathResolver.DirectoryPath(otherDataSetVersion);
            Directory.CreateDirectory(otherDataSetVersionDirectory);
            await System.IO.File.Create(Path.Combine(otherDataSetVersionDirectory, "version1.txt")).DisposeAsync();

            var response = await BulkDeleteDataSetVersions(targetReleaseVersion.Id);

            response.AssertNoContent();

            await using var publicDataDbContext = GetDbContext<PublicDataDbContext>();
            await using var contentDataDbContext = GetDbContext<ContentDbContext>();

            // Assertions for the TARGET data sets linked to the TARGET release version being deleted
            for (var i = 0; i < 3; i++)
            {
                var targetDataSet = targetDataSets[i];
                var targetDataSetVersion = targetDataSetVersions[i];
                var targetReleaseFile = targetReleaseFiles[i];

                // Assert that the TARGET parquet data set folder, linked to the release version being deleted, is now removed
                var targetDataSetDirectory = Directory.GetParent(_dataSetVersionPathResolver.DirectoryPath(targetDataSetVersion));
                Assert.False(Directory.Exists(targetDataSetDirectory!.FullName));

                // Assert that the TARGET Data Set has been deleted
                Assert.Null(await publicDataDbContext.DataSets.SingleOrDefaultAsync(ds => ds.Id == targetDataSet.Id));

                // Assert that the TARGET Data Set Version has been deleted
                Assert.Null(await publicDataDbContext.DataSetVersions.SingleOrDefaultAsync(dsv => dsv.Id == targetDataSetVersion.Id));

                // Assert that the TARGET Data Set Version metadata is deleted
                Assert.False(await publicDataDbContext.FilterMetas
                        .AnyAsync(fm => fm.DataSetVersionId == targetDataSetVersion.Id));
                Assert.False(await publicDataDbContext.FilterOptionMetaLinks
                        .AnyAsync(foml => targetDataSetVersion.FilterMetas.Contains(foml.Meta)));
                Assert.False(await publicDataDbContext.LocationMetas
                        .AnyAsync(fm => fm.DataSetVersionId == targetDataSetVersion.Id));
                Assert.False(await publicDataDbContext.LocationOptionMetaLinks
                        .AnyAsync(loml => targetDataSetVersion.LocationMetas.Contains(loml.Meta)));
                Assert.False(await publicDataDbContext.IndicatorMetas
                        .AnyAsync(fm => fm.DataSetVersionId == targetDataSetVersion.Id));
                Assert.False(await publicDataDbContext.GeographicLevelMetas
                        .AnyAsync(fm => fm.DataSetVersionId == targetDataSetVersion.Id));
                Assert.False(await publicDataDbContext.TimePeriodMetas
                        .AnyAsync(fm => fm.DataSetVersionId == targetDataSetVersion.Id));

                // Assert that the TARGET Data Set Version Import has been deleted
                Assert.Null(await publicDataDbContext.DataSetVersionImports.SingleOrDefaultAsync(dsvi => dsvi.Id == targetDataSetVersion.Imports.Single().Id));

                // Assert that the TARGET Release File has been unassociated with its Public API DataSet
                var targetFile = await contentDataDbContext.Files.SingleAsync(f => f.Id == targetReleaseFile.FileId);
                Assert.Null(targetFile.PublicApiDataSetId);
                Assert.Null(targetFile.PublicApiDataSetVersion);
            }

            // Below are the Assertions for the OTHER data set linked to the NON-TARGET release version. NOTHING should be deleted

            // Assert that the OTHER data set folder, and its contents, linked to the NON-TARGET release version remains
            // as it was
            var otherDataSetFolder = Directory.GetParent(_dataSetVersionPathResolver.DirectoryPath(otherDataSetVersion));
            Assert.True(Directory.Exists(otherDataSetFolder!.FullName));

            var otherDataSetFolderEntries = Directory.GetFileSystemEntries(otherDataSetFolder!.FullName);
            var otherDataSetVersionFolder = Assert.Single(otherDataSetFolderEntries,
                entry => new DirectoryInfo(entry).Name == $"v{otherDataSetVersion.Version}");

            var otherDataSetVersionFolderEntries = Directory.GetFileSystemEntries(otherDataSetVersionFolder);
            Assert.Single(otherDataSetVersionFolderEntries, entry => new FileInfo(entry).Name == "version1.txt");

            // Assert that the OTHER Data Set has NOT been deleted
            Assert.NotNull(await publicDataDbContext.DataSets.SingleAsync(ds => ds.Id == otherDataSet.Id));

            // Assert that the OTHER Data Set Version has NOT been deleted
            Assert.NotNull(await publicDataDbContext.DataSetVersions.SingleAsync(dsv => dsv.Id == otherDataSetVersion.Id));

            // Assert that the OTHER Data Set Version metadata is NOT deleted
            Assert.True(await publicDataDbContext.FilterMetas
                    .AnyAsync(fm => fm.DataSetVersionId == otherDataSetVersion.Id));
            Assert.True(await publicDataDbContext.FilterOptionMetaLinks
                    .AnyAsync(foml => otherDataSetVersion.FilterMetas.Contains(foml.Meta)));
            Assert.True(await publicDataDbContext.LocationMetas
                    .AnyAsync(fm => fm.DataSetVersionId == otherDataSetVersion.Id));
            Assert.True(await publicDataDbContext.LocationOptionMetaLinks
                    .AnyAsync(loml => otherDataSetVersion.LocationMetas.Contains(loml.Meta)));
            Assert.True(await publicDataDbContext.IndicatorMetas
                    .AnyAsync(fm => fm.DataSetVersionId == otherDataSetVersion.Id));
            Assert.True(await publicDataDbContext.GeographicLevelMetas
                    .AnyAsync(fm => fm.DataSetVersionId == otherDataSetVersion.Id));
            Assert.True(await publicDataDbContext.TimePeriodMetas
                    .AnyAsync(fm => fm.DataSetVersionId == otherDataSetVersion.Id));

            // Assert that the OTHER Data Set Version Import has NOT been deleted
            Assert.NotNull(await publicDataDbContext.DataSetVersionImports.SingleAsync(dsvi => dsvi.Id == otherDataSetVersion.Imports.Single().Id));

            // Assert that the OTHER Release File is still associated with its Public API DataSet
            var otherFilePostDelete = await contentDataDbContext.Files.SingleAsync(f => f.Id == otherReleaseFile.FileId);
            Assert.Equal(otherDataSet.Id, otherFilePostDelete.PublicApiDataSetId);
            Assert.Equal(otherDataSetVersion.FullSemanticVersion(), otherFilePostDelete.PublicApiDataSetVersion);
        }

        [Theory]
        [InlineData(DataSetVersionStatus.Processing)]
        [InlineData(DataSetVersionStatus.Published)]
        [InlineData(DataSetVersionStatus.Deprecated)]
        [InlineData(DataSetVersionStatus.Withdrawn)]
        public async Task VersionCanNotBeDeleted_FirstVersion_Returns400(DataSetVersionStatus dataSetVersionStatus)
        {
            ReleaseVersion releaseVersion = DataFixture.DefaultReleaseVersion()
               .WithPublication(DataFixture.DefaultPublication());

            ReleaseFile releaseFile = DataFixture.DefaultReleaseFile()
                .WithReleaseVersion(releaseVersion)
                .WithFile(DataFixture.DefaultFile());

            await AddTestData<ContentDbContext>(context =>
            {
                context.ReleaseFiles.Add(releaseFile);
            });

            DataSet dataSet = DataFixture
                .DefaultDataSet()
                .WithPublicationId(releaseVersion.Id)
                .WithStatusDraft();

            await AddTestData<PublicDataDbContext>(context => context.DataSets.Add(dataSet));

            DataSetVersion dataSetVersion = DataFixture
                .DefaultDataSetVersion(filters: 1, indicators: 1, locations: 1, timePeriods: 2)
                .WithVersionNumber(1, 0, 0)
                .WithStatus(dataSetVersionStatus)
                .WithReleaseFileId(releaseFile.Id)
                .WithDataSet(dataSet)
                .FinishWith(dsv => dsv.DataSet.LatestLiveVersion = dsv);

            await AddTestData<PublicDataDbContext>(context =>
            {
                context.DataSetVersions.Add(dataSetVersion);
                context.DataSets.Update(dataSet);
            });

            var response = await BulkDeleteDataSetVersions(releaseVersion.Id);

            var validationProblem = response.AssertBadRequestWithValidationProblem();

            validationProblem.AssertHasError(
                expectedPath: "releaseVersionId",
                expectedCode: ValidationMessages.MultipleDataSetVersionsCanNotBeDeleted.Code);
        }

        [Theory]
        [InlineData(DataSetVersionStatus.Processing)]
        [InlineData(DataSetVersionStatus.Published)]
        [InlineData(DataSetVersionStatus.Deprecated)]
        [InlineData(DataSetVersionStatus.Withdrawn)]
        public async Task VersionCanNotBeDeleted_SubsequentVersion_Returns400(DataSetVersionStatus dataSetVersionStatus)
        {
            ReleaseVersion releaseVersion = DataFixture.DefaultReleaseVersion()
               .WithPublication(DataFixture.DefaultPublication());

            ReleaseFile releaseFile = DataFixture.DefaultReleaseFile()
                .WithReleaseVersion(releaseVersion)
                .WithFile(DataFixture.DefaultFile());

            await AddTestData<ContentDbContext>(context =>
            {
                context.ReleaseFiles.Add(releaseFile);
            });

            DataSet dataSet = DataFixture
                .DefaultDataSet()
                .WithPublicationId(releaseVersion.Id)
                .WithStatusPublished();

            await AddTestData<PublicDataDbContext>(context => context.DataSets.Add(dataSet));

            DataSetVersion liveDataSetVersion = DataFixture
                .DefaultDataSetVersion(filters: 1, indicators: 1, locations: 1, timePeriods: 2)
                .WithReleaseFileId(releaseFile.Id)
                .WithDataSet(dataSet)
                .WithVersionNumber(1, 0, 0)
                .WithStatus(dataSetVersionStatus)
                .FinishWith(dsv => dsv.DataSet.LatestLiveVersion = dsv);

            DataSetVersion draftDataSetVersion = DataFixture
                .DefaultDataSetVersion(filters: 1, indicators: 1, locations: 1, timePeriods: 2)
                .WithReleaseFileId(releaseFile.Id)
                .WithDataSet(dataSet)
                .WithVersionNumber(2, 0, 0)
                .WithStatusDraft()
                .FinishWith(dsv => dsv.DataSet.LatestDraftVersion = dsv);

            await AddTestData<PublicDataDbContext>(context =>
            {
                context.DataSetVersions.AddRange(liveDataSetVersion, draftDataSetVersion);
                context.DataSets.Update(dataSet);
            });

            var response = await BulkDeleteDataSetVersions(releaseVersion.Id);

            var validationProblem = response.AssertBadRequestWithValidationProblem();

            validationProblem.AssertHasError(
                expectedPath: "releaseVersionId",
                expectedCode: ValidationMessages.MultipleDataSetVersionsCanNotBeDeleted.Code);
        }

        private async Task<IActionResult> BulkDeleteDataSetVersions(Guid releaseVersionId)
        {
            var function = GetRequiredService<BulkDeleteDataSetVersionsFunction>();

            return await function.BulkDeleteDataSetVersions(
                null!,
                releaseVersionId,
                CancellationToken.None);
        }
    }
}
