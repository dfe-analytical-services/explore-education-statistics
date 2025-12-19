using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace GovUk.Education.ExploreEducationStatistics.Common.IntegrationTests.FunctionApp;

public class OptimisedFunctionAppHostBuilder
{
    public IHost Build(
        List<Action<IServiceCollection>> serviceModifications,
        List<Action<IConfigurationBuilder>> configModifications,
        List<Action<IHostBuilder>> hostBuilderModifications
    )
    {
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
            .ConfigureWebHostDefaults(builder => builder.UseIntegrationTestEnvironment())
            .ConfigureAppConfiguration(config =>
            {
                foreach (var modification in configModifications)
                {
                    modification(config);
                }
            })
            // .ConfigureServices(services => GetFunctionTypes().ForEach(functionType => services.AddScoped(functionType)))
            .ConfigureServices(services =>
            {
                foreach (var modification in serviceModifications)
                {
                    modification(services);
                }
            });

        return hostBuilder.Build();
    }
}
