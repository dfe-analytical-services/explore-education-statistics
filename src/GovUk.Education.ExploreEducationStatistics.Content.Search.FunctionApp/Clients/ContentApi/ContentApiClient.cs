using System.Net.Http.Json;
using GovUk.Education.ExploreEducationStatistics.Content.Search.FunctionApp.Domain;
using GovUk.Education.ExploreEducationStatistics.Content.Search.FunctionApp.Exceptions;

namespace GovUk.Education.ExploreEducationStatistics.Content.Search.FunctionApp.Clients.ContentApi;

internal class ContentApiClient(HttpClient httpClient) : IContentApiClient
{
    internal HttpClient HttpClient { get; } = httpClient;

    private const string GetPublicationLatestReleaseSearchViewModelApiEndPointFormat =
        "api/publications/{0}/releases/latest/searchable";

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
}
