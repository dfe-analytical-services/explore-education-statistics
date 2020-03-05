using System.IO;
using GovUk.Education.ExploreEducationStatistics.Admin.Controllers.Api;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Xunit;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Tests.Controllers.Api
{
    public class ConfigurationControllerTests
    {
        [Fact]
        public void CheckApplicationInsightsConfigurationKeyExists()
        {
            var configuration = GetConfiguration();
            var configSection = configuration.GetSection("AppInsights");
            Assert.NotNull(configSection);
            var configValue = configSection.GetValue<string>("InstrumentationKey");
            Assert.Equal("change-me", configValue);
        }

        [Fact]
        public void CheckPublicUrlConfigurationKeyExists()
        {
            var configuration = GetConfiguration();
            var configValue = configuration.GetValue<string>("PublicUrl");
            Assert.Equal("http://localhost:3000", configValue);
        }

        [Fact]
        public void GetInsightsKey()
        {
            var controller = new ConfigurationController(GetConfiguration());
            var result = controller.GetInsightsKey();
            Assert.IsAssignableFrom<OkObjectResult>(result.Result);
            Assert.Equal("change-me", ((OkObjectResult) result.Result).Value);
        }

        [Fact]
        public void GetInsightsKey_NotFound()
        {
            var controller = new ConfigurationController(GetEmptyConfiguration());
            var result = controller.GetInsightsKey();
            Assert.IsAssignableFrom<NotFoundResult>(result.Result);
        }

        [Fact]
        public void GetPublicUrl()
        {
            var controller = new ConfigurationController(GetConfiguration());
            var result = controller.GetPublicUrl();
            Assert.IsAssignableFrom<OkObjectResult>(result.Result);
            Assert.Equal("http://localhost:3000", ((OkObjectResult) result.Result).Value);
        }

        [Fact]
        public void GetPublicUrl_NotFound()
        {
            var controller = new ConfigurationController(GetEmptyConfiguration());
            var result = controller.GetPublicUrl();
            Assert.IsAssignableFrom<NotFoundResult>(result.Result);
        }

        private static IConfiguration GetConfiguration()
        {
            return new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.Development.json", optional: false)
                .AddEnvironmentVariables()
                .Build();
        }

        private static IConfiguration GetEmptyConfiguration()
        {
            return new ConfigurationBuilder().Build();
        }
    }
}