using GovUk.Education.ExploreEducationStatistics.Common.Tests.Extensions;
using GovUk.Education.ExploreEducationStatistics.Public.Data.Model;
using GovUk.Education.ExploreEducationStatistics.Public.Data.Model.Database;
using GovUk.Education.ExploreEducationStatistics.Public.Data.Model.Parquet.Tables;
using GovUk.Education.ExploreEducationStatistics.Public.Data.Processor.Functions;
using GovUk.Education.ExploreEducationStatistics.Public.Data.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace GovUk.Education.ExploreEducationStatistics.Public.Data.Processor.Tests.Functions;

public abstract class CompleteProcessingFunctionTests(ProcessorFunctionsIntegrationTestFixture fixture)
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
    
    public class CompleteProcessingTests(
        ProcessorFunctionsIntegrationTestFixture fixture)
        : CompleteProcessingFunctionTests(fixture)
    {
        private const DataSetVersionImportStage Stage = DataSetVersionImportStage.Completing;

        [Fact]
        public async Task Success_InitialDataSetVersion()
        {
            var (dataSetVersion, instanceId) = await CreateDataSet(Stage.PreviousStage());

            var dataSetVersionPathResolver = GetRequiredService<IDataSetVersionPathResolver>();
            Directory.CreateDirectory(dataSetVersionPathResolver.DirectoryPath(dataSetVersion));

            await AssertCompletedProcessingSuccessfully(
                dataSetVersion: dataSetVersion,
                instanceId: instanceId,
                expectedStatus: DataSetVersionStatus.Draft);
        }
        
        [Fact]
        public async Task Success_NextDataSetVersion()
        {
            var (initialDataSetVersion, _) = await CreateDataSet(DataSetVersionImportStage.Completing);

            var dataSet = await GetDbContext<PublicDataDbContext>().DataSets.SingleAsync(dataSet => 
                dataSet.Id == initialDataSetVersion.DataSet.Id);
            
            var (nextDataSetVersion, instanceId) = await CreateDataSetVersionAndImport(
                dataSet: dataSet, 
                importStage: Stage.PreviousStage(),
                versionNumberMajor: 1,
                versionNumberMinor: 1);
            
            await AssertCompletedProcessingSuccessfully(
                dataSetVersion: nextDataSetVersion,
                instanceId: instanceId,
                expectedStatus: DataSetVersionStatus.Mapping);
        }

        private async Task AssertCompletedProcessingSuccessfully(
            DataSetVersion dataSetVersion, 
            Guid instanceId,
            DataSetVersionStatus expectedStatus)
        {
            var dataSetVersionPathResolver = GetRequiredService<IDataSetVersionPathResolver>();
            Directory.CreateDirectory(dataSetVersionPathResolver.DirectoryPath(dataSetVersion));

            await CompleteProcessing(instanceId);

            await using var publicDataDbContext = GetDbContext<PublicDataDbContext>();

            var savedImport = await publicDataDbContext.DataSetVersionImports
                .Include(i => i.DataSetVersion)
                .SingleAsync(i => i.InstanceId == instanceId);

            Assert.Equal(Stage, savedImport.Stage);
            savedImport.Completed.AssertUtcNow();

            Assert.Equal(expectedStatus, savedImport.DataSetVersion.Status);
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
            var function = GetRequiredService<CompleteProcessingFunction>();
            await function.CompleteProcessing(instanceId, CancellationToken.None);
        }
    }
}
