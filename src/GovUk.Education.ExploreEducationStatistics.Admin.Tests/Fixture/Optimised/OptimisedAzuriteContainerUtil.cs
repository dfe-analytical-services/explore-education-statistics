using Testcontainers.Azurite;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Tests.Fixture.Optimised;

// TODO EES-6450 - remove when this is being used?
// ReSharper disable once UnusedType.Global
public class OptimisedAzuriteContainerUtil
{
    private readonly AzuriteContainer _azuriteContainer = new AzuriteBuilder()
        .WithImage("mcr.microsoft.com/azure-storage/azurite:3.34.0")
        .Build();

    // TODO EES-6450 - sort out this WithAzurite cref
    /// <summary>
    /// Start the Azurite container. Once started, the test app must also
    /// be configured with <see cref="WithAzurite"/> to use it.
    /// </summary>
    /// <remarks>
    ///
    /// We don't start the Azurite container in a class fixture as there currently
    /// isn't a good way to clear it after each test. The current approach is to
    /// restart the container for each test case (which is quite slow).
    /// See: https://github.com/Azure/Azurite/issues/588.
    /// For now, we should manually control the Azurite container's lifecycle by
    /// calling this on a case-by-case basis.
    /// </remarks>
    public async Task Start()
    {
        await _azuriteContainer.StartAsync();
    }

    public async Task Stop()
    {
        await _azuriteContainer.StopAsync();
    }
}
