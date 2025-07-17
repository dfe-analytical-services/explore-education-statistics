namespace GovUk.Education.ExploreEducationStatistics.Content.Search.FunctionApp.Functions.HealthChecks.Strategies;

public interface IHealthCheckStrategy
{
    string Description { get; }
    Task<HealthCheckResult> Run(CancellationToken cancellationToken);
}
