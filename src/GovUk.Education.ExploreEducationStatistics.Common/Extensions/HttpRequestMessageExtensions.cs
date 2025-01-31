#nullable enable
using System.Collections.Generic;
using System.Net.Http;

namespace GovUk.Education.ExploreEducationStatistics.Common.Extensions;

public static class HttpRequestMessageExtensions
{
    public static void AddHeaders(this HttpRequestMessage message, IDictionary<string, string> headers)
    {
        foreach (var header in headers)
        {
            message.Headers.Add(header.Key, header.Value);
        }
    }
}
