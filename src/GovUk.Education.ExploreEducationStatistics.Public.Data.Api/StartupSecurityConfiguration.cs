using GovUk.Education.ExploreEducationStatistics.Common.Services.Interfaces.Security;
using GovUk.Education.ExploreEducationStatistics.Common.Services.Security;
using GovUk.Education.ExploreEducationStatistics.Public.Data.Api.Security;
using GovUk.Education.ExploreEducationStatistics.Public.Data.Api.Security.AuthorizationHandlers;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;

namespace GovUk.Education.ExploreEducationStatistics.Public.Data.Api;

public static class StartupSecurityConfiguration
{
    public static void AddSecurity(this IServiceCollection services)
    {
        services
            .AddAuthentication()
            .AddScheme<AuthenticationSchemeOptions, AnonymousAuthenticationHandler>("Anonymous", null);

        services
            .AddAuthorizationBuilder()
            .AddPolicy(
                PublicDataSecurityPolicies.CanViewDataSet.ToString(),
                policy => policy.Requirements.Add(new ViewDataSetRequirement())
            )
            .AddPolicy(
                PublicDataSecurityPolicies.CanQueryDataSetVersion.ToString(),
                policy => policy.Requirements.Add(new QueryDataSetVersionRequirement())
            )
            .AddPolicy(
                PublicDataSecurityPolicies.CanViewDataSetVersion.ToString(),
                policy => policy.Requirements.Add(new ViewDataSetVersionRequirement())
            );

        services.AddScoped<IUserService, UserService>();
        services.AddScoped<IAuthorizationHandler, ViewDataSetAuthorizationHandler>();
        services.AddScoped<IAuthorizationHandler, QueryDataSetVersionAuthorizationHandler>();
        services.AddScoped<IAuthorizationHandler, ViewDataSetVersionAuthorizationHandler>();
    }
}
