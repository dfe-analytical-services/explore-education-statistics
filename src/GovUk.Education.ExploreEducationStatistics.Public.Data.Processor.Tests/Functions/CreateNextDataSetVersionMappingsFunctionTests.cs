using GovUk.Education.ExploreEducationStatistics.Common.Extensions;
using GovUk.Education.ExploreEducationStatistics.Common.Tests.Extensions;
using GovUk.Education.ExploreEducationStatistics.Content.Model;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Database;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Tests.Fixtures;
using GovUk.Education.ExploreEducationStatistics.Public.Data.Model;
using GovUk.Education.ExploreEducationStatistics.Public.Data.Model.Database;
using GovUk.Education.ExploreEducationStatistics.Public.Data.Model.Tests.Fixtures;
using GovUk.Education.ExploreEducationStatistics.Public.Data.Processor.Functions;
using GovUk.Education.ExploreEducationStatistics.Public.Data.Processor.Model;
using GovUk.Education.ExploreEducationStatistics.Public.Data.Processor.Requests;
using GovUk.Education.ExploreEducationStatistics.Public.Data.Processor.Requests.Validators;
using GovUk.Education.ExploreEducationStatistics.Public.Data.Processor.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.DurableTask;
using Microsoft.DurableTask.Client;
using Microsoft.EntityFrameworkCore;
using Moq;
using static GovUk.Education.ExploreEducationStatistics.Common.Tests.Utils.MockUtils;
using FileType = GovUk.Education.ExploreEducationStatistics.Common.Model.FileType;

namespace GovUk.Education.ExploreEducationStatistics.Public.Data.Processor.Tests.Functions;

public abstract class CreateNextDataSetVersionMappingsFunctionTests(
    ProcessorFunctionsIntegrationTestFixture fixture)
    : ProcessorFunctionsIntegrationTest(fixture)
{
    public class CreateNextDataSetVersionMappingsTests(
        ProcessorFunctionsIntegrationTestFixture fixture)
        : CreateNextDataSetVersionMappingsFunctionTests(fixture)
    {
        [Fact]
        public async Task Success()
        {
            var (dataSet, liveDataSetVersion) = await AddDataSetAndLatestLiveVersion();
            var (nextReleaseFile, _) = await AddDataAndMetadataFiles(dataSet.PublicationId);

            var durableTaskClientMock = new Mock<DurableTaskClient>(MockBehavior.Strict, "TestClient");

            ProcessDataSetVersionContext? processNextDataSetVersionContext = null;
            StartOrchestrationOptions? startOrchestrationOptions = null;
            durableTaskClientMock.Setup(client =>
                    client.ScheduleNewOrchestrationInstanceAsync(
                        nameof(ProcessNextDataSetVersionMappingsFunctionOrchestration
                            .ProcessNextDataSetVersionMappings),
                        It.IsAny<ProcessDataSetVersionContext>(),
                        It.IsAny<StartOrchestrationOptions>(),
                        It.IsAny<CancellationToken>()))
                .ReturnsAsync((TaskName _, object _, StartOrchestrationOptions? options, CancellationToken _) =>
                    options?.InstanceId ?? Guid.NewGuid().ToString())
                .Callback<TaskName, object, StartOrchestrationOptions?, CancellationToken>(
                    (_, input, options, _) =>
                    {
                        processNextDataSetVersionContext =
                            Assert.IsAssignableFrom<ProcessDataSetVersionContext>(input);
                        startOrchestrationOptions = options;
                    });

            var result = await CreateNextDataSetVersion(
                dataSetId: dataSet.Id,
                releaseFileId: nextReleaseFile.Id,
                durableTaskClientMock.Object);

            VerifyAllMocks(durableTaskClientMock);

            var responseViewModel = result.AssertOkObjectResult<ProcessDataSetVersionResponseViewModel>();

            await using var publicDataDbContext = GetDbContext<PublicDataDbContext>();

            // Assert only the original data set exists.
            var updatedDataSet = Assert.Single(await publicDataDbContext.DataSets
                .Include(ds => ds.Versions)
                .ThenInclude(dsv => dsv.Imports)
                .ToListAsync());

            // Assert that the existing data set is left in its Published state and that its
            // name, summary and other properties that were created during its first creation
            // are untouched.
            Assert.Equal(DataSetStatus.Published, updatedDataSet.Status);
            Assert.Equal(dataSet.Title, updatedDataSet.Title);
            Assert.Equal(dataSet.Summary, updatedDataSet.Summary);
            Assert.Equal(dataSet.PublicationId, updatedDataSet.PublicationId);

            // Assert that the new DataSetVersion is set as the latest draft version, and that the
            // prior DataSetVersion is still set as the latest live version. 
            Assert.NotNull(updatedDataSet.LatestDraftVersion);
            Assert.NotEqual(liveDataSetVersion.Id, updatedDataSet.LatestDraftVersion!.Id);
            Assert.Equal(liveDataSetVersion.Id, updatedDataSet.LatestLiveVersion!.Id);

            // Assert the data set has a new version and that it is the latest draft version.
            Assert.Equal(2, updatedDataSet.Versions.Count);
            var nextDataSetVersion = updatedDataSet
                .Versions
                .OrderBy(v => v.SemVersion())
                .Last();
            Assert.Equal(nextDataSetVersion, updatedDataSet.LatestDraftVersion);

            Assert.Equal(updatedDataSet.Id, nextDataSetVersion.DataSetId);
            Assert.Equal(DataSetVersionStatus.Processing, nextDataSetVersion.Status);
            Assert.Empty(nextDataSetVersion.Notes);
            Assert.Equal(1, nextDataSetVersion.VersionMajor);
            Assert.Equal(1, nextDataSetVersion.VersionMinor);

            Assert.Equal(nextReleaseFile.File.DataSetFileId, nextDataSetVersion.Release.DataSetFileId);
            Assert.Equal(nextReleaseFile.Id, nextDataSetVersion.Release.ReleaseFileId);
            Assert.Equal(nextReleaseFile.ReleaseVersion.Release.Slug, nextDataSetVersion.Release.Slug);
            Assert.Equal(nextReleaseFile.ReleaseVersion.Release.Title, nextDataSetVersion.Release.Title);

            // Assert a single import was created.
            var dataSetVersionImport = Assert.Single(nextDataSetVersion.Imports);
            Assert.Equal(nextDataSetVersion.Id, dataSetVersionImport.DataSetVersionId);
            Assert.NotEqual(Guid.Empty, dataSetVersionImport.InstanceId);
            Assert.Equal(DataSetVersionImportStage.Pending, dataSetVersionImport.Stage);

            // Assert the response view model values match the created data set version and import.
            Assert.Equal(updatedDataSet.Id, responseViewModel.DataSetId);
            Assert.Equal(nextDataSetVersion.Id, responseViewModel.DataSetVersionId);
            Assert.Equal(dataSetVersionImport.InstanceId, responseViewModel.InstanceId);

            // Assert the processing orchestrator was scheduled with the correct arguments
            Assert.NotNull(processNextDataSetVersionContext);
            Assert.NotNull(startOrchestrationOptions);
            Assert.Equal(new ProcessDataSetVersionContext { DataSetVersionId = nextDataSetVersion.Id },
                processNextDataSetVersionContext);
            Assert.Equal(new StartOrchestrationOptions { InstanceId = dataSetVersionImport.InstanceId.ToString() },
                startOrchestrationOptions);
        }

        [Fact]
        public async Task ReleaseFileIdIsEmpty_ReturnsValidationProblem()
        {
            var result = await CreateNextDataSetVersion(
                dataSetId: Guid.NewGuid(),
                releaseFileId: Guid.Empty);

            var validationProblem = result.AssertBadRequestWithValidationProblem();

            validationProblem.AssertHasNotEmptyError(
                nameof(NextDataSetVersionMappingsCreateRequest.ReleaseFileId).ToLowerFirst());
        }

        [Fact]
        public async Task DataSetIdIsEmpty_ReturnsValidationProblem()
        {
            var result = await CreateNextDataSetVersion(
                dataSetId: Guid.Empty,
                releaseFileId: Guid.NewGuid());

            var validationProblem = result.AssertBadRequestWithValidationProblem();

            validationProblem.AssertHasNotEmptyError(
                nameof(NextDataSetVersionMappingsCreateRequest.DataSetId).ToLowerFirst());
        }

        [Fact]
        public async Task DataSetIdIsNotFound_ReturnsValidationProblem()
        {
            var (releaseFile, _) = await AddDataAndMetadataFiles(Guid.NewGuid());

            var dataSetId = Guid.NewGuid();

            var result = await CreateNextDataSetVersion(
                dataSetId: dataSetId,
                releaseFileId: releaseFile.Id);

            result.AssertNotFoundWithValidationProblem<DataSet, Guid>(
                expectedId: dataSetId,
                expectedPath: nameof(NextDataSetVersionMappingsCreateRequest.DataSetId).ToLowerFirst());
        }

        [Fact]
        public async Task ReleaseFileIdIsNotFound_ReturnsValidationProblem()
        {
            var (dataSet, _) = await AddDataSetAndLatestLiveVersion();

            var releaseFileId = Guid.NewGuid();

            var result = await CreateNextDataSetVersion(
                dataSetId: dataSet.Id,
                releaseFileId: releaseFileId);

            result.AssertNotFoundWithValidationProblem<ReleaseFile, Guid>(
                expectedId: releaseFileId,
                expectedPath: nameof(DataSetCreateRequest.ReleaseFileId).ToLowerFirst());
        }

        [Fact]
        public async Task ReleaseFileIdHasDataSetVersion_ReturnsValidationProblem()
        {
            var (dataSet, _) = await AddDataSetAndLatestLiveVersion();

            ReleaseFile releaseFile = DataFixture.DefaultReleaseFile()
                .WithReleaseVersion(DataFixture.DefaultReleaseVersion()
                    .WithRelease(DataFixture.DefaultRelease()
                        .WithPublicationId(dataSet.PublicationId)))
                .WithFile(DataFixture.DefaultFile(FileType.Data));

            await AddTestData<ContentDbContext>(context =>
            {
                context.ReleaseFiles.Add(releaseFile);
            });

            // Create another DataSet and DataSetVersion which already references the ReleaseFile's Id.
            DataSet otherDataSet = DataFixture.DefaultDataSet();

            DataSetVersion otherDataSetVersion = DataFixture.DefaultDataSetVersion()
                .WithDataSet(otherDataSet)
                .WithRelease(DataFixture.DefaultDataSetVersionRelease()
                    .WithReleaseFileId(releaseFile.Id));

            await AddTestData<PublicDataDbContext>(context =>
            {
                context.DataSets.Add(otherDataSet);
                context.SaveChanges();
                context.DataSetVersions.Add(otherDataSetVersion);
            });

            var result = await CreateNextDataSetVersion(
                dataSetId: dataSet.Id,
                releaseFileId: releaseFile.Id);

            var validationProblem = result.AssertBadRequestWithValidationProblem();

            validationProblem.AssertHasError(
                expectedPath: nameof(NextDataSetVersionMappingsCreateRequest.ReleaseFileId).ToLowerFirst(),
                expectedCode: ValidationMessages.FileHasApiDataSetVersion.Code);
        }

        [Fact]
        public async Task ReleaseVersionNotDraft_ReturnsValidationProblem()
        {
            var (dataSet, _) = await AddDataSetAndLatestLiveVersion();

            var subjectId = Guid.NewGuid();

            var (releaseFile, releaseMetaFile) = DataFixture.DefaultReleaseFile()
                .WithReleaseVersion(DataFixture.DefaultReleaseVersion()
                    .WithRelease(DataFixture.DefaultRelease()
                        .WithPublicationId(dataSet.PublicationId))
                    .WithApprovalStatus(ReleaseApprovalStatus.Approved))
                .WithFiles([
                    DataFixture
                        .DefaultFile(FileType.Data)
                        .WithSubjectId(subjectId),
                    DataFixture
                        .DefaultFile(FileType.Metadata)
                        .WithSubjectId(subjectId)
                ])
                .GenerateTuple2();

            await AddTestData<ContentDbContext>(context =>
            {
                context.ReleaseFiles.AddRange(releaseFile, releaseMetaFile);
            });

            var result = await CreateNextDataSetVersion(
                dataSetId: dataSet.Id,
                releaseFileId: releaseFile.Id);

            var validationProblem = result.AssertBadRequestWithValidationProblem();

            validationProblem.AssertHasError(
                expectedPath: nameof(DataSetCreateRequest.ReleaseFileId).ToLowerFirst(),
                expectedCode: ValidationMessages.FileReleaseVersionNotDraft.Code
            );
        }

        [Fact]
        public async Task ReleaseFileTypeNotData_ReturnsValidationProblem()
        {
            var (dataSet, _) = await AddDataSetAndLatestLiveVersion();

            ReleaseFile releaseFile = DataFixture.DefaultReleaseFile()
                .WithReleaseVersion(DataFixture.DefaultReleaseVersion()
                    .WithRelease(DataFixture.DefaultRelease()
                        .WithPublicationId(dataSet.PublicationId)))
                .WithFile(DataFixture.DefaultFile(FileType.Ancillary));

            await AddTestData<ContentDbContext>(context =>
            {
                context.ReleaseFiles.Add(releaseFile);
            });

            var result = await CreateNextDataSetVersion(
                dataSetId: dataSet.Id,
                releaseFileId: releaseFile.Id);

            var validationProblem = result.AssertBadRequestWithValidationProblem();

            validationProblem.AssertHasError(
                expectedPath: nameof(DataSetCreateRequest.ReleaseFileId).ToLowerFirst(),
                expectedCode: ValidationMessages.FileTypeNotData.Code
            );
        }

        [Fact]
        public async Task ReleaseFileHasNoMetaFile_ReturnsValidationProblem()
        {
            var (dataSet, _) = await AddDataSetAndLatestLiveVersion();

            ReleaseFile releaseFile = DataFixture.DefaultReleaseFile()
                .WithReleaseVersion(DataFixture.DefaultReleaseVersion()
                    .WithRelease(DataFixture.DefaultRelease()
                        .WithPublicationId(dataSet.PublicationId)))
                .WithFile(DataFixture.DefaultFile(FileType.Data));

            await AddTestData<ContentDbContext>(context =>
            {
                context.ReleaseFiles.Add(releaseFile);
            });

            var result = await CreateNextDataSetVersion(
                dataSetId: dataSet.Id,
                releaseFileId: releaseFile.Id);

            var validationProblem = result.AssertBadRequestWithValidationProblem();

            validationProblem.AssertHasError(
                expectedPath: nameof(DataSetCreateRequest.ReleaseFileId).ToLowerFirst(),
                expectedCode: ValidationMessages.NoMetadataFile.Code
            );
        }

        [Fact]
        public async Task DataSetAndReleaseFileFromDifferentPublications_ReturnsValidationProblem()
        {
            var (dataSet, _) = await AddDataSetAndLatestLiveVersion();

            // Add ReleaseFiles for a different Publication.
            var (releaseFile, _) = await AddDataAndMetadataFiles(publicationId: Guid.NewGuid());

            var result = await CreateNextDataSetVersion(
                dataSetId: dataSet.Id,
                releaseFileId: releaseFile.Id);

            var validationProblem = result.AssertBadRequestWithValidationProblem();

            validationProblem.AssertHasError(
                expectedPath: nameof(NextDataSetVersionMappingsCreateRequest.ReleaseFileId).ToLowerFirst(),
                expectedCode: ValidationMessages.FileNotInDataSetPublication.Code
            );
        }

        [Fact]
        public async Task DataSetWithoutLiveVersion_ReturnsValidationProblem()
        {
            DataSet dataSet = DataFixture
                .DefaultDataSet()
                .WithStatusPublished();

            await AddTestData<PublicDataDbContext>(context => context.DataSets.Add(dataSet));

            var (releaseFile, _) = await AddDataAndMetadataFiles(dataSet.PublicationId);

            var result = await CreateNextDataSetVersion(
                dataSetId: dataSet.Id,
                releaseFileId: releaseFile.Id);

            var validationProblem = result.AssertBadRequestWithValidationProblem();

            validationProblem.AssertHasError(
                expectedPath: nameof(NextDataSetVersionMappingsCreateRequest.DataSetId).ToLowerFirst(),
                expectedCode: ValidationMessages.DataSetNoLiveVersion.Code
            );
        }

        [Fact]
        public async Task ReleaseFileInSameReleaseSeriesAsCurrentLiveVersion_ReturnsValidationProblem()
        {
            var (dataSet, liveDataSetVersion) = await AddDataSetAndLatestLiveVersion();

            // TODO EES-5405 Make sure liveDataSetVersion has release info with a ReleaseFileId
            var currentReleaseFile = GetDbContext<ContentDbContext>()
                .ReleaseFiles
                .Single(releaseFile => releaseFile.Id == liveDataSetVersion.Release.ReleaseFileId);

            var releaseVersion = await GetDbContext<ContentDbContext>()
                .ReleaseVersions
                .SingleAsync(releaseVersion => releaseVersion.Id == currentReleaseFile.ReleaseVersionId);

            ReleaseVersion releaseAmendment = DataFixture
                .DefaultReleaseVersion()
                .WithReleaseId(releaseVersion.ReleaseId);

            var subjectId = Guid.NewGuid();

            var (nextDataFile, nextMetaFile) = DataFixture
                .DefaultReleaseFile()
                .WithReleaseVersion(releaseAmendment)
                .WithFiles([
                    DataFixture
                        .DefaultFile(FileType.Data)
                        .WithSubjectId(subjectId),
                    DataFixture
                        .DefaultFile(FileType.Metadata)
                        .WithSubjectId(subjectId)
                ])
                .GenerateTuple2();

            await AddTestData<ContentDbContext>(context =>
            {
                context.ReleaseFiles.AddRange(nextDataFile, nextMetaFile);
            });

            var result = await CreateNextDataSetVersion(
                dataSetId: dataSet.Id,
                releaseFileId: nextDataFile.Id);

            var validationProblem = result.AssertBadRequestWithValidationProblem();

            validationProblem.AssertHasError(
                expectedPath: nameof(NextDataSetVersionMappingsCreateRequest.ReleaseFileId).ToLowerFirst(),
                expectedCode: ValidationMessages.FileMustBeInDifferentRelease.Code
            );
        }

        private async Task<(DataSet, DataSetVersion)> AddDataSetAndLatestLiveVersion()
        {
            DataSet dataSet = DataFixture
                .DefaultDataSet()
                .WithStatusPublished();

            await AddTestData<PublicDataDbContext>(context => context.DataSets.Add(dataSet));

            var dataSetVersion = await AddLatestLiveDataSetVersion(dataSet);
            return (dataSet, dataSetVersion);
        }

        private async Task<DataSetVersion> AddLatestLiveDataSetVersion(DataSet dataSet)
        {
            var (dataFile, _) = await AddDataAndMetadataFiles(dataSet.PublicationId);

            DataSetVersion liveDataSetVersion = DataFixture
                .DefaultDataSetVersion(filters: 1, indicators: 1, locations: 1, timePeriods: 2)
                .WithVersionNumber(1, 0)
                .WithStatusPublished()
                .WithDataSet(dataSet)
                .WithRelease(DataFixture.DefaultDataSetVersionRelease()
                    .WithReleaseFileId(dataFile.Id))
                .WithImports(() => DataFixture
                    .DefaultDataSetVersionImport()
                    .Generate(1))
                .FinishWith(dsv => dsv.DataSet.LatestLiveVersion = dsv);

            await AddTestData<PublicDataDbContext>(context =>
            {
                context.DataSetVersions.Add(liveDataSetVersion);
                context.DataSets.Update(dataSet);
            });

            return liveDataSetVersion;
        }

        private async Task<(ReleaseFile, ReleaseFile)> AddDataAndMetadataFiles(Guid publicationId)
        {
            var subjectId = Guid.NewGuid();

            var (dataFile, metaFile) = DataFixture
                .DefaultReleaseFile()
                .WithReleaseVersion(DataFixture.DefaultReleaseVersion()
                    .WithRelease(DataFixture.DefaultRelease()
                        .WithPublicationId(publicationId)))
                .WithFiles([
                    DataFixture
                        .DefaultFile(FileType.Data)
                        .WithSubjectId(subjectId),
                    DataFixture
                        .DefaultFile(FileType.Metadata)
                        .WithSubjectId(subjectId)
                ])
                .GenerateTuple2();

            await AddTestData<ContentDbContext>(context =>
                context.ReleaseFiles.AddRange(dataFile, metaFile));

            return (dataFile, metaFile);
        }

        private async Task<IActionResult> CreateNextDataSetVersion(
            Guid dataSetId,
            Guid releaseFileId,
            DurableTaskClient? durableTaskClient = null)
        {
            var function = GetRequiredService<CreateNextDataSetVersionMappingsFunction>();
            return await function.CreateNextDataSetVersionMappings(
                new NextDataSetVersionMappingsCreateRequest
                {
                    DataSetId = dataSetId,
                    ReleaseFileId = releaseFileId
                },
                durableTaskClient ?? new Mock<DurableTaskClient>(MockBehavior.Strict, "TestClient").Object,
                CancellationToken.None);
        }
    }
}
