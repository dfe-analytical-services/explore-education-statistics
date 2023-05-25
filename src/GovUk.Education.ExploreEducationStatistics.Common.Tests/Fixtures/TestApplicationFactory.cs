#nullable enable
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Console;

namespace GovUk.Education.ExploreEducationStatistics.Common.Tests.Fixtures
{
    /// <summary>
    /// Factory for creating test applications in integration tests.
    /// </summary>
    /// <see cref="https://docs.microsoft.com/en-us/aspnet/core/test/integration-tests"/>
    public class TestApplicationFactory<TStartup> : WebApplicationFactory<TStartup> where TStartup : class
    {
        protected override IHostBuilder CreateHostBuilder()
        {
            return Host.CreateDefaultBuilder()
            .ConfigureLogging(
                builder =>
                {
                    builder
                        .AddFilter<ConsoleLoggerProvider>("Default", LogLevel.Warning)
                        .AddFilter<ConsoleLoggerProvider>("Microsoft", LogLevel.Warning);
                }
            )
            .ConfigureWebHostDefaults(builder =>
            {
                builder.UseStartup<TStartup>().UseTestServer();
            });
        }
    }
}
