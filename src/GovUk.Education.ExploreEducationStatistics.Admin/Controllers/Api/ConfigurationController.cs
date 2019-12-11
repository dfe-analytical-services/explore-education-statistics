using System;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Controllers.Api
{
    [ApiController]
    public class ConfigurationController
    {
        private readonly IConfiguration _configuration;
        public ConfigurationController(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        // GET api/configuration/application-insights/
        [HttpGet("api/configuration/application-insights")]
        public string GetInsightsKey()
        {
            var instrumentationKey = _configuration.GetSection("ApplicationInsights").GetValue("InstrumentationKey", "Instrumentation key not found");
            return instrumentationKey;
        }
    }
} 