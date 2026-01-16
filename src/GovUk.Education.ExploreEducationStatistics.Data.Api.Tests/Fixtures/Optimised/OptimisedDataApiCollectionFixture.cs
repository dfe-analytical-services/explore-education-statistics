using GovUk.Education.ExploreEducationStatistics.Common.IntegrationTests;
using GovUk.Education.ExploreEducationStatistics.Common.IntegrationTests.WebApp;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Database;
using GovUk.Education.ExploreEducationStatistics.Data.Model.Database;

namespace GovUk.Education.ExploreEducationStatistics.Data.Api.Tests.Fixtures.Optimised;

/// <summary>
///
/// A Collection-level test fixture to be used by Data API integration tests.
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
public abstract class OptimisedDataApiCollectionFixture()
    : OptimisedIntegrationTestFixtureBase<Startup>(
        dbContextTypes: [typeof(ContentDbContext), typeof(StatisticsDbContext)]
    )
{
    protected override void ConfigureServicesAndConfiguration(
        OptimisedServiceAndConfigModifications serviceModifications
    )
    {
        serviceModifications
            .AddInMemoryDbContext<ContentDbContext>(databaseName: $"{nameof(ContentDbContext)}_{Guid.NewGuid()}")
            .AddInMemoryDbContext<StatisticsDbContext>(databaseName: $"{nameof(StatisticsDbContext)}_{Guid.NewGuid()}")
            .AddControllers<Startup>();
    }

    /// <summary>
    /// Get a reusable DbContext that should be used for setting up test data and making test assertions.
    /// </summary>
    public ContentDbContext GetContentDbContext() => TestDbContexts.GetDbContext<ContentDbContext>();

    /// <summary>
    /// Get a reusable DbContext that should be used for setting up test data and making test assertions.
    /// </summary>
    public StatisticsDbContext GetStatisticsDbContext() => TestDbContexts.GetDbContext<StatisticsDbContext>();
}
