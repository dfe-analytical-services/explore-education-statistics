#nullable enable
using System.Security.Claims;
using System.Security.Claims;
using GovUk.Education.ExploreEducationStatistics.Admin.Security;
using GovUk.Education.ExploreEducationStatistics.Admin.Services.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Common.Extensions;
using GovUk.Education.ExploreEducationStatistics.Common.Model;
using GovUk.Education.ExploreEducationStatistics.Common.Services.Interfaces.Security;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Identity.Web;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Controllers.Api.Security;

[Route("api")]
[ApiController]
public class SignInController(
    ILogger<SignInController> logger,
    IHttpContextAccessor httpContextAccessor,
    ISignInService signInService,
    IUserService userService
) : ControllerBase
{
    [HttpPost("sign-in")]
    [AllowAnonymous]
    public async Task<ActionResult<SignInResponseViewModel>> RegisterOrSignIn()
    {
        // Manually check that the user has the correct "access-admin-api" scope as provided by the Identity Provider
        // prior to accessing this endpoint.  It is not possible to do this via the Authorize attribute as the other
        // default policies still check to see if the user has been allocated a valid role at this point.
        var authorizedByIdP = await userService.MatchesPolicy(SecurityPolicies.AuthenticatedByIdentityProvider);
        if (!authorizedByIdP)
        {
            return new ForbidResult();
        }

        var remoteUserId = httpContextAccessor.HttpContext!.User.FindFirstValue(ClaimConstants.NameIdentifierId);

        return await signInService
            .RegisterOrSignIn()
            .OnFailureDo(_ =>
                logger.LogWarning("User with remote Id \"{UserId}\" " + "failed to sign in or register", remoteUserId)
            )
            .HandleFailuresOr(signInResponse =>
            {
                switch (signInResponse.LoginResult)
                {
                    case LoginResult.NoInvite:
                        logger.LogWarning("User with remote Id \"{UserId}\" had no invite to accept", remoteUserId);
                        break;
                    case LoginResult.ExpiredInvite:
                        logger.LogWarning("User with remote Id \"{UserId}\" had an expired invite", remoteUserId);
                        break;
                }

                return new ActionResult<SignInResponseViewModel>(signInResponse);
            });
    }
}
