#nullable enable
using System.IO;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace GovUk.Education.ExploreEducationStatistics.Common.Extensions;

public static class HttpContentExtensions
{
    public static Task<T?> ReadFromJsonAsync<T>(
        this HttpContent content,
        JsonSerializerSettings? settings = null,
        CancellationToken cancellationToken = default)
    {
        return content.ReadFromJsonAsync<T>(Encoding.UTF8, settings, cancellationToken);
    }

    /// <summary>
    /// Read JSON serialised content using <see cref="Newtonsoft.Json"/> asynchronously.
    /// </summary>
    ///
    /// <remarks>
    /// <para>
    /// This should be used instead of the method from <see cref="System.Net.Http.Json.HttpContentJsonExtensions"/>,
    /// which uses the newer <see cref="System.Text.Json"/> APIs.
    /// </para>
    /// <para>
    /// Ideally, we'll want to remove this extension if we ever migrate to <see cref="System.Text.Json"/>
    /// for our API requests and responses.
    /// </para>
    /// </remarks>
    ///
    /// <param name="content">The JSON serialised HTTP content.</param>
    /// <param name="sourceEncoding">The content's character encoding.</param>
    /// <param name="settings">The settings for deserializing the JSON.</param>
    /// <param name="cancellationToken">A token to signal that the read should be cancelled.</param>
    /// <typeparam name="T">The type of the object that the JSON should deserialize to.</typeparam>
    /// <returns>The deserialized object.</returns>
    public static async Task<T?> ReadFromJsonAsync<T>(
        this HttpContent content,
        Encoding sourceEncoding,
        JsonSerializerSettings? settings = null,
        CancellationToken cancellationToken = default)
    {
        await using var contentStream =
            await GetContentStreamAsync(content, sourceEncoding, cancellationToken).ConfigureAwait(false);

        using var streamReader = new StreamReader(contentStream);
        await using var reader = new JsonTextReader(streamReader);

        // This isn't actually asynchronous. It doesn't look like
        // Json.NET is likely to ever implement this option.
        // See: https://github.com/JamesNK/Newtonsoft.Json/issues/1193.
        return JsonSerializer.Create(settings).Deserialize<T>(reader);
    }

    public static T? ReadFromJson<T>(
        this HttpContent content,
        JsonSerializerSettings? settings = null,
        CancellationToken cancellationToken = default)
    {
        return content.ReadFromJson<T>(Encoding.UTF8, settings, cancellationToken);
    }

    /// <summary>
    /// Read JSON serialised content using <see cref="Newtonsoft.Json"/>.
    /// </summary>
    ///
    /// <remarks>
    /// Prefer to use asynchronous version of this method, however, it can be acceptable
    /// to use this in test code.
    /// </remarks>
    ///
    /// <param name="content">The JSON serialised HTTP content.</param>
    /// <param name="sourceEncoding">The content's character encoding.</param>
    /// <param name="settings">The settings for deserializing the JSON.</param>
    /// <param name="cancellationToken">A token to signal that the read should be cancelled.</param>
    /// <typeparam name="T">The type of the object that the JSON should deserialize to.</typeparam>
    /// <returns>The deserialized object.</returns>
    public static T? ReadFromJson<T>(
        this HttpContent content,
        Encoding sourceEncoding,
        JsonSerializerSettings? settings = null,
        CancellationToken cancellationToken = default)
    {
        using var contentStream = GetContentStream(content, sourceEncoding, cancellationToken);
        using var streamReader = new StreamReader(contentStream);
        using var reader = new JsonTextReader(streamReader);

        return JsonSerializer.Create(settings).Deserialize<T>(reader);
    }

    private static Stream GetContentStream(
        HttpContent content,
        Encoding sourceEncoding,
        CancellationToken cancellationToken)
    {
        var contentStream = content.ReadAsStream(cancellationToken);

        return sourceEncoding.Equals(Encoding.UTF8)
            ? contentStream
            : GetTranscodingStream(contentStream, sourceEncoding);
    }

    private static async Task<Stream> GetContentStreamAsync(
        HttpContent content,
        Encoding sourceEncoding,
        CancellationToken cancellationToken)
    {
        var contentStream = await content.ReadAsStreamAsync(cancellationToken).ConfigureAwait(false);

        return sourceEncoding.Equals(Encoding.UTF8)
            ? contentStream
            : GetTranscodingStream(contentStream, sourceEncoding);
    }

    private static Stream GetTranscodingStream(Stream contentStream, Encoding sourceEncoding) =>
        Encoding.CreateTranscodingStream(contentStream, sourceEncoding, Encoding.UTF8);
}
