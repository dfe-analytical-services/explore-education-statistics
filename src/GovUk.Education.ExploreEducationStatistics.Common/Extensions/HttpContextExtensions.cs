#nullable enable
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Primitives;

namespace GovUk.Education.ExploreEducationStatistics.Common.Extensions;

public static class HttpContextExtensions
{
    public static bool TryGetRequestHeader(
        this HttpContext? httpContext,
        string headerName,
        out StringValues headerValues
    )
    {
        if (httpContext == null)
        {
            headerValues = StringValues.Empty;
            return false;
        }

        return httpContext.Request.TryGetHeader(headerName, out headerValues);
    }
}
