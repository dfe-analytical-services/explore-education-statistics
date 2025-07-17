using GovUk.Education.ExploreEducationStatistics.Events.EventGrid;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace GovUk.Education.ExploreEducationStatistics.Events.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddEventGridClient(this IServiceCollection services, IConfiguration configuration) =>
        services
            .AddTransient<IEventGridClientFactory, EventGridClientFactory>()
            .AddTransient<IEventRaiser, EventRaiser>()
            .AddTransient<IConfiguredEventGridClientFactory, ConfiguredEventGridClientFactory>()
            .AddTransient(typeof(Func<ILogger<SafeEventGridClient>>), sp => (Func<ILogger<SafeEventGridClient>>)sp.GetRequiredService<ILogger<SafeEventGridClient>>)
            .Configure<EventGridOptions>(configuration.GetSection(EventGridOptions.Section))
    ;
}
