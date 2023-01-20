#nullable enable
using System.Net.Http;
using GovUk.Education.ExploreEducationStatistics.Common.Extensions;
using Xunit;
using static System.Net.HttpStatusCode;

namespace GovUk.Education.ExploreEducationStatistics.Common.Tests.Extensions;

public static class HttpResponseMessageTestExtensions
{
    public static void AssertOk(this HttpResponseMessage message)
    {
        Assert.Equal(OK, message.StatusCode);
    }

    public static T AssertOk<T>(this HttpResponseMessage message)
    {
        Assert.Equal(OK, message.StatusCode);

        var body = message.Content.ReadFromJson<T>();

        return Assert.IsType<T>(body);
    }

    public static string AssertOk(this HttpResponseMessage message, string expectedBody)
    {
        Assert.Equal(OK, message.StatusCode);
        return message.AssertBodyEqualTo(expectedBody);
    }

    public static T AssertOk<T>(this HttpResponseMessage message, T expectedBody)
    {
        Assert.Equal(OK, message.StatusCode);
        return message.AssertBodyEqualTo(expectedBody);
    }

    public static void AssertNoContent(this HttpResponseMessage message)
    {
        Assert.Equal(NoContent, message.StatusCode);
        Assert.Empty(message.Content.ReadAsStream().ReadToEnd());
    }

    public static void AssertNotFound(this HttpResponseMessage message)
    {
        Assert.Equal(NotFound, message.StatusCode);
    }

    public static void AssertForbidden(this HttpResponseMessage message)
    {
        Assert.Equal(Forbidden, message.StatusCode);
    }

    public static void AssertUnauthorized(this HttpResponseMessage message)
    {
        Assert.Equal(Unauthorized, message.StatusCode);
    }

    public static void AssertNotModified(this HttpResponseMessage message)
    {
        Assert.Equal(NotModified, message.StatusCode);
    }

    public static string AssertBodyEqualTo(this HttpResponseMessage message, string expected)
    {
        var actual = message.Content.ReadAsStream().ReadToEnd();

        Assert.Equal(expected, actual);

        return actual;
    }

    public static T AssertBodyEqualTo<T>(this HttpResponseMessage message, T expected)
    {
        var body = message.Content.ReadFromJson<T>();

        body = Assert.IsType<T>(body);
        body.AssertDeepEqualTo(expected);

        return body;
    }
}
