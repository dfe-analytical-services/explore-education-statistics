using System.IO;
using GovUk.Education.ExploreEducationStatistics.Admin.Controllers.Api;
using GovUk.Education.ExploreEducationStatistics.Common.Tests.Extensions;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;
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
            var mainConfiguration = GetConfiguration();
            var openIdConnectSpaClientConfig = new OpenIdConnectSpaClientOptions();
            mainConfiguration.Bind("OpenIdConnectSpaClient", openIdConnectSpaClientConfig);
            var openIdConnectSpaClientOptions = Options.Create(openIdConnectSpaClientConfig);

            var controller = new ConfigurationController(GetConfiguration(), openIdConnectSpaClientOptions);
            var result = controller.GetConfig();
            var viewModel = result.AssertOkResult();

            Assert.Equal(string.Empty, viewModel.AppInsightsKey);
            Assert.Equal("http://localhost:3000", viewModel.PublicAppUrl);
            Assert.Equal(
                new []
                {
                    "https://department-for-education.shinyapps.io",
                    "https://dfe-analytical-services.github.io"
                },
                viewModel.PermittedEmbedUrlDomains);

            const string realm = "https://ees.local:5031/auth/realms/ees-realm";
            Assert.Equal("ees-admin-client", viewModel.Oidc.ClientId);
            Assert.Equal(realm, viewModel.Oidc.Authority);
            Assert.Equal(new [] { realm, "ees.local:5031" }, viewModel.Oidc.KnownAuthorities);
            Assert.Equal("access-admin-api", viewModel.Oidc.AdminApiScope);

            var authorityMetadata = viewModel.Oidc.AuthorityMetadata!;
            Assert.Equal($"{realm}/protocol/openid-connect/auth", authorityMetadata.AuthorizationEndpoint);
            Assert.Equal($"{realm}/protocol/openid-connect/token", authorityMetadata.TokenEndpoint);
            Assert.Equal(realm, authorityMetadata.Issuer);
            Assert.Equal($"{realm}/protocol/openid-connect/userinfo", authorityMetadata.UserInfoEndpoint);
        }

        private static IConfiguration GetConfiguration()
        {
            return new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.Development.json", optional: false)
                .AddJsonFile("appsettings.Keycloak.json", optional: false)
                .AddEnvironmentVariables()
                .Build();
        }
    }
}
