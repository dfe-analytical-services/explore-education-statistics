using GovUk.Education.ExploreEducationStatistics.Common.IntegrationTests;
using GovUk.Education.ExploreEducationStatistics.Common.IntegrationTests.FunctionApp;
using GovUk.Education.ExploreEducationStatistics.Common.Tests.Extensions;
using GovUk.Education.ExploreEducationStatistics.Public.Data.Model;
using GovUk.Education.ExploreEducationStatistics.Public.Data.Model.Parquet.Tables;
using GovUk.Education.ExploreEducationStatistics.Public.Data.Processor.Functions;
using GovUk.Education.ExploreEducationStatistics.Public.Data.Processor.Tests.Fixture;
using GovUk.Education.ExploreEducationStatistics.Public.Data.Processor.Tests.TestData;
using Microsoft.EntityFrameworkCore;

#pragma warning disable CS9107 // Parameter is captured into the state of the enclosing type and its value is also passed to the base constructor. The value might be captured by the base class as well.

namespace GovUk.Education.ExploreEducationStatistics.Public.Data.Processor.Tests.Functions;

// ReSharper disable once ClassNeverInstantiated.Global
public class CompleteInitialDataSetVersionProcessingFunctionTestsFixture()
    : OptimisedPublicDataProcessorCollectionFixture(
        capabilities: [PublicDataProcessorIntegrationTestCapability.Postgres]
    )
{
    public CompleteInitialDataSetVersionProcessingFunction Function = null!;

    protected override async Task AfterFactoryConstructed(OptimisedServiceCollectionLookups lookups)
    {
        await base.AfterFactoryConstructed(lookups);

        Function = lookups.GetService<CompleteInitialDataSetVersionProcessingFunction>();
    }
}

[CollectionDefinition(nameof(CompleteInitialDataSetVersionProcessingFunctionTestsFixture))]
public class CompleteInitialDataSetVersionProcessingFunctionTestsCollection
    : ICollectionFixture<CompleteInitialDataSetVersionProcessingFunctionTestsFixture>;

[Collection(nameof(CompleteInitialDataSetVersionProcessingFunctionTestsFixture))]
public abstract class CompleteInitialDataSetVersionProcessingFunctionTests(
    CompleteInitialDataSetVersionProcessingFunctionTestsFixture fixture
) : OptimisedFunctionAppIntegrationTestBase(fixture)
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
        CompleteInitialDataSetVersionProcessingFunctionTestsFixture fixture
    ) : CompleteInitialDataSetVersionProcessingFunctionTests(fixture)
    {
        private const DataSetVersionImportStage Stage = DataSetVersionImportStage.Completing;

        [Fact]
        public async Task Success()
        {
            var (dataSetVersion, instanceId) = await CommonTestDataUtils.CreateDataSetInitialVersion(
                fixture.GetPublicDataDbContext(),
                Stage.PreviousStage()
            );

            Directory.CreateDirectory(fixture.GetDataSetVersionPathResolver().DirectoryPath(dataSetVersion));

            await fixture.Function.CompleteInitialDataSetVersionProcessing(instanceId, CancellationToken.None);

            var savedImport = await fixture
                .GetPublicDataDbContext()
                .DataSetVersionImports.Include(i => i.DataSetVersion)
                .SingleAsync(i => i.InstanceId == instanceId);

            Assert.Equal(Stage, savedImport.Stage);
            savedImport.Completed.AssertUtcNow();

            Assert.Equal(DataSetVersionStatus.Draft, savedImport.DataSetVersion.Status);
        }

        [Fact]
        public async Task DuckDbFileIsDeleted()
        {
            var (dataSetVersion, instanceId) = await CommonTestDataUtils.CreateDataSetInitialVersion(
                fixture.GetPublicDataDbContext(),
                Stage.PreviousStage()
            );

            // Create empty data set version files for all file paths
            var directoryPath = fixture.GetDataSetVersionPathResolver().DirectoryPath(dataSetVersion);
            Directory.CreateDirectory(directoryPath);
            foreach (var filename in AllDataSetVersionFiles)
            {
                await File.Create(Path.Combine(directoryPath, filename)).DisposeAsync();
            }

            await fixture.Function.CompleteInitialDataSetVersionProcessing(instanceId, CancellationToken.None);

            // Ensure the duck db database file is the only file that was deleted
            CommonTestDataUtils.AssertDataSetVersionDirectoryContainsOnlyFiles(
                fixture.GetDataSetVersionPathResolver(),
                dataSetVersion,
                AllDataSetVersionFiles.Where(file => file != DataSetFilenames.DuckDbDatabaseFile).ToArray()
            );
        }
    }
}
