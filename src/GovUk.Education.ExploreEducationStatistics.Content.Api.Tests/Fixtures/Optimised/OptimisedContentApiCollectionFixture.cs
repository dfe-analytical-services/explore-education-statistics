using GovUk.Education.ExploreEducationStatistics.Common.IntegrationTests.Azurite;
using GovUk.Education.ExploreEducationStatistics.Common.IntegrationTests.WebApp;
using GovUk.Education.ExploreEducationStatistics.Common.Services.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Common.Tests.Extensions;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Database;
using GovUk.Education.ExploreEducationStatistics.Data.Model.Database;
using Moq;

namespace GovUk.Education.ExploreEducationStatistics.Content.Api.Tests.Fixtures.Optimised;

/// <summary>
///
/// A Collection-level test fixture to be used by Content API integration tests.
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
public abstract class OptimisedContentApiCollectionFixture(params ContentApiIntegrationTestCapability[] capabilities)
    : OptimisedIntegrationTestFixtureBase<Startup>
{
    private ContentDbContext _contentDbContext = null!;
    private StatisticsDbContext _statisticsDbContext = null!;

    private AzuriteWrapper _azuriteWrapper = null!;

    protected override void RegisterTestContainers(TestContainerRegistrations registrations)
    {
        if (capabilities.Contains(ContentApiIntegrationTestCapability.Azurite))
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
            .ReplaceServiceWithMock<IMemoryCacheService>(mockBehavior: MockBehavior.Loose)
            .AddControllers<Startup>();

        if (capabilities.Contains(ContentApiIntegrationTestCapability.Azurite))
        {
            serviceModifications.AddAzurite(
                connectionString: _azuriteWrapper.GetConnectionString(),
                connectionStringKeys: ["PublicStorage"]
            );
        }
        else
        {
            serviceModifications.ReplaceServiceWithMock<IPublicBlobStorageService>();
        }
    }

    protected override Task AfterFactoryConstructed(OptimisedServiceCollectionLookups<Startup> lookups)
    {
        // Grab reusable DbContexts that can be used for test data setup and test assertions. These are looked up once
        // per startup of a test class that uses this fixture and are disposed of at the end of its lifetime, via XUnit
        // calling "DisposeAsync" on this fixture.
        _contentDbContext = lookups.GetService<ContentDbContext>();
        _statisticsDbContext = lookups.GetService<StatisticsDbContext>();

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
        await _contentDbContext.DisposeAsync();
        await _statisticsDbContext.DisposeAsync();
    }

    /// <summary>
    /// Get a reusable DbContext that should be used for setting up test data and making test assertions.
    /// </summary>
    public ContentDbContext GetContentDbContext() => _contentDbContext;

    /// <summary>
    /// Get a reusable DbContext that should be used for setting up test data and making test assertions.
    /// </summary>
    public StatisticsDbContext GetStatisticsDbContext() => _statisticsDbContext;

    public override async Task BeforeEachTest()
    {
        // In-memory DbContexts can be cleared down by default with no speed penalty.
        // Proper DbContexts add considerable time to a full project run if clearing
        // between every test, and therefore we don't clear them down by default.
        await _contentDbContext.ClearTestDataIfInMemory();
        await _statisticsDbContext.ClearTestDataIfInMemory();
    }

    /// <summary>
    /// Retrieve the Azurity convenience class for the purposes of test data cleardown, etc.
    /// </summary>
    protected AzuriteWrapper GetAzuriteWrapper()
    {
        if (!capabilities.Contains(ContentApiIntegrationTestCapability.Azurite))
        {
            throw new Exception("Cannot get AzuriteWrapper if not using the Azurite capability.");
        }

        return _azuriteWrapper;
    }
}

public enum ContentApiIntegrationTestCapability
{
    Azurite,
}
