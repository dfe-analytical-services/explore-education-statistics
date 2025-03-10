namespace GovUk.Education.ExploreEducationStatistics.Content.Search.FunctionApp.Functions.HealthChecks.Strategies;

public record HeathCheckResult(bool IsHealthy, string? Message = null);
