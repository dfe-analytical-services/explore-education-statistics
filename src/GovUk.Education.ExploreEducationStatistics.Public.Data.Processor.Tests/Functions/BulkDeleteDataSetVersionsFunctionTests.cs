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
            ReleaseVersion releaseVersion = DataFixture.DefaultReleaseVersion()
                .WithPublication(DataFixture.DefaultPublication());

            var files = DataFixture.DefaultFile()
                .GenerateList(3);

            var releaseFiles = DataFixture.DefaultReleaseFile()
                .WithReleaseVersion(releaseVersion)
                .ForIndex(0, rf => rf.SetFile(files[0]))
                .ForIndex(1, rf => rf.SetFile(files[1]))
                .ForIndex(2, rf => rf.SetFile(files[2]))
                .GenerateList();

            await AddTestData<ContentDbContext>(context =>
            {
                context.ReleaseFiles.AddRange(releaseFiles);
            });

            var dataSets = DataFixture
                .DefaultDataSet()
                .WithStatusDraft()
                .WithPublicationId(releaseVersion.PublicationId)
                .GenerateList(3);

            await AddTestData<PublicDataDbContext>(context => context.DataSets.AddRange(dataSets));

            var dataSetVersions = DataFixture
                .DefaultDataSetVersion(filters: 1, indicators: 1, locations: 1, timePeriods: 2)
                .WithVersionNumber(1, 0, 0)
                .WithStatus(dataSetVersionStatus)
                .WithImports(() => DataFixture
                    .DefaultDataSetVersionImport()
                    .Generate(1))
                .ForIndex(0, dsv => dsv
                    .SetReleaseFileId(releaseFiles[0].Id)
                    .SetDataSet(dataSets[0]))
                .ForIndex(1, dsv => dsv
                    .SetReleaseFileId(releaseFiles[1].Id)
                    .SetDataSet(dataSets[1]))
                .ForIndex(2, dsv => dsv
                    .SetReleaseFileId(releaseFiles[2].Id)
                    .SetDataSet(dataSets[2]))
                .FinishWith(dsv => dsv.DataSet.LatestDraftVersion = dsv)
                .GenerateList();

            await AddTestData<PublicDataDbContext>(context =>
            {
                context.DataSetVersions.AddRange(dataSetVersions);
                context.DataSets.UpdateRange(dataSets);
            });

            foreach (var (file, index) in files.WithIndex())
            {
                file.PublicApiDataSetId = dataSets[index].Id;
                file.PublicApiDataSetVersion = dataSetVersions[index].FullSemanticVersion();
            }

            await AddTestData<ContentDbContext>(context => context.Files.UpdateRange(files));

            foreach (var dataSetVersion in dataSetVersions)
            {
                var dataSetVersionDirectory = _dataSetVersionPathResolver.DirectoryPath(dataSetVersion);

                Directory.CreateDirectory(dataSetVersionDirectory);
                await System.IO.File.Create(Path.Combine(dataSetVersionDirectory, "version1.txt")).DisposeAsync();
            }

            var response = await BulkDeleteDataSetVersions(releaseVersion.Id);

            response.AssertNoContent();

            await using var publicDataDbContext = GetDbContext<PublicDataDbContext>();
            await using var contentDataDbContext = GetDbContext<ContentDbContext>();

            // Assert that the base directory containing all parquet data file entries has been emptied
            var dataSetVersionBaseDirectoryEntries = Directory.GetFileSystemEntries(_dataSetVersionPathResolver.BasePath());
            Assert.Empty(dataSetVersionBaseDirectoryEntries);

            for (var i = 0; i < 3; i++)
            {
                var dataSet = dataSets[i];
                var dataSetVersion = dataSetVersions[i];
                var releaseFile = releaseFiles[i];

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
