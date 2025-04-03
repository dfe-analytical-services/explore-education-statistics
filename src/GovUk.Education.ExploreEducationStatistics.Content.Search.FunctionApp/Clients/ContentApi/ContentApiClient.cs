using System.Diagnostics.CodeAnalysis;
using System.Net.Http.Json;
using GovUk.Education.ExploreEducationStatistics.Content.Search.FunctionApp.Clients.ContentApi.Dtos;
using GovUk.Education.ExploreEducationStatistics.Content.Search.FunctionApp.Domain;
using GovUk.Education.ExploreEducationStatistics.Content.Search.FunctionApp.Exceptions;

namespace GovUk.Education.ExploreEducationStatistics.Content.Search.FunctionApp.Clients.ContentApi;

[ExcludeFromCodeCoverage]
internal class ContentApiClient(HttpClient httpClient) : IContentApiClient
{
    internal HttpClient HttpClient { get; } = httpClient;

    private const string GetPublicationLatestReleaseSearchViewModelApiEndpointFormat =
        "api/publications/{0}/releases/latest/searchable";

    private const string GetPublicationsPageEndpointFormat =
        "/api/publications?Page={0}&PageSize={1}";

    private const string GetPublicationsByThemePageEndpointFormat =
        "/api/publications?ThemeId={1}&Page={0}";


    private record GetResponse<T>
    {
        public record Success(T Result) : GetResponse<T>;

        public record Error(string ErrorMessage) : GetResponse<T>;
    }

    private async Task<GetResponse<TResponse>> Get<TResponse>(
        string apiEndpoint,
        CancellationToken cancellationToken = default)
    {
        HttpResponseMessage response;
        try
        {
            response = await HttpClient.GetAsync(apiEndpoint, cancellationToken);
            if (!response.IsSuccessStatusCode)
            {
                return new GetResponse<TResponse>.Error(
                    $"Error calling {apiEndpoint}: {response.ReasonPhrase}({response.StatusCode})");
            }
        }
        catch (HttpRequestException e)
        {
            return new GetResponse<TResponse>.Error($"Error calling {apiEndpoint}: {e.Message}({e.StatusCode})");
        }

        var responseData = await response.Content.ReadFromJsonAsync<TResponse>(cancellationToken);

        return
            responseData is null
                ? new GetResponse<TResponse>.Error($"Response content could not be deserialised")
                : new GetResponse<TResponse>.Success(responseData);
    }

    public async Task<ReleaseSearchableDocument> GetPublicationLatestReleaseSearchableDocument(
        string publicationSlug,
        CancellationToken cancellationToken)
    {
        var apiEndpoint = string.Format(GetPublicationLatestReleaseSearchViewModelApiEndpointFormat, publicationSlug);
        var response = await Get<ReleaseSearchViewModelDto>(apiEndpoint, cancellationToken);

        return response switch
        {
            GetResponse<ReleaseSearchViewModelDto>.Success success =>
                success.Result.ToModel(),

            GetResponse<ReleaseSearchViewModelDto>.Error error =>
                throw new UnableToGetPublicationLatestReleaseSearchViewModelException(
                    publicationSlug,
                    error.ErrorMessage),

            _ => throw new ArgumentOutOfRangeException()
        };
    }

    public async Task<(bool WasSuccesssful, string? ErrorMessage)> Ping(CancellationToken cancellationToken)
    {
        // Determine whether the Content API is available by making a simple call
        var apiEndpoint = BuildGetPublicationsPageEndpoint(page: 1, numberOfItems: 1);
        var response = await Get<PaginatedResultDto<PublicationDto>>(apiEndpoint, cancellationToken);
        return response switch
        {
            GetResponse<PaginatedResultDto<PublicationDto>>.Success => (true, null),
            GetResponse<PaginatedResultDto<PublicationDto>>.Error error => (false, error.ErrorMessage),
            _ => throw new ArgumentOutOfRangeException()
        };
    }

    public async Task<PublicationInfo[]> GetPublicationsForTheme(Guid themeId, CancellationToken cancellationToken)
    {
        var publicationDtos = await GetAllPaginatedItems<PublicationDto>(page => BuildGetPublicationsByThemePageEndpoint(themeId, page), cancellationToken);
        return publicationDtos
            .Where(dto => !string.IsNullOrEmpty(dto.Slug))
            .Select(dto => new PublicationInfo
            {
                PublicationSlug = dto.Slug!
            })
            .ToArray();
    }

    private async Task<TResponse[]> GetAllPaginatedItems<TResponse>(Func<int, string> getPageApiEndpoint, CancellationToken cancellationToken)
    {
        var page = 1;
        var morePagesToGet = true;
        var items = new List<TResponse>();

        while (morePagesToGet && cancellationToken.IsCancellationRequested == false)
        {
            // Get a page of results from the API
            var apiEndpoint = getPageApiEndpoint(page);
            var getResponse = await Get<PaginatedResultDto<TResponse>>(apiEndpoint, cancellationToken);
            
            // If there was an error then throw
            if (getResponse is GetResponse<PaginatedResultDto<TResponse>>.Error error)
            {
                throw new GetPaginatedItemsException(error.ErrorMessage);
            }

            // Store the latest page of results and if there are more to retrieve, loop around
            if (getResponse is GetResponse<PaginatedResultDto<TResponse>>.Success success)
            {
                if (success.Result.Paging is null)
                {
                    throw new GetPaginatedItemsException("Paginated response did not contain the expected page information.");
                }

                if (success.Result.Results is null)
                {
                    throw new GetPaginatedItemsException("Paginated response did not contain any data.");
                }

                items.AddRange(success.Result.Results);
                morePagesToGet = success.Result.Paging.Page < success.Result.Paging.TotalPages;
                page++;
            }
        }
        return items.ToArray();
    }
    
    private string BuildGetPublicationsPageEndpoint(int page = 1, int numberOfItems = 10) =>
        string.Format(GetPublicationsPageEndpointFormat, page, numberOfItems);

    private string BuildGetPublicationsByThemePageEndpoint(Guid themeId, int page = 1) =>
        string.Format(GetPublicationsByThemePageEndpointFormat, page, themeId.ToString());
}
