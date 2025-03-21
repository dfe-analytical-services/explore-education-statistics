using GovUk.Education.ExploreEducationStatistics.Content.Search.FunctionApp.Clients.ContentApi;
using GovUk.Education.ExploreEducationStatistics.Content.Search.FunctionApp.Functions.HealthChecks.Strategies;
using GovUk.Education.ExploreEducationStatistics.Content.Search.FunctionApp.Options;
using Xunit.Abstractions;

namespace GovUk.Education.ExploreEducationStatistics.Content.Search.FunctionApp.Tests.Functions.HealthChecks.Strategies;

public class ContentApiHealthCheckStrategyIntegrationTests(ITestOutputHelper output)
{
    private const string ContentApiUrl = "https://content.explore-education-statistics.service.gov.uk/";

    [Fact(Skip = "Will attempt to call the production content API. Change ContentApiUrl to your favourite endpoint.")]
    public async Task WhenContentApiIsAvailable_ThenReturnsHealthy()
    {
        // ARRANGE
        var sut = new ContentApiHealthCheckStrategy(
            Microsoft.Extensions.Options.Options.Create(
                new ContentApiOptions()),
                () => new ContentApiClient(
                    new HttpClient
                    {
                        BaseAddress = new Uri(ContentApiUrl)
                    }));
        
        // ACT
        var healthCheckResult = await sut.Run(CancellationToken.None);
        
        // ASSERT
        Print(healthCheckResult);
        Assert.NotNull(healthCheckResult);
        Assert.True(healthCheckResult.IsHealthy);
        Assert.Null(healthCheckResult.Message);
    }
    
    private void Print(object obj) => output.WriteLine(obj.ToString());
}
