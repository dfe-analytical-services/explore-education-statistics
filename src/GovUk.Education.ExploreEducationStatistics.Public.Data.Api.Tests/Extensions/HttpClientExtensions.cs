using GovUk.Education.ExploreEducationStatistics.Public.Data.Api.Constants;

namespace GovUk.Education.ExploreEducationStatistics.Public.Data.Api.Tests.Extensions;

public static class HttpClientExtensions
{
    public static void AddPreviewTokenHeader(
        this HttpClient client,
        Guid? previewToken = null)
    {
        if (previewToken != null)
        {
            client.DefaultRequestHeaders.Add(RequestHeaderNames.PreviewToken, previewToken.ToString());
        }
    }
}
