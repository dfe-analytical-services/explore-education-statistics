using GovUk.Education.ExploreEducationStatistics.Common.IntegrationTests.WebApp;

namespace GovUk.Education.ExploreEducationStatistics.Common.IntegrationTests.Azurite;

public static class OptimisedServiceCollectionModificationsAzuriteExtensions
{
    public static OptimisedServiceAndConfigModifications AddAzurite(
        this OptimisedServiceAndConfigModifications serviceModifications,
        string connectionString,
        string[] connectionStringKeys
    )
    {
        var connectionStringSettings = connectionStringKeys.Select(key => new KeyValuePair<string, string?>(
            key,
            connectionString
        ));

        serviceModifications.AddInMemoryCollection(connectionStringSettings);

        return serviceModifications;
    }
}
