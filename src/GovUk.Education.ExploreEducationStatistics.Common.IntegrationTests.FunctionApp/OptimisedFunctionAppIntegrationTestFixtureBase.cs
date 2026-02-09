using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace GovUk.Education.ExploreEducationStatistics.Common.IntegrationTests.FunctionApp;

/// <summary>
///
/// This fixture base class supports and controls a high-level and quite generic lifecycle.
///
/// It abstracts the WebApplicationFactory away from fixtures that subclass this fixture and
/// instead controls configuration and access through helper classes that are provided to the
/// subclass via lifecycle methods.
///
/// Essentially this fixture does the following steps:
///
/// 1. A test collection executes.
/// 2. XUnit creates a new instance of the subclass of this fixture.
/// 3. XUnit calls <see cref="InitializeAsync"/> on this fixture.
/// 4. <see cref="InitializeAsync"/> then:
///   1. Allows the subclass to register any Test Containers that it needs, by the subclass overriding the
///      <see cref="RegisterTestContainers"/> method.
///   2. Starts any Test Containers that the subclass needs.
///   3. Builds a new FunctionAppServiceProviderBuilder that will allow us to create an IServiceProvider that supports
///      integration testing.
///   4. Asks the subclass for any modifications it requires to registered services and configuration prior to the
///      IServiceProvider being built, by the subclass overriding the <see cref="ConfigureServicesAndConfiguration" /> method.
///   5. Applies any ServiceCollection, IConfigurationBuilder and IHostBuilder modifications that the subclass asked
///      for to the IServiceProvider.
///   6. Lets the subclass look up any services that it needs after the factory has been constructed, by overriding the
///      <see cref="AfterFactoryConstructed"/> method.
/// 5. The tests in the collection run in sequence, all using this single instance of the fixture subclass.
/// 6. XUnit calls <see cref="DisposeAsync"/> on this fixture.
/// 7. <see cref="DisposeAsync"/> then:
///   1. Stops any Test Containers that the subclass registered.
///   2. Allows the subclass to dispose of any other disposable resources that it's holding references to, by the
///      subclass overriding the <see cref="DisposeResources"/> methods.
///
/// </summary>
public abstract class OptimisedFunctionAppIntegrationTestFixtureBase(Type[] dbContextTypes) : IAsyncLifetime
{
    private TestContainerRegistrations _testContainers = null!;
    private IServiceProvider _serviceProvider = null!;
    private DbContext[] _productionDbContexts = null!;

    protected TestDbContextHolder TestDbContexts = null!;

    public async Task InitializeAsync()
    {
        // Allow the subclass of this fixture to register any Test Containers.
        _testContainers = new TestContainerRegistrations();
        RegisterTestContainers(_testContainers);

        // Start any registered Test Containers.
        await _testContainers.StartAll();

        // Allow the subclass to register changes to any service or configuration setups that need to differ
        // from those found in TStartup e.g. mocking out components that communicate with external services,
        // registering in-memory DbContexts etc.
        var modifications = new OptimisedServiceAndConfigModifications();
        ConfigureServicesAndConfiguration(modifications);

        // Build the standard WebApplicationFactory builder based on the configuration in TStartup, choosing the
        // correct approach based on the type of project being tested.
        var serviceProviderBuilder = new OptimisedFunctionAppServiceProviderBuilder();

        // Build the final IServiceProvider, originally based on a standard test host builder setup and then the
        // fixture subclass' requested service and configuration changes applied over the top.
        _serviceProvider = serviceProviderBuilder.Build(
            serviceModifications: [.. modifications.GetServiceModifications()],
            configModifications: [.. modifications.GetConfigModifications()],
            hostBuilderModifications: [.. modifications.GetHostBuilderModifications()]
        );

        // As Function App integration tests don't use a WebApplicationFactory, new service instances aren't
        // created between tests (as they would be in API integration tests via HttpClient calls).
        // The DbContexts that are available now in the IServiceProvider are therefore the same DbContexts that
        // are used in the production code for every test in a collection.
        //
        // We therefore keep track of them here, and reset their ChangeTrackers between tests so as not to
        // carry dirty DbContexts into the next test.
        _productionDbContexts = dbContextTypes
            .Select(type => _serviceProvider.GetService(type))
            .Cast<DbContext>()
            .ToArray();

        // As above, when looking up DbContexts from an IServiceProvider in Function App integration tests,
        // by default we will end up with the same DbContexts that are usd in the production code. As we want
        // separation between the DbContexts used in production code versus the DbContexts that are used to
        // set up test data and assert data changes, we explicitly look them up using a separate scope, thus
        // forcing the creation of new, separate instances.
        var testDbContexts = dbContextTypes
            .Select(type =>
            {
                var scope = _serviceProvider.CreateScope();
                return scope.ServiceProvider.GetRequiredService(type);
            })
            .Cast<DbContext>()
            .ToArray();

        // Store the separate, non-production DbContexts for subclass fixtures to use.
        TestDbContexts = new TestDbContextHolder(testDbContexts);

        // Finally, allow the fixture subclass to perform any actions post-factory creation and prior to any
        // tests from its collection starting. This might include looking up any services that the tests in the
        // collection might need, setting up some collection-level global test data etc.
        var lookups = new OptimisedServiceCollectionLookups(_serviceProvider);
        await AfterFactoryConstructed(lookups);
    }

    /// <summary>
    /// A method that can be overridden by subclasses of this fixture to register any test containers that they require.
    ///
    /// Containers registered here will be started prior to any tests in a collection running and stopped after all
    /// tests in the collection have finished.
    /// </summary>
    protected virtual void RegisterTestContainers(TestContainerRegistrations registrations) { }

    /// <summary>
    /// A method that can be overridden by subclasses of this fixture to provide any specific alteration of services
    /// and configuration prior to building the IServiceProvider e.g. replacing services with mocks, adding
    /// in-memory DbContexts etc.
    /// </summary>
    protected virtual void ConfigureServicesAndConfiguration(
        OptimisedServiceAndConfigModifications serviceModifications
    ) { }

    /// <summary>
    /// A method that can be overridden by subclasses of this fixture to perform any actions after the
    /// IServiceProvider has been constructed e.g. looking up any special services that the fixture may have
    /// registered via the <see cref="ConfigureServicesAndConfiguration"/> method.
    /// </summary>
    protected virtual Task AfterFactoryConstructed(OptimisedServiceCollectionLookups lookups)
    {
        return Task.CompletedTask;
    }

    /// <summary>
    /// A method that can be overridden by subclasses of this fixture to perform actions before each test
    /// in a collection runs e.g. resetting any shared Mocks that were dirtied in previous tests in the
    /// collection, unsetting users from handling requests etc.
    /// </summary>
    public virtual async Task BeforeEachTest()
    {
        foreach (var productionDbContext in _productionDbContexts)
        {
            productionDbContext.ChangeTracker.Clear();
        }

        TestDbContexts.ResetAnyMocks();

        // In-memory DbContexts can be cleared down by default with no speed penalty.
        // Proper DbContexts add considerable time to a full project run if clearing
        // between every test, and therefore we don't clear them down by default.
        await TestDbContexts.ClearInMemoryTestData();
    }

    protected virtual Task DisposeResources()
    {
        return Task.CompletedTask;
    }

    public async Task DisposeAsync()
    {
        await _testContainers.StopAll();
        await TestDbContexts.DisposeAll();
        await DisposeResources();
    }
}
