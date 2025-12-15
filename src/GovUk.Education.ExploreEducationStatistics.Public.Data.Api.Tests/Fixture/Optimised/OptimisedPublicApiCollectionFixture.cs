using System.Security.Claims;
using GovUk.Education.ExploreEducationStatistics.Analytics.Common.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Common.IntegrationTests.Postgres;
using GovUk.Education.ExploreEducationStatistics.Common.IntegrationTests.UserAuth;
using GovUk.Education.ExploreEducationStatistics.Common.IntegrationTests.WebApp;
using GovUk.Education.ExploreEducationStatistics.Common.Services;
using GovUk.Education.ExploreEducationStatistics.Public.Data.Api.Services.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Public.Data.Api.Services.Interfaces.Search;
using GovUk.Education.ExploreEducationStatistics.Public.Data.Model.Database;
using GovUk.Education.ExploreEducationStatistics.Public.Data.Services.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Public.Data.Services.Tests;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Time.Testing;
using Moq;

namespace GovUk.Education.ExploreEducationStatistics.Public.Data.Api.Tests.Fixture.Optimised;

/// <summary>
///
/// A Collection-level test fixture to be used by Public API integration tests.
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
public abstract class OptimisedPublicApiCollectionFixture(params PublicApiIntegrationTestCapability[] capabilities)
    : OptimisedIntegrationTestFixtureBase<Startup>(minimalApi: true)
{
    private OptimisedTestUserHolder? _userHolder;

    private Func<string> _psqlConnectionString = null!;

    private PublicDataDbContext? _publicDataDbContext;
    private IContentApiClient _contentApiClient = null!;
    private ISearchService _searchService = null!;
    private IAnalyticsService _analyticsService = null!;

    private readonly TestDataSetVersionPathResolver _testDataSetVersionPathResolver = new()
    {
        Directory = "AbsenceSchool",
    };

    private FakeTimeProvider _timeProvider = null!;
    private DateTimeProvider _dateTimeProvider = null!;

    protected override void RegisterTestContainers(TestContainerRegistrations registrations)
    {
        if (capabilities.Contains(PublicApiIntegrationTestCapability.Postgres))
        {
            _psqlConnectionString = registrations.RegisterPostgreSqlContainer();
        }
    }

    protected override void ConfigureServicesAndConfiguration(
        OptimisedServiceAndConfigModifications serviceModifications
    )
    {
        serviceModifications.AddControllers<Startup>();

        serviceModifications
            .ReplaceServiceWithMock<IContentApiClient>()
            .ReplaceServiceWithMock<ISearchService>()
            .ReplaceServiceWithMock<IAnalyticsService>(mockBehavior: MockBehavior.Loose);

        serviceModifications.ReplaceService<IDataSetVersionPathResolver>(_testDataSetVersionPathResolver);

        _timeProvider = new FakeTimeProvider(DateTimeOffset.UtcNow);
        _dateTimeProvider = new DateTimeProvider(_timeProvider.GetUtcNow().UtcDateTime);

        serviceModifications.ReplaceService<TimeProvider>(_timeProvider);
        serviceModifications.ReplaceService(_dateTimeProvider);

        if (capabilities.Contains(PublicApiIntegrationTestCapability.Postgres))
        {
            serviceModifications.AddPostgres<PublicDataDbContext>(_psqlConnectionString());
        }
        else
        {
            serviceModifications.AddSingleton(Mock.Of<PublicDataDbContext>());
        }

        if (capabilities.Contains(PublicApiIntegrationTestCapability.UserAuth))
        {
            serviceModifications.AddUserAuth();
        }
    }

    protected override Task AfterFactoryConstructed(OptimisedServiceCollectionLookups<Startup> lookups)
    {
        if (capabilities.Contains(PublicApiIntegrationTestCapability.UserAuth))
        {
            // Get a reference to the component that allows us to set the user we wish to use for a particular call.
            _userHolder = lookups.GetService<OptimisedTestUserHolder>();
        }

        // Grab reusable DbContexts that can be used for test data setup and test assertions. These are looked up once
        // per startup of a test class that uses this fixture and are disposed of at the end of its lifetime, via XUnit
        // calling "DisposeAsync" on this fixture.
        _publicDataDbContext = lookups.GetService<PublicDataDbContext>();

        // Look up the Mocks surrounding mocked-out dependencies once per test class using this fixture.
        // Test classes can then use the Mocks for setups and verifications, as the Mocks will be the same ones
        // as used in the tested code itself.
        _contentApiClient = lookups.GetService<IContentApiClient>();
        _searchService = lookups.GetService<ISearchService>();
        _analyticsService = lookups.GetService<IAnalyticsService>();

        if (capabilities.Contains(PublicApiIntegrationTestCapability.Postgres))
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
    }

    /// <summary>
    /// Create an HttpClient and set a specific user to handle the request.
    /// </summary>
    public HttpClient CreateClient(ClaimsPrincipal user)
    {
        SetUser(user);
        return base.CreateClient();
    }

    public DateTimeOffset GetUtcNow()
    {
        return _timeProvider.GetUtcNow();
    }

    /// <summary>
    /// Get a reusable DbContext that should be used for setting up test data and making test assertions.
    /// </summary>
    public PublicDataDbContext GetPublicDataDbContext() => _publicDataDbContext!;

    /// <summary>
    /// Get a Mock representing this dependency that can be used for setups and verifications. This mock will be used
    /// within the tested code itself.
    /// </summary>
    public Mock<IContentApiClient> GetContentApiClientMock() => Mock.Get(_contentApiClient);

    /// <summary>
    /// Get a Mock representing this dependency that can be used for setups and verifications. This mock will be used
    /// within the tested code itself.
    /// </summary>
    public Mock<ISearchService> GetSearchServiceMock() => Mock.Get(_searchService);

    /// <summary>
    /// Get a Mock representing this dependency that can be used for setups and verifications. This mock will be used
    /// within the tested code itself.
    /// </summary>
    public Mock<IAnalyticsService> GetAnalyticsServiceMock() => Mock.Get(_analyticsService);

    public TestDataSetVersionPathResolver GetTestDataSetVersionPathResolver() => _testDataSetVersionPathResolver;

    public override Task BeforeEachTest()
    {
        ResetIfMock(_contentApiClient);
        ResetIfMock(_searchService);
        ResetIfMock(_analyticsService);

        if (capabilities.Contains(PublicApiIntegrationTestCapability.UserAuth))
        {
            _userHolder!.SetUser(null);
        }

        return Task.CompletedTask;
    }

    /// <summary>
    /// Adds a user to the test user pool so that they can be used for HttpClient calls and looked up successfully.
    /// </summary>
    private void SetUser(ClaimsPrincipal user)
    {
        if (!capabilities.Contains(PublicApiIntegrationTestCapability.UserAuth))
        {
            throw new Exception("""Cannot register test users if "useTestUserAuthentication" is false.""");
        }

        _userHolder!.SetUser(user);
    }

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

public enum PublicApiIntegrationTestCapability
{
    Postgres,
    UserAuth,
    AnalyticsService,
}
