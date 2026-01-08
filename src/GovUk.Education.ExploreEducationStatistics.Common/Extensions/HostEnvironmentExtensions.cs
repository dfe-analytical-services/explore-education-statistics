using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Hosting;

namespace GovUk.Education.ExploreEducationStatistics.Common.Extensions;

// TODO EES-6450 - this can probably go when all the old integration test code is unused.
public static class HostEnvironmentExtensions
{
    public const string IntegrationTestEnvironment = "IntegrationTest";

    public static bool IsIntegrationTest(this IHostEnvironment? hostEnvironment) =>
        hostEnvironment?.IsEnvironment(IntegrationTestEnvironment)
        ?? throw new ArgumentNullException(nameof(hostEnvironment));

    public static IWebHostBuilder UseIntegrationTestEnvironment(this IWebHostBuilder hostBuilder)
    {
        return hostBuilder.UseEnvironment(IntegrationTestEnvironment);
    }
}
