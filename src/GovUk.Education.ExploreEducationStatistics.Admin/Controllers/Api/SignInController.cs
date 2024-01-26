#nullable enable
using System.Security.Claims;
using System.Threading.Tasks;
using GovUk.Education.ExploreEducationStatistics.Admin.Services.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Common.Extensions;
using GovUk.Education.ExploreEducationStatistics.Common.Model;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Microsoft.Identity.Web;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Controllers.Api;

[Route("api")]
[ApiController]
[Authorize]
public class SignInController : ControllerBase
{
    private readonly ILogger<SignInController> _logger;
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly ISignInService _signInService;

    public record SignInResponse([property: JsonConverter(typeof(StringEnumConverter))] LoginResult result);

    public SignInController(
        ILogger<SignInController> logger,
        IHttpContextAccessor httpContextAccessor,
        ISignInService signInService)
    {
        _logger = logger;
        _httpContextAccessor = httpContextAccessor;
        _signInService = signInService;
    }

    [HttpPost("sign-in")]
    public async Task<ActionResult<SignInResponse>> RegisterOrSignIn()
    {
        var remoteUserId = _httpContextAccessor
            .HttpContext!
            .User
            .FindFirstValue(ClaimConstants.NameIdentifierId);

        return await _signInService
            .RegisterOrSignIn()
            .OnFailureDo(_ =>
                _logger.LogWarning("User with remote Id \"{UserId}\" " +
                                   "failed to sign in or register", remoteUserId))
            .HandleFailuresOr(loginResult =>
            {
                switch (loginResult)
                {
                    case LoginResult.NoInvite:
                        _logger.LogWarning("User with remote Id \"{UserId}\" had no invite to accept", remoteUserId);
                        break;
                    case LoginResult.ExpiredInvite:
                        _logger.LogWarning("User with remote Id \"{UserId}\" had an expired invite", remoteUserId);
                        break;
                }

                return new OkObjectResult(new SignInResponse(loginResult));
            });
    }
}
