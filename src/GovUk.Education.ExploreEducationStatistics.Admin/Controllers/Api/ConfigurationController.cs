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

        [HttpGet("api/configuration/public-app-url")]
        public ActionResult<string> GetPublicAppUrl()
        {
            var value = _configuration.GetValue<string>("PublicAppUrl");
            if (string.IsNullOrEmpty(value))
            {
                return NotFound();
            }

            return Ok(value);
        }
    }
}
