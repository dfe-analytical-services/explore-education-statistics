#nullable enable
using System.Text.Json;
using GovUk.Education.ExploreEducationStatistics.Common.Extensions;
using GovUk.Education.ExploreEducationStatistics.Common.ViewModels;
using Microsoft.AspNetCore.Mvc;
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

    public static (T body, string createdEntityId) AssertCreated<T>(
        this HttpResponseMessage message,
        string expectedLocationPrefix,
        bool useSystemJson = false
    )
    {
        Assert.Equal(Created, message.StatusCode);
        Assert.NotNull(message.Headers.Location);

        var locationHeader = message.Headers.Location.ToString();
        Assert.StartsWith(expectedLocationPrefix, locationHeader);
        var createdEntityId = locationHeader[expectedLocationPrefix.Length..];

        var body = Deserialize<T>(message, useSystemJson);
        var bodyAsType = Assert.IsType<T>(body);

        return (bodyAsType, createdEntityId);
    }

    public static T AssertCreated<T>(
        this HttpResponseMessage message,
        T expectedBody,
        string expectedLocation,
        bool useSystemJson = false
    )
    {
        Assert.Equal(Created, message.StatusCode);
        Assert.Equal(new Uri(expectedLocation), message.Headers.Location);
        return message.AssertBodyEqualTo(expectedBody, useSystemJson);
    }

    public static void AssertHasHeader(this HttpResponseMessage message, string headerKey, string expectedHeaderValue)
    {
        message.Headers.TryGetValues(headerKey, out var headerValue);
        Assert.NotNull(headerValue);
        Assert.Equal(headerValue.FirstOrDefault(), expectedHeaderValue);
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

    public static void AssertGatewayTimeout(this HttpResponseMessage message)
    {
        Assert.Equal(GatewayTimeout, message.StatusCode);
    }

    public static void AssertNotModified(this HttpResponseMessage message)
    {
        Assert.Equal(NotModified, message.StatusCode);
    }

    public static ProblemDetails AssertBadRequest(this HttpResponseMessage message)
    {
        Assert.Equal(BadRequest, message.StatusCode);

        return message.AssertBodyIsProblemDetails();
    }

    public static ValidationProblemViewModel AssertValidationProblem(this HttpResponseMessage message)
    {
        Assert.Equal(BadRequest, message.StatusCode);

        return message.AssertBodyIsValidationProblem();
    }

    public static ProblemDetails AssertInternalServerError(this HttpResponseMessage message)
    {
        Assert.Equal(InternalServerError, message.StatusCode);

        return message.AssertBodyIsProblemDetails();
    }

    public static ProblemDetails AssertBodyIsProblemDetails(this HttpResponseMessage message)
    {
        var details = Deserialize<ProblemDetails>(message, useSystemJson: true);

        Assert.NotNull(details);

        return details;
    }

    public static ValidationProblemViewModel AssertBodyIsValidationProblem(this HttpResponseMessage message)
    {
        var details = Deserialize<ValidationProblemViewModel>(message, useSystemJson: true);

        Assert.NotNull(details);

        return details;
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

    private static T? Deserialize<T>(this HttpResponseMessage message, bool useSystemJson)
    {
        return useSystemJson
            ? JsonSerializer.Deserialize<T>(
                message.Content.ReadAsStream().ReadToEnd(),
                new JsonSerializerOptions { PropertyNameCaseInsensitive = true }
            )
            : message.Content.ReadFromJson<T>();
    }
}
