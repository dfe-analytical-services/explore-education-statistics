using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace GovUk.Education.ExploreEducationStatistics.Common.IntegrationTests.WebApp;

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
///   3. Builds a new WebApplicationFactoryBuilder that will allow us to  create a WebApplicationFactory that is based
///      on <see cref="TStartup"/> but with amendments made to allow testing.
///   4. Asks the subclass for any modifications it requires to registered services and configuration prior to the
///      WebApplicationFactory being built, by the subclass overriding the
///      <see cref="ConfigureServicesAndConfiguration" /> method.
///   5. Applies any ServiceCollection and IConfigurationBuilder modifications that the subclass asked for to the
///      builder.
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
///
/// <param name="minimalApi">
///
/// This parameter controls the mechanism involved when building the WebApplicationFactory, as classic MVC projects and
/// minimal API projects require different approaches.
///
/// </param>
public abstract class OptimisedIntegrationTestFixtureBase<TStartup>(Type[] dbContextTypes, bool minimalApi = false)
    : IAsyncLifetime
    where TStartup : class
{
    private TestContainerRegistrations _testContainers = null!;
    private WebApplicationFactory<TStartup> _factory = null!;

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
        OptimisedWebApplicationFactoryBuilderBase<TStartup> factoryBuilder = minimalApi
            ? new OptimisedWebApplicationFactoryMinimalApiBuilder<TStartup>()
            : new OptimisedWebApplicationFactoryMvcBuilder<TStartup>();

        // Build the final WebApplicationFactory, originally based on TStartup but with standard integration
        // testing support added by the WebApplicationFactory builder and then the fixture subclass' requested
        // service and configuration changes applied over the top.
        _factory = factoryBuilder.Build(
            serviceModifications: [.. modifications.GetServiceModifications()],
            configModifications: [.. modifications.GetConfigModifications()]
        );

        // Look up all required DbContexts and make them available to the fixture subclass.
        var dbContexts = dbContextTypes.Select(type => _factory.Services.GetService(type)).Cast<DbContext>().ToArray();
        TestDbContexts = new TestDbContextHolder(dbContexts);

        // Finally, allow the fixture subclass to perform any actions post-factory creation and prior to any
        // tests from its collection starting. This might include looking up any services that the tests in the
        // collection might need, setting up some collection-level global test data etc.
        var lookups = new OptimisedServiceCollectionLookups(_factory.Services);
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
    /// and configuration prior to building the WebApplicationFactory e.g. replacing services with mocks, adding
    /// in-memory DbContexts etc.
    /// </summary>
    protected virtual void ConfigureServicesAndConfiguration(
        OptimisedServiceAndConfigModifications serviceModifications
    ) { }

    /// <summary>
    /// A method that can be overridden by subclasses of this fixture to perform any actions after the
    /// WebApplicationFactory has been constructed e.g. looking up any special services that the fixture may have
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
        // In-memory DbContexts can be cleared down by default with no speed penalty.
        // Proper DbContexts add considerable time to a full project run if clearing
        // between every test, and therefore we don't clear them down by default.
        await TestDbContexts.ClearInMemoryTestData();

        TestDbContexts.ResetAnyMocks();
    }

    /// <summary>
    /// Creates an HttpClient that can be used to send HTTP requests to the WebApplicationFactory.
    /// </summary>
    public HttpClient CreateClient()
    {
        return _factory.CreateClient();
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
