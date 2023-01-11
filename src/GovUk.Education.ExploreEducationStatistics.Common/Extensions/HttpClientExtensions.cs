#nullable enable
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace GovUk.Education.ExploreEducationStatistics.Common.Extensions;

public static class HttpClientExtensions
{
    public static Task<HttpResponseMessage> GetAsync(
        this HttpClient client,
        string uri,
        IDictionary<string, string> headers,
        CancellationToken cancellationToken = default)
    {
        return client.SendAsync(HttpMethod.Get, uri, headers: headers, cancellationToken: cancellationToken);
    }

    public static Task<HttpResponseMessage> PostAsync(
        this HttpClient client,
        string uri,
        HttpContent content,
        IDictionary<string, string> headers,
        CancellationToken cancellationToken = default)
    {
        return client.SendAsync(HttpMethod.Post, uri, content, headers, cancellationToken);
    }

    public static Task<HttpResponseMessage> PatchAsync(
        this HttpClient client,
        string uri,
        HttpContent content,
        IDictionary<string, string> headers,
        CancellationToken cancellationToken = default)
    {
        return client.SendAsync(HttpMethod.Patch, uri, content, headers, cancellationToken);
    }


    public static Task<HttpResponseMessage> PutAsync(
        this HttpClient client,
        string uri,
        HttpContent content,
        IDictionary<string, string> headers,
        CancellationToken cancellationToken = default)
    {
        return client.SendAsync(HttpMethod.Put, uri, content, headers, cancellationToken);
    }

    public static Task<HttpResponseMessage> DeleteAsync(
        this HttpClient client,
        string uri,
        HttpContent content,
        IDictionary<string, string> headers,
        CancellationToken cancellationToken = default)
    {
        return client.SendAsync(HttpMethod.Delete, uri, content, headers, cancellationToken);
    }

    private static Task<HttpResponseMessage> SendAsync(
        this HttpClient client,
        HttpMethod method,
        string uri,
        HttpContent? content = null,
        IDictionary<string, string>? headers = null,
        CancellationToken cancellationToken = default)
    {
        var message = new HttpRequestMessage
        {
            Method = method,
            RequestUri = new Uri(uri, UriKind.RelativeOrAbsolute),
            Content = content,
        };

        if (headers is not null)
        {
            message.AddHeaders(headers);
        }

        return client.SendAsync(message, cancellationToken);
    }
}
