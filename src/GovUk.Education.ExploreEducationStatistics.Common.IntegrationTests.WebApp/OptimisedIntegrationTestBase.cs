using Xunit;

namespace GovUk.Education.ExploreEducationStatistics.Common.IntegrationTests.WebApp;

/// <summary>
/// A convenience base class that is simply used to allow us to reduce the boilerplate of calling
/// <see cref="OptimisedIntegrationTestFixtureBase{TStartup}.BeforeEachTest"/> before each test in
/// a test Collection. The alternative would be to make all child test Collection classes implement
/// <see cref="IAsyncLifetime.InitializeAsync"/> and call "fixture.BeforeEachTest()".
/// </summary>
public abstract class OptimisedIntegrationTestBase<TStartup>(OptimisedIntegrationTestFixtureBase<TStartup> fixture)
    : IAsyncLifetime
    where TStartup : class
{
    public virtual async Task InitializeAsync()
    {
        await fixture.BeforeEachTest();
    }

    public virtual Task DisposeAsync()
    {
        return Task.CompletedTask;
    }
}
