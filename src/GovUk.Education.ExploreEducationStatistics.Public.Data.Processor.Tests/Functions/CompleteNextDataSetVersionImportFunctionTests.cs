using GovUk.Education.ExploreEducationStatistics.Common.Extensions;
using GovUk.Education.ExploreEducationStatistics.Common.Tests.Extensions;
using GovUk.Education.ExploreEducationStatistics.Common.Utils;
using GovUk.Education.ExploreEducationStatistics.Content.Model;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Database;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Tests.Fixtures;
using GovUk.Education.ExploreEducationStatistics.Public.Data.Model;
using GovUk.Education.ExploreEducationStatistics.Public.Data.Model.Database;
using GovUk.Education.ExploreEducationStatistics.Public.Data.Model.Tests.Fixtures;
using GovUk.Education.ExploreEducationStatistics.Public.Data.Processor.Functions;
using GovUk.Education.ExploreEducationStatistics.Public.Data.Processor.Model;
using GovUk.Education.ExploreEducationStatistics.Public.Data.Processor.Requests;
using GovUk.Education.ExploreEducationStatistics.Public.Data.Processor.Tests.TheoryData;
using GovUk.Education.ExploreEducationStatistics.Public.Data.Processor.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.DurableTask;
using Microsoft.DurableTask.Client;
using Microsoft.EntityFrameworkCore;
using Moq;
using static GovUk.Education.ExploreEducationStatistics.Common.Tests.Utils.MockUtils;
using FileType = GovUk.Education.ExploreEducationStatistics.Common.Model.FileType;
using ValidationMessages = GovUk.Education.ExploreEducationStatistics.Public.Data.Processor.Requests.Validators.ValidationMessages;

namespace GovUk.Education.ExploreEducationStatistics.Public.Data.Processor.Tests.Functions;

public abstract class CompleteNextDataSetVersionImportFunctionTests(ProcessorFunctionsIntegrationTestFixture fixture)
    : ProcessorFunctionsIntegrationTest(fixture)
{
    public class CompleteNextDataSetVersionImportTests(ProcessorFunctionsIntegrationTestFixture fixture)
        : CompleteNextDataSetVersionImportFunctionTests(fixture)
    {
        public static TheoryData<DataSetVersionImportStage> NonManualMappingStages = new(
            EnumUtil.GetEnums<DataSetVersionImportStage>().Except([DataSetVersionImportStage.ManualMapping])
        );

        [Fact]
        public async Task Success()
        {
            var (_, _, nextVersion) = await AddDataSetAndLatestLiveAndNextVersion();

            var durableTaskClientMock = new Mock<DurableTaskClient>(MockBehavior.Strict, "TestClient");

            ProcessDataSetVersionContext? processNextDataSetVersionContext = null;
            StartOrchestrationOptions? startOrchestrationOptions = null;
            durableTaskClientMock
                .Setup(client =>
                    client.ScheduleNewOrchestrationInstanceAsync(
                        nameof(
                            ProcessCompletionOfNextDataSetVersionOrchestration.ProcessCompletionOfNextDataSetVersionImport
                        ),
                        It.IsAny<ProcessDataSetVersionContext>(),
                        It.IsAny<StartOrchestrationOptions>(),
                        It.IsAny<CancellationToken>()
                    )
                )
                .ReturnsAsync(
                    (TaskName _, object _, StartOrchestrationOptions? options, CancellationToken _) =>
                        options?.InstanceId ?? Guid.NewGuid().ToString()
                )
                .Callback<TaskName, object, StartOrchestrationOptions?, CancellationToken>(
                    (_, input, options, _) =>
                    {
                        processNextDataSetVersionContext = Assert.IsAssignableFrom<ProcessDataSetVersionContext>(input);
                        startOrchestrationOptions = options;
                    }
                );

            await using var publicDataDbContext = GetDbContext<PublicDataDbContext>();

            var originalDataSetVersionImport = publicDataDbContext
                .DataSetVersionImports.AsNoTracking()
                .Single(import => import.DataSetVersionId == nextVersion.Id);

            var result = await CompleteNextDataSetVersionImport(
                dataSetVersionId: nextVersion.Id,
                durableTaskClientMock.Object
            );

            VerifyAllMocks(durableTaskClientMock);

            var responseViewModel = result.AssertOkObjectResult<ProcessDataSetVersionResponseViewModel>();

            Assert.Equal(nextVersion.Id, responseViewModel.DataSetVersionId);
            Assert.Equal(nextVersion.DataSetId, responseViewModel.DataSetId);

            // Assert that the pre-existing import entry for the next data set version is re-used.
            var updatedDataSetVersionImport = publicDataDbContext.DataSetVersionImports.Single(import =>
                import.DataSetVersionId == nextVersion.Id
            );

            Assert.Equal(originalDataSetVersionImport.Id, updatedDataSetVersionImport.Id);

            var updatedDataSetVersion = await publicDataDbContext.DataSetVersions.SingleAsync(dsv =>
                dsv.Id == nextVersion.Id
            );

            // Assert the updated data set version is set to status 'Finalising'.
            Assert.Equal(DataSetVersionStatus.Finalising, updatedDataSetVersion.Status);

            // Assert that the InstanceId has been set to a new unique value.
            Assert.NotEqual(originalDataSetVersionImport.InstanceId, updatedDataSetVersionImport.InstanceId);

            // Assert that the InstanceId returned in the view model is the new one rather than the original.
            Assert.Equal(updatedDataSetVersionImport.InstanceId, responseViewModel.InstanceId);

            // Assert the processing orchestrator was scheduled with the correct arguments
            Assert.NotNull(processNextDataSetVersionContext);
            Assert.NotNull(startOrchestrationOptions);
            Assert.Equal(
                new ProcessDataSetVersionContext { DataSetVersionId = nextVersion.Id },
                processNextDataSetVersionContext
            );
            Assert.Equal(
                new StartOrchestrationOptions { InstanceId = updatedDataSetVersionImport.InstanceId.ToString() },
                startOrchestrationOptions
            );
        }

        [Fact]
        public async Task DataSetVersionIdIsEmpty_ReturnsValidationProblem()
        {
            var result = await CompleteNextDataSetVersionImport(dataSetVersionId: Guid.Empty);

            var validationProblem = result.AssertBadRequestWithValidationProblem();

            validationProblem.AssertHasNotEmptyError(
                nameof(NextDataSetVersionCompleteImportRequest.DataSetVersionId).ToLowerFirst()
            );
        }

        [Fact]
        public async Task DataSetVersionIsNotFound_ReturnsNotFound()
        {
            var dataSetVersionId = Guid.NewGuid();

            var result = await CompleteNextDataSetVersionImport(dataSetVersionId: dataSetVersionId);

            result.AssertNotFoundWithValidationProblem<DataSetVersion, Guid>(
                expectedId: dataSetVersionId,
                expectedPath: nameof(NextDataSetVersionCompleteImportRequest.DataSetVersionId).ToLowerFirst()
            );
        }

        [Theory]
        [MemberData(
            nameof(DataSetVersionStatusTheoryData.StatusesExceptMapping),
            MemberType = typeof(DataSetVersionStatusTheoryData)
        )]
        public async Task DataSetVersionNotInMappingStatus_ReturnsValidationProblem(DataSetVersionStatus status)
        {
            var (_, _, nextVersion) = await AddDataSetAndLatestLiveAndNextVersion();

            await using var publicDataDbContext = GetDbContext<PublicDataDbContext>();

            // Update the next data set version's status to no longer be "Mapping".
            var nextVersionWithIncorrectStatus = await publicDataDbContext.DataSetVersions.SingleAsync(dsv =>
                dsv.Id == nextVersion.Id
            );

            nextVersionWithIncorrectStatus.Status = status;

            await publicDataDbContext.SaveChangesAsync();

            var result = await CompleteNextDataSetVersionImport(dataSetVersionId: nextVersion.Id);

            var validationProblem = result.AssertBadRequestWithValidationProblem();

            validationProblem.AssertHasError(
                expectedPath: nameof(NextDataSetVersionCompleteImportRequest.DataSetVersionId).ToLowerFirst(),
                expectedCode: ValidationMessages.DataSetVersionNotInMappingStatus.Code
            );
        }

        [Fact]
        public async Task DataSetVersionMappingNotFound_ReturnsValidationProblem()
        {
            var (_, _, nextVersion) = await AddDataSetAndLatestLiveAndNextVersion();

            await using var publicDataDbContext = GetDbContext<PublicDataDbContext>();

            // Remove the DataSetVersionMapping entry from the database.
            var mapping = await publicDataDbContext.DataSetVersionMappings.SingleAsync();
            publicDataDbContext.Remove(mapping);
            await publicDataDbContext.SaveChangesAsync();

            var result = await CompleteNextDataSetVersionImport(dataSetVersionId: nextVersion.Id);

            var validationProblem = result.AssertBadRequestWithValidationProblem();

            validationProblem.AssertHasError(
                expectedPath: nameof(NextDataSetVersionCompleteImportRequest.DataSetVersionId).ToLowerFirst(),
                expectedCode: ValidationMessages.DataSetVersionMappingNotFound.Code
            );
        }

        [Fact]
        public async Task DataSetVersionMappings_LocationsNotComplete_ReturnsValidationProblem()
        {
            var (_, _, nextVersion) = await AddDataSetAndLatestLiveAndNextVersion();

            await using var publicDataDbContext = GetDbContext<PublicDataDbContext>();

            // Remove the DataSetVersionMapping entry from the database.
            var mapping = await publicDataDbContext.DataSetVersionMappings.SingleAsync();

            mapping.LocationMappingsComplete = false;

            await publicDataDbContext.SaveChangesAsync();

            var result = await CompleteNextDataSetVersionImport(dataSetVersionId: nextVersion.Id);

            var validationProblem = result.AssertBadRequestWithValidationProblem();

            validationProblem.AssertHasError(
                expectedPath: nameof(NextDataSetVersionCompleteImportRequest.DataSetVersionId).ToLowerFirst(),
                expectedCode: ValidationMessages.DataSetVersionMappingsNotComplete.Code
            );
        }

        [Fact]
        public async Task DataSetVersionMappings_FilterOptionsNotComplete_ReturnsValidationProblem()
        {
            var (_, _, nextVersion) = await AddDataSetAndLatestLiveAndNextVersion();

            await using var publicDataDbContext = GetDbContext<PublicDataDbContext>();

            // Remove the DataSetVersionMapping entry from the database.
            var mapping = await publicDataDbContext.DataSetVersionMappings.SingleAsync();

            mapping.FilterMappingsComplete = false;

            await publicDataDbContext.SaveChangesAsync();

            var result = await CompleteNextDataSetVersionImport(dataSetVersionId: nextVersion.Id);

            var validationProblem = result.AssertBadRequestWithValidationProblem();

            validationProblem.AssertHasError(
                expectedPath: nameof(NextDataSetVersionCompleteImportRequest.DataSetVersionId).ToLowerFirst(),
                expectedCode: ValidationMessages.DataSetVersionMappingsNotComplete.Code
            );
        }

        [Theory]
        [MemberData(nameof(NonManualMappingStages))]
        public async Task DataSetVersionImportInManualMappingStateNotFound_ReturnsValidationProblem(
            DataSetVersionImportStage importStage
        )
        {
            var (_, _, nextVersion) = await AddDataSetAndLatestLiveAndNextVersion();

            await using var publicDataDbContext = GetDbContext<PublicDataDbContext>();

            // Update the next data set version's import status to no longer be "ManualMapping".
            var importWithIncorrectStatus = await publicDataDbContext.DataSetVersionImports.SingleAsync(import =>
                import.DataSetVersionId == nextVersion.Id
            );

            importWithIncorrectStatus.Stage = importStage;

            await publicDataDbContext.SaveChangesAsync();

            var result = await CompleteNextDataSetVersionImport(dataSetVersionId: nextVersion.Id);

            var validationProblem = result.AssertBadRequestWithValidationProblem();

            validationProblem.AssertHasError(
                expectedPath: nameof(NextDataSetVersionCompleteImportRequest.DataSetVersionId).ToLowerFirst(),
                expectedCode: ValidationMessages.ImportInManualMappingStateNotFound.Code
            );
        }

        private async Task<(DataSet, DataSetVersion, DataSetVersion)> AddDataSetAndLatestLiveAndNextVersion()
        {
            DataSet dataSet = DataFixture.DefaultDataSet().WithStatusPublished();

            await AddTestData<PublicDataDbContext>(context => context.DataSets.Add(dataSet));

            var liveVersion = await AddLatestLiveDataSetVersion(dataSet);
            var nextVersion = await AddNextDataSetVersionAndMapping(dataSet.Id);
            return (dataSet, liveVersion, nextVersion);
        }

        private async Task<DataSetVersion> AddLatestLiveDataSetVersion(DataSet dataSet)
        {
            var (dataFile, _) = await AddDataAndMetadataFiles(dataSet.PublicationId);

            DataSetVersion liveDataSetVersion = DataFixture
                .DefaultDataSetVersion(filters: 1, indicators: 1, locations: 1, timePeriods: 2)
                .WithVersionNumber(1, 0)
                .WithStatusPublished()
                .WithDataSet(dataSet)
                .WithRelease(DataFixture.DefaultDataSetVersionRelease().WithReleaseFileId(dataFile.Id))
                .WithImports(() => DataFixture.DefaultDataSetVersionImport().Generate(1))
                .FinishWith(dsv => dsv.DataSet.LatestLiveVersion = dsv);

            await AddTestData<PublicDataDbContext>(context =>
            {
                context.DataSetVersions.Add(liveDataSetVersion);
                context.DataSets.Update(dataSet);
            });

            return liveDataSetVersion;
        }

        private async Task<DataSetVersion> AddNextDataSetVersionAndMapping(Guid dataSetId)
        {
            await using var publicDataDbContext = GetDbContext<PublicDataDbContext>();

            var dataSet = await publicDataDbContext.DataSets.SingleAsync(ds => ds.Id == dataSetId);

            var (dataFile, _) = await AddDataAndMetadataFiles(dataSet.PublicationId);

            DataSetVersion nextDataSetVersion = DataFixture
                .DefaultDataSetVersion(filters: 1, indicators: 1, locations: 1, timePeriods: 2)
                .WithVersionNumber(1, 1)
                .WithStatusMapping()
                .WithDataSet(dataSet)
                .WithRelease(DataFixture.DefaultDataSetVersionRelease().WithReleaseFileId(dataFile.Id))
                .WithImports(() =>
                    DataFixture
                        .DefaultDataSetVersionImport()
                        .WithStage(DataSetVersionImportStage.ManualMapping)
                        .Generate(1)
                )
                .FinishWith(dsv => dsv.DataSet.LatestDraftVersion = dsv);

            var dataSetVersionMapping = new DataSetVersionMapping
            {
                SourceDataSetVersionId = dataSet.LatestLiveVersionId!.Value,
                TargetDataSetVersionId = nextDataSetVersion.Id,
                FilterMappingPlan = new FilterMappingPlan(),
                LocationMappingPlan = new LocationMappingPlan(),
                LocationMappingsComplete = true,
                FilterMappingsComplete = true,
            };

            await AddTestData<PublicDataDbContext>(context =>
            {
                context.DataSetVersions.Add(nextDataSetVersion);
                context.DataSets.Update(dataSet);
                context.DataSetVersionMappings.Add(dataSetVersionMapping);
            });

            return nextDataSetVersion;
        }

        private async Task<(ReleaseFile, ReleaseFile)> AddDataAndMetadataFiles(Guid publicationId)
        {
            var subjectId = Guid.NewGuid();

            var (dataFile, metaFile) = DataFixture
                .DefaultReleaseFile()
                .WithReleaseVersion(
                    DataFixture
                        .DefaultReleaseVersion()
                        .WithRelease(DataFixture.DefaultRelease().WithPublicationId(publicationId))
                )
                .WithFiles([
                    DataFixture.DefaultFile(FileType.Data).WithSubjectId(subjectId),
                    DataFixture.DefaultFile(FileType.Metadata).WithSubjectId(subjectId),
                ])
                .GenerateTuple2();

            await AddTestData<ContentDbContext>(context => context.ReleaseFiles.AddRange(dataFile, metaFile));

            return (dataFile, metaFile);
        }

        private async Task<IActionResult> CompleteNextDataSetVersionImport(
            Guid dataSetVersionId,
            DurableTaskClient? durableTaskClient = null
        )
        {
            var function = GetRequiredService<CompleteNextDataSetVersionImportFunction>();
            return await function.CompleteNextDataSetVersionImport(
                new NextDataSetVersionCompleteImportRequest { DataSetVersionId = dataSetVersionId },
                durableTaskClient ?? new Mock<DurableTaskClient>(MockBehavior.Strict, "TestClient").Object,
                CancellationToken.None
            );
        }
    }
}
