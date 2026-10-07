using System.Text.Json;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace GovUk.Education.ExploreEducationStatistics.Common.Extensions;

public static class HealthCheckOptionsExtensions
{
    /// <summary>
    /// Causes health check endpoints to include a "deployedAt" field which allows us
    /// to identify the time at which a given app instance was deployed. This is fed in
    /// via the "Deploy:DeployedAt" appsetting in the code deployment pipeline, and
    /// subsequently allows us to verify that health check responses that we are polling
    /// for in the pipeline are being returned by the newly-started-up app instances
    /// rather than the pre-existing ones that were running prior to the deployment.
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
