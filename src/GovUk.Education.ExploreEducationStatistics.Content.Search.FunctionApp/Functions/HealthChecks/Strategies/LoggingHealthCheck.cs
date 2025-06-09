using Microsoft.Extensions.Logging;

namespace GovUk.Education.ExploreEducationStatistics.Content.Search.FunctionApp.Functions.HealthChecks.Strategies;

internal class LoggingHealthCheck(ILogger<LoggingHealthCheck> logger) : IHealthCheckStrategy
{
    public string Description => "Logging health check"; 
    public Task<HealthCheckResult> Run(CancellationToken cancellationToken)
    {
        logger.LogDebug("Logging health check test at Debug level");
        logger.LogInformation("Logging health check test at Information level");
        logger.LogWarning("Logging health check test at Warning level with structured logging test of {@TestObject}", new TestObject());

        try
        {
            throw new Exception("This is a test exception - part of the logging health check");
        }
        catch (Exception e)
        {
            logger.LogError(e,"Logging health check test at Error level - with a test exception");
        }

        return Task.FromResult(new HealthCheckResult(this, true, "Statements have been logged."));
    }

    private class TestObject
    {
        // ReSharper disable UnusedMember.Local -- These properties are used to test the structured logging
        public string TestProperty { get; set; } = "Test Value";
        public object[] TestArray { get; set; } = { "Test Array Value 1", "Test Array Value 2" };
    }
}
