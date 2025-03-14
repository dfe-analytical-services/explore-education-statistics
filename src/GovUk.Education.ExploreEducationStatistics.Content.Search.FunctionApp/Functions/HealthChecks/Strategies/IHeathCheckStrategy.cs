namespace GovUk.Education.ExploreEducationStatistics.Content.Search.FunctionApp.Functions.HealthChecks.Strategies;

public interface IHealthCheckStrategy
{
    Task<HealthCheckResult> Run(CancellationToken cancellationToken);
}
