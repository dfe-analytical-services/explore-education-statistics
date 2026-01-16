using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace GovUk.Education.ExploreEducationStatistics.Common.IntegrationTests.FunctionApp;

public class OptimisedFunctionAppServiceProviderBuilder
{
    public IServiceProvider Build(
        List<Action<IServiceCollection>> serviceModifications,
        List<Action<IConfigurationBuilder>> configModifications,
        List<Action<IHostBuilder>> hostBuilderModifications
    )
    {
        // TODO - we don't actually need to build an IHost given that we only need an IServiceProvider.
        // We can construct an IServiceCollection, IConfiguration and IHostEnvironment manually if we
        // refactor the way our Function Apps configure these in their HostBuilders so we can extract
        // the logic for building each of these objects more easily.
        var hostBuilder = new HostBuilder();

        foreach (var hostBuilderModification in hostBuilderModifications)
        {
            hostBuilderModification(hostBuilder);
        }

        hostBuilder
            .ConfigureAppConfiguration(
                (_, config) =>
                {
                    config
                        .AddJsonFile("local.settings.json", optional: true, reloadOnChange: false)
                        .AddEnvironmentVariables();
                }
            )
            .ConfigureWebHostDefaults(builder => builder.UseIntegrationTestEnvironment().UseTestServer())
            .ConfigureAppConfiguration(config =>
            {
                foreach (var modification in configModifications)
                {
                    modification(config);
                }
            })
            .ConfigureServices(services =>
            {
                foreach (var modification in serviceModifications)
                {
                    modification(services);
                }
            });

        return hostBuilder.Build().Services;
    }
}
