namespace GovUk.Education.ExploreEducationStatistics.Content.Search.FunctionApp.Functions.HealthChecks.Strategies;

public record HealthCheckResult(bool IsHealthy, string? Message = null);
