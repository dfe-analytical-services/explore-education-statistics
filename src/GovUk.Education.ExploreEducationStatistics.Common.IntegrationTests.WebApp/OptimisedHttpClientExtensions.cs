namespace GovUk.Education.ExploreEducationStatistics.Common.IntegrationTests.WebApp;

/// <summary>
/// Some convenience methods for <see cref="HttpClient"/> to be used within the test classes.
/// </summary>
public static class OptimisedHttpClientExtensions
{
    public static HttpClient WithOptionalHeader(this HttpClient client, string headerName, string? headerValue)
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
        if (additionalHeaders == null)
        {
            return client;
        }

        foreach (var header in additionalHeaders)
        {
            client.DefaultRequestHeaders.Add(header.Key, header.Value);
        }

        return client;
    }
}
