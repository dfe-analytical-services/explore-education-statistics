using DotNet.Testcontainers.Containers;
using Microsoft.AspNetCore.Mvc.Testing;
using Xunit;

namespace GovUk.Education.ExploreEducationStatistics.Common.IntegrationTests;

public abstract class OptimisedIntegrationTestFixtureBase<TStartup> : IAsyncLifetime
    where TStartup : class
{
    private readonly List<DockerContainer> _testContainers = new();
    private WebApplicationFactory<TStartup> _factory = null!;

    public async Task InitializeAsync()
    {
        RegisterTestContainers();

        var startupTasks = _testContainers.Select(container => container.StartAsync());
        await Task.WhenAll(startupTasks);

        var factory = new WebApplicationFactory<TStartup>();
        var factoryBuilder = new OptimisedWebApplicationFactoryBuilder<TStartup>(factory);
        var finalisedFactory = ConfigureWebApplicationFactory(factoryBuilder);

        var lookups = new OptimisedServiceCollectionLookups<TStartup>(finalisedFactory);
        AfterFactoryConstructed(lookups);

        _factory = finalisedFactory;
    }

    protected virtual void RegisterTestContainers() { }

    protected abstract WebApplicationFactory<TStartup> ConfigureWebApplicationFactory(
        OptimisedWebApplicationFactoryBuilder<TStartup> factoryBuilder
    );

    protected virtual void AfterFactoryConstructed(OptimisedServiceCollectionLookups<TStartup> lookups) { }

    /// <summary>
    /// Creates an HttpClient that can be used to send HTTP requests to the WebApplicationFactory.
    /// </summary>
    public HttpClient CreateClient()
    {
        return _factory.CreateClient();
    }

    protected virtual Task DisposeResources()
    {
        return Task.CompletedTask;
    }

    public void AddContainer(DockerContainer container)
    {
        _testContainers.Add(container);
    }

    public async Task DisposeAsync()
    {
        var shutdownTasks = _testContainers.Select(container => container.StopAsync());
        await Task.WhenAll(shutdownTasks);

        await DisposeResources();
    }
}
