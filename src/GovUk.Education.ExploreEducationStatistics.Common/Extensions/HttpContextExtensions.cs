#nullable enable
using System.Diagnostics.CodeAnalysis;
using Microsoft.AspNetCore.Http;

namespace GovUk.Education.ExploreEducationStatistics.Common.Extensions;

public static class HttpContextExtensions
{
    public static bool TryGetRequestHeader(
        this HttpContext? httpContext,
        string headerName,
        [MaybeNullWhen(false)] out string headerValue)
    {
        headerValue = null;

        if (httpContext == null)
        {
            return false;
        }

        if (!httpContext.Request.TryGetHeader(headerName, out var value))
        {
            return false;
        }

        headerValue = value;
        return true;
    }
}
