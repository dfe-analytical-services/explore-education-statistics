using IdentityServer4.Extensions;
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

        // GET api/configuration/application-insights/
        [HttpGet("api/configuration/application-insights")]
        public ActionResult<string> GetInsightsKey()
        {
            var instrumentationKey = _configuration.GetSection("ApplicationInsights").GetValue<string>("InstrumentationKey");
            if (instrumentationKey.IsNullOrEmpty())
            {
                return NotFound();
            }
            return Ok(instrumentationKey);
        }
    }
} 