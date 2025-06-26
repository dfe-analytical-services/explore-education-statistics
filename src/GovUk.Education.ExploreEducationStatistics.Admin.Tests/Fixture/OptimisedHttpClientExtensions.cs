using System.Collections.Generic;
using System.Net.Http;
using GovUk.Education.ExploreEducationStatistics.Common.Extensions;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Tests.Fixture;

public static class OptimisedHttpClientExtensions
{
    public static HttpClient WithUser(
        this HttpClient client,
        string userId)
    {
        return client.WithOptionalHeader("TestUser", userId);
    }
    
    private static HttpClient WithOptionalHeader(
        this HttpClient client,
        string headerName,
        string? headerValue)
    {
        if (headerValue != null)
        {
            client.WithAdditionalHeaders(new Dictionary<string, string>
            {
                { headerName, headerValue }
            });
        }

        return client;
    }

    public static HttpClient WithAdditionalHeaders(
        this HttpClient client,
        Dictionary<string, string>? additionalHeaders)
    {
        additionalHeaders?.ForEach(header =>
            client.DefaultRequestHeaders.Add(header.Key, header.Value));

        return client;
    }
}
