using GovUk.Education.ExploreEducationStatistics.Content.Search.FunctionApp.Functions.HealthChecks.Strategies;

namespace GovUk.Education.ExploreEducationStatistics.Content.Search.FunctionApp.Functions.HealthChecks.Dtos;

public record HealthCheckResponse
{
    public bool IsHealthy => Results.All(r => r.IsHealthy);
    public required HealthCheckResultViewModel[] Results { get; init; }
}

/// <summary>
/// The result of checking the health of a part of the system
/// </summary>
/// <param name="Description">The description of the strategy running the health check</param>
/// <param name="StrategyTypeName">The full type name of the strategy.</param>
/// <param name="IsHealthy">Did the check pass?</param>
/// <param name="Message">A helpful description of what did or did not pass, with any useful error messages to help diagnose any issues.</param>
public record HealthCheckResultViewModel(
    string Description,
    string StrategyTypeName,
    bool IsHealthy,
    string Message
)
{
    private HealthCheckResultViewModel(HealthCheckResult healthCheckResult)
        : this(
            healthCheckResult.Strategy.Description,
            healthCheckResult.Strategy.GetType().Name,
            healthCheckResult.IsHealthy,
            healthCheckResult.Message
        ) { }

    public static implicit operator HealthCheckResultViewModel(
        HealthCheckResult healthCheckResult
    ) => new(healthCheckResult);
};
