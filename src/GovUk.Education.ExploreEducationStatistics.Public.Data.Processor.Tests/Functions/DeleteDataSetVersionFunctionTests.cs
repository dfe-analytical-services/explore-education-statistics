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

public abstract class DeleteDataSetVersionFunctionTests(ProcessorFunctionsIntegrationTestFixture fixture)
    : ProcessorFunctionsIntegrationTest(fixture)
{
    public class DeleteDataSetVersionTests : DeleteDataSetVersionFunctionTests
    {
        private readonly IDataSetVersionPathResolver _dataSetVersionPathResolver;

        public DeleteDataSetVersionTests(ProcessorFunctionsIntegrationTestFixture fixture) : base(fixture)
        {
            _dataSetVersionPathResolver = GetRequiredService<IDataSetVersionPathResolver>();
        }

        [Theory]
        [InlineData(DataSetVersionStatus.Failed)]
        [InlineData(DataSetVersionStatus.Mapping)]
        [InlineData(DataSetVersionStatus.Draft)]
        [InlineData(DataSetVersionStatus.Cancelled)]
        public async Task Success_FirstDataSetVersion(DataSetVersionStatus dataSetVersionStatus)
        {
            ReleaseFile releaseFile = DataFixture.DefaultReleaseFile()
                .WithReleaseVersion(DataFixture.DefaultReleaseVersion()
                    .WithPublication(DataFixture
                        .DefaultPublication()))
                .WithFile(DataFixture.DefaultFile());

            await AddTestData<ContentDbContext>(context =>
            {
                context.ReleaseFiles.Add(releaseFile);
            });

            DataSet dataSet = DataFixture
                .DefaultDataSet()
                .WithStatusDraft()
                .WithPublicationId(releaseFile.ReleaseVersion.PublicationId);

            await AddTestData<PublicDataDbContext>(context => context.DataSets.Add(dataSet));

            DataSetVersion dataSetVersion = DataFixture
                .DefaultDataSetVersion(filters: 1, indicators: 1, locations: 1, timePeriods: 2)
                .WithVersionNumber(1, 0, 0)
                .WithStatus(dataSetVersionStatus)
                .WithReleaseFileId(releaseFile.Id)
                .WithDataSet(dataSet)
                .WithImports(() => DataFixture
                    .DefaultDataSetVersionImport()
                    .Generate(1))
                .FinishWith(dsv => dsv.DataSet.LatestDraftVersion = dsv);

            await AddTestData<PublicDataDbContext>(context =>
            {
                context.DataSetVersions.Add(dataSetVersion);
                context.DataSets.Update(dataSet);
            });

            releaseFile.File.PublicApiDataSetId = dataSet.Id;
            releaseFile.File.PublicApiDataSetVersion = dataSetVersion.FullSemanticVersion();

            await AddTestData<ContentDbContext>(context => context.Files.Update(releaseFile.File));

            var dataSetVersionDirectory = _dataSetVersionPathResolver.DirectoryPath(dataSetVersion);

            Directory.CreateDirectory(dataSetVersionDirectory);
            await System.IO.File.Create(Path.Combine(dataSetVersionDirectory, "version1.txt")).DisposeAsync();

            await DeleteDataSetVersion(dataSetVersion.Id);

            await using var publicDataDbContext = GetDbContext<PublicDataDbContext>();
            await using var contentDataDbContext = GetDbContext<ContentDbContext>();

            // Assert that the base directory containing all parquet data file entries has been emptied
            var dataSetVersionBaseDirectoryEntries = Directory.GetFileSystemEntries(_dataSetVersionPathResolver.BasePath());
            Assert.Empty(dataSetVersionBaseDirectoryEntries);

            // Assert that the Data Set has been deleted
            Assert.Null(await publicDataDbContext.DataSets.SingleOrDefaultAsync(ds => ds.Id == dataSet.Id));

            // Assert that the Data Set Version has been deleted
            Assert.Null(await publicDataDbContext.DataSetVersions.SingleOrDefaultAsync(dsv => dsv.Id == dataSetVersion.Id));

            // Assert that the Data Set Version metadata is deleted
            Assert.False(await publicDataDbContext.FilterMetas
                    .AnyAsync(fm => fm.DataSetVersionId == dataSetVersion.Id));
            Assert.False(await publicDataDbContext.FilterOptionMetaLinks
                    .AnyAsync(foml => dataSetVersion.FilterMetas.Contains(foml.Meta)));
            Assert.False(await publicDataDbContext.LocationMetas
                    .AnyAsync(fm => fm.DataSetVersionId == dataSetVersion.Id));
            Assert.False(await publicDataDbContext.LocationOptionMetaLinks
                    .AnyAsync(loml => dataSetVersion.LocationMetas.Contains(loml.Meta)));
            Assert.False(await publicDataDbContext.IndicatorMetas
                    .AnyAsync(fm => fm.DataSetVersionId == dataSetVersion.Id));
            Assert.False(await publicDataDbContext.GeographicLevelMetas
                    .AnyAsync(fm => fm.DataSetVersionId == dataSetVersion.Id));
            Assert.False(await publicDataDbContext.TimePeriodMetas
                    .AnyAsync(fm => fm.DataSetVersionId == dataSetVersion.Id));

            // Assert that the Data Set Version Import has been deleted
            Assert.Null(await publicDataDbContext.DataSetVersionImports.SingleOrDefaultAsync(dsvi => dsvi.Id == dataSetVersion.Imports.Single().Id));

            // Assert that the Release File has been unassociated with the Public API DataSet
            var file = await contentDataDbContext.Files.SingleAsync(f => f.Id == releaseFile.FileId);
            Assert.Null(file.PublicApiDataSetId);
            Assert.Null(file.PublicApiDataSetVersion);
        }

        [Theory]
        [InlineData(DataSetVersionStatus.Failed)]
        [InlineData(DataSetVersionStatus.Mapping)]
        [InlineData(DataSetVersionStatus.Draft)]
        [InlineData(DataSetVersionStatus.Cancelled)]
        public async Task Success_SubsequentDataSetVersion(DataSetVersionStatus dataSetVersionStatus)
        {
            File liveFile = DataFixture.DefaultFile();
            File draftFile = DataFixture.DefaultFile();

            var releaseFiles = DataFixture.DefaultReleaseFile()
                .WithReleaseVersion(DataFixture.DefaultReleaseVersion()
                    .WithPublication(DataFixture
                        .DefaultPublication()))
                .ForIndex(0, rf => rf.SetFile(liveFile))
                .ForIndex(1, rf => rf.SetFile(draftFile))
                .GenerateList(2);

            var liveReleaseFile = releaseFiles[0];
            var draftReleaseFile = releaseFiles[1];

            await AddTestData<ContentDbContext>(context =>
            {
                context.ReleaseFiles.AddRange(releaseFiles);
            });

            DataSet dataSet = DataFixture
                .DefaultDataSet()
                .WithStatusDraft()
                .WithPublicationId(releaseFiles[0].ReleaseVersion.PublicationId);

            await AddTestData<PublicDataDbContext>(context => context.DataSets.Add(dataSet));

            DataSetVersion liveDataSetVersion = DataFixture
                .DefaultDataSetVersion(filters: 1, indicators: 1, locations: 1, timePeriods: 2)
                .WithVersionNumber(1, 0, 0)
                .WithStatusPublished()
                .WithReleaseFileId(liveReleaseFile.Id)
                .WithDataSet(dataSet)
                .WithImports(() => DataFixture
                    .DefaultDataSetVersionImport()
                    .WithStage(DataSetVersionImportStage.Completing)
                    .Generate(1))
                .FinishWith(dsv => dsv.DataSet.LatestLiveVersion = dsv);

            DataSetVersion draftDataSetVersion = DataFixture
                .DefaultDataSetVersion(filters: 1, indicators: 1, locations: 1, timePeriods: 2)
                .WithVersionNumber(2, 0, 0)
                .WithStatus(dataSetVersionStatus)
                .WithReleaseFileId(draftReleaseFile.Id)
                .WithDataSet(dataSet)
                .WithImports(() => DataFixture
                    .DefaultDataSetVersionImport()
                    .Generate(1))
                .FinishWith(dsv => dsv.DataSet.LatestDraftVersion = dsv);

            await AddTestData<PublicDataDbContext>(context =>
            {
                context.DataSetVersions.AddRange(liveDataSetVersion, draftDataSetVersion);
                context.DataSets.Update(dataSet);
            });

            liveFile.PublicApiDataSetId = dataSet.Id;
            liveFile.PublicApiDataSetVersion = liveDataSetVersion.FullSemanticVersion();
            draftFile.PublicApiDataSetId = dataSet.Id;
            draftFile.PublicApiDataSetVersion = draftDataSetVersion.FullSemanticVersion();

            await AddTestData<ContentDbContext>(context => context.Files.UpdateRange(liveFile, draftFile));

            var liveDataSetVersionDirectory = _dataSetVersionPathResolver.DirectoryPath(liveDataSetVersion);
            var draftDataSetVersionDirectory = _dataSetVersionPathResolver.DirectoryPath(draftDataSetVersion);

            Directory.CreateDirectory(liveDataSetVersionDirectory);
            Directory.CreateDirectory(draftDataSetVersionDirectory);
            System.IO.File.WriteAllText(Path.Combine(liveDataSetVersionDirectory, "version1.txt"), "dummy file text");
            System.IO.File.WriteAllText(Path.Combine(draftDataSetVersionDirectory, "version2.txt"), "dummy file text");

            await DeleteDataSetVersion(draftDataSetVersion.Id);

            await using var publicDataDbContext = GetDbContext<PublicDataDbContext>();
            await using var contentDataDbContext = GetDbContext<ContentDbContext>();

            // Assert that the DRAFT Data Set Version directory has been deleted
            Assert.False(Directory.Exists(draftDataSetVersionDirectory));

            // Assert that the LIVE Data Set Version directory has been untouched
            var liveDataSetVersionFileName = Assert.Single(Directory.GetFileSystemEntries(liveDataSetVersionDirectory));
            Assert.Contains("version1.txt", liveDataSetVersionFileName);

            // Assert that the Data Set has NOT been deleted
            var updatedDataSet = Assert.Single(await publicDataDbContext.DataSets
                .Where(ds => ds.Id == dataSet.Id)
                .ToListAsync());

            // Assert that the latest DRAFT version is unassigned from the DataSet
            Assert.Null(updatedDataSet.LatestDraftVersionId);

            // Assert that the latest LIVE version is still assigned to the DataSet
            Assert.Equal(liveDataSetVersion.Id, updatedDataSet.LatestLiveVersionId);

            // Assert that the DRAFT Data Set Version has been deleted
            Assert.Null(await publicDataDbContext.DataSetVersions.SingleOrDefaultAsync(dsv => dsv.Id == draftDataSetVersion.Id));

            // Assert that the LIVE Data Set Version has NOT been deleted
            Assert.Single(await publicDataDbContext.DataSetVersions
                .Where(dsv => dsv.Id == liveDataSetVersion.Id)
                .ToListAsync());

            // Assert that the DRAFT Data Set Version metadata is deleted
            Assert.Equal(0,
                await publicDataDbContext.FilterMetas
                    .Where(fm => fm.DataSetVersionId == draftDataSetVersion.Id)
                    .CountAsync());
            Assert.Equal(0,
                await publicDataDbContext.FilterOptionMetaLinks
                    .Where(foml => draftDataSetVersion.FilterMetas.Contains(foml.Meta))
                    .CountAsync());
            Assert.Equal(0,
                await publicDataDbContext.LocationMetas
                    .Where(fm => fm.DataSetVersionId == draftDataSetVersion.Id)
                    .CountAsync());
            Assert.Equal(0,
                await publicDataDbContext.LocationOptionMetaLinks
                    .Where(loml => draftDataSetVersion.LocationMetas.Contains(loml.Meta))
                    .CountAsync());
            Assert.Equal(0,
                await publicDataDbContext.IndicatorMetas
                    .Where(fm => fm.DataSetVersionId == draftDataSetVersion.Id)
                    .CountAsync());
            Assert.Equal(0,
                await publicDataDbContext.GeographicLevelMetas
                    .Where(fm => fm.DataSetVersionId == draftDataSetVersion.Id)
                    .CountAsync());
            Assert.Equal(0,
                await publicDataDbContext.TimePeriodMetas
                    .Where(fm => fm.DataSetVersionId == draftDataSetVersion.Id)
                    .CountAsync());

            // Assert that the LIVE Data Set Version metadata has NOT been deleted
            Assert.NotEmpty(await publicDataDbContext.FilterMetas
                .Where(fm => fm.DataSetVersionId == liveDataSetVersion.Id)
                .ToListAsync());
            Assert.NotEmpty(await publicDataDbContext.FilterOptionMetaLinks
                .Where(foml => liveDataSetVersion.FilterMetas.Contains(foml.Meta))
                .ToListAsync());
            Assert.NotEmpty(await publicDataDbContext.LocationMetas
                .Where(fm => fm.DataSetVersionId == liveDataSetVersion.Id)
                .ToListAsync());
            Assert.NotEmpty(await publicDataDbContext.LocationOptionMetaLinks
                .Where(loml => liveDataSetVersion.LocationMetas.Contains(loml.Meta))
                .ToListAsync());
            Assert.NotEmpty(await publicDataDbContext.IndicatorMetas
                .Where(fm => fm.DataSetVersionId == liveDataSetVersion.Id)
                .ToListAsync());
            Assert.NotEmpty(await publicDataDbContext.GeographicLevelMetas
                .Where(fm => fm.DataSetVersionId == liveDataSetVersion.Id)
                .ToListAsync());
            Assert.NotEmpty(await publicDataDbContext.TimePeriodMetas
                .Where(fm => fm.DataSetVersionId == liveDataSetVersion.Id)
                .ToListAsync());

            // Assert that the DRAFT Data Set Version Import has been deleted
            Assert.Null(await publicDataDbContext.DataSetVersionImports.SingleOrDefaultAsync(dsvi => dsvi.Id == draftDataSetVersion.Imports.Single().Id));

            // Assert that the LIVE Data Set Version Import has NOT been deleted
            Assert.Single(await publicDataDbContext.DataSetVersionImports
                .Where(dsvi => dsvi.Id == liveDataSetVersion.Imports.Single().Id)
                .ToListAsync());

            // Assert that the Release File has been unassociated with the DRAFT Public API Data Set Version 
            var updatedDraftFile = await contentDataDbContext.Files.SingleAsync(f => f.Id == draftFile.Id);
            Assert.Null(updatedDraftFile.PublicApiDataSetId);
            Assert.Null(updatedDraftFile.PublicApiDataSetVersion);

            // Assert that the Release File is still associated with the LIVE Public API Data Set Version 
            var updatedLiveFile = await contentDataDbContext.Files.SingleAsync(f => f.Id == liveFile.Id);
            Assert.Equal(dataSet.Id, updatedLiveFile.PublicApiDataSetId);
            Assert.Equal(liveDataSetVersion.FullSemanticVersion(), updatedLiveFile.PublicApiDataSetVersion);
        }

        [Theory]
        [InlineData(DataSetVersionStatus.Processing)]
        [InlineData(DataSetVersionStatus.Published)]
        [InlineData(DataSetVersionStatus.Deprecated)]
        [InlineData(DataSetVersionStatus.Withdrawn)]
        public async Task VersionCanNotBeDeleted_Returns400(DataSetVersionStatus dataSetVersionStatus)
        {
            DataSet dataSet = DataFixture
                .DefaultDataSet()
                .WithStatusPublished();

            await AddTestData<PublicDataDbContext>(context => context.DataSets.Add(dataSet));

            DataSetVersion dataSetVersion = DataFixture
                .DefaultDataSetVersion(filters: 1, indicators: 1, locations: 1, timePeriods: 2)
                .WithVersionNumber(1, 0, 0)
                .WithStatus(dataSetVersionStatus)
                .WithDataSet(dataSet);

            await AddTestData<PublicDataDbContext>(context =>
            {
                context.DataSetVersions.Add(dataSetVersion);
                context.DataSets.Update(dataSet);
            });

            var response = await DeleteDataSetVersion(dataSetVersion.Id);

            var validationProblem = response.AssertBadRequestWithValidationProblem();

            validationProblem.AssertHasError(
                expectedPath: "dataSetVersionId",
                expectedCode: ValidationMessages.DataSetVersionCanNotBeDeleted.Code);
        }

        [Fact]
        public async Task ReleaseFileDoesNotExist_Returns500()
        {
            DataSet dataSet = DataFixture
                .DefaultDataSet()
                .WithStatusDraft();

            await AddTestData<PublicDataDbContext>(context => context.DataSets.Add(dataSet));

            DataSetVersion dataSetVersion = DataFixture
                .DefaultDataSetVersion(filters: 1, indicators: 1, locations: 1, timePeriods: 2)
                .WithVersionNumber(1, 0, 0)
                .WithStatusDraft()
                .WithDataSet(dataSet)
                .WithImports(() => DataFixture
                    .DefaultDataSetVersionImport()
                    .Generate(1))
                .FinishWith(dsv => dsv.DataSet.LatestDraftVersion = dsv);

            await AddTestData<PublicDataDbContext>(context =>
            {
                context.DataSetVersions.Add(dataSetVersion);
                context.DataSets.Update(dataSet);
            });

            var response = await DeleteDataSetVersion(dataSetVersion.Id);

            response.AssertInternalServerError();
        }

        [Fact]
        public async Task DataSetVersionDoesNotExist_Returns404()
        {
            ReleaseFile releaseFile = DataFixture.DefaultReleaseFile()
                .WithReleaseVersion(DataFixture.DefaultReleaseVersion()
                    .WithPublication(DataFixture
                        .DefaultPublication()))
                .WithFile(DataFixture.DefaultFile());

            await AddTestData<ContentDbContext>(context =>
            {
                context.ReleaseFiles.Add(releaseFile);
            });

            DataSet dataSet = DataFixture
                .DefaultDataSet()
                .WithStatusDraft()
                .WithPublicationId(releaseFile.ReleaseVersion.PublicationId);

            await AddTestData<PublicDataDbContext>(context => context.DataSets.Add(dataSet));

            DataSetVersion dataSetVersion = DataFixture
                .DefaultDataSetVersion(filters: 1, indicators: 1, locations: 1, timePeriods: 2)
                .WithVersionNumber(1, 0, 0)
                .WithStatusDraft()
                .WithReleaseFileId(releaseFile.Id)
                .WithDataSet(dataSet)
                .WithImports(() => DataFixture
                    .DefaultDataSetVersionImport()
                    .Generate(1))
                .FinishWith(dsv => dsv.DataSet.LatestDraftVersion = dsv);

            await AddTestData<PublicDataDbContext>(context =>
            {
                context.DataSetVersions.Add(dataSetVersion);
                context.DataSets.Update(dataSet);
            });

            var response = await DeleteDataSetVersion(Guid.NewGuid());

            response.AssertNotFoundResult();
        }

        private async Task<IActionResult> DeleteDataSetVersion(Guid dataSetVersionId)
        {
            var function = GetRequiredService<DeleteDataSetVersionFunction>();

            return await function.DeleteDataSetVersion(
                null!,
                dataSetVersionId,
                CancellationToken.None);
        }
    }
}