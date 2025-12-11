using System.Security.Claims;
using GovUk.Education.ExploreEducationStatistics.Common.IntegrationTests.Postgres;
using GovUk.Education.ExploreEducationStatistics.Common.IntegrationTests.UserAuth;
using GovUk.Education.ExploreEducationStatistics.Common.IntegrationTests.WebApp;
using GovUk.Education.ExploreEducationStatistics.Common.Services;
using GovUk.Education.ExploreEducationStatistics.Common.Tests.Fixtures;
using GovUk.Education.ExploreEducationStatistics.Public.Data.Api.Security;
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
public abstract class OptimisedPublicApiCollectionFixture(PublicApiIntegrationTestCapability[] capabilities)
    : OptimisedIntegrationTestFixtureBase<Startup>(minimalApi: true)
{
    private OptimisedTestUserHolder _userHolder = null!;

    private Func<string> _psqlConnectionString = null!;

    private PublicDataDbContext _publicDataDbContext = null!;
    private Mock<IContentApiClient> _contentApiClientMock = null!;
    private Mock<ISearchService> _searchServiceMock = null!;
    private Mock<IAnalyticsService> _analyticsServiceMock = null!;

    private readonly TestDataSetVersionPathResolver _testDataSetVersionPathResolver = new()
    {
        Directory = "AbsenceSchool",
    };

    private readonly FakeTimeProvider _timeProvider = new(DateTimeOffset.UtcNow);
    private readonly DateTimeProvider _dateTimeProvider = new(DateTime.UtcNow);

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
        _contentApiClientMock = lookups.GetMockService<IContentApiClient>();
        _searchServiceMock = lookups.GetMockService<ISearchService>();
        _analyticsServiceMock = lookups.GetMockService<IAnalyticsService>();

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
        await _publicDataDbContext.DisposeAsync();
    }

    /// <summary>
    /// Create an HttpClient and set a specific user to handle the request.
    /// </summary>
    public HttpClient CreateClient(ClaimsPrincipal? user)
    {
        if (user != null)
        {
            SetUser(user);
        }

        return CreateClient();
    }

    public void SetDataSetVersionDataDirectory(string directory)
    {
        _testDataSetVersionPathResolver.Directory = directory;
    }

    public DateTimeOffset GetUtcNow()
    {
        return _timeProvider.GetUtcNow();
    }

    public void SetUtcNow(DateTimeOffset utcNow)
    {
        _timeProvider.SetUtcNow(utcNow);
        _dateTimeProvider.SetFixedDateTimeUtc(utcNow.UtcDateTime);
    }

    /// <summary>
    /// Get a reusable DbContext that should be used for setting up test data and making test assertions.
    /// </summary>
    public PublicDataDbContext GetPublicDataDbContext() => _publicDataDbContext;

    /// <summary>
    ///
    /// Get a Mock representing this dependency that can be used for setups and verifications. This mock will be used
    /// within the tested code itself.
    /// </summary>
    ///
    /// <param name="resetMock">
    /// Controls whether this mock is firstly reset upon being requested via this method.
    /// See <see cref="MockExtensions.WithOptionalReset" /> for more details.
    ///
    /// </param>
    public Mock<IContentApiClient> GetContentApiClientMock(bool resetMock = true) =>
        _contentApiClientMock.WithOptionalReset(resetMock);

    /// <summary>
    /// Get a Mock representing this dependency that can be used for setups and verifications. This mock will be used
    /// within the tested code itself.
    /// </summary>
    ///
    /// <param name="resetMock">
    /// Controls whether this mock is firstly reset upon being requested via this method.
    /// See <see cref="MockExtensions.WithOptionalReset" /> for more details.
    /// </param>
    public Mock<ISearchService> GetSearchServiceMock(bool resetMock = true) =>
        _searchServiceMock.WithOptionalReset(resetMock);

    /// <summary>
    ///
    /// Get a Mock representing this dependency that can be used for setups and verifications. This mock will be used
    /// within the tested code itself.
    /// </summary>
    ///
    /// <param name="resetMock">
    /// Controls whether this mock is firstly reset upon being requested via this method.
    /// See <see cref="MockExtensions.WithOptionalReset" /> for more details.
    ///
    /// </param>
    public Mock<IAnalyticsService> GetAnalyticsServiceMock(bool resetMock = true) =>
        _analyticsServiceMock.WithOptionalReset(resetMock);

    public TestDataSetVersionPathResolver GetTestDataSetVersionPathResolver() => _testDataSetVersionPathResolver;

    /// <summary>
    /// Adds a user to the test user pool so that they can be used for HttpClient calls and looked up successfully.
    /// </summary>
    private void SetUser(ClaimsPrincipal user)
    {
        if (!capabilities.Contains(PublicApiIntegrationTestCapability.UserAuth))
        {
            throw new Exception("""Cannot register test users if "useTestUserAuthentication" is false.""");
        }

        _userHolder.SetUser(user);
    }
}

public enum PublicApiIntegrationTestCapability
{
    Postgres,
    UserAuth,
}

public static class OptimisedTestUsers
{
    public static readonly ClaimsPrincipal AdminAppUser = new DataFixture()
        .Generator<ClaimsPrincipal>()
        .WithRole(SecurityConstants.AdminAccessAppRole);

    public static readonly ClaimsPrincipal UnsupportedRoleUser = new DataFixture()
        .Generator<ClaimsPrincipal>()
        .WithRole("Unsupported Role");
}

public static class MockExtensions
{
    /// <summary>
    /// Allows mocks to be reset if requested.
    ///
    /// The reason that we support this and enable it by default when using a "GetXMock()" method is because this
    /// fixture is reused by all tests within a collection, and therefore each new test using a Mock will ideally
    /// want to use a Mock that has no pre-existing unverified setups on it. It is possible that prior tests might
    /// have left the Mock in an unclean state.
    ///
    /// A test class should therefore use Mocks from this fixture firstly by calling "GetXMock()" and assigning the
    /// Mock to a variable, then adding its setups to that variable. After the method under test has been performed,
    /// that same variable should be used for verifications.
    ///
    /// </summary>
    public static Mock<TMockType> WithOptionalReset<TMockType>(this Mock<TMockType> mock, bool resetMock)
        where TMockType : class
    {
        if (resetMock)
        {
            mock.Reset();
        }

        return mock;
    }
}
