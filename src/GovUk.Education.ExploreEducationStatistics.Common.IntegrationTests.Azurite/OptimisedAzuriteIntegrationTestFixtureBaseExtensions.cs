using Testcontainers.Azurite;

namespace GovUk.Education.ExploreEducationStatistics.Common.IntegrationTests.Azurite;

public static class OptimisedAzuriteIntegrationTestFixtureBaseExtensions
{
    public static string CreateAzuriteContainer(this OptimisedIntegrationTestFixtureBase fixture)
    {
        var container = new AzuriteBuilder()
            .WithImage("mcr.microsoft.com/azure-storage/azurite:3.34.0")
            .WithCommand("--skipApiVersionCheck")
            .Build();

        fixture.AddContainer(container);
        return container.GetConnectionString();
    }
}
