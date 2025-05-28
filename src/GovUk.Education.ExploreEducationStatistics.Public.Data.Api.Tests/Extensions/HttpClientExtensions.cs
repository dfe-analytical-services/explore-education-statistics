using GovUk.Education.ExploreEducationStatistics.Common.Extensions;
using GovUk.Education.ExploreEducationStatistics.Public.Data.Utils.Requests;

namespace GovUk.Education.ExploreEducationStatistics.Public.Data.Api.Tests.Extensions;

public static class HttpClientExtensions
{
    public static HttpClient WithPreviewTokenHeader(
        this HttpClient client,
        Guid? previewToken)
    {
        if (previewToken != null)
        {
            client.WithAdditionalHeaders(new Dictionary<string, string>
            {
                { RequestHeaderNames.PreviewToken, previewToken.Value.ToString() }
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
