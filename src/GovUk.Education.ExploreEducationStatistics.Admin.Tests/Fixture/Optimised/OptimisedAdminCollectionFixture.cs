using System.Security.Claims;
using GovUk.Education.ExploreEducationStatistics.Admin.Database;
using GovUk.Education.ExploreEducationStatistics.Admin.Services.Interfaces.Public.Data;
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
/// A Collection-level test fixture to be used by integration tests that require both an <see cref="HttpClient"/> and
/// access to a <see cref="PostgreSqlContainer"/> TestContainer instance. For best performance, each test suite class
/// requiring this fixture should create its own subclass of it so that it can run in parallel with other test suites
/// also requiring this fixture's features.
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
/// test class.
///
/// Each test suite using a subclass of this fixture will receive its own isolated version of it.
///
/// Note that in order to allow multiple suites to run in parallel whilst using this fixture, we do not allow automatic
/// test data teardowns of entire databases. Instead, each test using this fixture should be written in a fashion
/// whereby it does not assume that the database is empty at the start of its run, or that it can check for the
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
    public virtual async Task InitializeAsync()
    {
        if (capabilities.Contains(AdminIntegrationTestCapability.Postgres))
        {
            _psql = new OptimisedPsqlContainerWrapper();
            // Invoke "start" on the Postgres container only once per test collection (i.e. per test suite using this
            // fixture).
            await _psql.Start();
        }

        // Configure the factory that represents Admin (with some mocks swapped in where appropriate and commonly used).
        // Configure it with a PublicDataDbContext that has a connection to the PSQL TestContainer.
        // Each test class using this fixture will use this same factory.
        //
        // The factory is configured thus:
        //
        // 1. The "new WebApplicationFactory<Startup>()" line registers the services as laid out in Admin's Startup
        // class.
        // 2. The "WithReconfiguredAdmin()" line swaps out some of the production services from Startup with
        // test-appropriate ones e.g. swapping real DbContexts for test ones, and HttpClient-based clients that call
        // external services with Mocks.
        // 3. The "WithPostgres(_psql.GetContainer())" line swaps the PublicDataDbContext (which has previously been
        // swapped for a mocked version) with one that is connected to the PSQL TestContainer instance being user by
        // this test class.
        // 4. Finally, "Build()" generates the finalised WebApplicationFactory<Startup> instance. At this point, we
        // should not attempt to reconfigure the factory further.
        var factoryBuilder = new WebApplicationFactory<Startup>().WithReconfiguredAdmin();

        if (capabilities.Contains(AdminIntegrationTestCapability.Postgres))
        {
            factoryBuilder = factoryBuilder.WithPostgres(_psql.GetContainer());
        }

        if (capabilities.Contains(AdminIntegrationTestCapability.UserAuth))
        {
            factoryBuilder = factoryBuilder.WithTestUserAuthentication();
        }

        _factory = factoryBuilder.Build();

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
    ///
    /// This method provides access to services within the WebApplicationFactory.
    ///
    /// Because these lookups can be expensive when used excessively, use sparingly. If lots of test methods within
    /// a single test class require access to the same service (e.g. a test class explicitly for DataSetService),
    /// consider subclassing this Fixture class and looking up once in an overridden <see cref="InitializeAsync"/>
    /// method so that all test methods can reuse the same service.
    ///
    /// </summary>
    protected TService GetService<TService>()
        where TService : class
    {
        return _factory.Services.GetRequiredService<TService>();
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
