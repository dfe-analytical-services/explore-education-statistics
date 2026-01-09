#nullable enable
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Primitives;
using Moq;

namespace GovUk.Education.ExploreEducationStatistics.Common.Tests.Extensions;

public static class HttpContextAccessorMockExtensions
{
    /// <summary>
    /// A Moq setup to expect an HTTP header to be present with an optional value.
    /// </summary>
    public static Mock<IHttpContextAccessor> SetupHasHeader(
        this Mock<IHttpContextAccessor> accessor,
        string headerName,
        string? headerValue = null
    )
    {
        return SetupHeaderCall(accessor, headerName, true, headerValue);
    }

    /// <summary>
    /// A Moq setup to expect an HTTP header not to be present.
    /// </summary>
    public static Mock<IHttpContextAccessor> SetupDoesNotHaveHeader(
        this Mock<IHttpContextAccessor> accessor,
        string headerName
    )
    {
        return SetupHeaderCall(accessor, headerName, false);
    }

    private static Mock<IHttpContextAccessor> SetupHeaderCall(
        Mock<IHttpContextAccessor> accessor,
        string headerName,
        bool hasHeader,
        string? headerValue = null
    )
    {
        accessor
            .Setup(s => s.HttpContext!.Request.Headers.TryGetValue(headerName, out It.Ref<StringValues>.IsAny))
            .Callback(
                (string _, out StringValues value) =>
                {
                    value = hasHeader ? new StringValues(headerValue) : default;
                }
            )
            .Returns(hasHeader);
        return accessor;
    }
}
