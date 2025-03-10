using GovUk.Education.ExploreEducationStatistics.Content.Search.FunctionApp.Clients.ContentApi;
using GovUk.Education.ExploreEducationStatistics.Content.Search.FunctionApp.Functions.HealthChecks.Strategies;
using Xunit.Abstractions;

namespace GovUk.Education.ExploreEducationStatistics.Content.Search.FunctionApp.Tests.Functions.HeathChecks.Strategies;

public class ContentApiHealthCheckStrategyIntegrationTests(ITestOutputHelper output)
{
    private const string ContentApiUrl = "https://content.explore-education-statistics.service.gov.uk/";

    [Fact(Skip = "Will attempt to call the production content API. Change ContentApiUrl to your favourite endpoint.")]
    public async Task WhenContentApiIsAvailable_ThenReturnsHealthy()
    {
        // ARRANGE
        var sut = new ContentApiHealthCheckStrategy(
            new ContentApiClient(
                new HttpClient
                {
                    BaseAddress = new Uri(ContentApiUrl)
                }));
        
        // ACT
        var heathCheckResult = await sut.Run(CancellationToken.None);
        
        // ASSERT
        Print(heathCheckResult);
        Assert.NotNull(heathCheckResult);
        Assert.True(heathCheckResult.IsHealthy);
        Assert.Null(heathCheckResult.Message);
    }
    
    private void Print(object obj) => output.WriteLine(obj.ToString());
}
