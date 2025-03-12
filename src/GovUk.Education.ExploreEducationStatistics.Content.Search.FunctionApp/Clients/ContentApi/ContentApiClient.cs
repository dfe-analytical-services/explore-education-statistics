using System.Net.Http.Json;
using GovUk.Education.ExploreEducationStatistics.Content.Search.FunctionApp.Domain;
using GovUk.Education.ExploreEducationStatistics.Content.Search.FunctionApp.Exceptions;

namespace GovUk.Education.ExploreEducationStatistics.Content.Search.FunctionApp.Clients.ContentApi;

internal class ContentApiClient(HttpClient httpClient) : IContentApiClient
{
    internal HttpClient HttpClient { get; } = httpClient;

    private const string GetPublicationLatestReleaseSearchViewModelApiEndPointFormat =
        "api/publications/{0}/releases/latest/searchable";

    private const string GetPublicationsPageEndPointFormat = 
        "/api/publications?Page={0}&PageSize={1}";
    
    public async Task<ReleaseSearchableDocument> GetPublicationLatestReleaseSearchableDocumentAsync(
        string publicationSlug,
        CancellationToken cancellationToken)
    {
        var apiEndpoint = string.Format(GetPublicationLatestReleaseSearchViewModelApiEndPointFormat, publicationSlug);
        HttpResponseMessage response;
        try
        {
            response = await HttpClient.GetAsync(apiEndpoint, cancellationToken);
            if (!response.IsSuccessStatusCode)
            {
                throw new UnableToGetPublicationLatestReleaseSearchViewModelException(
                    publicationSlug,
                    response.StatusCode,
                    response.ReasonPhrase);
            }
        }
        catch (HttpRequestException e)
        {
            throw new UnableToGetPublicationLatestReleaseSearchViewModelException(
                publicationSlug,
                e.StatusCode,
                e.Message);
        }

        var releaseSearchViewModelDto =
            await response.Content.ReadFromJsonAsync<ReleaseSearchViewModelDto>(cancellationToken)
            ?? throw new UnableToGetPublicationLatestReleaseSearchViewModelException(
                publicationSlug,
                "Response content could not be deserialised");

        return releaseSearchViewModelDto.ToModel();
    }

    public async Task<(bool WasSuccesssful, string? ErrorMessage)> Ping(CancellationToken cancellationToken)
    {
        // Determine whether the Content API is available by making a simple call
        try
        {
            var endpoint = BuildGetPublicationsPageEndPoint(page: 1,numberOfItems: 1);
            var response = await HttpClient.GetAsync(endpoint, cancellationToken);
            if (!response.IsSuccessStatusCode)
            {
                var errorMessage = $"Error occured while calling Content API: {response.ReasonPhrase}({response.StatusCode})";
                return (false, errorMessage);
            }
        }
        catch (HttpRequestException e)
        {
            var errorMessage = $"Error occured while calling Content API: {e.Message}({e.StatusCode})";
            return (false, errorMessage);
        }
        catch (Exception e)
        {
            var errorMessage = $"Error occured while calling Content API: {e.Message}";
            return (false, errorMessage);
        }
        
        return (true, null);
    }
    
    private string BuildGetPublicationsPageEndPoint(int page = 1, int numberOfItems = 10) =>
        string.Format(GetPublicationsPageEndPointFormat, page, numberOfItems);
}
