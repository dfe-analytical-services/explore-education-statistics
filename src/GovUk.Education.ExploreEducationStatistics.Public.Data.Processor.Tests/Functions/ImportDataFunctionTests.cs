using Dapper;
using GovUk.Education.ExploreEducationStatistics.Common.Model;
using GovUk.Education.ExploreEducationStatistics.Common.Model.Data;
using GovUk.Education.ExploreEducationStatistics.Common.Utils;
using GovUk.Education.ExploreEducationStatistics.Public.Data.Model;
using GovUk.Education.ExploreEducationStatistics.Public.Data.Model.Database;
using GovUk.Education.ExploreEducationStatistics.Public.Data.Model.Parquet;
using GovUk.Education.ExploreEducationStatistics.Public.Data.Model.Parquet.Tables;
using GovUk.Education.ExploreEducationStatistics.Public.Data.Processor.Functions;
using GovUk.Education.ExploreEducationStatistics.Public.Data.Processor.Tests.Extensions;
using GovUk.Education.ExploreEducationStatistics.Public.Data.Utils;
using InterpolatedSql.Dapper;
using Microsoft.EntityFrameworkCore;

namespace GovUk.Education.ExploreEducationStatistics.Public.Data.Processor.Tests.Functions;

public abstract class ImportDataFunctionTests(ProcessorFunctionsIntegrationTestFixture fixture)
    : ProcessorFunctionsIntegrationTest(fixture)
{
    private const DataSetVersionImportStage Stage = DataSetVersionImportStage.ImportingData;

    public static readonly TheoryData<ProcessorTestData> DataSets =
    [
        ProcessorTestData.AbsenceSchool,
        ProcessorTestData.LargeDataSet,
    ];

    public class ImportDataTests(ProcessorFunctionsIntegrationTestFixture fixture) : ImportDataFunctionTests(fixture)
    {
        [Theory]
        [MemberData(nameof(DataSets))]
        public async Task Success(ProcessorTestData testData)
        {
            var (dataSetVersion, instanceId) = await CreateDataSetInitialVersion(Stage.PreviousStage());

            await ImportData(testData, dataSetVersion, instanceId);

            await using var publicDataDbContext = GetDbContext<PublicDataDbContext>();

            var savedImport = await publicDataDbContext
                .DataSetVersionImports.Include(dataSetVersionImport => dataSetVersionImport.DataSetVersion)
                .SingleAsync(i => i.InstanceId == instanceId);

            Assert.Equal(Stage, savedImport.Stage);
            Assert.Equal(DataSetVersionStatus.Processing, savedImport.DataSetVersion.Status);

            AssertDataSetVersionDirectoryContainsOnlyFiles(
                dataSetVersion,
                [DataSetFilenames.CsvDataFile, DataSetFilenames.CsvMetadataFile, DataSetFilenames.DuckDbDatabaseFile]
            );
        }

        [Theory]
        [MemberData(nameof(DataSets))]
        public async Task DuckDbDataTable_CorrectRowCount(ProcessorTestData testData)
        {
            var (dataSetVersion, instanceId) = await CreateDataSetInitialVersion(Stage.PreviousStage());

            await ImportData(testData, dataSetVersion, instanceId);

            await using var duckDbConnection = GetDuckDbConnection(dataSetVersion);

            var actualRowCount = await duckDbConnection
                .SqlBuilder(
                    $"""
                    SELECT COUNT(*)
                    FROM '{DataTable.TableName:raw}'
                    """
                )
                .QuerySingleAsync<int>();

            Assert.Equal(testData.ExpectedTotalResults, actualRowCount);
        }

        [Theory]
        [MemberData(nameof(DataSets))]
        public async Task DuckDbDataTable_CorrectColumns(ProcessorTestData testData)
        {
            var (dataSetVersion, instanceId) = await CreateDataSetInitialVersion(Stage.PreviousStage());

            await ImportData(testData, dataSetVersion, instanceId);

            await using var duckDbConnection = GetDuckDbConnection(dataSetVersion);

            var actualColumns = (
                await duckDbConnection
                    .SqlBuilder($"DESCRIBE SELECT * FROM '{DataTable.TableName:raw}' LIMIT 1")
                    .QueryAsync<ParquetColumn>()
            )
                .Select(col => col.ColumnName)
                .ToList();

            string[] expectedColumns =
            [
                DataTable.Cols.Id,
                DataTable.Cols.GeographicLevel,
                DataTable.Cols.TimePeriodId,
                .. testData.ExpectedGeographicLevels.Select(DataTable.Cols.LocationId),
                .. testData.ExpectedFilters.Select(fm => DataTable.Cols.Filter(fm).Trim('"')),
                .. testData.ExpectedIndicators.Select(im => DataTable.Cols.Indicator(im).Trim('"')),
            ];

            Assert.Equal(expectedColumns.Order(), actualColumns.Order());
        }

        [Theory]
        [MemberData(nameof(DataSets))]
        public async Task DuckDbDataTable_CorrectDistinctGeographicLevels(ProcessorTestData testData)
        {
            var (dataSetVersion, instanceId) = await CreateDataSetInitialVersion(Stage.PreviousStage());

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
        [MemberData(nameof(DataSets))]
        public async Task DuckDbDataTable_CorrectDistinctLocationOptions(ProcessorTestData testData)
        {
            var (dataSetVersion, instanceId) = await CreateDataSetInitialVersion(Stage.PreviousStage());

            await ImportData(testData, dataSetVersion, instanceId);

            await using var duckDbConnection = GetDuckDbConnection(dataSetVersion);

            await Assert.AllAsync(
                testData.ExpectedLocations,
                async expectedLocation =>
                {
                    var optionsCommand = duckDbConnection.SqlBuilder(
                        $"""
                        SELECT DISTINCT {LocationOptionsTable.TableName:raw}.*
                        FROM '{DataTable.TableName:raw}' JOIN '{LocationOptionsTable.TableName:raw}'
                        ON {DataTable.Ref().LocationId(
                            expectedLocation.Level
                        ):raw} = {LocationOptionsTable.Ref().Id:raw}
                        ORDER BY {LocationOptionsTable.Ref().Label:raw}
                        """
                    );

                    var actualOptions = (await optionsCommand.QueryAsync<ParquetLocationOption>()).AsList();

                    Assert.Equal(expectedLocation.Options.Count, actualOptions.Count);
                    Assert.All(
                        expectedLocation.Options.OrderBy(o => o.Label),
                        (expectedOption, index) => expectedOption.AssertEqual(actualOptions[index])
                    );
                }
            );
        }

        [Theory]
        [MemberData(nameof(DataSets))]
        public async Task DuckDbDataTable_CorrectDistinctFilterOptions(ProcessorTestData testData)
        {
            var (dataSetVersion, instanceId) = await CreateDataSetInitialVersion(Stage.PreviousStage());

            await ImportData(testData, dataSetVersion, instanceId);

            await using var duckDbConnection = GetDuckDbConnection(dataSetVersion);

            await Assert.AllAsync(
                testData.ExpectedFilters,
                async expectedFilter =>
                {
                    var optionsCommand = duckDbConnection.SqlBuilder(
                        $"""
                        SELECT DISTINCT {FilterOptionsTable.TableName:raw}.*
                        FROM '{DataTable.TableName:raw}' JOIN '{FilterOptionsTable.TableName:raw}'
                        ON {DataTable.Ref().Col(expectedFilter.Column):raw} = {FilterOptionsTable.Ref().Id:raw}
                        ORDER BY {FilterOptionsTable.Ref().Label:raw}
                        """
                    );

                    var actualOptions = (await optionsCommand.QueryAsync<ParquetFilterOption>()).AsList();

                    Assert.Equal(expectedFilter.Options.Count, actualOptions.Count);
                    Assert.All(
                        expectedFilter.Options,
                        (expectedOption, index) =>
                        {
                            var actualOption = actualOptions[index];
                            Assert.Equal(expectedOption.Label, actualOption.Label);
                            Assert.Equal(expectedFilter.PublicId, actualOption.FilterId);
                        }
                    );
                }
            );
        }

        [Theory]
        [MemberData(nameof(DataSets))]
        public async Task DuckDbDataTable_CorrectDistinctTimePeriods(ProcessorTestData testData)
        {
            var (dataSetVersion, instanceId) = await CreateDataSetInitialVersion(Stage.PreviousStage());

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

            var actualTimePeriods = (await timePeriodsCommand.QueryAsync<ParquetTimePeriod>()).AsList();

            Assert.Equal(testData.ExpectedTimePeriods.Count, actualTimePeriods.Count);
            Assert.All(
                testData.ExpectedTimePeriods,
                (expectedTimePeriod, index) =>
                {
                    var actualTimePeriod = actualTimePeriods[index];
                    Assert.Equal(
                        expectedTimePeriod.Code,
                        EnumUtil.GetFromEnumLabel<TimeIdentifier>(actualTimePeriod.Identifier)
                    );
                    Assert.Equal(expectedTimePeriod.Period, TimePeriodFormatter.FormatFromCsv(actualTimePeriod.Period));
                }
            );
        }

        private async Task ImportData(ProcessorTestData testData, DataSetVersion dataSetVersion, Guid instanceId)
        {
            SetupCsvDataFilesForDataSetVersion(testData, dataSetVersion);

            // Prepare the metadata before calling the ImportData function
            var importMetadataFunction = GetRequiredService<ImportMetadataFunction>();
            await importMetadataFunction.ImportMetadata(instanceId, CancellationToken.None);

            var importDataFunction = GetRequiredService<ImportDataFunction>();
            await importDataFunction.ImportData(instanceId, CancellationToken.None);
        }
    }
}
