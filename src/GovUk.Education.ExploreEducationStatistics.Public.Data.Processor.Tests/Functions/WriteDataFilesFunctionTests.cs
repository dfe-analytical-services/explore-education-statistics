using GovUk.Education.ExploreEducationStatistics.Common.IntegrationTests;
using GovUk.Education.ExploreEducationStatistics.Common.IntegrationTests.FunctionApp;
using GovUk.Education.ExploreEducationStatistics.Public.Data.Model;
using GovUk.Education.ExploreEducationStatistics.Public.Data.Model.Parquet.Tables;
using GovUk.Education.ExploreEducationStatistics.Public.Data.Processor.Functions;
using GovUk.Education.ExploreEducationStatistics.Public.Data.Processor.Tests.Fixture;
using GovUk.Education.ExploreEducationStatistics.Public.Data.Processor.Tests.TestData;
using Microsoft.EntityFrameworkCore;

#pragma warning disable CS9107 // Parameter is captured into the state of the enclosing type and its value is also passed to the base constructor. The value might be captured by the base class as well.

namespace GovUk.Education.ExploreEducationStatistics.Public.Data.Processor.Tests.Functions;

// ReSharper disable once ClassNeverInstantiated.Global
public class WriteDataFilesFunctionTestsFixture()
    : OptimisedPublicDataProcessorCollectionFixture(
        capabilities: [PublicDataProcessorIntegrationTestCapability.Postgres]
    )
{
    public ImportMetadataFunction ImportMetadataFunction = null!;
    public ImportDataFunction ImportDataFunction = null!;
    public WriteDataFilesFunction WriteDataFilesFunction = null!;

    protected override async Task AfterFactoryConstructed(OptimisedServiceCollectionLookups lookups)
    {
        await base.AfterFactoryConstructed(lookups);

        ImportMetadataFunction = lookups.GetService<ImportMetadataFunction>();
        ImportDataFunction = lookups.GetService<ImportDataFunction>();
        WriteDataFilesFunction = lookups.GetService<WriteDataFilesFunction>();
    }
}

[CollectionDefinition(nameof(WriteDataFilesFunctionTestsFixture))]
public class WriteDataFilesFunctionTestsCollection : ICollectionFixture<WriteDataFilesFunctionTestsFixture>;

[Collection(nameof(WriteDataFilesFunctionTestsFixture))]
public abstract class WriteDataFilesFunctionTests(WriteDataFilesFunctionTestsFixture fixture)
    : OptimisedFunctionAppIntegrationTestBase(fixture)
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

    public static readonly TheoryData<ProcessorTestData> Data = [ProcessorTestData.AbsenceSchool];

    public class WriteDataTests(WriteDataFilesFunctionTestsFixture fixture) : WriteDataFilesFunctionTests(fixture)
    {
        [Theory]
        [MemberData(nameof(Data))]
        public async Task Success(ProcessorTestData testData)
        {
            var (dataSetVersion, instanceId) = await CommonTestDataUtils.CreateDataSetInitialVersion(
                fixture.GetPublicDataDbContext(),
                Stage.PreviousStage()
            );

            await WriteData(testData, dataSetVersion, instanceId);

            var savedImport = await fixture
                .GetPublicDataDbContext()
                .DataSetVersionImports.Include(dataSetVersionImport => dataSetVersionImport.DataSetVersion)
                .SingleAsync(i => i.InstanceId == instanceId);

            Assert.Equal(Stage, savedImport.Stage);
            Assert.Equal(DataSetVersionStatus.Processing, savedImport.DataSetVersion.Status);

            CommonTestDataUtils.AssertDataSetVersionDirectoryContainsOnlyFiles(
                fixture.GetDataSetVersionPathResolver(),
                dataSetVersion,
                AllDataSetVersionFiles
            );
        }

        private async Task WriteData(ProcessorTestData testData, DataSetVersion dataSetVersion, Guid instanceId)
        {
            CommonTestDataUtils.SetupCsvDataFilesForDataSetVersion(
                fixture.GetDataSetVersionPathResolver(),
                testData,
                dataSetVersion
            );

            // Prepare the metadata and data before calling the WriteDataFiles function
            await fixture.ImportMetadataFunction.ImportMetadata(instanceId, CancellationToken.None);
            await fixture.ImportDataFunction.ImportData(instanceId, CancellationToken.None);
            await fixture.WriteDataFilesFunction.WriteDataFiles(instanceId, CancellationToken.None);
        }
    }
}
