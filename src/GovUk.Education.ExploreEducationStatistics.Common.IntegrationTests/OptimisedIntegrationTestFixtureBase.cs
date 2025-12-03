using DotNet.Testcontainers.Containers;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using Xunit;

namespace GovUk.Education.ExploreEducationStatistics.Common.IntegrationTests;

/// <summary>
///
/// This fixture base class supports and controls a high-level and quite generic lifecycle.
///
/// It abstracts the WebApplicationFactory away from fixtures that subclass this fixture and
/// instead controls configuration and access through helper classes that are provided to the
/// subclass via lifecycle methods.
///
/// Essentially this fixture does the following steps:
///
/// 1. A test collection executes.
/// 2. XUnit creates a new instance of the subclass of this fixture.
/// 3. XUnit calls <see cref="InitializeAsync"/> on this fixture.
/// 4. <see cref="InitializeAsync"/> then:
///   1. Allows the subclass to register any Test Containers that it needs, by the subclass overriding the
///      <see cref="RegisterTestContainers"/> method.
///   2. Starts any Test Containers that the subclass needs.
///   3. Lets the subclass modify any registered services and configuration prior to the WebApplicationFactory being
///      built, by the subclass overriding the <see cref="ConfigureServicesAndConfiguration" /> method.
///   4. Builds a new WebApplicationFactoryBuilder that will allow us to  create a WebApplicationFactory that is based
///      on <see cref="TStartup"/> but with amendments made to allow testing.
///   5. Applies any ServiceCollection and IConfigurationBuilder modifications that the subclass asked for to the
///      builder.
///   6. Lets the subclass look up any services that it needs after the factory has been constructed, by overriding the
///      <see cref="LookupServicesAfterFactoryConstructed"/> method.
/// 5. The tests in the collection run in sequence, all using this single instance of the fixture subclass.
/// 6. XUnit calls <see cref="DisposeAsync"/> on this fixture.
/// 7. <see cref="DisposeAsync"/> then:
///   1. Stops any Test Containers that the subclass registered.
///   2. Allows the subclass to dispose of any other disposable resources that it's holding references to, by the
///      subclass overriding the <see cref="DisposeResources"/> methods.
///
/// </summary>
public abstract class OptimisedIntegrationTestFixtureBase<TStartup> : IAsyncLifetime
    where TStartup : class
{
    private TestContainerRegistrations _testContainers = null!;
    private WebApplicationFactory<TStartup> _factory = null!;

    public async Task InitializeAsync()
    {
        _testContainers = new TestContainerRegistrations();
        RegisterTestContainers(_testContainers);

        await _testContainers.StartAll();

        var modifications = new OptimisedServiceAndConfigModifications();
        ConfigureServicesAndConfiguration(modifications);

        var factoryBuilder = new OptimisedWebApplicationFactoryBuilder<TStartup>();

        foreach (var modification in modifications.ServiceModifications)
        {
            factoryBuilder.AddServiceModifications(modification);
        }

        foreach (var modification in modifications.ConfigModifications)
        {
            factoryBuilder.AddConfigModifications(modification);
        }

        _factory = factoryBuilder.Build();

        var lookups = new OptimisedServiceCollectionLookups<TStartup>(_factory);
        LookupServicesAfterFactoryConstructed(lookups);
    }

    protected virtual void RegisterTestContainers(TestContainerRegistrations registrations) { }

    protected virtual void ConfigureServicesAndConfiguration(
        OptimisedServiceAndConfigModifications serviceModifications
    ) { }

    protected virtual void LookupServicesAfterFactoryConstructed(
        OptimisedServiceCollectionLookups<TStartup> lookups
    ) { }

    /// <summary>
    /// Creates an HttpClient that can be used to send HTTP requests to the WebApplicationFactory.
    /// </summary>
    public HttpClient CreateClient()
    {
        return _factory.CreateClient();
    }

    protected virtual Task DisposeResources()
    {
        return Task.CompletedTask;
    }

    public async Task DisposeAsync()
    {
        await _testContainers.StopAll();
        await DisposeResources();
    }
}

public class TestContainerRegistrations
{
    private readonly List<DockerContainer> _testContainers = new();

    public void RegisterContainer(DockerContainer container)
    {
        _testContainers.Add(container);
    }

    public async Task StartAll()
    {
        var startTasks = _testContainers.Select(container => container.StartAsync());
        await Task.WhenAll(startTasks);
    }

    public async Task StopAll()
    {
        var stopTasks = _testContainers.Select(container => container.StopAsync());
        await Task.WhenAll(stopTasks);
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
public class OptimisedServiceAndConfigModifications
{
    internal readonly List<Action<IServiceCollection>> ServiceModifications = new();
    internal readonly List<Action<IConfigurationBuilder>> ConfigModifications = new();

    public OptimisedServiceAndConfigModifications AddController(Type controllerType)
    {
        ServiceModifications.Add(services =>
            services.AddControllers().AddApplicationPart(controllerType.Assembly).AddControllersAsServices()
        );
        return this;
    }

    public OptimisedServiceAndConfigModifications AddControllers<TStartup>()
        where TStartup : class
    {
        ServiceModifications.Add(services => services.RegisterControllers<TStartup>());
        return this;
    }

    public OptimisedServiceAndConfigModifications AddInMemoryDbContext<TDbContext>(string databaseName)
        where TDbContext : DbContext
    {
        ServiceModifications.Add(services => services.UseInMemoryDbContext<TDbContext>(databaseName));
        return this;
    }

    public OptimisedServiceAndConfigModifications AddDbContext<TDbContext>(Action<DbContextOptionsBuilder> options)
        where TDbContext : DbContext
    {
        ServiceModifications.Add(services => services.AddDbContext<TDbContext>(options));
        return this;
    }

    public OptimisedServiceAndConfigModifications AddSingleton<TService>()
        where TService : class
    {
        ServiceModifications.Add(services => services.AddSingleton<TService>());
        return this;
    }

    public OptimisedServiceAndConfigModifications AddSingleton<TService>(TService service)
        where TService : class
    {
        ServiceModifications.Add(services => services.AddSingleton(service));
        return this;
    }

    public OptimisedServiceAndConfigModifications AddAuthentication<
        TAuthenticationHandler,
        TAuthenticationHandlerOptions
    >(string schemeName)
        where TAuthenticationHandler : AuthenticationHandler<TAuthenticationHandlerOptions>
        where TAuthenticationHandlerOptions : AuthenticationSchemeOptions, new()
    {
        ServiceModifications.Add(services =>
            services
                .AddAuthentication(schemeName)
                .AddScheme<TAuthenticationHandlerOptions, TAuthenticationHandler>(schemeName, null)
        );
        return this;
    }

    public OptimisedServiceAndConfigModifications ReplaceService<T>(T service, bool optional = false)
        where T : class
    {
        ServiceModifications.Add(services => services.ReplaceService(service, optional: optional));
        return this;
    }

    public OptimisedServiceAndConfigModifications ReplaceServiceWithMock<T>(
        MockBehavior mockBehavior = MockBehavior.Strict
    )
        where T : class
    {
        ServiceModifications.Add(services => services.MockService<T>(mockBehavior));
        return this;
    }

    public OptimisedServiceAndConfigModifications AddMock<T>(T service, bool optional = false)
        where T : class
    {
        ServiceModifications.Add(services => services.MockService<T>());
        return this;
    }

    public OptimisedServiceAndConfigModifications AddInMemoryCollection(
        IEnumerable<KeyValuePair<string, string?>> appsettings
    )
    {
        ConfigModifications.Add(config => config.AddInMemoryCollection(appsettings));
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
