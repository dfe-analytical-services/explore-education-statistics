#nullable enable
using GovUk.Education.ExploreEducationStatistics.Common.Services.Interfaces.Security;
using GovUk.Education.ExploreEducationStatistics.Common.Services.Security;
using GovUk.Education.ExploreEducationStatistics.Content.Security;
using GovUk.Education.ExploreEducationStatistics.Content.Security.AuthorizationHandlers;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.DependencyInjection;

namespace GovUk.Education.ExploreEducationStatistics.Content.Api;

public static class StartupSecurityConfiguration
{
    /// <summary>
    /// Configure security policies
    /// </summary>
    /// <param name="services"></param>
    public static void ConfigureAuthorizationPolicies(IServiceCollection services)
    {
        services.AddAuthorization(options =>
        {
            // does this use have permission to view a specific MethodologyVersion?
            options.AddPolicy(ContentSecurityPolicies.CanViewSpecificMethodologyVersion.ToString(), policy =>
                policy.Requirements.Add(new ViewMethodologyVersionRequirement()));

            // does this use have permission to view a specific Publication?
            options.AddPolicy(ContentSecurityPolicies.CanViewSpecificPublication.ToString(), policy =>
                policy.Requirements.Add(new ViewPublicationRequirement()));

            // does this use have permission to view a specific Release?
            options.AddPolicy(ContentSecurityPolicies.CanViewSpecificRelease.ToString(), policy =>
                policy.Requirements.Add(new ViewReleaseRequirement()));
        });
    }

    /// <summary>
    /// Set up our Resource-based Authorization Handlers and supporting services in DI
    /// </summary>
    /// <param name="services"></param>
    public static void ConfigureResourceBasedAuthorization(IServiceCollection services)
    {
        services.AddTransient<IUserService, UserService>();

        services.AddTransient<IAuthorizationHandler, ViewMethodologyVersionAuthorizationHandler>();
        services.AddTransient<IAuthorizationHandler, ViewPublicationAuthorizationHandler>();
        services.AddTransient<IAuthorizationHandler, ViewReleaseAuthorizationHandler>();
    }
}
