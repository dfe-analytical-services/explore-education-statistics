using DotNet.Testcontainers.Containers;

namespace GovUk.Education.ExploreEducationStatistics.Common.IntegrationTests;

public class TestContainerRegistrations
{
    private readonly List<DockerContainer> _testContainers = new();

    public void RegisterContainer(DockerContainer container)
    {
        _testContainers.Add(container);
    }

    public async Task StartAll()
    {
        var startTasks = _testContainers.Select(container => container.StartAsync());
        await Task.WhenAll(startTasks);
    }

    public async Task StopAll()
    {
        var stopTasks = _testContainers.Select(container => container.StopAsync());
        await Task.WhenAll(stopTasks);
    }
}
