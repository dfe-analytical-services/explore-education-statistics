using System.Reflection;
using GovUk.Education.ExploreEducationStatistics.Common.Extensions;
using GovUk.Education.ExploreEducationStatistics.Common.Tests.Extensions;
using GovUk.Education.ExploreEducationStatistics.Common.Utils;
using GovUk.Education.ExploreEducationStatistics.Public.Data.Model;
using GovUk.Education.ExploreEducationStatistics.Public.Data.Model.Database;
using GovUk.Education.ExploreEducationStatistics.Public.Data.Model.Parquet.Tables;
using GovUk.Education.ExploreEducationStatistics.Public.Data.Model.Tests.Fixtures;
using GovUk.Education.ExploreEducationStatistics.Public.Data.Processor.Functions;
using GovUk.Education.ExploreEducationStatistics.Public.Data.Processor.Model;
using GovUk.Education.ExploreEducationStatistics.Public.Data.Processor.Tests.Extensions;
using GovUk.Education.ExploreEducationStatistics.Public.Data.Services.Interfaces;
using Microsoft.DurableTask;
using Microsoft.DurableTask.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using Moq.Protected;
using static GovUk.Education.ExploreEducationStatistics.Common.Tests.Utils.MockUtils;

namespace GovUk.Education.ExploreEducationStatistics.Public.Data.Processor.Tests.Functions;

public abstract class ProcessInitialDataSetVersionFunctionTests(ProcessorFunctionsIntegrationTestFixture fixture)
    : ProcessorFunctionsIntegrationTest(fixture)
{
    private readonly string _testDataDirectoryPath = Path.Combine(
        Assembly.GetExecutingAssembly().GetDirectoryPath(),
        "Resources",
        "DataFiles",
        "AbsenceSchool"
    );

    private readonly string[] _allDataSetVersionFiles =
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

    public class ProcessInitialDataSetVersionTests(ProcessorFunctionsIntegrationTestFixture fixture)
        : ProcessInitialDataSetVersionFunctionTests(fixture)
    {
        [Fact]
        public async Task Success()
        {
            var mockOrchestrationContext = DefaultMockOrchestrationContext();

            // Expect an entity lock to be acquired for calling the ImportMetadata activity
            var mockEntityFeature = new Mock<TaskOrchestrationEntityFeature>(MockBehavior.Strict);
            mockEntityFeature.SetupLockForActivity(nameof(ProcessInitialDataSetVersionFunction.ImportMetadata));
            mockOrchestrationContext.SetupGet(context => context.Entities)
                .Returns(mockEntityFeature.Object);

            var activitySequence = new MockSequence();

            string[] expectedActivitySequence =
            [
                nameof(CopyCsvFilesFunction.CopyCsvFiles),
                nameof(ProcessInitialDataSetVersionFunction.ImportMetadata),
                nameof(ProcessInitialDataSetVersionFunction.ImportData),
                nameof(ProcessInitialDataSetVersionFunction.WriteDataFiles),
                nameof(ProcessInitialDataSetVersionFunction.CompleteProcessing)
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

            await ProcessInitialDataSetVersion(mockOrchestrationContext.Object);

            VerifyAllMocks(mockOrchestrationContext, mockEntityFeature);
        }

        [Fact]
        public async Task ActivityFunctionThrowsException_CallsHandleFailureActivity()
        {
            var mockOrchestrationContext = DefaultMockOrchestrationContext();

            var activitySequence = new MockSequence();

            mockOrchestrationContext
                .InSequence(activitySequence)
                .Setup(context =>
                    context.CallActivityAsync(nameof(CopyCsvFilesFunction.CopyCsvFiles),
                        mockOrchestrationContext.Object.InstanceId,
                        null))
                .Throws<Exception>();

            mockOrchestrationContext
                .InSequence(activitySequence)
                .Setup(context =>
                    context.CallActivityAsync(nameof(ProcessInitialDataSetVersionFunction.HandleProcessingFailure),
                        null,
                        null))
                .Returns(Task.CompletedTask);

            await ProcessInitialDataSetVersion(mockOrchestrationContext.Object);

            VerifyAllMocks(mockOrchestrationContext);
        }

        private async Task ProcessInitialDataSetVersion(TaskOrchestrationContext orchestrationContext)
        {
            var function = GetRequiredService<ProcessInitialDataSetVersionFunction>();
            await function.ProcessInitialDataSetVersion(
                orchestrationContext,
                new ProcessInitialDataSetVersionContext
                {
                    DataSetVersionId = Guid.NewGuid()
                });
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

    public class ImportMetadataTests(ProcessorFunctionsIntegrationTestFixture fixture)
        : ProcessInitialDataSetVersionFunctionTests(fixture)
    {
        private const DataSetVersionImportStage Stage = DataSetVersionImportStage.ImportingMetadata;

        [Fact]
        public async Task Success()
        {
            var (dataSetVersion, instanceId) = await CreateDataSetVersion(Stage.PreviousStage());

            // Prepare the data set version directory with data and metadata CSV files
            SetupCsvDataFiles(dataSetVersion);

            var function = GetRequiredService<ProcessInitialDataSetVersionFunction>();
            await function.ImportMetadata(instanceId, CancellationToken.None);

            await using var publicDataDbContext = GetDbContext<PublicDataDbContext>();

            var savedImport = await publicDataDbContext.DataSetVersionImports
                .Include(dataSetVersionImport => dataSetVersionImport.DataSetVersion)
                .SingleAsync(i => i.InstanceId == instanceId);

            Assert.Equal(DataSetVersionStatus.Processing, savedImport.DataSetVersion.Status);
            Assert.Equal(Stage, savedImport.Stage);

            AssertDataSetVersionDirectoryContainsOnlyFiles(dataSetVersion,
            [
                DataSetFilenames.CsvDataFile,
                DataSetFilenames.CsvMetadataFile,
                DataSetFilenames.DuckDbDatabaseFile
            ]);
        }
    }

    public class ImportDataTests(ProcessorFunctionsIntegrationTestFixture fixture)
        : ProcessInitialDataSetVersionFunctionTests(fixture)
    {
        private const DataSetVersionImportStage Stage = DataSetVersionImportStage.ImportingData;

        [Fact]
        public async Task Success()
        {
            var (dataSetVersion, instanceId) = await CreateDataSetVersion(Stage.PreviousStage());

            // Prepare the data set version directory with data and metadata CSV files
            SetupCsvDataFiles(dataSetVersion);

            var function = GetRequiredService<ProcessInitialDataSetVersionFunction>();

            // Prepare the metadata before calling the ImportData function
            await function.ImportMetadata(instanceId, CancellationToken.None);

            await function.ImportData(instanceId, CancellationToken.None);

            await using var publicDataDbContext = GetDbContext<PublicDataDbContext>();

            var savedImport = await publicDataDbContext.DataSetVersionImports
                .Include(dataSetVersionImport => dataSetVersionImport.DataSetVersion)
                .SingleAsync(i => i.InstanceId == instanceId);

            Assert.Equal(DataSetVersionStatus.Processing, savedImport.DataSetVersion.Status);
            Assert.Equal(Stage, savedImport.Stage);

            AssertDataSetVersionDirectoryContainsOnlyFiles(dataSetVersion,
            [
                DataSetFilenames.CsvDataFile,
                DataSetFilenames.CsvMetadataFile,
                DataSetFilenames.DuckDbDatabaseFile
            ]);
        }
    }

    public class WriteDataFilesTests(ProcessorFunctionsIntegrationTestFixture fixture)
        : ProcessInitialDataSetVersionFunctionTests(fixture)
    {
        private const DataSetVersionImportStage Stage = DataSetVersionImportStage.WritingDataFiles;

        [Fact]
        public async Task Success()
        {
            var (dataSetVersion, instanceId) = await CreateDataSetVersion(Stage.PreviousStage());

            // Prepare the data set version directory with data and metadata CSV files
            SetupCsvDataFiles(dataSetVersion);

            var function = GetRequiredService<ProcessInitialDataSetVersionFunction>();

            // Prepare the metadata and data before calling the WriteDataFiles function
            await function.ImportMetadata(instanceId, CancellationToken.None);
            await function.ImportData(instanceId, CancellationToken.None);

            await function.WriteDataFiles(instanceId, CancellationToken.None);

            await using var publicDataDbContext = GetDbContext<PublicDataDbContext>();

            var savedImport = await publicDataDbContext.DataSetVersionImports
                .Include(dataSetVersionImport => dataSetVersionImport.DataSetVersion)
                .SingleAsync(i => i.InstanceId == instanceId);

            Assert.Equal(DataSetVersionStatus.Processing, savedImport.DataSetVersion.Status);
            Assert.Equal(Stage, savedImport.Stage);

            AssertDataSetVersionDirectoryContainsOnlyFiles(dataSetVersion, _allDataSetVersionFiles);
        }
    }

    public class HandleProcessingFailureTests(ProcessorFunctionsIntegrationTestFixture fixture)
        : ProcessInitialDataSetVersionFunctionTests(fixture)
    {
        [Fact]
        public async Task Success()
        {
            // The stage which the failure occured in - This should not be altered by the handler
            const DataSetVersionImportStage failedStage = DataSetVersionImportStage.CopyingCsvFiles;

            var (_, instanceId) = await CreateDataSetVersion(failedStage);

            var function = GetRequiredService<ProcessInitialDataSetVersionFunction>();
            await function.HandleProcessingFailure(instanceId, CancellationToken.None);

            await using var publicDataDbContext = GetDbContext<PublicDataDbContext>();

            var savedImport = await publicDataDbContext.DataSetVersionImports
                .Include(i => i.DataSetVersion)
                .SingleAsync(i => i.InstanceId == instanceId);

            Assert.Equal(DataSetVersionStatus.Failed, savedImport.DataSetVersion.Status);
            Assert.Equal(failedStage, savedImport.Stage);
            savedImport.Completed.AssertUtcNow();
        }
    }

    public class CompleteProcessingTests(ProcessorFunctionsIntegrationTestFixture fixture)
        : ProcessInitialDataSetVersionFunctionTests(fixture)
    {
        private const DataSetVersionImportStage Stage = DataSetVersionImportStage.Completing;

        [Fact]
        public async Task Success()
        {
            var (dataSetVersion, instanceId) = await CreateDataSetVersion(Stage.PreviousStage());

            var dataSetVersionPathResolver = GetRequiredService<IDataSetVersionPathResolver>();
            Directory.CreateDirectory(dataSetVersionPathResolver.DirectoryPath(dataSetVersion));

            await CompleteProcessing(instanceId);

            await using var publicDataDbContext = GetDbContext<PublicDataDbContext>();

            var savedImport = await publicDataDbContext.DataSetVersionImports
                .Include(i => i.DataSetVersion)
                .SingleAsync(i => i.InstanceId == instanceId);

            Assert.Equal(DataSetVersionStatus.Draft, savedImport.DataSetVersion.Status);
            Assert.Equal(Stage, savedImport.Stage);
            savedImport.Completed.AssertUtcNow();
        }

        [Fact]
        public async Task DuckDbFileIsDeleted()
        {
            var (dataSetVersion, instanceId) = await CreateDataSetVersion(Stage.PreviousStage());

            // Create empty data set version files for all file paths
            var dataSetVersionPathResolver = GetRequiredService<IDataSetVersionPathResolver>();
            var directoryPath = dataSetVersionPathResolver.DirectoryPath(dataSetVersion);
            Directory.CreateDirectory(directoryPath);
            foreach (var filename in _allDataSetVersionFiles)
            {
                await File.Create(Path.Combine(directoryPath, filename)).DisposeAsync();
            }

            await CompleteProcessing(instanceId);

            // Ensure the duck db database file is the only file that was deleted
            AssertDataSetVersionDirectoryContainsOnlyFiles(dataSetVersion,
                _allDataSetVersionFiles
                    .Where(file => file != DataSetFilenames.DuckDbDatabaseFile)
                    .ToArray());
        }

        private async Task CompleteProcessing(Guid instanceId)
        {
            var function = GetRequiredService<ProcessInitialDataSetVersionFunction>();
            await function.CompleteProcessing(instanceId, CancellationToken.None);
        }
    }

    private async Task<(DataSetVersion dataSetVersion, Guid instanceId)> CreateDataSetVersion(
        DataSetVersionImportStage importStage)
    {
        DataSet dataSet = DataFixture.DefaultDataSet();

        await AddTestData<PublicDataDbContext>(context => context.DataSets.Add(dataSet));

        DataSetVersionImport dataSetVersionImport = DataFixture
            .DefaultDataSetVersionImport()
            .WithStage(importStage);

        DataSetVersion dataSetVersion = DataFixture
            .DefaultDataSetVersion()
            .WithDataSet(dataSet)
            .WithStatus(DataSetVersionStatus.Processing)
            .WithImports(() => [dataSetVersionImport])
            .FinishWith(dsv => dsv.DataSet.LatestDraftVersion = dsv);

        await AddTestData<PublicDataDbContext>(context =>
        {
            context.DataSetVersions.Add(dataSetVersion);
            context.DataSets.Update(dataSet);
        });

        return (dataSetVersion, dataSetVersionImport.InstanceId);
    }

    private void SetupCsvDataFiles(DataSetVersion dataSetVersion)
    {
        var dataSetVersionPathResolver = GetRequiredService<IDataSetVersionPathResolver>();
        Directory.CreateDirectory(dataSetVersionPathResolver.DirectoryPath(dataSetVersion));
        File.Copy(Path.Combine(_testDataDirectoryPath, DataSetFilenames.CsvDataFile),
            dataSetVersionPathResolver.CsvDataPath(dataSetVersion));
        File.Copy(Path.Combine(_testDataDirectoryPath, DataSetFilenames.CsvMetadataFile),
            dataSetVersionPathResolver.CsvMetadataPath(dataSetVersion));
    }

    private void AssertDataSetVersionDirectoryContainsOnlyFiles(
        DataSetVersion dataSetVersion,
        params string[] expectedFiles)
    {
        var dataSetVersionPathResolver = GetRequiredService<IDataSetVersionPathResolver>();
        var actualFiles = Directory.GetFiles(dataSetVersionPathResolver.DirectoryPath(dataSetVersion))
            .Select(Path.GetFileName)
            .ToArray();

        // Assert that the directory contains the expected files and no others
        Assert.Equal(expectedFiles.Length, actualFiles.Length);
        Assert.True(ComparerUtils.SequencesAreEqualIgnoringOrder(expectedFiles, actualFiles));
    }
}
