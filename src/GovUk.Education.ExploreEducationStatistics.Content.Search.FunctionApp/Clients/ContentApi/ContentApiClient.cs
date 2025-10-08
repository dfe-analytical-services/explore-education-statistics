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

    private const string GetPublicationReleaseSummaryEndpointFormat = "/api/publications/{0}/releases/{1}/summary";

    private const string GetReleasesForPublicationEndpointFormat = "api/publications/{0}/releases";

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

    public async Task<ReleaseInfo[]> GetReleasesForPublication(
        string publicationSlug,
        CancellationToken cancellationToken = default
    )
    {
        var apiEndpoint = BuildGetReleasesForPublicationEndpoint(publicationSlug);
        var response = await Get<ReleaseSummaryViewModelDto[]>(apiEndpoint, cancellationToken);

        return Process(
            response,
            onSuccess: dtos => dtos.Select(dto => new ReleaseInfo { ReleaseId = dto.ReleaseId }).ToArray(),
            onError: errorMessage => new UnableToGetReleasesForPublicationException(publicationSlug, errorMessage)
        );
    }

    public async Task<ReleaseSummary> GetReleaseSummary(
        string publicationSlug,
        string releaseSlug,
        CancellationToken cancellationToken = default
    )
    {
        var apiEndpoint = BuildGetPublicationReleaseSummaryEndpoint(publicationSlug, releaseSlug);
        var response = await Get<ReleaseSummaryDto>(apiEndpoint, cancellationToken);

        return Process(
            response,
            onSuccess: dto => dto.ToModel(),
            onError: errorMessage => new UnableToGetReleaseSummaryForPublicationException(
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

    private string BuildGetPublicationsByThemeEndpoint(Guid themeId) =>
        string.Format(GetPublicationsByThemeEndpointFormat, themeId.ToString());

    private string BuildGetPublicationReleaseSummaryEndpoint(string publicationSlug, string releaseSlug) =>
        string.Format(GetPublicationReleaseSummaryEndpointFormat, publicationSlug, releaseSlug);

    private string BuildGetReleasesForPublicationEndpoint(string publicationSlug) =>
        string.Format(GetReleasesForPublicationEndpointFormat, publicationSlug);

    private string BuildGetPublicationLatestReleaseSearchViewModelApiEndpoint(string publicationSlug) =>
        string.Format(GetPublicationLatestReleaseSearchViewModelApiEndpointFormat, publicationSlug);

    private TResult Process<TResponse, TResult>(
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
