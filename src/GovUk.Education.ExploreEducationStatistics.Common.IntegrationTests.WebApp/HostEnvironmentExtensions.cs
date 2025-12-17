using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Hosting;

namespace GovUk.Education.ExploreEducationStatistics.Common.IntegrationTests.WebApp;

public static class HostEnvironmentExtensions
{
    public const string IntegrationTestEnvironment = "IntegrationTest";

    public static IHostBuilder UseIntegrationTestEnvironment(this IHostBuilder hostBuilder)
    {
        return hostBuilder.UseEnvironment(IntegrationTestEnvironment);
    }

    public static IWebHostBuilder UseIntegrationTestEnvironment(this IWebHostBuilder hostBuilder)
    {
        return hostBuilder.UseEnvironment(IntegrationTestEnvironment);
    }
}
