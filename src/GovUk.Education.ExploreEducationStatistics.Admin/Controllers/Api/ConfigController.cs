#nullable enable
using GovUk.Education.ExploreEducationStatistics.Admin.Options;
using GovUk.Education.ExploreEducationStatistics.Admin.Services;
using GovUk.Education.ExploreEducationStatistics.Admin.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Controllers.Api;

[ApiController]
public class ConfigController(
    IOptions<OpenIdConnectSpaClientOptions> oidcOptions,
    IOptions<AppInsightsOptions> appInsightsOptions,
    IOptions<PublicAppOptions> publicAppOptions)
    : ControllerBase
{
    [AllowAnonymous]
    [HttpGet("api/config")]
    public ActionResult<ConfigViewModel> GetConfig()
    {
        return new ConfigViewModel
        {
            AppInsightsKey = appInsightsOptions.Value.InstrumentationKey,
            PublicAppUrl = publicAppOptions.Value.Url,
            PermittedEmbedUrlDomains = EmbedBlockService.PermittedDomains,
            Oidc = oidcOptions.Value
        };
    }
}
