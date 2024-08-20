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

public abstract class CreateDataSetFunctionTests(
    ProcessorFunctionsIntegrationTestFixture fixture)
    : ProcessorFunctionsIntegrationTest(fixture)
{
    public class CreateDataSetTests(
        ProcessorFunctionsIntegrationTestFixture fixture)
        : CreateDataSetFunctionTests(fixture)
    {
        [Fact]
        public async Task Success()
        {
            var subjectId = Guid.NewGuid();

            var (releaseFile, releaseMetaFile) = DataFixture.DefaultReleaseFile()
                .WithReleaseVersion(DataFixture.DefaultReleaseVersion()
                    .WithPublication(DataFixture.DefaultPublication()))
                .WithFiles([
                    DataFixture
                        .DefaultFile(FileType.Data)
                        .WithSubjectId(subjectId),
                    DataFixture
                        .DefaultFile(FileType.Metadata)
                        .WithSubjectId(subjectId)
                ])
                .GenerateList()
                .ToTuple2();

            await AddTestData<ContentDbContext>(context =>
            {
                context.ReleaseFiles.AddRange(releaseFile, releaseMetaFile);
            });

            var durableTaskClientMock = new Mock<DurableTaskClient>(MockBehavior.Strict, "TestClient");

            ProcessDataSetVersionContext? processInitialDataSetVersionContext = null;
            StartOrchestrationOptions? startOrchestrationOptions = null;
            durableTaskClientMock.Setup(client =>
                    client.ScheduleNewOrchestrationInstanceAsync(
                        nameof(ProcessInitialDataSetVersionFunction.ProcessInitialDataSetVersion),
                        It.IsAny<ProcessDataSetVersionContext>(),
                        It.IsAny<StartOrchestrationOptions>(),
                        It.IsAny<CancellationToken>()))
                .ReturnsAsync((TaskName _, object _, StartOrchestrationOptions? options, CancellationToken _) =>
                    options?.InstanceId ?? Guid.NewGuid().ToString())
                .Callback<TaskName, object, StartOrchestrationOptions?, CancellationToken>(
                    (_, input, options, _) =>
                    {
                        processInitialDataSetVersionContext =
                            Assert.IsAssignableFrom<ProcessDataSetVersionContext>(input);
                        startOrchestrationOptions = options;
                    });

            var result = await CreateDataSet(
                releaseFileId: releaseFile.Id,
                durableTaskClientMock.Object);

            VerifyAllMocks(durableTaskClientMock);

            var responseViewModel = result.AssertOkObjectResult<ProcessDataSetVersionResponseViewModel>();

            await using var publicDataDbContext = GetDbContext<PublicDataDbContext>();

            // Assert a single data set was created
            var dataSet = Assert.Single(await publicDataDbContext.DataSets
                .Include(ds => ds.Versions)
                .ThenInclude(dsv => dsv.Imports)
                .ToListAsync());

            Assert.Equal(DataSetStatus.Draft, dataSet.Status);
            Assert.Equal(releaseFile.Name, dataSet.Title);
            Assert.Equal(releaseFile.Summary, dataSet.Summary);
            Assert.Equal(releaseFile.ReleaseVersion.PublicationId, dataSet.PublicationId);
            Assert.Null(dataSet.LatestLiveVersion);

            // Assert the data set has a single version and that it is the latest draft version
            var dataSetVersion = Assert.Single(dataSet.Versions);
            Assert.Equal(dataSetVersion, dataSet.LatestDraftVersion);

            Assert.Equal(dataSet.Id, dataSetVersion.DataSetId);
            Assert.Equal(DataSetVersionStatus.Processing, dataSetVersion.Status);
            Assert.Empty(dataSetVersion.Notes);
            Assert.Equal(1, dataSetVersion.VersionMajor);
            Assert.Equal(0, dataSetVersion.VersionMinor);

            Assert.Equal(releaseFile.File.DataSetFileId, dataSetVersion.Release.DataSetFileId);
            Assert.Equal(releaseFile.Id, dataSetVersion.Release.ReleaseFileId);
            Assert.Equal(releaseFile.ReleaseVersion.Slug, dataSetVersion.Release.Slug);
            Assert.Equal(releaseFile.ReleaseVersion.Title, dataSetVersion.Release.Title);

            // Assert a single import was created
            var dataSetVersionImport = Assert.Single(dataSetVersion.Imports);

            Assert.Equal(dataSetVersion.Id, dataSetVersionImport.DataSetVersionId);
            Assert.NotEqual(Guid.Empty, dataSetVersionImport.InstanceId);
            Assert.Equal(DataSetVersionImportStage.Pending, dataSetVersionImport.Stage);

            // Assert the response view model values match the created data set version and import
            Assert.Equal(dataSet.Id, responseViewModel.DataSetId);
            Assert.Equal(dataSetVersion.Id, responseViewModel.DataSetVersionId);
            Assert.Equal(dataSetVersionImport.InstanceId, responseViewModel.InstanceId);

            // Assert the processing orchestrator was scheduled with the correct arguments
            Assert.NotNull(processInitialDataSetVersionContext);
            Assert.NotNull(startOrchestrationOptions);
            Assert.Equal(new ProcessDataSetVersionContext { DataSetVersionId = dataSetVersion.Id },
                processInitialDataSetVersionContext);
            Assert.Equal(new StartOrchestrationOptions { InstanceId = dataSetVersionImport.InstanceId.ToString() },
                startOrchestrationOptions);
        }

        [Fact]
        public async Task ReleaseFileIdIsEmpty_ReturnsValidationProblem()
        {
            var result = await CreateDataSet(releaseFileId: Guid.Empty);

            var validationProblem = result.AssertBadRequestWithValidationProblem();

            validationProblem.AssertHasNotEmptyError(
                nameof(DataSetCreateRequest.ReleaseFileId).ToLowerFirst());
        }

        [Fact]
        public async Task ReleaseFileIdIsNotFound_ReturnsValidationProblem()
        {
            var releaseFileId = Guid.NewGuid();

            var result = await CreateDataSet(releaseFileId: releaseFileId);

            result.AssertNotFoundWithValidationProblem<ReleaseFile, Guid>(
                expectedId: releaseFileId,
                expectedPath: nameof(DataSetCreateRequest.ReleaseFileId).ToLowerFirst());
        }

        [Fact]
        public async Task ReleaseFileIdHasDataSetVersion_ReturnsValidationProblem()
        {
            ReleaseFile releaseFile = DataFixture.DefaultReleaseFile()
                .WithReleaseVersion(DataFixture.DefaultReleaseVersion())
                .WithFile(DataFixture.DefaultFile(FileType.Data));

            DataSet dataSet = DataFixture.DefaultDataSet();

            DataSetVersion dataSetVersion = DataFixture.DefaultDataSetVersion()
                 .WithRelease(DataFixture.DefaultDataSetVersionRelease()
                     .WithReleaseFileId(releaseFile.Id))
                .WithDataSet(dataSet);

            await AddTestData<ContentDbContext>(context =>
            {
                context.ReleaseFiles.Add(releaseFile);
            });
            await AddTestData<PublicDataDbContext>(context =>
            {
                context.DataSets.Add(dataSet);
                context.SaveChanges();

                context.DataSetVersions.Add(dataSetVersion);
            });

            var result = await CreateDataSet(releaseFileId: releaseFile.Id);

            var validationProblem = result.AssertBadRequestWithValidationProblem();

            validationProblem.AssertHasError(
                expectedPath: nameof(DataSetCreateRequest.ReleaseFileId).ToLowerFirst(),
                expectedCode: ValidationMessages.FileHasApiDataSetVersion.Code);
        }

        [Fact]
        public async Task ReleaseVersionNotDraft_ReturnsValidationProblem()
        {
            var subjectId = Guid.NewGuid();

            var (releaseFile, releaseMetaFile) = DataFixture.DefaultReleaseFile()
                .WithReleaseVersion(DataFixture.DefaultReleaseVersion()
                    .WithApprovalStatus(ReleaseApprovalStatus.Approved))
                .WithFiles([
                    DataFixture
                        .DefaultFile(FileType.Data)
                        .WithSubjectId(subjectId),
                    DataFixture
                        .DefaultFile(FileType.Metadata)
                        .WithSubjectId(subjectId)
                ])
                .GenerateList()
                .ToTuple2();

            await AddTestData<ContentDbContext>(context =>
            {
                context.ReleaseFiles.AddRange(releaseFile, releaseMetaFile);
            });

            var result = await CreateDataSet(releaseFileId: releaseFile.Id);

            var validationProblem = result.AssertBadRequestWithValidationProblem();

            validationProblem.AssertHasError(
                expectedPath: nameof(DataSetCreateRequest.ReleaseFileId).ToLowerFirst(),
                expectedCode: ValidationMessages.FileReleaseVersionNotDraft.Code
            );
        }

        [Fact]
        public async Task ReleaseFileTypeNotData_ReturnsValidationProblem()
        {
            ReleaseFile releaseFile = DataFixture.DefaultReleaseFile()
                .WithReleaseVersion(DataFixture.DefaultReleaseVersion())
                .WithFile(DataFixture.DefaultFile(FileType.Ancillary));

            await AddTestData<ContentDbContext>(context =>
            {
                context.ReleaseFiles.Add(releaseFile);
            });

            var result = await CreateDataSet(releaseFileId: releaseFile.Id);

            var validationProblem = result.AssertBadRequestWithValidationProblem();

            validationProblem.AssertHasError(
                expectedPath: nameof(DataSetCreateRequest.ReleaseFileId).ToLowerFirst(),
                expectedCode: ValidationMessages.FileTypeNotData.Code
            );
        }

        [Fact]
        public async Task ReleaseFileHasNoMetaFile_ReturnsValidationProblem()
        {
            ReleaseFile releaseFile = DataFixture.DefaultReleaseFile()
                .WithReleaseVersion(DataFixture.DefaultReleaseVersion())
                .WithFile(DataFixture.DefaultFile(FileType.Data));

            await AddTestData<ContentDbContext>(context =>
            {
                context.ReleaseFiles.Add(releaseFile);
            });

            var result = await CreateDataSet(releaseFileId: releaseFile.Id);

            var validationProblem = result.AssertBadRequestWithValidationProblem();

            validationProblem.AssertHasError(
                expectedPath: nameof(DataSetCreateRequest.ReleaseFileId).ToLowerFirst(),
                expectedCode: ValidationMessages.NoMetadataFile.Code
            );
        }

        private async Task<IActionResult> CreateDataSet(
            Guid releaseFileId,
            DurableTaskClient? durableTaskClient = null)
        {
            var function = GetRequiredService<CreateDataSetFunction>();
            return await function.CreateDataSet(new DataSetCreateRequest { ReleaseFileId = releaseFileId },
                durableTaskClient ?? new Mock<DurableTaskClient>(MockBehavior.Strict, "TestClient").Object,
                CancellationToken.None);
        }
    }
}
