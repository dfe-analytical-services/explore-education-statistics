using GovUk.Education.ExploreEducationStatistics.Common.Tests;
using GovUk.Education.ExploreEducationStatistics.Common.Tests.Extensions;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Database;
using GovUk.Education.ExploreEducationStatistics.Public.Data.Model.Database;
using GovUk.Education.ExploreEducationStatistics.Public.Data.Processor.Functions;
using GovUk.Education.ExploreEducationStatistics.Public.Data.Services.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Public.Data.Services.Tests;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Testcontainers.Azurite;
using Testcontainers.PostgreSql;

namespace GovUk.Education.ExploreEducationStatistics.Public.Data.Processor.Tests;

public abstract class ProcessorFunctionsIntegrationTest
    : FunctionsIntegrationTest<ProcessorFunctionsIntegrationTestFixture>
{
    protected ProcessorFunctionsIntegrationTest(FunctionsIntegrationTestFixture fixture) : base(fixture)
    {
        ResetDbContext<ContentDbContext>();
        ClearTestData<PublicDataDbContext>();
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
                    {
                        "CoreStorage", _azuriteContainer.GetConnectionString()
                    }
                });
            })
            .ConfigureServices(services =>
            {
                services.UseInMemoryDbContext<ContentDbContext>(databaseName: Guid.NewGuid().ToString());

                services.AddDbContext<PublicDataDbContext>(
                    options => options.UseNpgsql(_postgreSqlContainer.GetConnectionString()));

                using var serviceScope = services.BuildServiceProvider()
                    .GetRequiredService<IServiceScopeFactory>()
                    .CreateScope();

                using var context = serviceScope.ServiceProvider.GetRequiredService<PublicDataDbContext>();
                context.Database.Migrate();

                services.ReplaceService<IDataSetVersionPathResolver>(new TestDataSetVersionPathResolver());
            });
    }

    protected override IEnumerable<Type> GetFunctionTypes()
    {
        return
        [
            typeof(CopyCsvFilesFunction),
            typeof(CreateInitialDataSetVersionFunction),
            typeof(ProcessInitialDataSetVersionFunction),
        ];
    }
}
