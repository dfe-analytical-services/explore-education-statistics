using System.Diagnostics;
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

    private const string GetPublicationsEndpoint = "/api/publicationInfos";

    private const string GetPublicationsByThemeEndpointFormat = "/api/publicationInfos?ThemeId={0}";

    private const string GetReleaseVersionSummaryEndpointFormat = "/api/publications/{0}/releases/{1}/version-summary";

    private const string GetPublicationReleaseIdsEndpointFormat = "api/publications/{0}/release-ids";

    private const string GetThemesEndpoint = "/api/themes";

    private record GetResponse<T>
    {
        public record Success(T Result) : GetResponse<T>;

        public record Error(string ErrorMessage) : GetResponse<T>;
    }

    private async Task<GetResponse<TResponse>> Get<TResponse>(
        string apiEndpoint,
        CancellationToken cancellationToken = default
    )
    {
        HttpResponseMessage response;
        try
        {
            response = await HttpClient.GetAsync(apiEndpoint, cancellationToken);
            if (!response.IsSuccessStatusCode)
            {
                return new GetResponse<TResponse>.Error(
                    $"Error calling {apiEndpoint}: {response.ReasonPhrase}({response.StatusCode})"
                );
            }
        }
        catch (HttpRequestException e)
        {
            return new GetResponse<TResponse>.Error($"Error calling {apiEndpoint}: {e.Message}({e.StatusCode})");
        }

        var responseData = await response.Content.ReadFromJsonAsync<TResponse>(cancellationToken);

        return responseData is null
            ? new GetResponse<TResponse>.Error($"Response content could not be deserialised")
            : new GetResponse<TResponse>.Success(responseData);
    }

    public async Task<ReleaseSearchableDocument> GetPublicationLatestReleaseSearchableDocument(
        string publicationSlug,
        CancellationToken cancellationToken
    )
    {
        var apiEndpoint = BuildGetPublicationLatestReleaseSearchViewModelApiEndpoint(publicationSlug);
        var response = await Get<ReleaseSearchViewModelDto>(apiEndpoint, cancellationToken);

        return Process(
            response,
            onSuccess: dto => dto.ToModel(),
            onError: errorMessage => new UnableToGetPublicationLatestReleaseSearchViewModelException(
                publicationSlug,
                errorMessage
            )
        );
    }

    public async Task<(bool WasSuccesssful, string? ErrorMessage)> Ping(CancellationToken cancellationToken = default)
    {
        // Determine whether the Content API is available by making a simple call
        var response = await Get<object>(GetThemesEndpoint, cancellationToken);

        return response switch
        {
            GetResponse<object>.Success => (true, null),
            GetResponse<object>.Error error => (false, error.ErrorMessage),
            _ => throw new UnreachableException(),
        };
    }

    public async Task<PublicationInfo[]> GetPublicationsForTheme(Guid themeId, CancellationToken cancellationToken)
    {
        var apiEndpoint = BuildGetPublicationsByThemeEndpoint(themeId);
        var response = await Get<PublicationInfoDto[]>(apiEndpoint, cancellationToken);

        return Process(
            response,
            onSuccess: dtos => dtos.Select(dto => dto.ToModel()).ToArray(),
            onError: errorMessage => new UnableToGetPublicationsByThemeException(themeId, errorMessage)
        );
    }

    public async Task<Guid[]> GetPublicationReleaseIds(
        string publicationSlug,
        CancellationToken cancellationToken = default
    )
    {
        var apiEndpoint = BuildGetPublicationReleaseIdsEndpoint(publicationSlug);
        var response = await Get<Guid[]>(apiEndpoint, cancellationToken);

        return Process(
            response,
            onSuccess: releaseIds => releaseIds,
            onError: errorMessage => new UnableToGetPublicationReleaseIdsException(publicationSlug, errorMessage)
        );
    }

    public async Task<ReleaseVersionSummary> GetReleaseVersionSummary(
        string publicationSlug,
        string releaseSlug,
        CancellationToken cancellationToken = default
    )
    {
        var apiEndpoint = BuildGetReleaseVersionSummaryEndpoint(publicationSlug, releaseSlug);
        var response = await Get<ReleaseVersionSummaryDto>(apiEndpoint, cancellationToken);

        return Process(
            response,
            onSuccess: dto => dto.ToModel(),
            onError: errorMessage => new UnableToGetReleaseVersionSummaryException(
                publicationSlug,
                releaseSlug,
                errorMessage
            )
        );
    }

    public async Task<PublicationInfo[]> GetAllLivePublicationInfos(CancellationToken cancellationToken)
    {
        var response = await Get<PublicationInfoDto[]>(GetPublicationsEndpoint, cancellationToken);

        return Process(
            response,
            onSuccess: dtos => dtos.Select(dto => dto.ToModel()).ToArray(),
            onError: errorMessage => new UnableToGetPublicationInfosException(errorMessage)
        );
    }

    private static string BuildGetPublicationsByThemeEndpoint(Guid themeId) =>
        string.Format(GetPublicationsByThemeEndpointFormat, themeId.ToString());

    private static string BuildGetReleaseVersionSummaryEndpoint(string publicationSlug, string releaseSlug) =>
        string.Format(GetReleaseVersionSummaryEndpointFormat, publicationSlug, releaseSlug);

    private static string BuildGetPublicationReleaseIdsEndpoint(string publicationSlug) =>
        string.Format(GetPublicationReleaseIdsEndpointFormat, publicationSlug);

    private static string BuildGetPublicationLatestReleaseSearchViewModelApiEndpoint(string publicationSlug) =>
        string.Format(GetPublicationLatestReleaseSearchViewModelApiEndpointFormat, publicationSlug);

    private static TResult Process<TResponse, TResult>(
        GetResponse<TResponse> response,
        Func<TResponse, TResult> onSuccess,
        Func<string, Exception> onError
    ) =>
        response switch
        {
            GetResponse<TResponse>.Success success => onSuccess(success.Result),
            GetResponse<TResponse>.Error error => throw onError(error.ErrorMessage),
            _ => throw new UnreachableException(),
        };
}
