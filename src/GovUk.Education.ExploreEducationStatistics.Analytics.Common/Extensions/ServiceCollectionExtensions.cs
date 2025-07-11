using GovUk.Education.ExploreEducationStatistics.Analytics.Common.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Analytics.Common.Strategies;
using GovUk.Education.ExploreEducationStatistics.Common.Services;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace GovUk.Education.ExploreEducationStatistics.Analytics.Common.Extensions;

public static class ServiceCollectionExtensions
{
    public static AnalyticsWritersRegistrar AddAnalyticsCommon(this IServiceCollection services, bool isAnalyticsEnabled) =>
        new(
            isAnalyticsEnabled
            ? services
                .AddSingleton<IAnalyticsManager, AnalyticsManager>()
                .AddSingleton<IAnalyticsWriter, AnalyticsWriter>()
                .AddHostedService<AnalyticsConsumer>()
                .AddTransient(
                    typeof(ICommonAnalyticsWriteStrategyWorkflow<>),
                    typeof(CommonAnalyticsWriteStrategyWorkflow<>))
                .TryAddSingletonInline<DateTimeProvider>()
            : services
                .AddSingleton<IAnalyticsManager, NoOpAnalyticsManager>(),
            isAnalyticsEnabled
        );

    public class AnalyticsWritersRegistrar(IServiceCollection services, bool isAnalyticsEnabled)
    {
        public AnalyticsWritersRegistrar AddWriteStrategy<TWriter>() 
            where TWriter : class, IAnalyticsWriteStrategy
        {
            if (isAnalyticsEnabled)
            {
                services.AddTransient<IAnalyticsWriteStrategy, TWriter>();
            }
            return this;
        }
        
        public IServiceCollection Services => services;
    }
    
    private static IServiceCollection TryAddSingletonInline<TService>(this IServiceCollection services)
        where TService : class
    {
        services.TryAddSingleton<TService>();
        return services;
    }
}
