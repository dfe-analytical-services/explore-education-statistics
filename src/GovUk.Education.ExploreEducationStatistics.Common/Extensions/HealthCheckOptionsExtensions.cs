using System.Text.Json;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace GovUk.Education.ExploreEducationStatistics.Common.Extensions;

public static class HealthCheckOptionsExtensions
{
    /// <summary>
    /// Has the health check response include the app's currently-applied
    /// "Deploy:DeployedAt" appsetting value, as "deployedAt". Each deploy generates a
    /// fresh marker for this (see deploy-admin.yml/deploy-content-api.yml/
    /// deploy-data-api.yml), and the pipeline polls for it to appear here to confirm the
    /// new appsettings and code have actually taken effect, rather than relying on a
    /// transient 503 that the in-process hosting model doesn't guarantee - see
    /// wait-for-app-service-restart.yml.
    /// </summary>
    public static HealthCheckOptions IncludeDeployedAt(this HealthCheckOptions options)
    {
        options.ResponseWriter = (context, report) =>
        {
            var configuration = context.RequestServices.GetRequiredService<IConfiguration>();
            context.Response.ContentType = "application/json";
            var json = JsonSerializer.Serialize(
                new { status = report.Status.ToString(), deployedAt = configuration["Deploy:DeployedAt"] }
            );
            return context.Response.WriteAsync(json);
        };
        return options;
    }
}
