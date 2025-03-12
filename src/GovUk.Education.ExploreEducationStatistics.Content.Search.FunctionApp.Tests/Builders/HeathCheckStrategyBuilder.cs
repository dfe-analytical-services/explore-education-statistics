using GovUk.Education.ExploreEducationStatistics.Content.Search.FunctionApp.Functions.HealthChecks.Strategies;
using Moq;

namespace GovUk.Education.ExploreEducationStatistics.Content.Search.FunctionApp.Tests.Builders;

public class HealthCheckStrategyBuilder
{
    private readonly Mock<IHealthCheckStrategy> _mock = new(MockBehavior.Strict);
    private bool _healthCheckIsSuccessful = true;
    private string? _healthCheckMessage = string.Empty;
    
    public IHealthCheckStrategy Build()
    {
        _mock
            .Setup(m => m.Run(It.IsAny<CancellationToken>()))
            .ReturnsAsync(new HealthCheckResult(_healthCheckIsSuccessful, _healthCheckMessage));
        
        return _mock.Object;
    }

    public HealthCheckStrategyBuilder WhereResultIsHealthy(string? message = null)
    {
        _healthCheckIsSuccessful = true;
        _healthCheckMessage = message;
        return this;
    }

    public HealthCheckStrategyBuilder WhereIsHealthyResultIs(bool isHealthy, string? message = null)
    {
        _healthCheckIsSuccessful = isHealthy;
        _healthCheckMessage = message;
        return this;
    }

    public HealthCheckStrategyBuilder WhereResultIsUnhealthy(string? message = null)
    {
        _healthCheckIsSuccessful = false;
        _healthCheckMessage = message;
        return this;
    }
}
