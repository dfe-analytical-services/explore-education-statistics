namespace GovUk.Education.ExploreEducationStatistics.Content.Search.FunctionApp.Functions.HealthChecks.Strategies;

/// <summary>
/// The result of checking the health of a part of the system
/// </summary>
/// <param name="Strategy">The strategy running the health check</param>
/// <param name="IsHealthy">Did the check pass?</param>
/// <param name="Message">A helpful description of what did or did not pass, with any useful error messages to help diagnose any issues.</param>
public record HealthCheckResult(IHealthCheckStrategy Strategy, bool IsHealthy, string Message);
