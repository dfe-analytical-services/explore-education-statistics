using GovUk.Education.ExploreEducationStatistics.Common.Tests;
using GovUk.Education.ExploreEducationStatistics.Common.Tests.Extensions;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Database;
using GovUk.Education.ExploreEducationStatistics.Public.Data.Model;
using GovUk.Education.ExploreEducationStatistics.Public.Data.Model.Database;
using GovUk.Education.ExploreEducationStatistics.Public.Data.Model.DuckDb;
using GovUk.Education.ExploreEducationStatistics.Public.Data.Model.Tests.Fixtures;
using GovUk.Education.ExploreEducationStatistics.Public.Data.Processor.Functions;
using GovUk.Education.ExploreEducationStatistics.Public.Data.Services.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Testcontainers.Azurite;
using Testcontainers.PostgreSql;

namespace GovUk.Education.ExploreEducationStatistics.Public.Data.Processor.Tests;

public abstract class ProcessorFunctionsIntegrationTest(
    FunctionsIntegrationTestFixture fixture)
    : FunctionsIntegrationTest<ProcessorFunctionsIntegrationTestFixture>(fixture), IAsyncLifetime
{
    public async Task InitializeAsync()
    {
        ResetDbContext<ContentDbContext>();
        await ClearTestData<PublicDataDbContext>();
    }

    public Task DisposeAsync()
    {
        var dataSetVersionPathResolver = GetRequiredService<IDataSetVersionPathResolver>();

        var testInstanceDataFilesDirectory = dataSetVersionPathResolver.BasePath();
        if (Directory.Exists(testInstanceDataFilesDirectory))
        {
            Directory.Delete(testInstanceDataFilesDirectory, recursive: true);
        }

        return Task.CompletedTask;
    }

    protected void SetupCsvDataFilesForDataSetVersion(ProcessorTestData processorTestData,
        DataSetVersion dataSetVersion)
    {
        var dataSetVersionPathResolver = GetRequiredService<IDataSetVersionPathResolver>();

        var dataSetVersionDir = dataSetVersionPathResolver.DirectoryPath(dataSetVersion);
        if (!Directory.Exists(dataSetVersionDir))
        {
            Directory.CreateDirectory(dataSetVersionDir);
        }

        // Prepare the data set version directory with data and metadata CSV files
        File.Copy(sourceFileName: processorTestData.CsvDataGzipFilePath,
            destFileName: dataSetVersionPathResolver.CsvDataPath(dataSetVersion));
        File.Copy(sourceFileName: processorTestData.CsvMetadataFilePath,
            destFileName: dataSetVersionPathResolver.CsvMetadataPath(dataSetVersion));
    }

    protected async Task<(DataSetVersion dataSetVersion, Guid instanceId)> CreateDataSet(
        DataSetVersionImportStage importStage,
        DataSetVersionStatus? status = null,
        Guid? releaseFileId = null)
    {
        DataSet dataSet = DataFixture.DefaultDataSet();

        await AddTestData<PublicDataDbContext>(context => context.DataSets.Add(dataSet));

        return await CreateDataSetVersionAndImport(dataSet, importStage, status, releaseFileId);
    }

    protected async Task<(DataSetVersion dataSetVersion, Guid instanceId)> CreateDataSetVersionAndImport(
        DataSet dataSet,
        DataSetVersionImportStage importStage,
        DataSetVersionStatus? status = null,
        Guid? releaseFileId = null,
        int versionMajor = 1,
        int versionMinor = 0)
    {
        DataSetVersionImport dataSetVersionImport = DataFixture
            .DefaultDataSetVersionImport()
            .WithStage(importStage);

        DataSetVersion dataSetVersion = DataFixture
            .DefaultDataSetVersion()
            .WithDataSet(dataSet)
            .WithReleaseFileId(releaseFileId ?? Guid.NewGuid())
            .WithStatus(status ?? DataSetVersionStatus.Processing)
            .WithImports(() => [dataSetVersionImport])
            .WithVersionNumber(major: versionMajor, minor: versionMinor)
            .FinishWith(dsv =>
            {
                if (status == DataSetVersionStatus.Published)
                {
                    dsv.DataSet.LatestLiveVersion = dsv;
                }
                else
                {
                    dsv.DataSet.LatestDraftVersion = dsv;
                }
            });

        await AddTestData<PublicDataDbContext>(context =>
        {
            context.DataSetVersions.Add(dataSetVersion);
            context.DataSets.Update(dataSet);
        });

        return (dataSetVersion, dataSetVersionImport.InstanceId);
    }

    protected DuckDbConnection GetDuckDbConnection(DataSetVersion dataSetVersion)
    {
        var dataSetVersionPathResolver = GetRequiredService<IDataSetVersionPathResolver>();
        return DuckDbConnection.CreateFileConnectionReadOnly(dataSetVersionPathResolver.DuckDbPath(dataSetVersion));
    }

    protected void AssertDataSetVersionDirectoryContainsOnlyFiles(
        DataSetVersion dataSetVersion,
        params string[] expectedFiles)
    {
        var dataSetVersionPathResolver = GetRequiredService<IDataSetVersionPathResolver>();
        var actualFiles = Directory.GetFiles(dataSetVersionPathResolver.DirectoryPath(dataSetVersion))
            .Select(Path.GetFileName)
            .ToArray();

        // Assert that the directory contains the expected files and no others
        Assert.Equal(expectedFiles.Order(), actualFiles.Order());
    }
}

// ReSharper disable once ClassNeverInstantiated.Global
public class ProcessorFunctionsIntegrationTestFixture : FunctionsIntegrationTestFixture, IAsyncLifetime
{
    private readonly PostgreSqlContainer _postgreSqlContainer = new PostgreSqlBuilder()
        .WithImage("postgres:16.1-alpine")
        .Build();

    private readonly AzuriteContainer _azuriteContainer = new AzuriteBuilder()
        .WithImage("mcr.microsoft.com/azure-storage/azurite:3.27.0")
        .Build();

    public async Task DisposeAsync()
    {
        await _azuriteContainer.DisposeAsync();
        await _postgreSqlContainer.DisposeAsync();
    }

    public async Task InitializeAsync()
    {
        await _azuriteContainer.StartAsync();
        await _postgreSqlContainer.StartAsync();
    }

    public override IHostBuilder ConfigureTestHostBuilder()
    {
        return base
            .ConfigureTestHostBuilder()
            .ConfigureProcessorHostBuilder()
            .ConfigureAppConfiguration((_, config) =>
            {
                config.AddInMemoryCollection(new Dictionary<string, string?>
                {
                    { "CoreStorage", _azuriteContainer.GetConnectionString() }
                });
            })
            .ConfigureServices(services =>
            {
                services.UseInMemoryDbContext<ContentDbContext>(databaseName: Guid.NewGuid().ToString());

                services.AddDbContext<PublicDataDbContext>(
                    options =>
                    {
                        options.UseNpgsql(
                            _postgreSqlContainer.GetConnectionString(),
                            psqlOptions => psqlOptions.EnableRetryOnFailure());
                    });

                using var serviceScope = services.BuildServiceProvider()
                    .GetRequiredService<IServiceScopeFactory>()
                    .CreateScope();

                using var context = serviceScope.ServiceProvider.GetRequiredService<PublicDataDbContext>();
                context.Database.Migrate();
            });
    }

    protected override IEnumerable<Type> GetFunctionTypes()
    {
        return
        [
            typeof(CreateDataSetFunction),
            typeof(ProcessInitialDataSetVersionFunction),
            typeof(CreateNextDataSetVersionMappingsFunction),
            typeof(ProcessNextDataSetVersionMappingsFunction),
            typeof(CompleteNextDataSetVersionImportFunction),
            typeof(ProcessCompletionOfNextDataSetVersionFunction),
            typeof(DeleteDataSetVersionFunction),
            typeof(CopyCsvFilesFunction),
            typeof(ImportMetadataFunction),
            typeof(ImportDataFunction),
            typeof(WriteDataFilesFunction),
            typeof(HandleProcessingFailureFunction),
            typeof(HealthCheckFunctions),
            typeof(BulkDeleteDataSetVersionsFunction),
        ];
    }
}
