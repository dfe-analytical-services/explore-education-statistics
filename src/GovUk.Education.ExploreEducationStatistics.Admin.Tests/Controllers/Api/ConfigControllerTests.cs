using System.IO;
using GovUk.Education.ExploreEducationStatistics.Admin.Controllers.Api;
using GovUk.Education.ExploreEducationStatistics.Admin.Options;
using GovUk.Education.ExploreEducationStatistics.Admin.Security;
using GovUk.Education.ExploreEducationStatistics.Common.Tests.Extensions;
using Microsoft.Extensions.Configuration;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Tests.Controllers.Api
{
    public class ConfigControllerTests
    {
        [Fact]
        public void CheckApplicationInsightsConfigurationKeyExists()
        {
            var configuration = GetConfiguration();
            var configSection = configuration.GetSection("AppInsights");

            Assert.NotNull(configSection);
            Assert.Equal(string.Empty, configSection.GetValue<string>("InstrumentationKey"));
        }

        [Fact]
        public void CheckPublicAppUrlConfigurationKeyExists()
        {
            var configuration = GetConfiguration();
            var configSection = configuration.GetSection("PublicApp");

            Assert.NotNull(configSection);
            Assert.Equal("http://localhost:3000", configSection.GetValue<string>("Url"));
        }

        [Fact]
        public void GetConfig()
        {
            var mainConfiguration = GetConfiguration();
            var openIdConnectSpaClientOptions = new OpenIdConnectSpaClientOptions();
            mainConfiguration.Bind(OpenIdConnectSpaClientOptions.Section, openIdConnectSpaClientOptions);

            var publicAppOptions = new PublicAppOptions();
            mainConfiguration.Bind(PublicAppOptions.Section, publicAppOptions);

            var appInsightsOptions = new AppInsightsOptions();
            mainConfiguration.Bind(AppInsightsOptions.Section, appInsightsOptions);

            var controller = new ConfigController(
                openIdConnectSpaClientOptions.ToOptionsWrapper(),
                appInsightsOptions.ToOptionsWrapper(),
                publicAppOptions.ToOptionsWrapper()
            );

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
            Assert.Equal(SecurityScopes.AccessAdminApiScope, viewModel.Oidc.AdminApiScope);

            var authorityMetadata = viewModel.Oidc.AuthorityMetadata!;
            Assert.Equal($"{realm}/protocol/openid-connect/auth", authorityMetadata.AuthorizationEndpoint);
            Assert.Equal($"{realm}/protocol/openid-connect/token", authorityMetadata.TokenEndpoint);
            Assert.Equal(realm, authorityMetadata.Issuer);
            Assert.Equal($"{realm}/protocol/openid-connect/userinfo", authorityMetadata.UserInfoEndpoint);
            Assert.Equal($"{realm}/protocol/openid-connect/logout", authorityMetadata.EndSessionEndpoint);
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
