#nullable enable
using System.Linq;
using Microsoft.AspNetCore.Http;
using Microsoft.Net.Http.Headers;

namespace GovUk.Education.ExploreEducationStatistics.Common.Extensions;

public static class HttpRequestExtensions
{
    public static bool AcceptsCsv(this HttpRequest request, bool exact = false)
    {
        return Accepts(request, exact, "text/csv");
    }

    public static bool Accepts(this HttpRequest request, bool exact, params string[] mediaTypes)
    {
        return request.Accepts(
            exact: exact,
            mediaTypes: mediaTypes
                .Select(mediaType => MediaTypeHeaderValue.Parse(mediaType))
                .ToArray()
        );
    }

    public static bool Accepts(this HttpRequest request, params string[] mediaTypes)
    {
        return request.Accepts(exact: false, mediaTypes: mediaTypes);
    }

    public static bool Accepts(this HttpRequest request, params MediaTypeHeaderValue[] mediaTypes)
    {
        return request.Accepts(exact: false, mediaTypes: mediaTypes);
    }

    /// <summary>Check that some media types are accepted by the HTTP request.</summary>
    /// <remarks>Wildcard sub-types can also be used e.g. */*, text/*, application/*.</remarks>
    ///
    /// <param name="request">The HTTP request.</param>
    /// <param name="exact">Match media types exactly. Wildcard sub-types will no longer be matched.</param>
    /// <param name="mediaTypes">The media type values to check.</param>
    /// <returns>True if the media types are accepted.</returns>
    public static bool Accepts(this HttpRequest request, bool exact = false, params MediaTypeHeaderValue[] mediaTypes)
    {
        // If not exact, and no Accept header field is present, then
        // it is assumed that the client accepts all media types.
        // See RFC2616: https://www.rfc-editor.org/rfc/rfc2616#section-14.1
        if (!exact && !request.Headers.ContainsKey(HeaderNames.Accept))
        {
            return true;
        }

        var accept = request.GetTypedHeaders().Accept;

        // No media types could be parsed, so none of the
        // expected media types can be matched.
        if (accept.Count == 0)
        {
            return false;
        }

        return accept?.Any(acceptedType =>
            mediaTypes.Any(type => exact ? type.Equals(acceptedType) : type.IsSubsetOf(acceptedType))
        ) ?? false;
    }
}
