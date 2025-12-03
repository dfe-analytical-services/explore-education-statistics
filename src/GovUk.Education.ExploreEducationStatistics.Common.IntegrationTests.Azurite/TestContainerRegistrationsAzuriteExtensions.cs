using Testcontainers.Azurite;

namespace GovUk.Education.ExploreEducationStatistics.Common.IntegrationTests.Azurite;

public static class TestContainerRegistrationsAzuriteExtensions
{
    public static Func<string> RegisterAzuriteContainer(this TestContainerRegistrations registrations)
    {
        var container = new AzuriteBuilder()
            .WithImage("mcr.microsoft.com/azure-storage/azurite:3.34.0")
            .WithCommand("--skipApiVersionCheck")
            .Build();

        registrations.RegisterContainer(container);
        return () => container.GetConnectionString();
    }
}
