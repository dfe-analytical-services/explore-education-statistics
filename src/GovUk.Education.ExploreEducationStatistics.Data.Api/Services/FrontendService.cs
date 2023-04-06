#nullable enable
using System.IO;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using GovUk.Education.ExploreEducationStatistics.Common.Utils;
using GovUk.Education.ExploreEducationStatistics.Data.Api.Requests;
using GovUk.Education.ExploreEducationStatistics.Data.Api.Services.Interfaces;


namespace GovUk.Education.ExploreEducationStatistics.Data.Api.Services;

public class FrontendService : IFrontendService
{
    private readonly IHttpClientFactory _httpClientFactory;

    public FrontendService(IHttpClientFactory httpClientFactory)
    {
        _httpClientFactory = httpClientFactory;
    }


    private HttpClient CreateFrontendClient()
    {
        return _httpClientFactory.CreateClient("PublicFrontend");
        // BaseAddress = new Uri(Configuration.GetValue<string>("PublicFrontendUrl"))
        // .DefaultRequestHeaders.Add(HeaderNames.UserAgent, "DataApi");

    }


    public async Task<Stream> CreateUniversalTableFormat(PermalinkTableCreateRequest request,
        CancellationToken cancellationToken)
    {
        var payload = new JsonNetContent(request);

        var _httpClient = CreateFrontendClient();

        var response = await _httpClient.PostAsync("/api/permalink", payload, cancellationToken);

        response.EnsureSuccessStatusCode();

        return await response.Content.ReadAsStreamAsync(cancellationToken);
    }
}