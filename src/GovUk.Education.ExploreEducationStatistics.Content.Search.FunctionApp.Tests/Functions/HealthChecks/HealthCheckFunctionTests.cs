using System.Net;
using GovUk.Education.ExploreEducationStatistics.Content.Search.FunctionApp.Functions.HealthChecks;
using GovUk.Education.ExploreEducationStatistics.Content.Search.FunctionApp.Functions.HealthChecks.Dtos;
using GovUk.Education.ExploreEducationStatistics.Content.Search.FunctionApp.Functions.HealthChecks.Strategies;
using GovUk.Education.ExploreEducationStatistics.Content.Search.FunctionApp.Tests.Builders;
using Microsoft.AspNetCore.Mvc;

namespace GovUk.Education.ExploreEducationStatistics.Content.Search.FunctionApp.Tests.Functions.HealthChecks;

public class HealthCheckFunctionTests
{
    private HealthCheckFunction GetSut(IEnumerable<IHealthCheckStrategy> strategies) => new(strategies);

    [Fact]
    public void Can_instantiate_sut() => Assert.NotNull(GetSut([]));

    [Fact]
    public async Task WhenStrategiesAreAllHealthy_ThenHealthCheckIsHealthy()
    {
        // ARRANGE
        IHealthCheckStrategy[] strategies =
        [
            new HealthCheckStrategyMockBuilder().WhereResultIsHealthy().Build(),
            new HealthCheckStrategyMockBuilder().WhereResultIsHealthy().Build(),
            new HealthCheckStrategyMockBuilder().WhereResultIsHealthy().Build()
        ];
        var sut = GetSut(strategies);
        
        // ACT
        var result = await sut.HealthCheck(null!, CancellationToken.None);
        
        // ASSERT
        Assert.NotNull(result);
        var okResult = Assert.IsType<OkObjectResult>(result);
        var healthCheckResult = Assert.IsType<HealthCheckResponse>(okResult.Value);
        Assert.True(healthCheckResult.IsHealthy);
    }
    
    [Theory]
    [InlineData(true, true, false)]
    [InlineData(false, true, true)]
    [InlineData(true, false, false)]
    [InlineData(false, false, false)]
    public async Task WhenAnyOrAllStrategiesAreUnhealthy_ThenHealthCheckIsUnhealthy(
        bool isHealthy1,
        bool isHealthy2,
        bool isHealthy3
        )
    {
        // ARRANGE
        IHealthCheckStrategy[] strategies =
        [
            new HealthCheckStrategyMockBuilder().WhereIsHealthyResultIs(isHealthy1).Build(),
            new HealthCheckStrategyMockBuilder().WhereIsHealthyResultIs(isHealthy2).Build(),
            new HealthCheckStrategyMockBuilder().WhereIsHealthyResultIs(isHealthy3).Build(),
        ];
        var sut = GetSut(strategies);
        
        // ACT
        var result = await sut.HealthCheck(null!, CancellationToken.None);
        
        // ASSERT
        Assert.NotNull(result);
        var httpResult = Assert.IsType<ObjectResult>(result);
        Assert.Equal((int)HttpStatusCode.InternalServerError, httpResult.StatusCode);
        var healthCheckResult = Assert.IsType<HealthCheckResponse>(httpResult.Value);
        Assert.False(healthCheckResult.IsHealthy);
    }
    
    [Fact]
    public async Task WhenStrategiesReturnResults_ThenHealthCheckResponseContainsResults()
    {
        // ARRANGE
        IHealthCheckStrategy[] strategies =
        [
            new HealthCheckStrategyMockBuilder().WhereResultIsHealthy("Result one").Build(),
            new HealthCheckStrategyMockBuilder().WhereResultIsUnhealthy("Result two").Build(),
            new HealthCheckStrategyMockBuilder().WhereResultIsHealthy("Result three").Build()
        ];
        var sut = GetSut(strategies);
        
        // ACT
        var result = await sut.HealthCheck(null!, CancellationToken.None);
        
        // ASSERT
        Assert.NotNull(result);
        var okResult = Assert.IsType<ObjectResult>(result);
        var healthCheckResult = Assert.IsType<HealthCheckResponse>(okResult.Value);
        var expectedResults = new[]
        {
            new HealthCheckResult(true, "Result one"),
            new HealthCheckResult(false, "Result two"),
            new HealthCheckResult(true, "Result three")
        };
        Assert.Equal(expectedResults, healthCheckResult.Results);
    }

}
