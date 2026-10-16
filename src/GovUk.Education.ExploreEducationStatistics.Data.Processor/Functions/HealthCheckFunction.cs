using System.Net;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Extensions.Configuration;

namespace GovUk.Education.ExploreEducationStatistics.Data.Processor.Functions;

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
    [Function("Health")]
    public async Task<HttpResponseData> Run(
        [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "health")] HttpRequestData req
    )
    {
        var response = req.CreateResponse(HttpStatusCode.OK);
        await response.WriteAsJsonAsync(new { status = "Healthy", deployedAt = configuration["Deploy:DeployedAt"] });
        return response;
    }
}
