#nullable enable
using System;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Hosting;

namespace GovUk.Education.ExploreEducationStatistics.Common.Extensions;

public static class HostEnvironmentExtensions
{
    public const string IntegrationTestEnvironment = "IntegrationTest";

    public static bool IsIntegrationTest(this IHostEnvironment? hostEnvironment) => 
        hostEnvironment?.IsEnvironment(IntegrationTestEnvironment) ?? throw new ArgumentNullException(nameof(hostEnvironment));
    
    public static IWebHostBuilder UseIntegrationTestEnvironment(
        this IWebHostBuilder hostBuilder)
    {
        return hostBuilder.UseSetting(WebHostDefaults.EnvironmentKey, IntegrationTestEnvironment);
    }
}