using System.Text.Encodings.Web;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.Primitives;

namespace GovUk.Education.ExploreEducationStatistics.Common.IntegrationTests;

/// <summary>
///
/// An AuthenticationHandler that allows the tests to make a ClaimsPrincipal available in the HttpContext
/// for authentication and authorization mechanisms to use.
///
/// In order to use this handler, the caller sets the HTTP header <see cref="TestUserId"/> to match the ID of a user
/// that is registered in the <see cref="OptimisedTestUserPool"/> that operates for the lifetime of a test class.
///
/// </summary>
public class OptimisedTestAuthHandler(
    IOptionsMonitor<AuthenticationSchemeOptions> options,
    ILoggerFactory logger,
    UrlEncoder encoder,
    IHttpContextAccessor httpContextAccessor,
    OptimisedTestUserPool userPool
) : AuthenticationHandler<AuthenticationSchemeOptions>(options, logger, encoder)
{
    public const string TestUserId = "TestUserId";

    protected override Task<AuthenticateResult> HandleAuthenticateAsync()
    {
        if (!httpContextAccessor.HttpContext.TryGetRequestHeader(TestUserId, out var testUserId))
        {
            return Task.FromResult(AuthenticateResult.NoResult());
        }

        if (!Guid.TryParse(testUserId, out var id))
        {
            throw new ArgumentException($"{testUserId} is not a Guid");
        }

        var user = userPool.GetUser(id);

        if (user == null)
        {
            throw new ArgumentException(
                $"{testUserId} is not a recognised user in the test user pool. "
                    + $"Use fixture.RegisterTestUser() to add non-standard users to authentication."
            );
        }

        var ticket = new AuthenticationTicket(user, "Bearer");
        var result = AuthenticateResult.Success(ticket);
        return Task.FromResult(result);
    }
}

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

    public static bool TryGetHeader(this HttpRequest httpRequest, string headerName, out StringValues headerValues)
    {
        return httpRequest.Headers.TryGetValue(headerName, out headerValues);
    }
}
