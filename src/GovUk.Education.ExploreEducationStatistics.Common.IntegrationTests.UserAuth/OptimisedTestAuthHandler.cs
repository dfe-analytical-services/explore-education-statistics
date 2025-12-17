using System.Text.Encodings.Web;
using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace GovUk.Education.ExploreEducationStatistics.Common.IntegrationTests.UserAuth;

/// <summary>
///
/// An AuthenticationHandler that allows the tests to make a ClaimsPrincipal available in the HttpContext
/// for authentication and authorization mechanisms to use.
///
/// In order to use this handler, the caller sets the desired user to the <see cref="OptimisedTestUserHolder"/> using
/// the <see cref="OptimisedTestUserHolder.SetUser"/> method.
///
/// </summary>
// ReSharper disable once ClassNeverInstantiated.Global
public class OptimisedTestAuthHandler(
    IOptionsMonitor<AuthenticationSchemeOptions> options,
    ILoggerFactory logger,
    UrlEncoder encoder,
    OptimisedTestUserHolder userHolder
) : AuthenticationHandler<AuthenticationSchemeOptions>(options, logger, encoder)
{
    private readonly ILoggerFactory _logger = logger;

    protected override Task<AuthenticateResult> HandleAuthenticateAsync()
    {
        var user = userHolder.GetUser();

        if (user == null)
        {
            _logger.CreateLogger(GetType()).LogWarning("No test user has been set to handle this HTTP request.");
            return Task.FromResult(AuthenticateResult.NoResult());
        }

        var ticket = new AuthenticationTicket(user, "Bearer");
        var result = AuthenticateResult.Success(ticket);
        return Task.FromResult(result);
    }
}
