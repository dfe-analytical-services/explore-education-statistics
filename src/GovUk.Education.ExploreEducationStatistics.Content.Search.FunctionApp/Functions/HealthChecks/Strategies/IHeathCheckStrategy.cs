namespace GovUk.Education.ExploreEducationStatistics.Content.Search.FunctionApp.Functions.HealthChecks.Strategies;

public interface IHeathCheckStrategy
{
    Task<HeathCheckResult> Run(CancellationToken cancellationToken);
}
