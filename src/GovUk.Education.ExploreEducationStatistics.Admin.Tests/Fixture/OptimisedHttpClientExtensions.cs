#nullable enable
using System.Collections.Generic;
using System.Net.Http;
using System.Security.Claims;
using GovUk.Education.ExploreEducationStatistics.Common.Extensions;
using GovUk.Education.ExploreEducationStatistics.Common.Services.Security;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Tests.Fixture;

public static class OptimisedHttpClientExtensions
{
    public static HttpClient WithUser(this HttpClient client, ClaimsPrincipal user)
    {
        return client.WithOptionalHeader("TestUser", user.GetUserId().ToString());
    }

    private static HttpClient WithOptionalHeader(
        this HttpClient client,
        string headerName,
        string? headerValue
    )
    {
        if (headerValue != null)
        {
            client.WithAdditionalHeaders(
                new Dictionary<string, string> { { headerName, headerValue } }
            );
        }

        return client;
    }

    private static HttpClient WithAdditionalHeaders(
        this HttpClient client,
        Dictionary<string, string>? additionalHeaders
    )
    {
        additionalHeaders?.ForEach(header =>
            client.DefaultRequestHeaders.Add(header.Key, header.Value)
        );

        return client;
    }
}
