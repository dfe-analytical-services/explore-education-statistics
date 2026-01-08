using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace GovUk.Education.ExploreEducationStatistics.Common.IntegrationTests.WebApp;

public abstract class OptimisedWebApplicationFactoryBuilderBase<TStartup>
    where TStartup : class
{
    public abstract WebApplicationFactory<TStartup> Build(
        List<Action<IServiceCollection>> serviceModifications,
        List<Action<IConfigurationBuilder>> configModifications
    );
}
