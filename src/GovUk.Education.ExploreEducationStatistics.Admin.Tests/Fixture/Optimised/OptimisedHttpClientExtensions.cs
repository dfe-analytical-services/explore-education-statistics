#nullable enable
using System.Security.Claims;
using GovUk.Education.ExploreEducationStatistics.Common.Extensions;
using GovUk.Education.ExploreEducationStatistics.Common.Services.Security;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Tests.Fixture.Optimised;

/// <summary>
/// Some convenience methods for <see cref="HttpClient"/> to be used within the test classes.
/// </summary>
public static class OptimisedHttpClientExtensions
{
    /// <summary>
    ///
    /// Set the user who will be making the call via the HttpClient. This will make the user available to the
    /// <see cref="OptimisedTestAuthHandler"/> which can then look up the user from the <see cref="OptimisedTestUserPool"/>.
    ///
    /// In order to find the appropriate user in the pool, the user must be registered in the pool prior to
    /// using the HttpClient to make any calls. This can be done via the appropriate Collection fixture's
    /// "RegisterTestUser" method.
    ///
    /// </summary>
    public static HttpClient WithUser(this HttpClient client, ClaimsPrincipal user)
    {
        return client.WithOptionalHeader(OptimisedTestAuthHandler.TestUserId, user.GetUserId().ToString());
    }

    private static HttpClient WithOptionalHeader(this HttpClient client, string headerName, string? headerValue)
    {
        if (headerValue != null)
        {
            return client.WithAdditionalHeaders(new Dictionary<string, string> { { headerName, headerValue } });
        }

        return client;
    }

    private static HttpClient WithAdditionalHeaders(
        this HttpClient client,
        Dictionary<string, string>? additionalHeaders
    )
    {
        additionalHeaders?.ForEach(header => client.DefaultRequestHeaders.Add(header.Key, header.Value));
        return client;
    }
}
