using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using Azure.Core;
using GovUk.Education.ExploreEducationStatistics.Publisher.Options;
using GovUk.Education.ExploreEducationStatistics.Publisher.Services.Interfaces;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace GovUk.Education.ExploreEducationStatistics.Publisher.Services;

public class FrontDoorCacheService(
    HttpClient httpClient,
    TokenCredential tokenCredential,
    IOptions<AzureFrontDoorOptions> options,
    TimeProvider timeProvider,
    ILogger<FrontDoorCacheService> logger
) : IFrontDoorCacheService
{
    private const string AzureManagementScope = "https://management.azure.com/.default";
    private const string ApiVersion = "2025-04-15";
    private static readonly TimeSpan[] RetryDelays = [TimeSpan.FromSeconds(1), TimeSpan.FromSeconds(2)];

    public async Task PurgeAllFilesZipCache(
        IReadOnlySet<Guid> releaseVersionIds,
        CancellationToken cancellationToken = default
    )
    {
        var frontDoorOptions = options.Value;
        if (!frontDoorOptions.CachePurgeEnabled || releaseVersionIds.Count == 0)
        {
            return;
        }

        var contentPaths = releaseVersionIds.Select(id => $"/api/all-files/{id}/*").Order().ToArray();

        try
        {
            var accessToken = await tokenCredential.GetTokenAsync(
                new TokenRequestContext([AzureManagementScope]),
                cancellationToken
            );

            for (var attempt = 0; attempt <= RetryDelays.Length; attempt++)
            {
                try
                {
                    using var request = CreateRequest(frontDoorOptions, contentPaths, accessToken.Token);
                    using var response = await httpClient.SendAsync(request, cancellationToken);

                    if (response.StatusCode is HttpStatusCode.OK or HttpStatusCode.Accepted)
                    {
                        logger.LogInformation(
                            "Accepted Azure Front Door cache purge for paths {ContentPaths}",
                            contentPaths
                        );
                        return;
                    }

                    var responseBody = await response.Content.ReadAsStringAsync(cancellationToken);
                    if (!IsTransient(response.StatusCode) || attempt == RetryDelays.Length)
                    {
                        logger.LogError(
                            "Azure Front Door cache purge failed with status {StatusCode} for paths {ContentPaths}. Response: {ResponseBody}",
                            (int)response.StatusCode,
                            contentPaths,
                            responseBody
                        );
                        return;
                    }
                }
                catch (HttpRequestException exception) when (attempt < RetryDelays.Length)
                {
                    logger.LogWarning(
                        exception,
                        "Azure Front Door cache purge attempt {Attempt} failed for paths {ContentPaths}",
                        attempt + 1,
                        contentPaths
                    );
                }
                catch (OperationCanceledException exception)
                    when (!cancellationToken.IsCancellationRequested && attempt < RetryDelays.Length)
                {
                    logger.LogWarning(
                        exception,
                        "Azure Front Door cache purge attempt {Attempt} timed out for paths {ContentPaths}",
                        attempt + 1,
                        contentPaths
                    );
                }

                await Task.Delay(RetryDelays[attempt], timeProvider, cancellationToken);
            }
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            throw;
        }
        catch (Exception exception)
        {
            logger.LogError(exception, "Azure Front Door cache purge failed for paths {ContentPaths}", contentPaths);
        }
    }

    private static HttpRequestMessage CreateRequest(
        AzureFrontDoorOptions options,
        string[] contentPaths,
        string accessToken
    )
    {
        var endpointResourceId = options.EndpointResourceId.TrimEnd('/');
        var request = new HttpRequestMessage(
            HttpMethod.Post,
            $"https://management.azure.com{endpointResourceId}/purge?api-version={ApiVersion}"
        )
        {
            Content = JsonContent.Create(new { contentPaths, domains = new[] { options.ContentApiHostName } }),
        };
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);
        return request;
    }

    private static bool IsTransient(HttpStatusCode statusCode) =>
        statusCode is HttpStatusCode.RequestTimeout or HttpStatusCode.TooManyRequests || (int)statusCode >= 500;
}
