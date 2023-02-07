using System.IO;
using GovUk.Education.ExploreEducationStatistics.Admin.Controllers.Api;
using GovUk.Education.ExploreEducationStatistics.Common.Tests.Extensions;
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
            Assert.Equal(string.Empty, configValue);
        }

        [Fact]
        public void CheckPublicAppUrlConfigurationKeyExists()
        {
            var configuration = GetConfiguration();
            var configValue = configuration.GetValue<string>("PublicAppUrl");
            Assert.Equal("http://localhost:3000", configValue);
        }

        [Fact]
        public void GetConfig()
        {
            var controller = new ConfigurationController(GetConfiguration());
            var result = controller.GetConfig();
            var unboxed = result.AssertOkResult();

            Assert.Equal(string.Empty, unboxed["AppInsightsKey"]);
            Assert.Equal("http://localhost:3000", unboxed["PublicAppUrl"]);

            var permittedEmbedUrlDomains = unboxed["PermittedEmbedUrlDomains"];
            var expectedDomains = new []{ "https://department-for-education.shinyapps.io", "https://dfe-analytical-services.github.io" };
            Assert.IsType<string[]>(permittedEmbedUrlDomains);
            Assert.Equal(expectedDomains, permittedEmbedUrlDomains);
        }

        private static IConfiguration GetConfiguration()
        {
            return new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.Development.json", optional: false)
                .AddEnvironmentVariables()
                .Build();
        }
    }
}
