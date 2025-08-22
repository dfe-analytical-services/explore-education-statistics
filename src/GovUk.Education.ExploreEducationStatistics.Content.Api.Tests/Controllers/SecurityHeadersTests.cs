using GovUk.Education.ExploreEducationStatistics.Common.Tests.Extensions;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.Hosting;
using Xunit;

namespace GovUk.Education.ExploreEducationStatistics.Content.Api.Tests.Controllers;

public class SecurityHeadersTests
{
    [Fact]
    public async Task SeoSecurityHeaderMiddleware_AddsRequiredHeadersToResponse()
    {
        using var host = await new HostBuilder()
            .ConfigureWebHost(webBuilder =>
            {
                webBuilder
                    .UseTestServer()
                    .Configure(app =>
                    {
                        app.UseMiddleware(typeof(SeoSecurityHeaderMiddleware));
                    });
            })
            .StartAsync();

        var response = await host.GetTestClient().GetAsync("/");

        response.AssertNotFound();
        response.AssertHasHeader("X-Frame-Options", "DENY");
        response.AssertHasHeader("Referrer-Policy", "strict-origin-when-cross-origin");
        response.AssertHasHeader("X-Content-Type-Options", "nosniff");
        response.AssertHasHeader("Content-Security-Policy", "default-src 'self'");
    }
}
