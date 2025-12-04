#nullable enable
using System.Security.Claims;
using GovUk.Education.ExploreEducationStatistics.Admin.Database;
using GovUk.Education.ExploreEducationStatistics.Admin.Services.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Admin.Services.Interfaces.Public.Data;
using GovUk.Education.ExploreEducationStatistics.Common.IntegrationTests.Azurite;
using GovUk.Education.ExploreEducationStatistics.Common.IntegrationTests.Postgres;
using GovUk.Education.ExploreEducationStatistics.Common.IntegrationTests.UserAuth;
using GovUk.Education.ExploreEducationStatistics.Common.IntegrationTests.WebApp;
using GovUk.Education.ExploreEducationStatistics.Common.Services.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Common.Tests.Fixtures;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Database;
using GovUk.Education.ExploreEducationStatistics.Data.Model.Database;
using GovUk.Education.ExploreEducationStatistics.Data.Processor.Model;
using GovUk.Education.ExploreEducationStatistics.Public.Data.Model.Database;
using GovUk.Education.ExploreEducationStatistics.Publisher.Model;
using Microsoft.EntityFrameworkCore;
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
public abstract class OptimisedAdminCollectionFixture(AdminIntegrationTestCapability[] capabilities)
    : OptimisedIntegrationTestFixtureBase<Startup>
{
    private PublicDataDbContext _publicDataDbContext = null!;
    private ContentDbContext _contentDbContext = null!;
    private StatisticsDbContext _statisticsDbContext = null!;
    private UsersAndRolesDbContext _usersAndRolesDbContext = null!;
    private Mock<IProcessorClient> _processorClientMock = null!;
    private Mock<IPublicDataApiClient> _publicDataApiClientMock = null!;
    private OptimisedTestUserPool _userPool = null!;

    private Func<string> _psqlConnectionString = null!;
    private Func<string> _azuriteConnectionString = null!;

    protected override void RegisterTestContainers(TestContainerRegistrations registrations)
    {
        if (capabilities.Contains(AdminIntegrationTestCapability.Postgres))
        {
            _psqlConnectionString = registrations.RegisterPostgreSqlContainer();
        }

        if (capabilities.Contains(AdminIntegrationTestCapability.Azurite))
        {
            _azuriteConnectionString = registrations.RegisterAzuriteContainer();
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
                connectionString: _azuriteConnectionString(),
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

    protected override void LookupServicesAfterFactoryConstructed(OptimisedServiceCollectionLookups<Startup> lookups)
    {
        if (capabilities.Contains(AdminIntegrationTestCapability.UserAuth))
        {
            // Get a reference to the shared user pool that will be used throughout the life of the test class using this
            // fixture.
            _userPool = lookups.GetService<OptimisedTestUserPool>();
            _userPool.AddUserIfNotExists(OptimisedTestUsers.Authenticated);
            _userPool.AddUserIfNotExists(OptimisedTestUsers.Bau);
            _userPool.AddUserIfNotExists(OptimisedTestUsers.Analyst);
            _userPool.AddUserIfNotExists(OptimisedTestUsers.PreReleaseUser);
            _userPool.AddUserIfNotExists(OptimisedTestUsers.Verified);
            _userPool.AddUserIfNotExists(OptimisedTestUsers.VerifiedButNotAuthorized);
        }

        // Grab reusable DbContexts that can be used for test data setup and test assertions. These are looked up once
        // per startup of a test class that uses this fixture and are disposed of at the end of its lifetime, via XUnit
        // calling "DisposeAsync" on this fixture.
        _publicDataDbContext = lookups.GetService<PublicDataDbContext>();
        _contentDbContext = lookups.GetService<ContentDbContext>();
        _statisticsDbContext = lookups.GetService<StatisticsDbContext>();
        _usersAndRolesDbContext = lookups.GetService<UsersAndRolesDbContext>();

        // Look up the Mocks surrounding mocked-out dependencies once per test class using this fixture.
        // Test classes can then use the Mocks for setups and verifications, as the Mocks will be the same ones
        // as used in the tested code itself.
        _processorClientMock = Mock.Get(lookups.GetService<IProcessorClient>());
        _publicDataApiClientMock = Mock.Get(lookups.GetService<IPublicDataApiClient>());

        if (capabilities.Contains(AdminIntegrationTestCapability.Postgres))
        {
            _publicDataDbContext.Database.Migrate();
        }
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
        await DisposeReusableDbContexts();
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

public static class OptimisedTestUsers
{
    public static ClaimsPrincipal Bau = new DataFixture().BauUser().Generate();

    public static ClaimsPrincipal Analyst = new DataFixture().AnalystUser().Generate();

    public static ClaimsPrincipal Authenticated = new DataFixture().AuthenticatedUser().Generate();

    public static ClaimsPrincipal Verified = new DataFixture().VerifiedByIdentityProviderUser().Generate();

    public static ClaimsPrincipal VerifiedButNotAuthorized = new DataFixture()
        .VerifiedButNotAuthorizedByIdentityProviderUser()
        .Generate();

    public static ClaimsPrincipal PreReleaseUser = new DataFixture().PreReleaseUser().Generate();
}
