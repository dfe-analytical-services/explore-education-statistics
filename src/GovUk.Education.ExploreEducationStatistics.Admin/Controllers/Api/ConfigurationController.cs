#nullable enable
using GovUk.Education.ExploreEducationStatistics.Admin.Options;
using GovUk.Education.ExploreEducationStatistics.Admin.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Controllers.Api
{
    [ApiController]
    public class ConfigurationController(
        IOptions<OpenIdConnectSpaClientOptions> oidcOptions,
        IOptions<AppInsightsOptions> appInsightsOptions,
        IOptions<PublicAppOptions> publicAppOptions)
        : ControllerBase
    {
        [AllowAnonymous]
        [HttpGet("api/config")]
        public ActionResult<ConfigurationViewModel> GetConfig()
        {
            return new ConfigurationViewModel(
                AppInsightsKey: appInsightsOptions.Value.InstrumentationKey,
                PublicAppUrl: publicAppOptions.Value.Url,
                PermittedEmbedUrlDomains: EmbedBlockService.PermittedDomains,
                Oidc: oidcOptions.Value);
        }
    }

    // ReSharper disable once ClassNeverInstantiated.Global

    public record ConfigurationViewModel(
        string AppInsightsKey,
        string PublicAppUrl,
        string[] PermittedEmbedUrlDomains,
        OpenIdConnectSpaClientOptions Oidc);
}
