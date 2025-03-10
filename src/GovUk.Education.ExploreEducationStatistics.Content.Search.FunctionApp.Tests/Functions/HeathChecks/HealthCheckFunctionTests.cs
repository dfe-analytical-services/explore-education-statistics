using System.Net;
using GovUk.Education.ExploreEducationStatistics.Content.Search.FunctionApp.Functions.HealthChecks;
using GovUk.Education.ExploreEducationStatistics.Content.Search.FunctionApp.Functions.HealthChecks.Dtos;
using GovUk.Education.ExploreEducationStatistics.Content.Search.FunctionApp.Functions.HealthChecks.Strategies;
using GovUk.Education.ExploreEducationStatistics.Content.Search.FunctionApp.Tests.Builders;
using Microsoft.AspNetCore.Mvc;

namespace GovUk.Education.ExploreEducationStatistics.Content.Search.FunctionApp.Tests.Functions.HeathChecks;

public class HealthCheckFunctionTests
{
    private HealthCheckFunction GetSut(IEnumerable<IHeathCheckStrategy> strategies) => new(strategies);

    [Fact]
    public void Can_instantiate_sut() => Assert.NotNull(GetSut([]));

    [Fact]
    public async Task WhenStrategiesAreAllHealthy_ThenHeathCheckIsHealthy()
    {
        // ARRANGE
        IHeathCheckStrategy[] strategies =
        [
            new HeathCheckStrategyBuilder().WhereResultIsHealthy().Build(),
            new HeathCheckStrategyBuilder().WhereResultIsHealthy().Build(),
            new HeathCheckStrategyBuilder().WhereResultIsHealthy().Build()
        ];
        var sut = GetSut(strategies);
        
        // ACT
        var result = await sut.HealthCheck(null!, CancellationToken.None);
        
        // ASSERT
        Assert.NotNull(result);
        var okResult = Assert.IsType<OkObjectResult>(result);
        var heathCheckResult = Assert.IsType<HealthCheckResponse>(okResult.Value);
        Assert.True(heathCheckResult.IsHealthy);
    }
    
    [Theory]
    [InlineData(true, true, false)]
    [InlineData(false, true, true)]
    [InlineData(true, false, false)]
    [InlineData(false, false, false)]
    public async Task WhenAnyOrAllStrategiesAreUnheatlhy_ThenHeathCheckIsUnhealthy(
        bool isHealthy1,
        bool isHealthy2,
        bool isHealthy3
        )
    {
        // ARRANGE
        IHeathCheckStrategy[] strategies =
        [
            new HeathCheckStrategyBuilder().WhereIsHeathyResultIs(isHealthy1).Build(),
            new HeathCheckStrategyBuilder().WhereIsHeathyResultIs(isHealthy2).Build(),
            new HeathCheckStrategyBuilder().WhereIsHeathyResultIs(isHealthy3).Build(),
        ];
        var sut = GetSut(strategies);
        
        // ACT
        var result = await sut.HealthCheck(null!, CancellationToken.None);
        
        // ASSERT
        Assert.NotNull(result);
        var httpResult = Assert.IsType<ObjectResult>(result);
        Assert.Equal((int)HttpStatusCode.InternalServerError, httpResult.StatusCode);
        var heathCheckResult = Assert.IsType<HealthCheckResponse>(httpResult.Value);
        Assert.False(heathCheckResult.IsHealthy);
    }
    
    [Fact]
    public async Task WhenStrategiesReturnResults_ThenHeathCheckResponseContainsResults()
    {
        // ARRANGE
        IHeathCheckStrategy[] strategies =
        [
            new HeathCheckStrategyBuilder().WhereResultIsHealthy("Result one").Build(),
            new HeathCheckStrategyBuilder().WhereResultIsUnhealthy("Result two").Build(),
            new HeathCheckStrategyBuilder().WhereResultIsHealthy(null).Build()
        ];
        var sut = GetSut(strategies);
        
        // ACT
        var result = await sut.HealthCheck(null!, CancellationToken.None);
        
        // ASSERT
        Assert.NotNull(result);
        var okResult = Assert.IsType<ObjectResult>(result);
        var heathCheckResult = Assert.IsType<HealthCheckResponse>(okResult.Value);
        var expectedResults = new[]
        {
            new HeathCheckResult(true, "Result one"),
            new HeathCheckResult(false, "Result two"),
            new HeathCheckResult(true, null)
        };
        Assert.Equal(expectedResults, heathCheckResult.Results);
    }

}
