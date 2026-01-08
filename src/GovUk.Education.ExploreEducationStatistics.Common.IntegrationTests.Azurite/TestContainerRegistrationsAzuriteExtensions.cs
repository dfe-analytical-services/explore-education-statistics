using GovUk.Education.ExploreEducationStatistics.Common.IntegrationTests.WebApp;
using Testcontainers.Azurite;

namespace GovUk.Education.ExploreEducationStatistics.Common.IntegrationTests.Azurite;

public static class TestContainerRegistrationsAzuriteExtensions
{
    public static AzuriteWrapper RegisterAzuriteContainer(this TestContainerRegistrations registrations)
    {
        var container = new AzuriteBuilder()
            .WithImage("mcr.microsoft.com/azure-storage/azurite:3.34.0")
            .WithCommand("--skipApiVersionCheck")
            .Build();

        registrations.RegisterContainer(container);
        return new AzuriteWrapper(() => container.GetConnectionString());
    }
}
