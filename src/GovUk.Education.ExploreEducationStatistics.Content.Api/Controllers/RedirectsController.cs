#nullable enable
using System.Threading.Tasks;
using GovUk.Education.ExploreEducationStatistics.Common.Extensions;
using GovUk.Education.ExploreEducationStatistics.Content.Services.Interfaces.Cache;
using GovUk.Education.ExploreEducationStatistics.Content.ViewModels;
using Microsoft.AspNetCore.Mvc;

namespace GovUk.Education.ExploreEducationStatistics.Content.Api.Controllers;

[Route("api")]
public class RedirectsController : ControllerBase
{
    private readonly IRedirectsCacheService _redirectsCacheService;

    public RedirectsController(
        IRedirectsCacheService redirectsCacheService)
    {
        _redirectsCacheService = redirectsCacheService;
    }

    [HttpGet("redirects")]
    public async Task<ActionResult<RedirectsViewModel>> List()
    {
        return await _redirectsCacheService.List()
            .HandleFailuresOrOk();
    }
}
