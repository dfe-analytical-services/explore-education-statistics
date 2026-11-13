using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using GovUk.Education.ExploreEducationStatistics.Content.Search.FunctionApp.Clients.ContentApi.Dtos;
using GovUk.Education.ExploreEducationStatistics.Content.Search.FunctionApp.Domain;
using GovUk.Education.ExploreEducationStatistics.Content.Search.FunctionApp.Exceptions;
using Microsoft.Extensions.Logging;

namespace GovUk.Education.ExploreEducationStatistics.Content.Search.FunctionApp.Clients.ContentApi;

[ExcludeFromCodeCoverage]
internal class ContentApiClient(HttpClient httpClient, ILogger<ContentApiClient> logger) : IContentApiClient
{
    internal HttpClient HttpClient { get; } = httpClient;

    private const string GetPublicationLatestReleaseSearchableDocumentApiEndpointFormat =
        "api/publications/{0}/releases/latest/searchable";

    private const string GetPublicationsEndpoint = "/api/publicationInfos";

    private const string GetPublicationsByThemeEndpointFormat = "/api/publicationInfos?ThemeId={0}";

    private const string GetReleaseVersionSummaryEndpointFormat = "/api/publications/{0}/releases/{1}/version-summary";

    private const string GetPublicationReleaseIdsEndpointFormat = "api/publications/{0}/release-ids";

    private const string GetDataSetDocumentKeysEndpoint = "/api/data-sets/searchable/keys";

    private const string GetDataSetDocumentPublicationsEndpoint = "/api/data-sets/searchable/publications";

    private const string GetReleaseDataSetDocumentsEndpointFormat =
        "/api/publications/{0}/releases/{1}/data-sets/searchable";

    private const string GetPublicationDataSetDocumentsEndpointFormat = "/api/publications/{0}/data-sets/searchable";

    private const string GetThemesEndpoint = "/api/themes";

    private record GetResponse<T>
    {
        public record Success(T Result) : GetResponse<T>;

        public record Error(string ErrorMessage) : GetResponse<T>;

        public record NotFound(string ErrorMessage) : Error(ErrorMessage);
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
                var errorMessage = $"Error calling {apiEndpoint}: {response.ReasonPhrase}({response.StatusCode})";
                return response.StatusCode == HttpStatusCode.NotFound
                    ? new GetResponse<TResponse>.NotFound(errorMessage)
                    : new GetResponse<TResponse>.Error(errorMessage);
            }
        }
        catch (HttpRequestException e)
        {
            return new GetResponse<TResponse>.Error($"Error calling {apiEndpoint}: {e.Message}({e.StatusCode})");
        }

        TResponse? responseData;
        try
        {
            responseData = await response.Content.ReadFromJsonAsync<TResponse>(cancellationToken);
        }
        catch (JsonException e)
        {
            return new GetResponse<TResponse>.Error(
                $"Error deserialising response content from {apiEndpoint}: {e.Message}"
            );
        }

        return responseData is null
            ? new GetResponse<TResponse>.Error($"Response content could not be deserialised")
            : new GetResponse<TResponse>.Success(responseData);
    }

    public async Task<ReleaseDocument> GetPublicationLatestReleaseDocument(
        string publicationSlug,
        CancellationToken cancellationToken
    )
    {
        var apiEndpoint = BuildGetPublicationLatestReleaseSearchableDocumentApiEndpoint(publicationSlug);
        var response = await Get<ReleaseSearchableDocumentDto>(apiEndpoint, cancellationToken);

        return Process(
            response,
            onSuccess: dto => dto.ToModel(),
            onError: errorMessage => new UnableToGetPublicationLatestReleaseDocumentException(
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

    public async Task<DataSetDocumentKey[]> GetDataSetDocumentKeys(CancellationToken cancellationToken = default)
    {
        var response = await Get<DataSetDocumentKeyDto[]>(GetDataSetDocumentKeysEndpoint, cancellationToken);

        return Process(
            response,
            onSuccess: dtos => dtos.Select(dto => dto.ToModel()).ToArray(),
            onError: errorMessage => new UnableToGetDataSetDocumentKeysException(errorMessage)
        );
    }

    public async Task<DataSetDocumentPublication[]> GetDataSetDocumentPublications(
        CancellationToken cancellationToken = default
    )
    {
        var response = await Get<DataSetDocumentPublicationDto[]>(
            GetDataSetDocumentPublicationsEndpoint,
            cancellationToken
        );

        return Process(
            response,
            onSuccess: dtos => dtos.Select(dto => dto.ToModel()).ToArray(),
            onError: errorMessage => new UnableToGetDataSetDocumentPublicationsException(errorMessage)
        );
    }

    public async Task<DataSetDocument[]> GetReleaseDataSetDocuments(
        string publicationSlug,
        string releaseSlug,
        CancellationToken cancellationToken = default
    )
    {
        var apiEndpoint = string.Format(GetReleaseDataSetDocumentsEndpointFormat, publicationSlug, releaseSlug);
        var response = await Get<DataSetSearchableDocumentDto[]>(apiEndpoint, cancellationToken);

        // A release that is not found is treated as an error rather than as a release without data set documents.
        // Otherwise a slug that no longer resolves would lead to the removal of the documents of a published release.
        if (response is GetResponse<DataSetSearchableDocumentDto[]>.NotFound)
        {
            logger.LogWarning(
                """Release "{ReleaseSlug}" of publication "{PublicationSlug}" was not found by the Content API. Its data set documents have not been refreshed or removed.""",
                releaseSlug,
                publicationSlug
            );
        }

        return Process(
            response,
            onSuccess: dtos => dtos.Select(dto => dto.ToModel()).ToArray(),
            onError: errorMessage => new UnableToGetReleaseDataSetDocumentsException(
                publicationSlug,
                releaseSlug,
                errorMessage
            )
        );
    }

    public async Task<DataSetDocument[]> GetPublicationDataSetDocuments(
        string publicationSlug,
        CancellationToken cancellationToken = default
    )
    {
        var apiEndpoint = string.Format(GetPublicationDataSetDocumentsEndpointFormat, publicationSlug);
        var response = await Get<DataSetSearchableDocumentDto[]>(apiEndpoint, cancellationToken);

        // A publication that is not found is treated as an error rather than as a publication without data set
        // documents. Otherwise a slug that no longer resolves would lead to the removal of the documents of a
        // published publication.
        if (response is GetResponse<DataSetSearchableDocumentDto[]>.NotFound)
        {
            logger.LogWarning(
                """Publication "{PublicationSlug}" was not found by the Content API. Its data set documents have not been refreshed or removed.""",
                publicationSlug
            );
        }

        return Process(
            response,
            onSuccess: dtos => dtos.Select(dto => dto.ToModel()).ToArray(),
            onError: errorMessage => new UnableToGetPublicationDataSetDocumentsException(publicationSlug, errorMessage)
        );
    }

    private static string BuildGetPublicationsByThemeEndpoint(Guid themeId) =>
        string.Format(GetPublicationsByThemeEndpointFormat, themeId.ToString());

    private static string BuildGetReleaseVersionSummaryEndpoint(string publicationSlug, string releaseSlug) =>
        string.Format(GetReleaseVersionSummaryEndpointFormat, publicationSlug, releaseSlug);

    private static string BuildGetPublicationReleaseIdsEndpoint(string publicationSlug) =>
        string.Format(GetPublicationReleaseIdsEndpointFormat, publicationSlug);

    private static string BuildGetPublicationLatestReleaseSearchableDocumentApiEndpoint(string publicationSlug) =>
        string.Format(GetPublicationLatestReleaseSearchableDocumentApiEndpointFormat, publicationSlug);

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
