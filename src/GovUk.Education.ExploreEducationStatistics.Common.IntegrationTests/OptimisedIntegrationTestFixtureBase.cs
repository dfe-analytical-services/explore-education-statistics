using DotNet.Testcontainers.Containers;
using Xunit;

namespace GovUk.Education.ExploreEducationStatistics.Common.IntegrationTests;

public abstract class OptimisedIntegrationTestFixtureBase : IAsyncLifetime
{
    private readonly List<DockerContainer> _testContainers = new();

    public async Task InitializeAsync()
    {
        var startupTasks = _testContainers.Select(container => container.StartAsync());
        await Task.WhenAll(startupTasks);
    }

    public async Task DisposeAsync()
    {
        var shutdownTasks = _testContainers.Select(container => container.StopAsync());
        await Task.WhenAll(shutdownTasks);
    }

    public void AddContainer(DockerContainer container)
    {
        _testContainers.Add(container);
    }
}
