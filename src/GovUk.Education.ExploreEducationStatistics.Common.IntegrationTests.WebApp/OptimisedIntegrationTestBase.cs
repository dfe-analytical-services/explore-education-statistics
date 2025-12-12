using Xunit;

namespace GovUk.Education.ExploreEducationStatistics.Common.IntegrationTests.WebApp;

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
