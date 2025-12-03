using Microsoft.Extensions.Configuration;

namespace GovUk.Education.ExploreEducationStatistics.Common.IntegrationTests.Azurite;

public static class OptimisedWebApplicationFactoryBuilderPostgresExtensions
{
    public static OptimisedWebApplicationFactoryBuilder<TStartup> WithAzurite<TStartup>(
        this OptimisedWebApplicationFactoryBuilder<TStartup> builder,
        string connectionString,
        string[] connectionStringKeys
    )
        where TStartup : class
    {
        builder.AddConfigRegistration(config =>
            RegisterAzuriteAppConfiguration(config, connectionString, connectionStringKeys)
        );
        return builder;
    }

    private static void RegisterAzuriteAppConfiguration(
        IConfigurationBuilder config,
        string connectionString,
        string[] connectionStringKeys
    )
    {
        var connectionStringSettings = connectionStringKeys.Select(key => new KeyValuePair<string, string?>(
            key,
            connectionString
        ));

        config.AddInMemoryCollection(connectionStringSettings);
    }
}
