using System.Text.Encodings.Web;
using GovUk.Education.ExploreEducationStatistics.Common.Extensions;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Tests.Fixture.Optimised;

/// <summary>
///
/// An AuthenticationHandler that allows the tests to make a ClaimsPrincipal available in the HttpContext
/// for authentication and authorization mechanisms to use.
///
/// In order to use this handler, the caller sets the HTTP header <see cref="TestUserId"/> to match the ID of a user
/// that is registered in the <see cref="OptimisedTestUserPool"/> that operates for the lifetime of a test class.
///
/// For convenience, a number of commonly used user types are automatically added to the test pool. These are available
/// in <see cref="OptimisedTestUsers"/> and can be used at any time without the need to register them first.
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

        var ticket = new AuthenticationTicket(user, JwtBearerDefaults.AuthenticationScheme);
        var result = AuthenticateResult.Success(ticket);
        return Task.FromResult(result);
    }
}
