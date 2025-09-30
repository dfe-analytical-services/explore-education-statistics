using GovUk.Education.ExploreEducationStatistics.Common.Extensions;
using GovUk.Education.ExploreEducationStatistics.Public.Data.Utils.Requests;

namespace GovUk.Education.ExploreEducationStatistics.Public.Data.Api.Tests.Extensions;

public static class HttpClientExtensions
{
    public static HttpClient WithPreviewTokenHeader(
        this HttpClient client,
        Guid? previewToken)
    {
        return client.WithOptionalHeader(RequestHeaderNames.PreviewToken, previewToken?.ToString());
    }

    public static HttpClient WithRequestSourceHeader(
        this HttpClient client,
        string? requestSource)
    {
        return client.WithOptionalHeader(RequestHeaderNames.RequestSource, requestSource);
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
