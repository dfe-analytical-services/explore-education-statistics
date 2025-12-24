#nullable enable
using System.Security.Claims;
using GovUk.Education.ExploreEducationStatistics.Admin.Database;
using GovUk.Education.ExploreEducationStatistics.Admin.Services.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Admin.Services.Interfaces.Public.Data;
using GovUk.Education.ExploreEducationStatistics.Common.IntegrationTests;
using GovUk.Education.ExploreEducationStatistics.Common.IntegrationTests.Azurite;
using GovUk.Education.ExploreEducationStatistics.Common.IntegrationTests.Postgres;
using GovUk.Education.ExploreEducationStatistics.Common.IntegrationTests.UserAuth;
using GovUk.Education.ExploreEducationStatistics.Common.IntegrationTests.WebApp;
using GovUk.Education.ExploreEducationStatistics.Common.Services.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Common.Tests.Extensions;
using GovUk.Education.ExploreEducationStatistics.Common.Tests.Fixtures;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Database;
using GovUk.Education.ExploreEducationStatistics.Data.Model.Database;
using GovUk.Education.ExploreEducationStatistics.Data.Processor.Model;
using GovUk.Education.ExploreEducationStatistics.Public.Data.Model.Database;
using GovUk.Education.ExploreEducationStatistics.Publisher.Model;
using Microsoft.EntityFrameworkCore;
using Moq;

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
public abstract class OptimisedAdminCollectionFixture(params AdminIntegrationTestCapability[] capabilities)
    : OptimisedIntegrationTestFixtureBase<Startup>
{
    private PublicDataDbContext _publicDataDbContext = null!;
    private ContentDbContext _contentDbContext = null!;
    private StatisticsDbContext _statisticsDbContext = null!;
    private UsersAndRolesDbContext _usersAndRolesDbContext = null!;
    private IProcessorClient _processorClient = null!;
    private IPublicDataApiClient _publicDataApiClient = null!;
    private OptimisedTestUserHolder? _userHolder;

    private Func<string> _psqlConnectionString = null!;
    private AzuriteWrapper _azuriteWrapper = null!;

    protected override void RegisterTestContainers(TestContainerRegistrations registrations)
    {
        if (capabilities.Contains(AdminIntegrationTestCapability.Postgres))
        {
            _psqlConnectionString = registrations.RegisterPostgreSqlContainer();
        }

        if (capabilities.Contains(AdminIntegrationTestCapability.Azurite))
        {
            _azuriteWrapper = registrations.RegisterAzuriteContainer();
        }
    }

    protected override void ConfigureServicesAndConfiguration(
        OptimisedServiceAndConfigModifications serviceModifications
    )
    {
        serviceModifications
            .AddInMemoryDbContext<ContentDbContext>(databaseName: $"{nameof(ContentDbContext)}_{Guid.NewGuid()}")
            .AddInMemoryDbContext<StatisticsDbContext>(databaseName: $"{nameof(StatisticsDbContext)}_{Guid.NewGuid()}")
            .AddInMemoryDbContext<UsersAndRolesDbContext>(
                databaseName: $"{nameof(UsersAndRolesDbContext)}_{Guid.NewGuid()}"
            )
            .ReplaceServiceWithMock<IProcessorClient>()
            .ReplaceServiceWithMock<IPublicDataApiClient>()
            .ReplaceServiceWithMock<IDataProcessorClient>()
            .ReplaceServiceWithMock<IPublisherClient>()
            .ReplaceServiceWithMock<IAdminEventRaiser>(MockBehavior.Loose) // Ignore calls to publish events
            .AddControllers<Startup>();

        if (capabilities.Contains(AdminIntegrationTestCapability.Azurite))
        {
            serviceModifications.AddAzurite(
                connectionString: _azuriteWrapper.GetConnectionString(),
                connectionStringKeys: ["PublicStorage", "PublisherStorage", "CoreStorage"]
            );
        }
        else
        {
            serviceModifications
                .ReplaceServiceWithMock<IPublisherTableStorageService>()
                .ReplaceServiceWithMock<IPrivateBlobStorageService>()
                .ReplaceServiceWithMock<IPublicBlobStorageService>();
        }

        if (capabilities.Contains(AdminIntegrationTestCapability.Postgres))
        {
            serviceModifications.AddPostgres<PublicDataDbContext>(_psqlConnectionString());
        }
        else
        {
            serviceModifications.AddSingleton(Mock.Of<PublicDataDbContext>());
        }

        if (capabilities.Contains(AdminIntegrationTestCapability.UserAuth))
        {
            serviceModifications.AddUserAuth();
        }
    }

    protected override Task AfterFactoryConstructed(OptimisedServiceCollectionLookups lookups)
    {
        if (capabilities.Contains(AdminIntegrationTestCapability.UserAuth))
        {
            // Get a reference to the component that allows us to set the user we wish to use for a particular call.
            _userHolder = lookups.GetService<OptimisedTestUserHolder>();
        }

        // Grab reusable DbContexts that can be used for test data setup and test assertions. These are looked up once
        // per startup of a test class that uses this fixture and are disposed of at the end of its lifetime, via XUnit
        // calling "DisposeAsync" on this fixture.
        _publicDataDbContext = lookups.GetService<PublicDataDbContext>();
        _contentDbContext = lookups.GetService<ContentDbContext>();
        _statisticsDbContext = lookups.GetService<StatisticsDbContext>();
        _usersAndRolesDbContext = lookups.GetService<UsersAndRolesDbContext>();

        // Look up commonly mocked-out dependencies once per test class using this fixture. If the test collection
        // needs these as mocks, they can access them using the respective "GetXMock" method in this fixture e.g.
        // "GetProcessorClientMock()". We look up just the plain services here because an individual test collection
        // fixture may have chosen to inject a real service here rather than the standard mock.
        _processorClient = lookups.GetService<IProcessorClient>();
        _publicDataApiClient = lookups.GetService<IPublicDataApiClient>();

        if (capabilities.Contains(AdminIntegrationTestCapability.Postgres))
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
        await _publicDataDbContext.DisposeAsync();
        await _contentDbContext.DisposeAsync();
        await _statisticsDbContext.DisposeAsync();
        await _usersAndRolesDbContext.DisposeAsync();
    }

    /// <summary>
    /// Create an HttpClient and set a specific user to handle the request.
    /// </summary>
    public HttpClient CreateClient(ClaimsPrincipal user)
    {
        SetUser(user);
        return base.CreateClient();
    }

    /// <summary>
    /// Get a reusable DbContext that should be used for setting up test data and making test assertions.
    /// </summary>
    public ContentDbContext GetContentDbContext() => _contentDbContext;

    /// <summary>
    /// Get a reusable DbContext that should be used for setting up test data and making test assertions.
    /// </summary>
    public StatisticsDbContext GetStatisticsDbContext() => _statisticsDbContext;

    /// <summary>
    /// Get a reusable DbContext that should be used for setting up test data and making test assertions.
    /// </summary>
    public UsersAndRolesDbContext GetUsersAndRolesDbContext() => _usersAndRolesDbContext;

    /// <summary>
    /// Get a reusable DbContext that should be used for setting up test data and making test assertions.
    /// </summary>
    public PublicDataDbContext GetPublicDataDbContext() => _publicDataDbContext;

    /// <summary>
    /// Get a Mock representing this dependency that can be used for setups and verifications. This mock will be used
    /// within the tested code itself.
    /// </summary>
    public Mock<IProcessorClient> GetProcessorClientMock() => Mock.Get(_processorClient);

    /// <summary>
    /// Get a Mock representing this dependency that can be used for setups and verifications. This mock will be used
    /// within the tested code itself.
    /// </summary>
    public Mock<IPublicDataApiClient> GetPublicDataApiClientMock() => Mock.Get(_publicDataApiClient);

    /// <summary>
    /// This method is run prior to each individual test in a collection. Here we reset any commonly-used mocks and
    /// ensure no users are set to handle HttpClient requests by default.
    /// </summary>
    public override async Task BeforeEachTest()
    {
        ResetIfMock(_processorClient);
        ResetIfMock(_publicDataApiClient);

        if (capabilities.Contains(AdminIntegrationTestCapability.UserAuth))
        {
            _userHolder!.SetUser(null);
        }

        // In-memory DbContexts can be cleared down by default with no speed penalty.
        // Proper DbContexts add considerable time to a full project run if clearing
        // between every test, and therefore we don't clear them down by default.
        await _contentDbContext.ClearTestDataIfInMemory();
        await _statisticsDbContext.ClearTestDataIfInMemory();
        await _publicDataDbContext.ClearTestDataIfInMemory();
    }

    /// <summary>
    /// Adds a user to the test user pool so that they can be used for HttpClient calls and looked up successfully.
    /// </summary>
    private void SetUser(ClaimsPrincipal? user)
    {
        if (!capabilities.Contains(AdminIntegrationTestCapability.UserAuth))
        {
            throw new Exception("""Cannot register test users if "useTestUserAuthentication" is false.""");
        }

        _userHolder!.SetUser(user);
    }

    /// <summary>
    /// Resets a mock for a given service if the service has been mocked.
    ///
    /// We support this not necessarily being a mock because a fixture subclass may have chosen to inject a real
    /// service in place of a service that is generally mocked out.
    /// </summary>
    private void ResetIfMock<TService>(TService service)
        where TService : class
    {
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

public enum AdminIntegrationTestCapability
{
    Postgres,
    Azurite,
    UserAuth,
}

public static class OptimisedTestUsers
{
    public static readonly ClaimsPrincipal Bau = new DataFixture().BauUser();

    public static readonly ClaimsPrincipal Authenticated = new DataFixture().AuthenticatedUser();

    public static readonly ClaimsPrincipal PreReleaseUser = new DataFixture().PreReleaseUser();
}
