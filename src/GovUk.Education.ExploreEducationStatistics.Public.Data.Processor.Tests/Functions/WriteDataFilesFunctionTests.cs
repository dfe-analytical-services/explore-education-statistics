using GovUk.Education.ExploreEducationStatistics.Public.Data.Model;
using GovUk.Education.ExploreEducationStatistics.Public.Data.Model.Database;
using GovUk.Education.ExploreEducationStatistics.Public.Data.Model.Parquet.Tables;
using GovUk.Education.ExploreEducationStatistics.Public.Data.Processor.Functions;
using Microsoft.EntityFrameworkCore;

namespace GovUk.Education.ExploreEducationStatistics.Public.Data.Processor.Tests.Functions;

public abstract class WriteDataFilesFunctionTests(ProcessorFunctionsIntegrationTestFixture fixture)
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
        TimePeriodsTable.ParquetFile,
    ];

    private const DataSetVersionImportStage Stage = DataSetVersionImportStage.WritingDataFiles;

    public static readonly TheoryData<ProcessorTestData> Data = new()
    {
        ProcessorTestData.AbsenceSchool,
    };

    public class WriteDataTests(ProcessorFunctionsIntegrationTestFixture fixture)
        : WriteDataFilesFunctionTests(fixture)
    {
        [Theory]
        [MemberData(nameof(Data))]
        public async Task Success(ProcessorTestData testData)
        {
            var (dataSetVersion, instanceId) = await CreateDataSetInitialVersion(
                Stage.PreviousStage()
            );

            await WriteData(testData, dataSetVersion, instanceId);

            await using var publicDataDbContext = GetDbContext<PublicDataDbContext>();

            var savedImport = await publicDataDbContext
                .DataSetVersionImports.Include(dataSetVersionImport =>
                    dataSetVersionImport.DataSetVersion
                )
                .SingleAsync(i => i.InstanceId == instanceId);

            Assert.Equal(Stage, savedImport.Stage);
            Assert.Equal(DataSetVersionStatus.Processing, savedImport.DataSetVersion.Status);

            AssertDataSetVersionDirectoryContainsOnlyFiles(dataSetVersion, AllDataSetVersionFiles);
        }

        private async Task WriteData(
            ProcessorTestData testData,
            DataSetVersion dataSetVersion,
            Guid instanceId
        )
        {
            SetupCsvDataFilesForDataSetVersion(testData, dataSetVersion);

            // Prepare the metadata and data before calling the WriteDataFiles function
            var importMetadataFunction = GetRequiredService<ImportMetadataFunction>();
            await importMetadataFunction.ImportMetadata(instanceId, CancellationToken.None);

            var importDataFunction = GetRequiredService<ImportDataFunction>();
            await importDataFunction.ImportData(instanceId, CancellationToken.None);

            var writeDataFilesFunction = GetRequiredService<WriteDataFilesFunction>();
            await writeDataFilesFunction.WriteDataFiles(instanceId, CancellationToken.None);
        }
    }
}
