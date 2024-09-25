#nullable enable
using GovUk.Education.ExploreEducationStatistics.Admin.Options;
using GovUk.Education.ExploreEducationStatistics.Admin.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Controllers.Api
{
    [ApiController]
    public class ConfigurationController : ControllerBase
    {
        private readonly IConfiguration _configuration;
        private readonly OpenIdConnectSpaClientOptions _oidcOptions;

        public ConfigurationController(
            IConfiguration configuration,
            IOptions<OpenIdConnectSpaClientOptions> oidcOptions)
        {
            _configuration = configuration;
            _oidcOptions = oidcOptions.Value;
        }

        [AllowAnonymous]
        [HttpGet("api/config")]
        public ActionResult<ConfigurationViewModel> GetConfig()
        {
            return new ConfigurationViewModel(
                AppInsightsKey: GetValue(GetRootSection("AppInsights"), "InstrumentationKey"),
                PublicAppUrl: GetRootValue("PublicAppUrl"),
                PermittedEmbedUrlDomains: EmbedBlockService.PermittedDomains,
                Oidc: _oidcOptions);
        }

        private IConfigurationSection GetRootSection(string key)
        {
            return _configuration.GetSection(key);
        }

        private string GetRootValue(string key)
        {
            return _configuration.GetValue<string>(key);
        }

        private static string GetValue(IConfiguration configuration, string key)
        {
            return configuration.GetValue<string>(key);
        }
    }

    // ReSharper disable once ClassNeverInstantiated.Global

    public record ConfigurationViewModel(
        string AppInsightsKey,
        string PublicAppUrl,
        string[] PermittedEmbedUrlDomains,
        OpenIdConnectSpaClientOptions Oidc);
}
