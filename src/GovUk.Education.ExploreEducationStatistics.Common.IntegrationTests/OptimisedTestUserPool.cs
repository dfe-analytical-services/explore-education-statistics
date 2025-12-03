using System.Security.Claims;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using Moq;

namespace GovUk.Education.ExploreEducationStatistics.Common.IntegrationTests;

/// <summary>
///
/// This class holds a pool of users that can be used in tandem with the <see cref="OptimisedTestAuthHandler"/> to
/// allow HTTP requests to be issued by particular users via HttpClients, or directly via setting the
/// <see cref="OptimisedTestAuthHandler.TestUserId"/> HTTP header.
///
/// Users can be added to the pool by using <see cref="AddUserIfNotExists"/> and looked up using <see cref="GetUser"/>.
///
/// </summary>
public class OptimisedTestUserPool
{
    private readonly Dictionary<Guid, ClaimsPrincipal> _users = new();

    public void AddUserIfNotExists(ClaimsPrincipal user)
    {
        if (!_users.ContainsKey(user.GetUserId()))
        {
            _users.Add(user.GetUserId(), user);
        }
    }

    public ClaimsPrincipal? GetUser(Guid id)
    {
        return _users.GetValueOrDefault(id);
    }
}

/// <summary>
///
/// A helper class to control access to WebApplicationFactory modifications for tests.
///
/// By exposing only safe methods of reconfiguring the factory, we can better control the types of changes that we
/// are able to perform, and it also makes it more visible when classes need particular additional support through
/// reconfiguration.
///
/// By using this helper class, we also prevent direct access to the factory from test classes.
///
/// </summary>
public class OptimisedServiceCollectionModifications
{
    internal readonly List<Action<IServiceCollection>> Actions = new();

    public OptimisedServiceCollectionModifications AddController(Type controllerType)
    {
        Actions.Add(services =>
            services.AddControllers().AddApplicationPart(controllerType.Assembly).AddControllersAsServices()
        );
        return this;
    }

    public OptimisedServiceCollectionModifications ReplaceService<T>(T service)
        where T : class
    {
        Actions.Add(services => services.ReplaceService(service));
        return this;
    }

    public OptimisedServiceCollectionModifications ReplaceServiceWithMock<T>()
        where T : class
    {
        Actions.Add(services => services.MockService<T>());
        return this;
    }
}

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
    /// Look up a service from the factory.  If it's required to look up a mocked service, use the
    /// <see cref="GetMockService{TService}" /> method instead.
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
