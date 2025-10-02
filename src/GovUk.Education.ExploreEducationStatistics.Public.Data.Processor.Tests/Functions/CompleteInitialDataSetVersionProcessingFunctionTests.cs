using GovUk.Education.ExploreEducationStatistics.Common.Tests.Extensions;
using GovUk.Education.ExploreEducationStatistics.Public.Data.Model;
using GovUk.Education.ExploreEducationStatistics.Public.Data.Model.Database;
using GovUk.Education.ExploreEducationStatistics.Public.Data.Model.Parquet.Tables;
using GovUk.Education.ExploreEducationStatistics.Public.Data.Processor.Functions;
using GovUk.Education.ExploreEducationStatistics.Public.Data.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace GovUk.Education.ExploreEducationStatistics.Public.Data.Processor.Tests.Functions;

public abstract class CompleteInitialDataSetVersionProcessingFunctionTests(
    ProcessorFunctionsIntegrationTestFixture fixture
) : ProcessorFunctionsIntegrationTest(fixture)
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

    public class CompleteInitialDataSetVersionProcessingProcessingTests(
        ProcessorFunctionsIntegrationTestFixture fixture
    ) : CompleteInitialDataSetVersionProcessingFunctionTests(fixture)
    {
        private const DataSetVersionImportStage Stage = DataSetVersionImportStage.Completing;

        [Fact]
        public async Task Success()
        {
            var (dataSetVersion, instanceId) = await CreateDataSetInitialVersion(Stage.PreviousStage());

            var dataSetVersionPathResolver = GetRequiredService<IDataSetVersionPathResolver>();
            Directory.CreateDirectory(dataSetVersionPathResolver.DirectoryPath(dataSetVersion));

            await CompleteInitialDataSetVersionProcessing(instanceId);

            await using var publicDataDbContext = GetDbContext<PublicDataDbContext>();

            var savedImport = await publicDataDbContext
                .DataSetVersionImports.Include(i => i.DataSetVersion)
                .SingleAsync(i => i.InstanceId == instanceId);

            Assert.Equal(Stage, savedImport.Stage);
            savedImport.Completed.AssertUtcNow();

            Assert.Equal(DataSetVersionStatus.Draft, savedImport.DataSetVersion.Status);
        }

        [Fact]
        public async Task DuckDbFileIsDeleted()
        {
            var (dataSetVersion, instanceId) = await CreateDataSetInitialVersion(Stage.PreviousStage());

            // Create empty data set version files for all file paths
            var dataSetVersionPathResolver = GetRequiredService<IDataSetVersionPathResolver>();
            var directoryPath = dataSetVersionPathResolver.DirectoryPath(dataSetVersion);
            Directory.CreateDirectory(directoryPath);
            foreach (var filename in AllDataSetVersionFiles)
            {
                await File.Create(Path.Combine(directoryPath, filename)).DisposeAsync();
            }

            await CompleteInitialDataSetVersionProcessing(instanceId);

            // Ensure the duck db database file is the only file that was deleted
            AssertDataSetVersionDirectoryContainsOnlyFiles(
                dataSetVersion,
                AllDataSetVersionFiles.Where(file => file != DataSetFilenames.DuckDbDatabaseFile).ToArray()
            );
        }

        private async Task CompleteInitialDataSetVersionProcessing(Guid instanceId)
        {
            var function = GetRequiredService<CompleteInitialDataSetVersionProcessingFunction>();
            await function.CompleteInitialDataSetVersionProcessing(instanceId, CancellationToken.None);
        }
    }
}
