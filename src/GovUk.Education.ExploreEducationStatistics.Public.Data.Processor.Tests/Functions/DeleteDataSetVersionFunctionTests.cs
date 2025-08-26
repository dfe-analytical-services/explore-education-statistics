using GovUk.Education.ExploreEducationStatistics.Common.Model;
using GovUk.Education.ExploreEducationStatistics.Common.Tests.Extensions;
using GovUk.Education.ExploreEducationStatistics.Content.Model;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Database;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Tests.Fixtures;
using GovUk.Education.ExploreEducationStatistics.Public.Data.Model;
using GovUk.Education.ExploreEducationStatistics.Public.Data.Model.Database;
using GovUk.Education.ExploreEducationStatistics.Public.Data.Model.Tests.Fixtures;
using GovUk.Education.ExploreEducationStatistics.Public.Data.Processor.Functions;
using GovUk.Education.ExploreEducationStatistics.Public.Data.Processor.Requests.Validators;
using GovUk.Education.ExploreEducationStatistics.Public.Data.Processor.Tests.TheoryData;
using GovUk.Education.ExploreEducationStatistics.Public.Data.Services.Interfaces;
using LinqToDB;
using Microsoft.AspNetCore.Mvc;
using File = System.IO.File;

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
        [MemberData(nameof(DataSetVersionStatusTheoryData.DeletableStatuses),
            MemberType = typeof(DataSetVersionStatusTheoryData))]
        public async Task Success_FirstDataSetVersion(DataSetVersionStatus dataSetVersionStatus)
        {
            ReleaseFile releaseFile = DataFixture.DefaultReleaseFile()
                .WithReleaseVersion(DataFixture.DefaultReleaseVersion()
                    .WithPublication(DataFixture.DefaultPublication()))
                .WithFile(DataFixture.DefaultFile(FileType.Data));

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
                .WithVersionNumber(major: 1, minor: 0)
                .WithStatus(dataSetVersionStatus)
                .WithDataSet(dataSet)
                .WithRelease(DataFixture.DefaultDataSetVersionRelease()
                    .WithReleaseFileId(releaseFile.Id))
                .WithImports(() => DataFixture
                    .DefaultDataSetVersionImport()
                    .Generate(1))
                .FinishWith(dsv => dsv.DataSet.LatestDraftVersion = dsv);

            await AddTestData<PublicDataDbContext>(context =>
            {
                context.DataSetVersions.Add(dataSetVersion);
                context.DataSets.Update(dataSet);
            });

            releaseFile.PublicApiDataSetId = dataSet.Id;
            releaseFile.PublicApiDataSetVersion = dataSetVersion.SemVersion();

            await AddTestData<ContentDbContext>(context => context.ReleaseFiles.Update(releaseFile));

            var dataSetVersionDirectory = _dataSetVersionPathResolver.DirectoryPath(dataSetVersion);

            Directory.CreateDirectory(dataSetVersionDirectory);
            await File.Create(Path.Combine(dataSetVersionDirectory, "version1.txt")).DisposeAsync();

            await DeleteDataSetVersion(dataSetVersion.Id);

            await using var publicDataDbContext = GetDbContext<PublicDataDbContext>();
            await using var contentDataDbContext = GetDbContext<ContentDbContext>();

            // Assert that the base directory containing all parquet data file entries has been emptied
            var dataSetVersionBaseDirectoryEntries = Directory.GetFileSystemEntries(_dataSetVersionPathResolver.DataSetsPath());
            Assert.Empty(dataSetVersionBaseDirectoryEntries);

            // Assert that the Data Set has been deleted
            Assert.Null(await publicDataDbContext.DataSets.SingleOrDefaultAsync(ds => ds.Id == dataSet.Id));

            // Assert that the Data Set Version has been deleted
            Assert.Null(await publicDataDbContext.DataSetVersions.SingleOrDefaultAsync(dsv => dsv.Id == dataSetVersion.Id));

            // Assert that the Data Set Version metadata is deleted
            Assert.False(await publicDataDbContext.FilterMetas
                    .AnyAsync(fm => fm.DataSetVersionId == dataSetVersion.Id));
            Assert.False(await publicDataDbContext.FilterOptionMetaLinks
                    .AnyAsync(ml => dataSetVersion.FilterMetas.Contains(ml.Meta)));
            Assert.False(await publicDataDbContext.LocationMetas
                    .AnyAsync(fm => fm.DataSetVersionId == dataSetVersion.Id));
            Assert.False(await publicDataDbContext.LocationOptionMetaLinks
                    .AnyAsync(ml => dataSetVersion.LocationMetas.Contains(ml.Meta)));
            Assert.False(await publicDataDbContext.IndicatorMetas
                    .AnyAsync(fm => fm.DataSetVersionId == dataSetVersion.Id));
            Assert.False(await publicDataDbContext.GeographicLevelMetas
                    .AnyAsync(fm => fm.DataSetVersionId == dataSetVersion.Id));
            Assert.False(await publicDataDbContext.TimePeriodMetas
                    .AnyAsync(fm => fm.DataSetVersionId == dataSetVersion.Id));

            // Assert that the Data Set Version Import has been deleted
            Assert.Null(await publicDataDbContext.DataSetVersionImports
                .SingleOrDefaultAsync(v => v.Id == dataSetVersion.Imports.Single().Id));

            // Assert that the ReleaseFile has been unassociated with the Public API DataSet
            var updatedReleaseFile = await contentDataDbContext.ReleaseFiles
                .SingleAsync(rf => rf.Id == releaseFile.Id);

            Assert.Null(updatedReleaseFile.PublicApiDataSetId);
            Assert.Null(updatedReleaseFile.PublicApiDataSetVersion);
        }

        [Theory]
        [MemberData(nameof(DataSetVersionStatusTheoryData.DeletableStatuses),
            MemberType = typeof(DataSetVersionStatusTheoryData))]
        public async Task Success_SubsequentDataSetVersion(DataSetVersionStatus dataSetVersionStatus)
        {
            var releaseFiles = DataFixture.DefaultReleaseFile()
                .WithReleaseVersion(DataFixture.DefaultReleaseVersion()
                    .WithPublication(DataFixture.DefaultPublication()))
                .WithFile(() => DataFixture.DefaultFile(FileType.Data))
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
                .WithVersionNumber(major: 1, minor: 0)
                .WithStatusPublished()
                .WithDataSet(dataSet)
                .WithRelease(DataFixture.DefaultDataSetVersionRelease()
                    .WithReleaseFileId(liveReleaseFile.Id))
                .WithImports(() => DataFixture
                    .DefaultDataSetVersionImport()
                    .WithStage(DataSetVersionImportStage.Completing)
                    .Generate(1))
                .FinishWith(dsv => dsv.DataSet.LatestLiveVersion = dsv);

            DataSetVersion draftDataSetVersion = DataFixture
                .DefaultDataSetVersion(filters: 1, indicators: 1, locations: 1, timePeriods: 2)
                .WithVersionNumber(major: 2, minor: 0, patch: 1)
                .WithStatus(dataSetVersionStatus)
                .WithDataSet(dataSet)
                .WithRelease(DataFixture.DefaultDataSetVersionRelease()
                    .WithReleaseFileId(draftReleaseFile.Id))
                .WithImports(() => DataFixture
                    .DefaultDataSetVersionImport()
                    .Generate(1))
                .FinishWith(dsv => dsv.DataSet.LatestDraftVersion = dsv);

            await AddTestData<PublicDataDbContext>(context =>
            {
                context.DataSetVersions.AddRange(liveDataSetVersion, draftDataSetVersion);
                context.DataSets.Update(dataSet);
            });

            liveReleaseFile.PublicApiDataSetId = dataSet.Id;
            liveReleaseFile.PublicApiDataSetVersion = liveDataSetVersion.SemVersion();
            draftReleaseFile.PublicApiDataSetId = dataSet.Id;
            draftReleaseFile.PublicApiDataSetVersion = draftDataSetVersion.SemVersion();

            await AddTestData<ContentDbContext>(context =>
                context.ReleaseFiles.UpdateRange(liveReleaseFile, draftReleaseFile));

            var liveDataSetVersionDirectory = _dataSetVersionPathResolver.DirectoryPath(liveDataSetVersion);
            var draftDataSetVersionDirectory = _dataSetVersionPathResolver.DirectoryPath(draftDataSetVersion);

            Directory.CreateDirectory(liveDataSetVersionDirectory);
            Directory.CreateDirectory(draftDataSetVersionDirectory);

            await File.WriteAllTextAsync(
                Path.Combine(liveDataSetVersionDirectory, "version1.txt"),
                "dummy file text"
            );
            await File.WriteAllTextAsync(
                Path.Combine(draftDataSetVersionDirectory, "version2.txt"),
                "dummy file text"
            );

            await DeleteDataSetVersion(draftDataSetVersion.Id);

            await using var publicDataDbContext = GetDbContext<PublicDataDbContext>();
            await using var contentDataDbContext = GetDbContext<ContentDbContext>();

            // Assert that the DRAFT Data Set Version directory has been deleted
            Assert.False(Directory.Exists(draftDataSetVersionDirectory));

            // Assert that the LIVE Data Set Version directory has been untouched
            var liveDataSetVersionFileName = Assert.Single(
                Directory.GetFileSystemEntries(liveDataSetVersionDirectory)
            );
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
            Assert.Null(await publicDataDbContext.DataSetVersions
                .SingleOrDefaultAsync(dsv => dsv.Id == draftDataSetVersion.Id));

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
                    .Where(ml => draftDataSetVersion.FilterMetas.Contains(ml.Meta))
                    .CountAsync());
            Assert.Equal(0,
                await publicDataDbContext.LocationMetas
                    .Where(fm => fm.DataSetVersionId == draftDataSetVersion.Id)
                    .CountAsync());
            Assert.Equal(0,
                await publicDataDbContext.LocationOptionMetaLinks
                    .Where(ml => draftDataSetVersion.LocationMetas.Contains(ml.Meta))
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
                .Where(ml => liveDataSetVersion.FilterMetas.Contains(ml.Meta))
                .ToListAsync());
            Assert.NotEmpty(await publicDataDbContext.LocationMetas
                .Where(fm => fm.DataSetVersionId == liveDataSetVersion.Id)
                .ToListAsync());
            Assert.NotEmpty(await publicDataDbContext.LocationOptionMetaLinks
                .Where(ml => liveDataSetVersion.LocationMetas.Contains(ml.Meta))
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
            Assert.Null(await publicDataDbContext.DataSetVersionImports
                .SingleOrDefaultAsync(i => i.Id == draftDataSetVersion.Imports.Single().Id));

            // Assert that the LIVE Data Set Version Import has NOT been deleted
            Assert.Single(await publicDataDbContext.DataSetVersionImports
                .Where(i => i.Id == liveDataSetVersion.Imports.Single().Id)
                .ToListAsync());

            // Assert that the DRAFT ReleaseFile has been unassociated with the DRAFT Public API Data Set Version
            var updatedDraftReleaseFile = await contentDataDbContext.ReleaseFiles
                .SingleAsync(f => f.Id == draftReleaseFile.Id);

            Assert.Null(updatedDraftReleaseFile.PublicApiDataSetId);
            Assert.Null(updatedDraftReleaseFile.PublicApiDataSetVersion);

            // Assert that the LIVE ReleaseFile is still associated with the LIVE Public API Data Set Version
            var updatedLiveReleaseFile = await contentDataDbContext.ReleaseFiles
                .SingleAsync(f => f.Id == liveReleaseFile.Id);

            Assert.Equal(dataSet.Id, updatedLiveReleaseFile.PublicApiDataSetId);
            Assert.Equal(liveDataSetVersion.SemVersion(), updatedLiveReleaseFile.PublicApiDataSetVersion);
        }

        [Theory]
        [MemberData(nameof(DataSetVersionStatusTheoryData.NonDeletableStatuses),
            MemberType = typeof(DataSetVersionStatusTheoryData))]
        public async Task VersionCanNotBeDeleted_Returns400(DataSetVersionStatus dataSetVersionStatus)
        {
            DataSet dataSet = DataFixture
                .DefaultDataSet()
                .WithStatusPublished();

            await AddTestData<PublicDataDbContext>(context => context.DataSets.Add(dataSet));

            DataSetVersion dataSetVersion = DataFixture
                .DefaultDataSetVersion(filters: 1, indicators: 1, locations: 1, timePeriods: 2)
                .WithVersionNumber(major: 1, minor: 0)
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
        public async Task DataSetVersionDoesNotExist_Returns404()
        {
            ReleaseFile releaseFile = DataFixture.DefaultReleaseFile()
                .WithReleaseVersion(DataFixture.DefaultReleaseVersion()
                    .WithPublication(DataFixture.DefaultPublication()))
                .WithFile(DataFixture.DefaultFile(FileType.Data));

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
                .WithVersionNumber(major: 1, minor: 0)
                .WithStatusDraft()
                .WithDataSet(dataSet)
                .WithRelease(DataFixture.DefaultDataSetVersionRelease()
                    .WithReleaseFileId(releaseFile.Id))
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
