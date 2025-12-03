using Microsoft.AspNetCore.Authentication;

namespace GovUk.Education.ExploreEducationStatistics.Common.IntegrationTests.UserAuth;

public static class OptimisedServiceCollectionModificationsPostgresExtensions
{
    public static OptimisedServiceAndConfigModifications AddUserAuth(
        this OptimisedServiceAndConfigModifications serviceModifications
    )
    {
        serviceModifications
            .AddSingleton<OptimisedTestUserPool>()
            .AddAuthentication<OptimisedTestAuthHandler, AuthenticationSchemeOptions>("Bearer");

        return serviceModifications;
    }
}
