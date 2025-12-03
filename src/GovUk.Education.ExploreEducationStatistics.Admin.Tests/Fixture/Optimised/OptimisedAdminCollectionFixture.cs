using System.Security.Claims;
using GovUk.Education.ExploreEducationStatistics.Admin.Database;
using GovUk.Education.ExploreEducationStatistics.Admin.Services.Interfaces.Public.Data;
using GovUk.Education.ExploreEducationStatistics.Admin.Tests.Fixture.Optimised.TestContainerWrappers;
using GovUk.Education.ExploreEducationStatistics.Common.Tests.Extensions;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Database;
using GovUk.Education.ExploreEducationStatistics.Data.Model.Database;
using GovUk.Education.ExploreEducationStatistics.Public.Data.Model.Database;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using Testcontainers.PostgreSql;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Tests.Fixture.Optimised;

/// <summary>
///
/// A Collection-level test fixture to be used by Admin integration tests.
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
public abstract class OptimisedAdminCollectionFixture(AdminIntegrationTestCapability[] capabilities) : IAsyncLifetime
{
    private WebApplicationFactory<Startup> _factory;
    private PublicDataDbContext _publicDataDbContext;
    private ContentDbContext _contentDbContext;
    private StatisticsDbContext _statisticsDbContext;
    private UsersAndRolesDbContext _usersAndRolesDbContext;
    private Mock<IProcessorClient> _processorClientMock;
    private Mock<IPublicDataApiClient> _publicDataApiClientMock;

    private OptimisedPsqlContainerWrapper _psql;
    private OptimisedTestUserPool _userPool;

    private OptimisedAzuriteContainerWrapper _azurite;

    /// <summary>
    ///
    /// This method is invoked by XUnit's standard fixture lifetime management. When a test class is registered to
    /// use this fixture, XUnit will create a new instance of this fixture and call "InitializeAsync" on it prior to
    /// executing the test class itself.
    ///
    /// Note that if the test class is also implementing <see cref="IAsyncLifetime"/>, its own "InitializeAsync" method
    /// will be called after being constructed. It is therefore safe to use this fixture within a test class's
    /// "InitializeAsync" method, or indeed in its constructor or anywhere else for that matter. Its "InitializeAsync"
    /// method is preferable so that we can use async methods of this fixture more effectively.
    ///
    /// </summary>
    public async Task InitializeAsync()
    {
        if (capabilities.Contains(AdminIntegrationTestCapability.Postgres))
        {
            _psql = new OptimisedPsqlContainerWrapper();
            // Invoke "start" on the Postgres container only once per test collection (i.e. per test suite using this
            // fixture).
            await _psql.Start();
        }

        if (capabilities.Contains(AdminIntegrationTestCapability.Azurite))
        {
            _azurite = new OptimisedAzuriteContainerWrapper();
            // Invoke "start" on the Azurite container only once per test collection (i.e. per test suite using this
            // fixture).
            await _azurite.Start();
        }

        // Build the WebApplicationFactory from the various standard capabilities that have been chosen by the test
        // class using this fixture, plus any additional test-specific configuration that a particular class may need to
        // apply prior to building the factory.
        _factory = BuildWebApplicationFactory();

        if (capabilities.Contains(AdminIntegrationTestCapability.UserAuth))
        {
            // Get a reference to the shared user pool that will be used throughout the life of the test class using this
            // fixture.
            _userPool = _factory.Services.GetRequiredService<OptimisedTestUserPool>();
        }

        // Grab reusable DbContexts that can be used for test data setup and test assertions. These are looked up once
        // per startup of a test class that uses this fixture and are disposed of at the end of its lifetime, via XUnit
        // calling "DisposeAsync" on this fixture.
        _publicDataDbContext = _factory.Services.GetRequiredService<PublicDataDbContext>();
        _contentDbContext = _factory.Services.GetRequiredService<ContentDbContext>();
        _statisticsDbContext = _factory.Services.GetRequiredService<StatisticsDbContext>();
        _usersAndRolesDbContext = _factory.Services.GetRequiredService<UsersAndRolesDbContext>();

        // Look up the Mocks surrounding mocked-out dependencies once per test class using this fixture.
        // Test classes can then use the Mocks for setups and verifications, as the Mocks will be the same ones
        // as used in the tested code itself.
        _processorClientMock = Mock.Get(_factory.Services.GetRequiredService<IProcessorClient>());
        _publicDataApiClientMock = Mock.Get(_factory.Services.GetRequiredService<IPublicDataApiClient>());

        // Call a lifecycle method to allow fixture subclasses to look up services after the finalised factory has been
        // created.
        var lookups = new OptimisedServiceCollectionLookups(_factory);
        AfterFactoryConstructed(lookups);
    }

    private WebApplicationFactory<Startup> BuildWebApplicationFactory()
    {
        // Configure the factory that represents Admin (with some mocks swapped in where appropriate and commonly used).
        //
        // This factory will also be built with additional capabilities according to the needs of the individual fixture
        // classes that extend this base class.
        //
        // The factory is configured thus:
        //
        // 1. The "new WebApplicationFactory<Startup>()" line registers the services as laid out in Admin's Startup
        // class.
        // 2. The "WithReconfiguredAdmin()" line swaps out some of the production services from Startup with
        // test-appropriate ones e.g. swapping real DbContexts for test ones, and HttpClient-based clients that call
        // external services with Mocks.
        // 3. If the "Postgres" capability is required, the "WithPostgres(_psql.GetContainer())" line swaps the
        // PublicDataDbContext (which had previously been swapped for a mocked version) with one that is connected to
        // the PSQL TestContainer instance being user by this test class. A TestContainer will be started for this test
        // or an existing one reused if one is already running.
        // 4. If the "Azurite" capability is required, the "factoryBuilder.WithAzurite(_azurite.GetContainer())" line
        // swaps the various Blob services (which had previously been swapped for a mocked version) with one that is
        // connected to the Azurite TestContainer instance being user by this test class. A TestContainer will be
        // started for this test or an existing one reused if one is already running.
        // 5. If the "UserAuth" capability is required, the "factoryBuilder.WithTestUserAuthentication()" line registers
        // a test AuthenticationHandler that can be used with an HttpClient to specify a particular user to be using
        // during a test.
        // 6. If necessary, particular fixtures that are subclassing this fixture class can then make any final
        // adjustments to the factory before it is built, by implementing the ModifyServices() method. The line
        // factoryBuilder.WithServiceCollectionModification(testSpecificServiceModifications) will use these amendments
        // to make final adjustments to the factory.
        // 7. Finally, "Build()" generates the finalised WebApplicationFactory<Startup> instance. At this point, we
        // should not attempt to reconfigure the factory further.
        var factoryBuilder = new WebApplicationFactory<Startup>().WithReconfiguredAdmin();

        if (capabilities.Contains(AdminIntegrationTestCapability.Postgres))
        {
            factoryBuilder = factoryBuilder.WithPostgres(_psql.GetContainer());
        }

        if (capabilities.Contains(AdminIntegrationTestCapability.Azurite))
        {
            factoryBuilder = factoryBuilder.WithAzurite(_azurite.GetContainer());
        }

        if (capabilities.Contains(AdminIntegrationTestCapability.UserAuth))
        {
            factoryBuilder = factoryBuilder.WithTestUserAuthentication();
        }

        // Call a lifecycle method to allow any test classes that require fine-tuned modification of the
        // WebApplicationFactory to apply their modifications at the end of the factory setup for their fixtures.
        var serviceModifications = new OptimisedServiceCollectionModifications();
        ModifyServices(serviceModifications);
        factoryBuilder = factoryBuilder.WithServiceCollectionModification(serviceModifications);

        // Finally, build the final factory that will be used for all the test classes that use this fixture.
        return factoryBuilder.Build();
    }

    protected virtual void ModifyServices(OptimisedServiceCollectionModifications serviceModifications) { }

    protected virtual void AfterFactoryConstructed(OptimisedServiceCollectionLookups lookups) { }

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
    public async Task DisposeAsync()
    {
        // TODO EES-6450 - do we need this?
        // if (capabilities.Contains(AdminIntegrationTestCapability.Postgres))
        // {
        //     await _psql.Stop();
        // }

        await DisposeReusableDbContexts();
    }

    /// <summary>
    /// Creates an HttpClient that can be used to send HTTP requests to the WebApplicationFactory.
    /// </summary>
    public HttpClient CreateClient()
    {
        return _factory.CreateClient();
    }

    /// <summary>
    /// Adds a user to the test user pool so that they can be used for HttpClient calls and looked up successfully.
    /// </summary>
    public void RegisterTestUser(ClaimsPrincipal user)
    {
        if (!capabilities.Contains(AdminIntegrationTestCapability.UserAuth))
        {
            throw new Exception("""Cannot register test users if "useTestUserAuthentication" is false.""");
        }
        _userPool.AddUserIfNotExists(user);
    }

    /// <summary>
    /// Get a reusable DbContext that should be used for setting up test data and making test assertions.
    /// </summary>
    public ContentDbContext GetContentDbContext()
    {
        return _contentDbContext;
    }

    /// <summary>
    /// Get a reusable DbContext that should be used for setting up test data and making test assertions.
    /// </summary>
    public StatisticsDbContext GetStatisticsDbContext()
    {
        return _statisticsDbContext;
    }

    /// <summary>
    /// Get a reusable DbContext that should be used for setting up test data and making test assertions.
    /// </summary>
    public UsersAndRolesDbContext GetUsersAndRolesDbContext()
    {
        return _usersAndRolesDbContext;
    }

    /// <summary>
    /// Get a reusable DbContext that should be used for setting up test data and making test assertions.
    /// </summary>
    public PublicDataDbContext GetPublicDataDbContext()
    {
        return _publicDataDbContext;
    }

    /// <summary>
    /// Get a Mock representing this dependency that can be used for setups and verifications. This mock will be used
    /// within the tested code itself.
    /// </summary>
    public Mock<IProcessorClient> GetProcessorClientMock()
    {
        return _processorClientMock;
    }

    /// <summary>
    /// Get a Mock representing this dependency that can be used for setups and verifications. This mock will be used
    /// within the tested code itself.
    /// </summary>
    public Mock<IPublicDataApiClient> GetPublicDataApiClientMock()
    {
        return _publicDataApiClientMock;
    }

    /// <summary>
    /// Dispose of any DbContexts when the test class that was using this fixture has completed.
    /// </summary>
    private async Task DisposeReusableDbContexts()
    {
        await _publicDataDbContext.DisposeAsync();
        await _contentDbContext.DisposeAsync();
        await _statisticsDbContext.DisposeAsync();
        await _usersAndRolesDbContext.DisposeAsync();
    }
}

public enum AdminIntegrationTestCapability
{
    Postgres,
    Azurite,
    UserAuth,
}

/// <summary>
///
/// A helper class to control access to WebApplicationFactory modifications for tests.
///
/// By exposing only safe methods of reconfiguring the factory, we can better control the types of changes that we
/// are able to perform, and it also makes it more visible when classes need particular additional support through
/// reconfiguration.
///
/// By using this helper class, we also prevent direct access to the factory from test classes.
///
/// </summary>
public class OptimisedServiceCollectionModifications
{
    internal readonly List<Action<IServiceCollection>> Actions = new();

    public OptimisedServiceCollectionModifications AddController(Type controllerType)
    {
        Actions.Add(services =>
            services.AddControllers().AddApplicationPart(controllerType.Assembly).AddControllersAsServices()
        );
        return this;
    }

    public OptimisedServiceCollectionModifications ReplaceService<T>(T service)
        where T : class
    {
        Actions.Add(services => services.ReplaceService(service));
        return this;
    }

    public OptimisedServiceCollectionModifications ReplaceServiceWithMock<T>()
        where T : class
    {
        Actions.Add(services => services.MockService<T>());
        return this;
    }
}

/// <summary>
///
/// A helper class to allow tests to look up services and mocks after the factory has been configured.
/// Through using this helper class to perform service lookups, we prevent direct access to the factory from tests.
///
/// </summary>
public class OptimisedServiceCollectionLookups(WebApplicationFactory<Startup> factory)
{
    /// <summary>
    ///
    /// Look up a service from the factory.  If it's required to look up a mocked service, use the
    /// <see cref="GetMockService{TService}" /> method instead.
    ///
    /// </summary>
    public TService GetService<TService>()
        where TService : class
    {
        return factory.Services.GetRequiredService<TService>();
    }

    /// <summary>
    ///
    /// Look up a mocked service from the factory.
    ///
    /// </summary>
    public Mock<TService> GetMockService<TService>()
        where TService : class
    {
        return Mock.Get(factory.Services.GetRequiredService<TService>());
    }
}
