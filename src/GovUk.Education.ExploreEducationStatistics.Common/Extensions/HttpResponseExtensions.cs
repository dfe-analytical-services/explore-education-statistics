#nullable enable
using Microsoft.AspNetCore.Http;
using Microsoft.Net.Http.Headers;

namespace GovUk.Education.ExploreEducationStatistics.Common.Extensions;

public static class HttpResponseExtensions
{
    public static HttpResponse ContentDispositionInline(this HttpResponse response)
    {
        response.Headers[HeaderNames.ContentDisposition] = "inline";
        return response;
    }

    public static HttpResponse ContentDispositionAttachment(
        this HttpResponse response,
        MediaTypeHeaderValue contentType,
        string? filename = null)
    {
        response.ContentDispositionAttachment(contentType: contentType.ToString(), filename: filename);
        return response;
    }

    public static HttpResponse ContentDispositionAttachment(
        this HttpResponse response,
        string contentType,
        string? filename = null)
    {
        response.ContentType = contentType;
        response.Headers[HeaderNames.ContentDisposition] = filename is not null
            ? @$"attachment; filename=""{filename}"""
            : "attachment";

        return response;
    }
}
