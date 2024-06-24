using GovUk.Education.ExploreEducationStatistics.Common.Model.Data;
using GovUk.Education.ExploreEducationStatistics.Common.Tests.Extensions;
using GovUk.Education.ExploreEducationStatistics.Public.Data.Model;
using GovUk.Education.ExploreEducationStatistics.Public.Data.Model.Database;
using GovUk.Education.ExploreEducationStatistics.Public.Data.Model.Tests.Fixtures;
using GovUk.Education.ExploreEducationStatistics.Public.Data.Processor.Functions;
using GovUk.Education.ExploreEducationStatistics.Public.Data.Processor.Model;
using GovUk.Education.ExploreEducationStatistics.Public.Data.Services.Interfaces;
using Microsoft.DurableTask;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using Moq.Protected;
using static GovUk.Education.ExploreEducationStatistics.Common.Tests.Utils.MockUtils;

namespace GovUk.Education.ExploreEducationStatistics.Public.Data.Processor.Tests.Functions;

public abstract class ProcessNextDataSetVersionFunctionTests(
    ProcessorFunctionsIntegrationTestFixture fixture)
    : ProcessorFunctionsIntegrationTest(fixture)
{
    public class ProcessNextDataSetVersionTests(
        ProcessorFunctionsIntegrationTestFixture fixture)
        : ProcessNextDataSetVersionFunctionTests(fixture)
    {
        [Fact]
        public async Task Success()
        {
            var mockOrchestrationContext = DefaultMockOrchestrationContext();
            var activitySequence = new MockSequence();

            string[] expectedActivitySequence =
            [
                ActivityNames.CopyCsvFiles,
                ActivityNames.CreateMappings,
                ActivityNames.CompleteNextDataSetVersionMappingProcessing,
            ];

            foreach (var activityName in expectedActivitySequence)
            {
                mockOrchestrationContext
                    .InSequence(activitySequence)
                    .Setup(context => context.CallActivityAsync(activityName,
                        mockOrchestrationContext.Object.InstanceId,
                        null))
                    .Returns(Task.CompletedTask);
            }

            await ProcessNextDataSetVersion(mockOrchestrationContext.Object);

            VerifyAllMocks(mockOrchestrationContext);
        }

        [Fact]
        public async Task ActivityFunctionThrowsException_CallsHandleFailureActivity()
        {
            var mockOrchestrationContext = DefaultMockOrchestrationContext();

            var activitySequence = new MockSequence();

            mockOrchestrationContext
                .InSequence(activitySequence)
                .Setup(context =>
                    context.CallActivityAsync(ActivityNames.CopyCsvFiles,
                        mockOrchestrationContext.Object.InstanceId,
                        null))
                .Throws<Exception>();

            mockOrchestrationContext
                .InSequence(activitySequence)
                .Setup(context =>
                    context.CallActivityAsync(ActivityNames.HandleProcessingFailure,
                        mockOrchestrationContext.Object.InstanceId,
                        null))
                .Returns(Task.CompletedTask);

            await ProcessNextDataSetVersion(mockOrchestrationContext.Object);

            VerifyAllMocks(mockOrchestrationContext);
        }

        private async Task ProcessNextDataSetVersion(TaskOrchestrationContext orchestrationContext)
        {
            var function = GetRequiredService<ProcessNextDataSetVersionFunction>();
            await function.ProcessNextDataSetVersion(
                orchestrationContext,
                new ProcessDataSetVersionContext { DataSetVersionId = Guid.NewGuid() });
        }

        private static Mock<TaskOrchestrationContext> DefaultMockOrchestrationContext(
            Guid? instanceId = null,
            bool isReplaying = false)
        {
            var mock = new Mock<TaskOrchestrationContext>(MockBehavior.Strict);

            mock
                .Protected()
                .Setup<ILoggerFactory>("LoggerFactory")
                .Returns(NullLoggerFactory.Instance);

            mock
                .SetupGet(context => context.InstanceId)
                .Returns(instanceId?.ToString() ?? Guid.NewGuid().ToString());

            mock
                .SetupGet(context => context.IsReplaying)
                .Returns(isReplaying);

            return mock;
        }
    }

    public class CreateMappingsProcessingTests(
        ProcessorFunctionsIntegrationTestFixture fixture)
        : ProcessInitialDataSetVersionFunctionTests(fixture)
    {
        private const DataSetVersionImportStage Stage = DataSetVersionImportStage.ImportingMetadata;

        [Fact]
        public async Task Success()
        {
            var (initialDataSetVersion, _) = await CreateDataSet(DataSetVersionImportStage.Completing);

            var dataSet = await GetDbContext<PublicDataDbContext>().DataSets.SingleAsync(dataSet =>
                dataSet.Id == initialDataSetVersion.DataSet.Id);

            var (nextDataSetVersion, instanceId) = await CreateDataSetVersionAndImport(
                dataSet: dataSet,
                importStage: Stage.PreviousStage(),
                versionMajor: 1,
                versionMinor: 1);

            SetupCsvDataFilesForDataSetVersion(ProcessorTestData.AbsenceSchool, nextDataSetVersion);

            await CreateMappings(instanceId);

            var updatedDataSetVersion = GetDbContext<PublicDataDbContext>()
                .DataSetVersions
                .Single(dsv => dsv.Id == nextDataSetVersion.Id);

            // Assert that the MetaSummary has been generated correctly from the CSV.
            var metaSummary = updatedDataSetVersion.MetaSummary;
            Assert.NotNull(metaSummary);

            Assert.Equal(
                ProcessorTestData
                    .AbsenceSchool
                    .ExpectedGeographicLevels
                    .Order(),
                metaSummary.GeographicLevels.Order());

            Assert.Equal(
                ProcessorTestData
                    .AbsenceSchool
                    .ExpectedFilters
                    .Select(f => f.Label)
                    .Order(),
                metaSummary.Filters.Order());

            Assert.Equal(
                ProcessorTestData
                    .AbsenceSchool
                    .ExpectedIndicators
                    .Select(i => i.Label)
                    .Order(),
                metaSummary.Indicators.Order());

            Assert.Equal(
                new TimePeriodRange
                {
                    Start = TimePeriodRangeBound.Create(ProcessorTestData.AbsenceSchool.ExpectedTimePeriods[0]),
                    End = TimePeriodRangeBound.Create(ProcessorTestData.AbsenceSchool.ExpectedTimePeriods[^1])
                },
                metaSummary.TimePeriodRange);

            // TODO EES-4945 - implement checks for successful extraction of metadata.
        }

        private async Task CreateMappings(Guid instanceId)
        {
            var function = GetRequiredService<ProcessNextDataSetVersionFunction>();
            await function.CreateMappings(instanceId, CancellationToken.None);
        }
    }

    public class CompleteNextDataSetVersionMappingProcessingTests(
        ProcessorFunctionsIntegrationTestFixture fixture)
        : ProcessNextDataSetVersionFunctionTests(fixture)
    {
        private const DataSetVersionImportStage Stage = DataSetVersionImportStage.Completing;

        [Fact]
        public async Task Success()
        {
            var (initialDataSetVersion, _) = await CreateDataSet(DataSetVersionImportStage.Completing);

            var dataSet = await GetDbContext<PublicDataDbContext>().DataSets.SingleAsync(dataSet =>
                dataSet.Id == initialDataSetVersion.DataSet.Id);

            var (_, instanceId) = await CreateDataSetVersionAndImport(
                dataSet: dataSet,
                importStage: Stage.PreviousStage(),
                versionMajor: 1,
                versionMinor: 1);

            await CompleteProcessing(instanceId);

            await using var publicDataDbContext = GetDbContext<PublicDataDbContext>();

            var savedImport = await publicDataDbContext.DataSetVersionImports
                .Include(i => i.DataSetVersion)
                .SingleAsync(i => i.InstanceId == instanceId);

            Assert.Equal(Stage, savedImport.Stage);
            savedImport.Completed.AssertUtcNow();

            Assert.Equal(DataSetVersionStatus.Mapping, savedImport.DataSetVersion.Status);
        }

        private async Task CompleteProcessing(Guid instanceId)
        {
            var function = GetRequiredService<ProcessNextDataSetVersionFunction>();
            await function.CompleteNextDataSetVersionMappingProcessing(instanceId, CancellationToken.None);
        }
    }
}
