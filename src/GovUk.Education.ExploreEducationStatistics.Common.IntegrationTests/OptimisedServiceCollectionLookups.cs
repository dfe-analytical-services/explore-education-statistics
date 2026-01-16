using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Moq;

namespace GovUk.Education.ExploreEducationStatistics.Common.IntegrationTests;

/// <summary>
///
/// A helper class to allow tests to look up services and mocks after the factory has been configured.
/// Through using this helper class to perform service lookups, we prevent direct access to the factory from tests.
///
/// </summary>
public class OptimisedServiceCollectionLookups(IServiceProvider services)
{
    /// <summary>
    ///
    /// Look up a service from the factory.
    ///
    /// </summary>
    public TService GetService<TService>()
        where TService : class
    {
        if (typeof(TService).IsAssignableTo(typeof(DbContext)))
        {
            throw new ArgumentException(
                $"Error looking up service of type {typeof(TService)} - use DbContextHolder instead to get test DbContexts"
            );
        }

        return services.GetRequiredService<TService>();
    }

    /// <summary>
    ///
    /// Look up a mocked service from the factory.
    ///
    /// </summary>
    public Mock<TService> GetMockService<TService>()
        where TService : class
    {
        return Mock.Get(services.GetRequiredService<TService>());
    }
}
