using GovUk.Education.ExploreEducationStatistics.Content.Search.FunctionApp.Functions.HealthChecks.Strategies;

namespace GovUk.Education.ExploreEducationStatistics.Content.Search.FunctionApp.Functions.HealthChecks.Dtos;

public record HealthCheckResponse
{
    public bool IsHealthy => Results.All(r => r.IsHealthy);
    public required HeathCheckResult[] Results { get; init; }
}
