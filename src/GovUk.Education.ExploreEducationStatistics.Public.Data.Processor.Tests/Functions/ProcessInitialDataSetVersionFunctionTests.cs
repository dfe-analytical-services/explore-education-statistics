using Dapper;
using GovUk.Education.ExploreEducationStatistics.Common.Model;
using GovUk.Education.ExploreEducationStatistics.Common.Model.Data;
using GovUk.Education.ExploreEducationStatistics.Common.Tests.Extensions;
using GovUk.Education.ExploreEducationStatistics.Common.Utils;
using GovUk.Education.ExploreEducationStatistics.Public.Data.Model;
using GovUk.Education.ExploreEducationStatistics.Public.Data.Model.Database;
using GovUk.Education.ExploreEducationStatistics.Public.Data.Model.Parquet;
using GovUk.Education.ExploreEducationStatistics.Public.Data.Model.Parquet.Tables;
using GovUk.Education.ExploreEducationStatistics.Public.Data.Processor.Functions;
using GovUk.Education.ExploreEducationStatistics.Public.Data.Processor.Model;
using GovUk.Education.ExploreEducationStatistics.Public.Data.Processor.Tests.Extensions;
using GovUk.Education.ExploreEducationStatistics.Public.Data.Services.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Public.Data.Utils;
using InterpolatedSql.Dapper;
using Microsoft.DurableTask;
using Microsoft.DurableTask.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using Moq.Protected;
using static GovUk.Education.ExploreEducationStatistics.Common.Tests.Utils.MockUtils;

namespace GovUk.Education.ExploreEducationStatistics.Public.Data.Processor.Tests.Functions;

public abstract class ProcessInitialDataSetVersionFunctionTests(
    ProcessorFunctionsIntegrationTestFixture fixture)
    : ProcessorFunctionsIntegrationTest(fixture)
{
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

    public class ProcessInitialDataSetVersionTests(
        ProcessorFunctionsIntegrationTestFixture fixture)
        : ProcessInitialDataSetVersionFunctionTests(fixture)
    {
        [Fact]
        public async Task Success()
        {
            var mockOrchestrationContext = DefaultMockOrchestrationContext();

            // Expect an entity lock to be acquired for calling the ImportMetadata activity
            var mockEntityFeature = new Mock<TaskOrchestrationEntityFeature>(MockBehavior.Strict);
            mockEntityFeature.SetupLockForActivity(ActivityNames.ImportMetadata);
            mockOrchestrationContext.SetupGet(context => context.Entities)
                .Returns(mockEntityFeature.Object);

            var activitySequence = new MockSequence();

            string[] expectedActivitySequence =
            [
                ActivityNames.CopyCsvFiles,
                ActivityNames.ImportMetadata,
                ActivityNames.ImportData,
                ActivityNames.WriteDataFiles,
                ActivityNames.CompleteInitialDataSetVersionProcessing,
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

            await ProcessInitialDataSetVersion(mockOrchestrationContext.Object);

            VerifyAllMocks(mockOrchestrationContext);
        }

        private async Task ProcessInitialDataSetVersion(TaskOrchestrationContext orchestrationContext)
        {
            var function = GetRequiredService<ProcessInitialDataSetVersionFunction>();
            await function.ProcessInitialDataSetVersion(
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

    public class ImportDataTests(
        ProcessorFunctionsIntegrationTestFixture fixture)
        : ProcessInitialDataSetVersionFunctionTests(fixture)
    {
        private const DataSetVersionImportStage Stage = DataSetVersionImportStage.ImportingData;

        public static readonly TheoryData<ProcessorTestData> Data = new()
        {
            ProcessorTestData.AbsenceSchool,
        };

        [Theory]
        [MemberData(nameof(Data))]
        public async Task Success(ProcessorTestData testData)
        {
            var (dataSetVersion, instanceId) = await CreateDataSet(Stage.PreviousStage());

            await ImportData(testData, dataSetVersion, instanceId);

            await using var publicDataDbContext = GetDbContext<PublicDataDbContext>();

            var savedImport = await publicDataDbContext.DataSetVersionImports
                .Include(dataSetVersionImport => dataSetVersionImport.DataSetVersion)
                .SingleAsync(i => i.InstanceId == instanceId);

            Assert.Equal(Stage, savedImport.Stage);
            Assert.Equal(DataSetVersionStatus.Processing, savedImport.DataSetVersion.Status);

            AssertDataSetVersionDirectoryContainsOnlyFiles(dataSetVersion,
            [
                DataSetFilenames.CsvDataFile,
                DataSetFilenames.CsvMetadataFile,
                DataSetFilenames.DuckDbDatabaseFile
            ]);
        }

        [Theory]
        [MemberData(nameof(Data))]
        public async Task DuckDbDataTable_CorrectRowCount(ProcessorTestData testData)
        {
            var (dataSetVersion, instanceId) = await CreateDataSet(Stage.PreviousStage());

            await ImportData(testData, dataSetVersion, instanceId);

            await using var duckDbConnection = GetDuckDbConnection(dataSetVersion);

            var actualRowCount = await duckDbConnection.SqlBuilder(
                $"""
                 SELECT COUNT(*)
                 FROM '{DataTable.TableName:raw}'
                 """
            ).QuerySingleAsync<int>();

            Assert.Equal(testData.ExpectedTotalResults, actualRowCount);
        }

        [Theory]
        [MemberData(nameof(Data))]
        public async Task DuckDbDataTable_CorrectColumns(ProcessorTestData testData)
        {
            var (dataSetVersion, instanceId) = await CreateDataSet(Stage.PreviousStage());

            await ImportData(testData, dataSetVersion, instanceId);

            await using var duckDbConnection = GetDuckDbConnection(dataSetVersion);

            var actualColumns = (await duckDbConnection.SqlBuilder(
                        $"DESCRIBE SELECT * FROM '{DataTable.TableName:raw}' LIMIT 1")
                    .QueryAsync<ParquetColumn>())
                .Select(col => col.ColumnName)
                .ToList();

            string[] expectedColumns =
            [
                DataTable.Cols.Id,
                DataTable.Cols.GeographicLevel,
                DataTable.Cols.TimePeriodId,
                ..testData.ExpectedGeographicLevels.Select(DataTable.Cols.LocationId),
                ..testData.ExpectedFilters.Select(fm => DataTable.Cols.Filter(fm).Trim('"')),
                ..testData.ExpectedIndicators.Select(im => DataTable.Cols.Indicator(im).Trim('"')),
            ];

            Assert.Equal(expectedColumns.Order(), actualColumns.Order());
        }

        [Theory]
        [MemberData(nameof(Data))]
        public async Task DuckDbDataTable_CorrectDistinctGeographicLevels(ProcessorTestData testData)
        {
            var (dataSetVersion, instanceId) = await CreateDataSet(Stage.PreviousStage());

            await ImportData(testData, dataSetVersion, instanceId);

            await using var duckDbConnection = GetDuckDbConnection(dataSetVersion);

            var geographicLevelsCommand = duckDbConnection.SqlBuilder(
                $"""
                 SELECT DISTINCT {DataTable.Ref().GeographicLevel:raw}
                 FROM '{DataTable.TableName:raw}'
                 ORDER BY {DataTable.Ref().GeographicLevel:raw}
                 """
            );

            var actualGeographicLevels = (await geographicLevelsCommand.QueryAsync<string>())
                .Select(EnumUtil.GetFromEnumLabel<GeographicLevel>)
                .AsList();

            Assert.Equal(testData.ExpectedGeographicLevels.Order(), actualGeographicLevels.Order());
        }

        [Theory]
        [MemberData(nameof(Data))]
        public async Task DuckDbDataTable_CorrectDistinctLocationOptions(ProcessorTestData testData)
        {
            var (dataSetVersion, instanceId) = await CreateDataSet(Stage.PreviousStage());

            await ImportData(testData, dataSetVersion, instanceId);

            await using var duckDbConnection = GetDuckDbConnection(dataSetVersion);

            await Assert.AllAsync(testData.ExpectedLocations,
                async expectedLocation =>
                {
                    var optionsCommand = duckDbConnection.SqlBuilder(
                        $"""
                         SELECT DISTINCT {LocationOptionsTable.TableName:raw}.*
                         FROM '{DataTable.TableName:raw}' JOIN '{LocationOptionsTable.TableName:raw}'
                         ON {DataTable.Ref().LocationId(expectedLocation.Level):raw} = {LocationOptionsTable.Ref().Id:raw}
                         ORDER BY {LocationOptionsTable.Ref().Label:raw}
                         """
                    );

                    var actualOptions = (await optionsCommand.QueryAsync<ParquetLocationOption>()).AsList();

                    Assert.Equal(expectedLocation.Options.Count, actualOptions.Count);
                    Assert.All(expectedLocation.Options.OrderBy(o => o.Label),
                        (expectedOption, index) => expectedOption.AssertEqual(actualOptions[index]));
                });
        }

        [Theory]
        [MemberData(nameof(Data))]
        public async Task DuckDbDataTable_CorrectDistinctFilterOptions(ProcessorTestData testData)
        {
            var (dataSetVersion, instanceId) = await CreateDataSet(Stage.PreviousStage());

            await ImportData(testData, dataSetVersion, instanceId);

            await using var duckDbConnection = GetDuckDbConnection(dataSetVersion);

            await Assert.AllAsync(testData.ExpectedFilters,
                async expectedFilter =>
                {
                    var optionsCommand = duckDbConnection.SqlBuilder(
                        $"""
                         SELECT DISTINCT {FilterOptionsTable.TableName:raw}.*
                         FROM '{DataTable.TableName:raw}' JOIN '{FilterOptionsTable.TableName:raw}'
                         ON {DataTable.Ref().Col(expectedFilter.PublicId):raw} = {FilterOptionsTable.Ref().Id:raw}
                         ORDER BY {FilterOptionsTable.Ref().Label:raw}
                         """
                    );

                    var actualOptions = (await optionsCommand.QueryAsync<ParquetFilterOption>()).AsList();

                    Assert.Equal(expectedFilter.Options.Count, actualOptions.Count);
                    Assert.All(expectedFilter.Options,
                        (expectedOption, index) =>
                        {
                            var actualOption = actualOptions[index];
                            Assert.Equal(expectedOption.Label, actualOption.Label);
                            Assert.Equal(expectedFilter.PublicId, actualOption.FilterId);
                        });
                });
        }

        [Theory]
        [MemberData(nameof(Data))]
        public async Task DuckDbDataTable_CorrectDistinctTimePeriods(ProcessorTestData testData)
        {
            var (dataSetVersion, instanceId) = await CreateDataSet(Stage.PreviousStage());

            await ImportData(testData, dataSetVersion, instanceId);

            await using var duckDbConnection = GetDuckDbConnection(dataSetVersion);

            var timePeriodsCommand = duckDbConnection.SqlBuilder(
                $"""
                 SELECT DISTINCT {TimePeriodsTable.TableName:raw}.*
                 FROM '{DataTable.TableName:raw}' JOIN '{TimePeriodsTable.TableName:raw}'
                 ON {DataTable.Ref().TimePeriodId:raw} = {TimePeriodsTable.Ref().Id:raw}
                 ORDER BY {TimePeriodsTable.Ref().Period:raw}
                 """
            );

            var actualTimePeriods = (
                await timePeriodsCommand.QueryAsync<ParquetTimePeriod>()
            ).AsList();

            Assert.Equal(testData.ExpectedTimePeriods.Count, actualTimePeriods.Count);
            Assert.All(testData.ExpectedTimePeriods,
                (expectedTimePeriod, index) =>
                {
                    var actualTimePeriod = actualTimePeriods[index];
                    Assert.Equal(expectedTimePeriod.Code,
                        EnumUtil.GetFromEnumLabel<TimeIdentifier>(actualTimePeriod.Identifier));
                    Assert.Equal(expectedTimePeriod.Period, TimePeriodFormatter.FormatFromCsv(actualTimePeriod.Period));
                });
        }

        private async Task ImportData(
            ProcessorTestData testData,
            DataSetVersion dataSetVersion,
            Guid instanceId)
        {
            SetupCsvDataFilesForDataSetVersion(testData, dataSetVersion);

            // Prepare the metadata before calling the ImportData function
            var importMetadataFunction = GetRequiredService<ImportMetadataFunction>();
            await importMetadataFunction.ImportMetadata(instanceId, CancellationToken.None);

            var processInitialDataSetVersionFunction = GetRequiredService<ProcessInitialDataSetVersionFunction>();
            await processInitialDataSetVersionFunction.ImportData(instanceId, CancellationToken.None);
        }
    }

    public class WriteDataFilesTests(
        ProcessorFunctionsIntegrationTestFixture fixture)
        : ProcessInitialDataSetVersionFunctionTests(fixture)
    {
        private const DataSetVersionImportStage Stage = DataSetVersionImportStage.WritingDataFiles;

        public static readonly TheoryData<ProcessorTestData> Data = new()
        {
            ProcessorTestData.AbsenceSchool,
        };

        [Theory]
        [MemberData(nameof(Data))]
        public async Task Success(ProcessorTestData testData)
        {
            var (dataSetVersion, instanceId) = await CreateDataSet(Stage.PreviousStage());

            await WriteData(testData, dataSetVersion, instanceId);

            await using var publicDataDbContext = GetDbContext<PublicDataDbContext>();

            var savedImport = await publicDataDbContext.DataSetVersionImports
                .Include(dataSetVersionImport => dataSetVersionImport.DataSetVersion)
                .SingleAsync(i => i.InstanceId == instanceId);

            Assert.Equal(Stage, savedImport.Stage);
            Assert.Equal(DataSetVersionStatus.Processing, savedImport.DataSetVersion.Status);

            AssertDataSetVersionDirectoryContainsOnlyFiles(dataSetVersion, _allDataSetVersionFiles);
        }

        private async Task WriteData(
            ProcessorTestData testData,
            DataSetVersion dataSetVersion,
            Guid instanceId)
        {
            SetupCsvDataFilesForDataSetVersion(testData, dataSetVersion);

            // Prepare the metadata and data before calling the WriteDataFiles function
            var importMetadataFunction = GetRequiredService<ImportMetadataFunction>();
            await importMetadataFunction.ImportMetadata(instanceId, CancellationToken.None);

            var importDataFunction = GetRequiredService<ProcessInitialDataSetVersionFunction>();
            await importDataFunction.ImportData(instanceId, CancellationToken.None);

            var processInitialDataSetVersionFunction = GetRequiredService<ProcessInitialDataSetVersionFunction>();
            await processInitialDataSetVersionFunction.WriteDataFiles(instanceId, CancellationToken.None);
        }
    }

    public class CompleteInitialDataSetVersionProcessingTests(
        ProcessorFunctionsIntegrationTestFixture fixture)
        : ProcessInitialDataSetVersionFunctionTests(fixture)
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
            await function.CompleteInitialDataSetVersionProcessing(instanceId, CancellationToken.None);
        }
    }
}
