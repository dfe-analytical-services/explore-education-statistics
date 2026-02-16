using Microsoft.AspNetCore.Authentication;

namespace GovUk.Education.ExploreEducationStatistics.Common.IntegrationTests.UserAuth;

public static class OptimisedServiceCollectionModificationsUserAuthExtensions
{
    public static OptimisedServiceAndConfigModifications AddUserAuth(
        this OptimisedServiceAndConfigModifications serviceModifications
    )
    {
        serviceModifications
            .AddSingleton<OptimisedTestUserHolder>()
            .AddAuthentication<OptimisedTestAuthHandler, AuthenticationSchemeOptions>("Bearer");

        return serviceModifications;
    }
}
