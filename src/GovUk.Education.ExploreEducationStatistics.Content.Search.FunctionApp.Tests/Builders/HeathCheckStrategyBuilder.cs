using GovUk.Education.ExploreEducationStatistics.Content.Search.FunctionApp.Functions.HealthChecks.Strategies;
using Moq;

namespace GovUk.Education.ExploreEducationStatistics.Content.Search.FunctionApp.Tests.Builders;

public class HealthCheckStrategyMockBuilder
{
    private readonly Mock<IHealthCheckStrategy> _mock = new(MockBehavior.Strict);
    private bool _healthCheckIsSuccessful = true;
    private string _healthCheckMessage = string.Empty;
    
    public IHealthCheckStrategy Build()
    {
        _mock
            .Setup(m => m.Description)
            .Returns(() => "Mock Health Check Strategy");
        
        _mock
            .Setup(m => m.Run(It.IsAny<CancellationToken>()))
            .ReturnsAsync(new HealthCheckResult(_mock.Object, _healthCheckIsSuccessful, _healthCheckMessage));
        
        return _mock.Object;
    }

    public HealthCheckStrategyMockBuilder WhereResultIsHealthy(string? message = null)
    {
        _healthCheckIsSuccessful = true;
        _healthCheckMessage = message ?? "Everything is OK";
        return this;
    }

    public HealthCheckStrategyMockBuilder WhereIsHealthyResultIs(bool isHealthy, string? message = null)
    {
        _healthCheckIsSuccessful = isHealthy;
        _healthCheckMessage = message ?? "Everything is OK";
        return this;
    }

    public HealthCheckStrategyMockBuilder WhereResultIsUnhealthy(string? message = null)
    {
        _healthCheckIsSuccessful = false;
        _healthCheckMessage = message ?? "Boo. It didn't work.";
        return this;
    }
}
