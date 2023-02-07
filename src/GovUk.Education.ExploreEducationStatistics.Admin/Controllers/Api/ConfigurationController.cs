using System.Collections.Generic;
using GovUk.Education.ExploreEducationStatistics.Admin.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Controllers.Api
{
    [ApiController]
    public class ConfigurationController : ControllerBase
    {
        private readonly IConfiguration _configuration;

        public ConfigurationController(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        [AllowAnonymous]
        [HttpGet("api/config")]
        public ActionResult<Dictionary<string, object>> GetConfig()
        {
            return new ActionResult<Dictionary<string, object>>(BuildConfigurationValues());
        }

        private Dictionary<string, object> BuildConfigurationValues()
        {
            return new Dictionary<string, object>
            {
                {
                    ExposedKeys.AppInsightsKey.ToString(), GetValue(GetRootSection("AppInsights"), "InstrumentationKey")
                },
                {
                    ExposedKeys.PublicAppUrl.ToString(), GetRootValue("PublicAppUrl")
                },
                {
                    ExposedKeys.PermittedEmbedUrlDomains.ToString(), EmbedBlockService.PermittedDomains
                }
            };
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
            return configuration?.GetValue<string>(key);
        }

        private enum ExposedKeys
        {
            AppInsightsKey,
            PublicAppUrl,
            PermittedEmbedUrlDomains
        }
    }
}
