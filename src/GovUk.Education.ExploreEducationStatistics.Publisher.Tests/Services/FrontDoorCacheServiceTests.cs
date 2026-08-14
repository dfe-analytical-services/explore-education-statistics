using System.Net;
using System.Text.Json;
using Azure.Core;
using GovUk.Education.ExploreEducationStatistics.Publisher.Options;
using GovUk.Education.ExploreEducationStatistics.Publisher.Services;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace GovUk.Education.ExploreEducationStatistics.Publisher.Tests.Services;

public class FrontDoorCacheServiceTests
{
    private static readonly AzureFrontDoorOptions EnabledOptions = new()
    {
        CachePurgeEnabled = true,
        EndpointResourceId =
            "/subscriptions/sub/resourceGroups/rg/providers/Microsoft.Cdn/profiles/profile/afdEndpoints/endpoint",
        ContentApiHostName = "content.dev.explore-education-statistics.service.gov.uk",
    };

    [Fact]
    public async Task PurgeAllFilesZipCache_Disabled_DoesNotSendRequest()
    {
        var handler = new RecordingHttpMessageHandler(HttpStatusCode.OK);
        var service = BuildService(
            handler,
            new AzureFrontDoorOptions
            {
                CachePurgeEnabled = false,
                EndpointResourceId = EnabledOptions.EndpointResourceId,
                ContentApiHostName = EnabledOptions.ContentApiHostName,
            }
        );

        await service.PurgeAllFilesZipCache(new HashSet<Guid> { Guid.NewGuid() });

        Assert.Empty(handler.Requests);
    }

    [Fact]
    public async Task PurgeAllFilesZipCache_Success_SendsSingleBatchedRequest()
    {
        var releaseVersionId1 = Guid.NewGuid();
        var releaseVersionId2 = Guid.NewGuid();
        var handler = new RecordingHttpMessageHandler(HttpStatusCode.Accepted);
        var service = BuildService(handler);

        await service.PurgeAllFilesZipCache(new HashSet<Guid> { releaseVersionId2, releaseVersionId1 });

        var request = Assert.Single(handler.Requests);
        Assert.Equal(HttpMethod.Post, request.Method);
        Assert.Equal(
            $"https://management.azure.com{EnabledOptions.EndpointResourceId}/purge?api-version=2025-04-15",
            request.RequestUri?.ToString()
        );
        Assert.Equal("Bearer", request.AuthorizationScheme);
        Assert.Equal("test-token", request.AuthorizationToken);

        using var body = JsonDocument.Parse(request.Body);
        Assert.Equal(
            new[] { $"/api/all-files/{releaseVersionId1}/*", $"/api/all-files/{releaseVersionId2}/*" }.Order(),
            body.RootElement.GetProperty("contentPaths").EnumerateArray().Select(value => value.GetString()).Order()
        );
        Assert.Equal(EnabledOptions.ContentApiHostName, body.RootElement.GetProperty("domains")[0].GetString());
    }

    [Fact]
    public async Task PurgeAllFilesZipCache_TransientFailures_RetriesAndDoesNotThrow()
    {
        var handler = new RecordingHttpMessageHandler(
            HttpStatusCode.InternalServerError,
            HttpStatusCode.TooManyRequests,
            HttpStatusCode.ServiceUnavailable
        );
        var service = BuildService(handler);

        await service.PurgeAllFilesZipCache(new HashSet<Guid> { Guid.NewGuid() });

        Assert.Equal(3, handler.Requests.Count);
    }

    private static FrontDoorCacheService BuildService(
        RecordingHttpMessageHandler handler,
        AzureFrontDoorOptions? options = null
    ) =>
        new(
            new HttpClient(handler),
            new TestTokenCredential(),
            Microsoft.Extensions.Options.Options.Create(options ?? EnabledOptions),
            TimeProvider.System,
            Mock.Of<ILogger<FrontDoorCacheService>>()
        );

    private sealed class TestTokenCredential : TokenCredential
    {
        public override AccessToken GetToken(TokenRequestContext requestContext, CancellationToken cancellationToken) =>
            new("test-token", DateTimeOffset.MaxValue);

        public override ValueTask<AccessToken> GetTokenAsync(
            TokenRequestContext requestContext,
            CancellationToken cancellationToken
        ) => ValueTask.FromResult(new AccessToken("test-token", DateTimeOffset.MaxValue));
    }

    private sealed class RecordingHttpMessageHandler(params HttpStatusCode[] responses) : HttpMessageHandler
    {
        private readonly Queue<HttpStatusCode> _responses = new(responses);

        public List<RecordedRequest> Requests { get; } = [];

        protected override async Task<HttpResponseMessage> SendAsync(
            HttpRequestMessage request,
            CancellationToken cancellationToken
        )
        {
            Requests.Add(
                new RecordedRequest(
                    request.Method,
                    request.RequestUri,
                    request.Headers.Authorization?.Scheme,
                    request.Headers.Authorization?.Parameter,
                    await request.Content!.ReadAsStringAsync(cancellationToken)
                )
            );

            return new HttpResponseMessage(_responses.Dequeue());
        }
    }

    private record RecordedRequest(
        HttpMethod Method,
        Uri? RequestUri,
        string? AuthorizationScheme,
        string? AuthorizationToken,
        string Body
    );
}
