using GovUk.Education.ExploreEducationStatistics.Analytics.Common.Config;
using GovUk.Education.ExploreEducationStatistics.Analytics.Common.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Analytics.Common.Strategies;
using GovUk.Education.ExploreEducationStatistics.Analytics.Common.Utils;
using GovUk.Education.ExploreEducationStatistics.Common.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace GovUk.Education.ExploreEducationStatistics.Analytics.Common.Extensions;

public static class ServiceCollectionExtensions
{
    /// <summary>
    /// Registers the appropriate Analytics components depending on whether the service is
    /// enabled in the standardised config.
    /// </summary>
    public static AnalyticsRegistrar AddAnalyticsCommon(
        this IServiceCollection services,
        IConfiguration configuration) =>
        new(services, configuration);

    public class AnalyticsRegistrar(IServiceCollection services, IConfiguration configuration)
    {
        private readonly bool _isAnalyticsEnabled =
            configuration
                .GetSection(AnalyticsOptions.Section)
                .Get<AnalyticsOptions>()?
                .Enabled == true;

        private readonly IServiceCollection _services =
            services
                .AddOptions<AnalyticsOptions>().Bind(configuration.GetSection(AnalyticsOptions.Section)).Services
                .AddSingleton<IAnalyticsPathResolver, AnalyticsPathResolver>();

        public AnalyticsEnabledRegistrar WhenEnabled => _services.AddAnalyticsCommon(_isAnalyticsEnabled);
    }

    private static AnalyticsEnabledRegistrar AddAnalyticsCommon(this IServiceCollection services, bool isAnalyticsEnabled) =>
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

    public class AnalyticsDisabledRegistrar(IServiceCollection services, bool isAnalyticsEnabled)
    {
        public AnalyticsDisabledRegistrar WithService(Action<IServiceCollection> registrations)
        {
            if (!isAnalyticsEnabled)
            {
                registrations(services);
            }
            return this;
        }
        public IServiceCollection Services => services;
    }
    public class AnalyticsEnabledRegistrar(IServiceCollection services, bool isAnalyticsEnabled)
    {
        public AnalyticsEnabledRegistrar AddWriteStrategy<TWriter>()
            where TWriter : class, IAnalyticsWriteStrategy
        {
            if (isAnalyticsEnabled)
            {
                services.AddTransient<IAnalyticsWriteStrategy, TWriter>();
            }
            return this;
        }
        public AnalyticsEnabledRegistrar WithService(Action<IServiceCollection> registrations)
        {
            if (isAnalyticsEnabled)
            {
                registrations(services);
            }
            return this;
        }
        public IServiceCollection Services => services;
        public AnalyticsDisabledRegistrar WhenDisabled => new(services, isAnalyticsEnabled);
    }

    private static IServiceCollection TryAddSingletonInline<TService>(this IServiceCollection services)
        where TService : class
    {
        services.TryAddSingleton<TService>();
        return services;
    }
}
