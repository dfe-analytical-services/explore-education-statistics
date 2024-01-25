#nullable enable
using System;
using System.Net.Http;
using System.Text.Json;
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

    public static T AssertOk<T>(this HttpResponseMessage message, bool useSystemJson = false)
    {
        Assert.Equal(OK, message.StatusCode);

        var body = Deserialize<T>(message, useSystemJson);

        return Assert.IsType<T>(body);
    }

    private static T? Deserialize<T>(this HttpResponseMessage message, bool useSystemJson)
    {
        return useSystemJson
            ? JsonSerializer.Deserialize<T>(
                message.Content.ReadAsStream().ReadToEnd(), 
                new JsonSerializerOptions { PropertyNameCaseInsensitive = true })
            : message.Content.ReadFromJson<T>();
    }

    public static string AssertOk(this HttpResponseMessage message, string expectedBody)
    {
        Assert.Equal(OK, message.StatusCode);
        return message.AssertBodyEqualTo(expectedBody);
    }

    public static T AssertOk<T>(this HttpResponseMessage message, T expectedBody, bool useSystemJson = false)
    {
        Assert.Equal(OK, message.StatusCode);
        return message.AssertBodyEqualTo(expectedBody, useSystemJson);
    }

    public static T AssertCreated<T>(this HttpResponseMessage message, T expectedBody, string expectedLocation, bool useSystemJson = false)
    {
        Assert.Equal(Created, message.StatusCode);
        Assert.Equal(new Uri(expectedLocation), message.Headers.Location);
        return message.AssertBodyEqualTo(expectedBody, useSystemJson);
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

    public static T AssertNotFound<T>(this HttpResponseMessage message, T expectedBody, bool useSystemJson = false)
    {
        Assert.Equal(NotFound, message.StatusCode);
        return message.AssertBodyEqualTo(expectedBody, useSystemJson);
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

    public static T AssertBodyEqualTo<T>(this HttpResponseMessage message, T expected, bool useSystemJson = false)
    {
        var body = Deserialize<T>(message, useSystemJson: useSystemJson);

        body = Assert.IsType<T>(body);
        body.AssertDeepEqualTo(expected);

        return body;
    }

    public static void AssertPathAndQueryEqualTo(this HttpResponseMessage response, string expected)
    {
        Assert.Equal(expected, response.RequestMessage?.RequestUri?.PathAndQuery);
    }
}
