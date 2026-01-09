using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using Moq;

namespace GovUk.Education.ExploreEducationStatistics.Common.IntegrationTests.WebApp;

/// <summary>
///
/// A helper class to allow tests to look up services and mocks after the factory has been configured.
/// Through using this helper class to perform service lookups, we prevent direct access to the factory from tests.
///
/// </summary>
public class OptimisedServiceCollectionLookups<TStartup>(WebApplicationFactory<TStartup> factory)
    where TStartup : class
{
    /// <summary>
    ///
    /// Look up a service from the factory.
    ///
    /// </summary>
    public TService GetService<TService>()
        where TService : class
    {
        return factory.Services.GetRequiredService<TService>();
    }

    /// <summary>
    ///
    /// Look up a mocked service from the factory.
    ///
    /// </summary>
    public Mock<TService> GetMockService<TService>()
        where TService : class
    {
        return Mock.Get(factory.Services.GetRequiredService<TService>());
    }
}
