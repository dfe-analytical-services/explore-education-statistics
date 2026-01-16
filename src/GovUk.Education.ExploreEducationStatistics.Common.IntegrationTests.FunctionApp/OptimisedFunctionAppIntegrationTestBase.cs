using Xunit;

namespace GovUk.Education.ExploreEducationStatistics.Common.IntegrationTests.FunctionApp;

/// <summary>
/// A convenience base class that is simply used to allow us to reduce the boilerplate of calling
/// <see cref="OptimisedFunctionAppIntegrationTestFixtureBase.BeforeEachTest"/> before each test in
/// a test Collection. The alternative would be to make all child test Collection classes implement
/// <see cref="IAsyncLifetime.InitializeAsync"/> and call "fixture.BeforeEachTest()".
/// </summary>
public abstract class OptimisedFunctionAppIntegrationTestBase(OptimisedFunctionAppIntegrationTestFixtureBase fixture)
    : IAsyncLifetime
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
