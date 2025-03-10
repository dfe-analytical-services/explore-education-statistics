using GovUk.Education.ExploreEducationStatistics.Content.Search.FunctionApp.Functions.HealthChecks.Strategies;
using Moq;

namespace GovUk.Education.ExploreEducationStatistics.Content.Search.FunctionApp.Tests.Builders;

public class HeathCheckStrategyBuilder
{
    private readonly Mock<IHeathCheckStrategy> _mock = new(MockBehavior.Strict);
    private bool _heathCheckIsSuccessful = true;
    private string? _heathCheckMessage = string.Empty;
    
    public IHeathCheckStrategy Build()
    {
        _mock
            .Setup(m => m.Run(It.IsAny<CancellationToken>()))
            .ReturnsAsync(new HeathCheckResult(_heathCheckIsSuccessful, _heathCheckMessage));
        
        return _mock.Object;
    }

    public HeathCheckStrategyBuilder WhereResultIsHealthy(string? message = null)
    {
        _heathCheckIsSuccessful = true;
        _heathCheckMessage = message;
        return this;
    }

    public HeathCheckStrategyBuilder WhereIsHeathyResultIs(bool isHeathy, string? message = null)
    {
        _heathCheckIsSuccessful = isHeathy;
        _heathCheckMessage = message;
        return this;
    }

    public HeathCheckStrategyBuilder WhereResultIsUnhealthy(string? message = null)
    {
        _heathCheckIsSuccessful = false;
        _heathCheckMessage = message;
        return this;
    }
}
