using System.Net;
using System.Net.Http.Json;

namespace GovUk.Education.ExploreEducationStatistics.Content.Search.FunctionApp.Clients.ContentApi;

internal class ContentApiClient(HttpClient httpClient) : IContentApiClient
{
    internal HttpClient HttpClient { get; } = httpClient;
    
    private const string GetPublicationLatestReleaseSearchViewModelFormat = "api/publications/{0}/releases/latest/searchable";
    public async Task<GetResponse> GetPublicationLatestReleaseSearchableDocumentAsync(GetRequest request, CancellationToken cancellationToken)
    {
        var url = string.Format(GetPublicationLatestReleaseSearchViewModelFormat, request.PublicationSlug);
        HttpResponseMessage response;
        try
        {
            response = await HttpClient.GetAsync(url, cancellationToken);
            if (!response.IsSuccessStatusCode)
            {
                return response.StatusCode == HttpStatusCode.NotFound
                    ? new GetResponse.NotFound()
                    : new GetResponse.Error($"Error {response.StatusCode}: {response.ReasonPhrase}");
            }
        }
        catch (HttpRequestException e)
        {
            return new GetResponse.Error($"Error {e.StatusCode}: {e.Message}");
        }

        var dto = await response.Content.ReadFromJsonAsync<ReleaseSearchViewModelDto>(cancellationToken: cancellationToken);
        return dto is null
            ? new GetResponse.Error($"Could not deserialise response from API")
            : new GetResponse.Successful(dto.ToModel());
    }
}
