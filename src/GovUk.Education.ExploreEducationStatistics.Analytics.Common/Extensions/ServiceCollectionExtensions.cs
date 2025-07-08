using GovUk.Education.ExploreEducationStatistics.Analytics.Common.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Analytics.Common.Strategies;
using Microsoft.Extensions.DependencyInjection;

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
}
