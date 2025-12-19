using GovUk.Education.ExploreEducationStatistics.Common.IntegrationTests;
using GovUk.Education.ExploreEducationStatistics.Common.IntegrationTests.Azurite;
using GovUk.Education.ExploreEducationStatistics.Common.IntegrationTests.FunctionApp;
using GovUk.Education.ExploreEducationStatistics.Common.IntegrationTests.Postgres;
using GovUk.Education.ExploreEducationStatistics.Common.Services.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Database;
using GovUk.Education.ExploreEducationStatistics.Public.Data.Model.Database;
using GovUk.Education.ExploreEducationStatistics.Public.Data.Processor.Functions;
using GovUk.Education.ExploreEducationStatistics.Public.Data.Services.Interfaces;
using Microsoft.EntityFrameworkCore;
using Moq;

namespace GovUk.Education.ExploreEducationStatistics.Public.Data.Processor.Tests.Fixture;

/// <summary>
///
/// A Collection-level test fixture to be used by the Public Data Processor integration tests.
///
/// A number of capabilities are supported by this fixture, and each subclass can specify the capabilities that they
/// need.  The relevant configuration changes and Test Containers will then be put in place to support this for the
/// lifetime of this fixture.
///
/// This fixture is intended to be used specifically as a Collection-level fixture, and thus is able to provide
/// reusable components that might otherwise not be thread-safe e.g. DbContexts. The reason why this is possible
/// by being a Collection-level fixture is that no tests within a single Collection will ever run in parallel.
///
/// For best optimisation, each test suite intending to use this fixture should create its *own* subclass of this
/// class. The reason for this is that it allows multiple test classes to run in parallel, but with their own versions
/// of this Collection fixture. Within the individual test classes themselves, their tests will run sequentially and
/// therefore able to safely use the reusable components of this fixture. We therefore get the speed of running multiple
/// test suites in parallel, along with the speed of reusing expensive-to-look-up components with only one lookup per
/// test collection.
///
/// Each test suite using a subclass of this fixture will receive its own isolated version of it.
///
/// Note that while test data teardown after each test in a collection is supported, it's encouraged to write
/// integration tests in a way that allows them to run without the need to have an empty database, as teardowns per
/// test method adds up to significant extra time added.  Instead, each test using this fixture should be written in a
/// fashion whereby it does not assume that the database is empty at the start of its run, or that it can check for the
/// existence of data in the assertions phase without providing decent enough "WHERE" clauses to look for specific data
/// that was created, deleted or updated by its own actions.
///
/// </summary>
// ReSharper disable once ClassNeverInstantiated.Global
public class OptimisedPublicDataProcessorCollectionFixture(
    params PublicDataProcessorIntegrationTestCapability[] capabilities
) : OptimisedFunctionAppIntegrationTestFixtureBase
{
    private Func<string> _psqlConnectionString = null!;
    private AzuriteWrapper _azuriteWrapper = null!;

    private PublicDataDbContext? _publicDataDbContext;
    private ContentDbContext _contentDbContext = null!;

    private IDataSetVersionPathResolver _dataSetVersionPathResolver = null!;

    protected override void RegisterTestContainers(TestContainerRegistrations registrations)
    {
        if (capabilities.Contains(PublicDataProcessorIntegrationTestCapability.Postgres))
        {
            _psqlConnectionString = registrations.RegisterPostgreSqlContainer();
        }

        if (capabilities.Contains(PublicDataProcessorIntegrationTestCapability.Azurite))
        {
            _azuriteWrapper = registrations.RegisterAzuriteContainer();
        }
    }

    protected override void ConfigureServicesAndConfiguration(
        OptimisedServiceAndConfigModifications serviceModifications
    )
    {
        serviceModifications.AddHostBuilderModifications(hostBuilder => hostBuilder.ConfigureProcessorHostBuilder());

        serviceModifications.AddInMemoryDbContext<ContentDbContext>();

        if (capabilities.Contains(PublicDataProcessorIntegrationTestCapability.Postgres))
        {
            serviceModifications.AddPostgres<PublicDataDbContext>(_psqlConnectionString());
        }
        else
        {
            serviceModifications.AddSingleton(Mock.Of<PublicDataDbContext>());
        }

        if (capabilities.Contains(PublicDataProcessorIntegrationTestCapability.Azurite))
        {
            serviceModifications.AddAzurite(
                connectionString: _azuriteWrapper.GetConnectionString(),
                connectionStringKeys: ["CoreStorage"]
            );
        }
        else
        {
            serviceModifications.ReplaceServiceWithMock<IPrivateBlobStorageService>();
        }

        var dataFilesBasePath = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());

        serviceModifications.AddInMemoryCollection([
            new KeyValuePair<string, string?>("DataFiles:BasePath", dataFilesBasePath),
        ]);

        serviceModifications.AddSingleton<CreateDataSetFunction>();
        serviceModifications.AddSingleton<CompleteInitialDataSetVersionProcessingFunction>();
        serviceModifications.AddSingleton<CreateNextDataSetVersionMappingsFunction>();
        serviceModifications.AddSingleton<ProcessNextDataSetVersionMappingsFunctions>();
        serviceModifications.AddSingleton<CompleteNextDataSetVersionImportFunction>();
        serviceModifications.AddSingleton<ProcessCompletionOfNextDataSetVersionFunctions>();
        serviceModifications.AddSingleton<DeleteDataSetVersionFunction>();
        serviceModifications.AddSingleton<CopyCsvFilesFunction>();
        serviceModifications.AddSingleton<ImportMetadataFunction>();
        serviceModifications.AddSingleton<ImportDataFunction>();
        serviceModifications.AddSingleton<WriteDataFilesFunction>();
        serviceModifications.AddSingleton<HandleProcessingFailureFunction>();
        serviceModifications.AddSingleton<HealthCheckFunctions>();
        serviceModifications.AddSingleton<BulkDeleteDataSetVersionsFunction>();
        serviceModifications.AddSingleton<StatusCheckFunction>();
    }

    protected override Task AfterFactoryConstructed(OptimisedServiceCollectionLookups lookups)
    {
        // Grab reusable DbContexts that can be used for test data setup and test assertions. These are looked up once
        // per startup of a test class that uses this fixture and are disposed of at the end of its lifetime, via XUnit
        // calling "DisposeAsync" on this fixture.
        _publicDataDbContext = lookups.GetService<PublicDataDbContext>();
        _contentDbContext = lookups.GetService<ContentDbContext>();

        _dataSetVersionPathResolver = lookups.GetService<IDataSetVersionPathResolver>();

        if (capabilities.Contains(PublicDataProcessorIntegrationTestCapability.Postgres))
        {
            _publicDataDbContext.Database.Migrate();
        }

        return Task.CompletedTask;
    }

    /// <summary>
    ///
    /// This is called by the XUnit lifecycle management of test fixtures. Once the test suite that is using this
    /// fixture has finished, this method is called for us to dispose of any disposable resources that we are keeping
    /// handles on.
    ///
    /// For example, the reusable DbContexts that we use to seed test data and make assertions with are disposed of
    /// here, ensuring that they do not hang around and allows the full disposal of the WebApplicationFactory instance
    /// that was used by that test class.
    ///
    /// </summary>
    protected override async Task DisposeResources()
    {
        // Dispose of any DbContexts when the test class that was using this fixture has completed.
        if (_publicDataDbContext != null)
        {
            await _publicDataDbContext.DisposeAsync();
        }

        await _contentDbContext.DisposeAsync();
    }

    /// <summary>
    /// Get a reusable DbContext that should be used for setting up test data and making test assertions.
    /// </summary>
    public PublicDataDbContext GetPublicDataDbContext() => _publicDataDbContext!;

    /// <summary>
    /// Get a reusable DbContext that should be used for setting up test data and making test assertions.
    /// </summary>
    public ContentDbContext GetContentDbContext() => _contentDbContext;

    public IDataSetVersionPathResolver GetDataSetVersionPathResolver() => _dataSetVersionPathResolver;

    /// <summary>
    /// This method is run prior to each individual test in a collection. Here we reset any commonly-used mocks and
    /// ensure no users are set to handle HttpClient requests by default.
    /// </summary>
    public override Task BeforeEachTest()
    {
        ResetIfMock(_publicDataDbContext);
        ResetIfMock(_contentDbContext);

        return Task.CompletedTask;
    }

    /// <summary>
    /// Resets a mock for a given service if the service has been mocked.
    ///
    /// We support this not necessarily being a mock because a fixture subclass may have chosen to inject a real
    /// service in place of a service that is generally mocked out.
    /// </summary>
    private void ResetIfMock<TService>(TService? service)
        where TService : class
    {
        if (service == null)
        {
            return;
        }

        try
        {
            var mock = Mock.Get(service);
            mock.Reset();
        }
        catch
        {
            // "service" is not a Mock. This is fine.
        }
    }
}

public enum PublicDataProcessorIntegrationTestCapability
{
    Postgres,
    Azurite,
}
