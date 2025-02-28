using System.Net.Http.Json;
using GovUk.Education.ExploreEducationStatistics.Content.Search.FunctionApp.Exceptions;

namespace GovUk.Education.ExploreEducationStatistics.Content.Search.FunctionApp.Clients.ContentApi;

internal class ContentApiClient(HttpClient httpClient) : IContentApiClient
{
    internal HttpClient HttpClient { get; } = httpClient;
    
    private const string GetPublicationLatestReleaseSearchViewModelFormat = "api/publications/{0}/releases/latest/searchable";
    public async Task<ReleaseSearchViewModelDto> GetPublicationLatestReleaseSearchViewModelAsync(string publicationSlug, CancellationToken cancellationToken)
    {
        var url = string.Format(GetPublicationLatestReleaseSearchViewModelFormat, publicationSlug);
        HttpResponseMessage response;
        try
        {
            response = await HttpClient.GetAsync(url, cancellationToken);
            if (!response.IsSuccessStatusCode)
            {
                throw new UnableToGetPublicationLatestReleaseSearchViewModelException(publicationSlug, response.StatusCode, response.ReasonPhrase);
            }
        }
        catch (HttpRequestException e)
        {
            throw new UnableToGetPublicationLatestReleaseSearchViewModelException(publicationSlug, e.StatusCode, e.Message);
        }

        var result = await response.Content.ReadFromJsonAsync<ReleaseSearchViewModelDto>(cancellationToken: cancellationToken);
        if (result == null)
        {
            throw new UnableToGetPublicationLatestReleaseSearchViewModelException(publicationSlug, "Response content could not be deserialised");
        }

        return result;
    }
}
