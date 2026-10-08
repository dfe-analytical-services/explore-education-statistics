using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Configuration;

namespace GovUk.Education.ExploreEducationStatistics.Publisher.Functions;

/// <summary>
/// Reports a "deployedAt" field sourced from the "Deploy:DeployedAt" appsetting, which the code
/// deployment pipeline uses to confirm that a newly-deployed instance (rather than a pre-existing
/// one still shutting down) is the one answering health checks - see
/// GovUk.Education.ExploreEducationStatistics.Common.Extensions.HealthCheckOptionsExtensions,
/// whose ASP.NET Core health check middleware this mirrors for an app with no HTTP pipeline of
/// its own to attach that middleware to.
/// </summary>
public class HealthCheckFunction(IConfiguration configuration)
{
    [Function(nameof(Health))]
    public IActionResult Health([HttpTrigger(AuthorizationLevel.Anonymous, "get")] HttpRequest request)
    {
        return new OkObjectResult(new { status = "Healthy", deployedAt = configuration["Deploy:DeployedAt"] });
    }
}
