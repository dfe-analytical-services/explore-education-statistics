using GovUk.Education.ExploreEducationStatistics.Common.Extensions;
using GovUk.Education.ExploreEducationStatistics.Common.IntegrationTests;
using GovUk.Education.ExploreEducationStatistics.Common.IntegrationTests.FunctionApp;
using GovUk.Education.ExploreEducationStatistics.Common.Tests.Extensions;
using GovUk.Education.ExploreEducationStatistics.Common.Tests.Fixtures;
using GovUk.Education.ExploreEducationStatistics.Common.Utils;
using GovUk.Education.ExploreEducationStatistics.Content.Model;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Tests.Fixtures;
using GovUk.Education.ExploreEducationStatistics.Public.Data.Model;
using GovUk.Education.ExploreEducationStatistics.Public.Data.Model.Tests.Fixtures;
using GovUk.Education.ExploreEducationStatistics.Public.Data.Processor.Functions;
using GovUk.Education.ExploreEducationStatistics.Public.Data.Processor.Model;
using GovUk.Education.ExploreEducationStatistics.Public.Data.Processor.Requests;
using GovUk.Education.ExploreEducationStatistics.Public.Data.Processor.Tests.Fixture;
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

#pragma warning disable CS9107 // Parameter is captured into the state of the enclosing type and its value is also passed to the base constructor. The value might be captured by the base class as well.

namespace GovUk.Education.ExploreEducationStatistics.Public.Data.Processor.Tests.Functions;

// ReSharper disable once ClassNeverInstantiated.Global
public class CompleteNextDataSetVersionImportFunctionTestsFixture()
    : OptimisedPublicDataProcessorCollectionFixture(
        capabilities: [PublicDataProcessorIntegrationTestCapability.Postgres]
    )
{
    public CompleteNextDataSetVersionImportFunction Function = null!;

    protected override async Task AfterFactoryConstructed(OptimisedServiceCollectionLookups lookups)
    {
        await base.AfterFactoryConstructed(lookups);

        Function = lookups.GetService<CompleteNextDataSetVersionImportFunction>();
    }
}

[CollectionDefinition(nameof(CompleteNextDataSetVersionImportFunctionTestsFixture))]
public class CompleteNextDataSetVersionImportFunctionTestsCollection
    : ICollectionFixture<CompleteNextDataSetVersionImportFunctionTestsFixture>;

[Collection(nameof(CompleteNextDataSetVersionImportFunctionTestsFixture))]
public abstract class CompleteNextDataSetVersionImportFunctionTests(
    CompleteNextDataSetVersionImportFunctionTestsFixture fixture
) : OptimisedFunctionAppIntegrationTestBase(fixture)
{
    private static readonly DataFixture DataFixture = new();

    public class CompleteNextDataSetVersionImportTests(CompleteNextDataSetVersionImportFunctionTestsFixture fixture)
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

            var originalDataSetVersionImport = fixture
                .GetPublicDataDbContext()
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
            var updatedDataSetVersionImport = fixture
                .GetPublicDataDbContext()
                .DataSetVersionImports.Single(import => import.DataSetVersionId == nextVersion.Id);

            Assert.Equal(originalDataSetVersionImport.Id, updatedDataSetVersionImport.Id);

            var updatedDataSetVersion = await fixture
                .GetPublicDataDbContext()
                .DataSetVersions.SingleAsync(dsv => dsv.Id == nextVersion.Id);

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

            // Update the next data set version's status to no longer be "Mapping".
            var nextVersionWithIncorrectStatus = await fixture
                .GetPublicDataDbContext()
                .DataSetVersions.SingleAsync(dsv => dsv.Id == nextVersion.Id);

            nextVersionWithIncorrectStatus.Status = status;

            await fixture.GetPublicDataDbContext().SaveChangesAsync();

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

            // Remove the DataSetVersionMapping entry from the database.
            var mapping = await fixture
                .GetPublicDataDbContext()
                .DataSetVersionMappings.SingleAsync(m => m.TargetDataSetVersionId == nextVersion.Id);
            fixture.GetPublicDataDbContext().Remove(mapping);
            await fixture.GetPublicDataDbContext().SaveChangesAsync();

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

            // Remove the DataSetVersionMapping entry from the database.
            var mapping = await fixture
                .GetPublicDataDbContext()
                .DataSetVersionMappings.SingleAsync(m => m.TargetDataSetVersionId == nextVersion.Id);

            mapping.LocationMappingsComplete = false;

            await fixture.GetPublicDataDbContext().SaveChangesAsync();

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

            // Remove the DataSetVersionMapping entry from the database.
            var mapping = await fixture
                .GetPublicDataDbContext()
                .DataSetVersionMappings.SingleAsync(m => m.TargetDataSetVersionId == nextVersion.Id);

            mapping.FilterMappingsComplete = false;

            await fixture.GetPublicDataDbContext().SaveChangesAsync();

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

            // Update the next data set version's import status to no longer be "ManualMapping".
            var importWithIncorrectStatus = await fixture
                .GetPublicDataDbContext()
                .DataSetVersionImports.SingleAsync(import => import.DataSetVersionId == nextVersion.Id);

            importWithIncorrectStatus.Stage = importStage;

            await fixture.GetPublicDataDbContext().SaveChangesAsync();

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

            await fixture.GetPublicDataDbContext().AddTestData(context => context.DataSets.Add(dataSet));

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

            await fixture
                .GetPublicDataDbContext()
                .AddTestData(context =>
                {
                    context.DataSetVersions.Add(liveDataSetVersion);
                    context.DataSets.Update(dataSet);
                });

            return liveDataSetVersion;
        }

        private async Task<DataSetVersion> AddNextDataSetVersionAndMapping(Guid dataSetId)
        {
            var dataSet = await fixture.GetPublicDataDbContext().DataSets.SingleAsync(ds => ds.Id == dataSetId);

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

            await fixture
                .GetPublicDataDbContext()
                .AddTestData(context =>
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

            await fixture
                .GetContentDbContext()
                .AddTestData(context => context.ReleaseFiles.AddRange(dataFile, metaFile));

            return (dataFile, metaFile);
        }

        private async Task<IActionResult> CompleteNextDataSetVersionImport(
            Guid dataSetVersionId,
            DurableTaskClient? durableTaskClient = null
        )
        {
            return await fixture.Function.CompleteNextDataSetVersionImport(
                new NextDataSetVersionCompleteImportRequest { DataSetVersionId = dataSetVersionId },
                durableTaskClient ?? new Mock<DurableTaskClient>(MockBehavior.Strict, "TestClient").Object,
                CancellationToken.None
            );
        }
    }
}
