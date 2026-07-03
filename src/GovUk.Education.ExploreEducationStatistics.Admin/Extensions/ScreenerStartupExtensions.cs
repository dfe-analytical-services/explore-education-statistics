using GovUk.Education.ExploreEducationStatistics.Admin.Options;
using GovUk.Education.ExploreEducationStatistics.Admin.Services.Interfaces.Screener;
using GovUk.Education.ExploreEducationStatistics.Admin.Services.Screener;
using GovUk.Education.ExploreEducationStatistics.Common.Services;
using GovUk.Education.ExploreEducationStatistics.Common.Services.Interfaces;
using Microsoft.Extensions.Options;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Extensions;

public static class ScreenerStartupExtensions
{
    public static IServiceCollection AddScreener(this IServiceCollection services, IConfiguration configuration)
    {
        services
            .Configure<DataScreenerOptions>(configuration.GetRequiredSection(DataScreenerOptions.Section))
            .AddScoped<IDataSetScreenerClient, DataSetScreenerClient>()
            .AddScoped<IDataSetScreenerService, DataSetScreenerService>()
            .AddKeyedSingleton<IQueueServiceClient>(
                serviceKey: nameof(DataSetScreenerService),
                implementationFactory: (serviceProvider, _) =>
                {
                    var screenerOptions = serviceProvider.GetRequiredService<IOptions<DataScreenerOptions>>();
                    return new QueueServiceClient(screenerOptions.Value.ScreenerStorage);
                }
            )
            .AddHostedService<DataSetScreenerProgressUpdaterJob>();

        services.AddHttpClient<IDataSetScreenerClient, DataSetScreenerClient>(
            (provider, httpClient) =>
            {
                var options = provider.GetRequiredService<IOptions<DataScreenerOptions>>();
                httpClient.BaseAddress = new Uri(options.Value.Url);
            }
        );

        return services;
    }
}
