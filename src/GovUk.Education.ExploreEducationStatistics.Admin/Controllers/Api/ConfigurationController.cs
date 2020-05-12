using System.Collections.Generic;
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
        [HttpGet("api/configuration/application-insights")]
        public ActionResult<string> GetInsightsKey()
        {
            var value = _configuration.GetSection("AppInsights")?.GetValue<string>("InstrumentationKey");
            if (string.IsNullOrEmpty(value))
            {
                return Ok("");
            }

            return Ok(value);
        }

        [HttpGet("api/config")]
        public ActionResult<Dictionary<string, string>> GetConfig()
        {
            return Ok(BuildConfigurationValues());
        }

        private Dictionary<string, string> BuildConfigurationValues()
        {
            return new Dictionary<string, string>
            {
                {
                    ExposedKeys.AppInsightsKey.ToString(), GetValue(GetRootSection("AppInsights"), "InstrumentationKey")
                },
                {
                    ExposedKeys.PublicAppUrl.ToString(), GetRootValue("PublicAppUrl")
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
            PublicAppUrl
        }
    }
}