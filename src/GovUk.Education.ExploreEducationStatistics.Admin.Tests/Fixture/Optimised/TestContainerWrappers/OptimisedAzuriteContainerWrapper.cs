using Testcontainers.Azurite;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Tests.Fixture.Optimised.TestContainerWrappers;

public class OptimisedAzuriteContainerWrapper
{
    private readonly AzuriteContainer _azuriteContainer = new AzuriteBuilder()
        .WithImage("mcr.microsoft.com/azure-storage/azurite:3.34.0")
        .WithCommand("--skipApiVersionCheck")
        .Build();

    /// <summary>
    /// Start the Azurite container.
    /// </summary>
    public async Task Start()
    {
        await _azuriteContainer.StartAsync();
    }

    /// <summary>
    /// Stop the Azurite container.
    /// </summary>
    public async Task Stop()
    {
        await _azuriteContainer.StopAsync();
    }

    public AzuriteContainer GetContainer()
    {
        return _azuriteContainer;
    }
}
