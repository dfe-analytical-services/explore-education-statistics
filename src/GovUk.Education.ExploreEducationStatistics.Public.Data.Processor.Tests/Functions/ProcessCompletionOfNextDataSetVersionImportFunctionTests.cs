using GovUk.Education.ExploreEducationStatistics.Common.Tests.Extensions;
using GovUk.Education.ExploreEducationStatistics.Public.Data.Model;
using GovUk.Education.ExploreEducationStatistics.Public.Data.Model.Database;
using GovUk.Education.ExploreEducationStatistics.Public.Data.Model.Parquet.Tables;
using GovUk.Education.ExploreEducationStatistics.Public.Data.Processor.Functions;
using GovUk.Education.ExploreEducationStatistics.Public.Data.Processor.Model;
using GovUk.Education.ExploreEducationStatistics.Public.Data.Services.Interfaces;
using Microsoft.DurableTask;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using static GovUk.Education.ExploreEducationStatistics.Common.Tests.Utils.MockUtils;

namespace GovUk.Education.ExploreEducationStatistics.Public.Data.Processor.Tests.Functions;

public abstract class ProcessCompletionOfNextDataSetVersionImportFunctionTests(
    ProcessorFunctionsIntegrationTestFixture fixture)
    : ProcessorFunctionsIntegrationTest(fixture)
{
    private static readonly string[] AllDataSetVersionFiles =
    [
        DataSetFilenames.CsvDataFile,
        DataSetFilenames.CsvMetadataFile,
        DataSetFilenames.DuckDbDatabaseFile,
        DataSetFilenames.DuckDbLoadSqlFile,
        DataSetFilenames.DuckDbSchemaSqlFile,
        DataTable.ParquetFile,
        FilterOptionsTable.ParquetFile,
        IndicatorsTable.ParquetFile,
        LocationOptionsTable.ParquetFile,
        TimePeriodsTable.ParquetFile
    ];

    public class ProcessCompletionOfNextDataSetVersionImportTests(
        ProcessorFunctionsIntegrationTestFixture fixture)
        : ProcessCompletionOfNextDataSetVersionImportFunctionTests(fixture)
    {
        [Fact]
        public async Task Success()
        {
            var mockOrchestrationContext = DefaultMockOrchestrationContext();
            var activitySequence = new MockSequence();

            string[] expectedActivitySequence =
            [
                ActivityNames.ImportMetadata,
                ActivityNames.ImportData,
                ActivityNames.WriteDataFiles,
                ActivityNames.CompleteNextDataSetVersionImportProcessing
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

            await ProcessCompletionOfNextDataSetVersionImport(mockOrchestrationContext.Object);

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
                    context.CallActivityAsync(ActivityNames.ImportMetadata,
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

            await ProcessCompletionOfNextDataSetVersionImport(mockOrchestrationContext.Object);

            VerifyAllMocks(mockOrchestrationContext);
        }

        private async Task ProcessCompletionOfNextDataSetVersionImport(TaskOrchestrationContext orchestrationContext)
        {
            var function = GetRequiredService<ProcessCompletionOfNextDataSetVersionFunction>();
            await function.ProcessCompletionOfNextDataSetVersion(
                orchestrationContext,
                new ProcessDataSetVersionContext { DataSetVersionId = Guid.NewGuid() });
        }

        private static Mock<TaskOrchestrationContext> DefaultMockOrchestrationContext(Guid? instanceId = null)
        {
            var mock = new Mock<TaskOrchestrationContext>(MockBehavior.Strict);

            mock.Setup(context =>
                    context.CreateReplaySafeLogger(
                        nameof(ProcessCompletionOfNextDataSetVersionFunction.ProcessCompletionOfNextDataSetVersion)))
                .Returns(NullLogger.Instance);

            mock.SetupGet(context => context.InstanceId)
                .Returns(instanceId?.ToString() ?? Guid.NewGuid().ToString());

            return mock;
        }
    }

    public class CompleteNextDataSetVersionImportProcessingTests(
        ProcessorFunctionsIntegrationTestFixture fixture)
        : ProcessCompletionOfNextDataSetVersionImportFunctionTests(fixture)
    {
        private const DataSetVersionImportStage Stage = DataSetVersionImportStage.Completing;

        [Fact]
        public async Task Success()
        {
            var (dataSetVersion, instanceId) = await CreateDataSet(Stage.PreviousStage());

            var dataSetVersionPathResolver = GetRequiredService<IDataSetVersionPathResolver>();
            Directory.CreateDirectory(dataSetVersionPathResolver.DirectoryPath(dataSetVersion));

            await CompleteProcessing(instanceId);

            await using var publicDataDbContext = GetDbContext<PublicDataDbContext>();

            var savedImport = await publicDataDbContext.DataSetVersionImports
                .Include(i => i.DataSetVersion)
                .SingleAsync(i => i.InstanceId == instanceId);

            Assert.Equal(Stage, savedImport.Stage);
            savedImport.Completed.AssertUtcNow();

            Assert.Equal(DataSetVersionStatus.Draft, savedImport.DataSetVersion.Status);
        }

        [Fact]
        public async Task DuckDbFileIsDeleted()
        {
            var (dataSetVersion, instanceId) = await CreateDataSet(Stage.PreviousStage());

            // Create empty data set version files for all file paths
            var dataSetVersionPathResolver = GetRequiredService<IDataSetVersionPathResolver>();
            var directoryPath = dataSetVersionPathResolver.DirectoryPath(dataSetVersion);
            Directory.CreateDirectory(directoryPath);
            foreach (var filename in AllDataSetVersionFiles)
            {
                await File.Create(Path.Combine(directoryPath, filename)).DisposeAsync();
            }

            await CompleteProcessing(instanceId);

            // Ensure the duck db database file is the only file that was deleted
            AssertDataSetVersionDirectoryContainsOnlyFiles(dataSetVersion,
                AllDataSetVersionFiles
                    .Where(file => file != DataSetFilenames.DuckDbDatabaseFile)
                    .ToArray());
        }

        private async Task CompleteProcessing(Guid instanceId)
        {
            var function = GetRequiredService<ProcessCompletionOfNextDataSetVersionFunction>();
            await function.CompleteNextDataSetVersionImportProcessing(instanceId, CancellationToken.None);
        }
    }
}
