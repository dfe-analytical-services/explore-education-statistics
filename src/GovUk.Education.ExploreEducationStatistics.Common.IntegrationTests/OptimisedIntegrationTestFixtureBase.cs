using DotNet.Testcontainers.Containers;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using Xunit;

namespace GovUk.Education.ExploreEducationStatistics.Common.IntegrationTests;

public abstract class OptimisedIntegrationTestFixtureBase<TStartup> : IAsyncLifetime
    where TStartup : class
{
    private readonly List<DockerContainer> _testContainers = new();
    private WebApplicationFactory<TStartup> _factory = null!;

    public async Task InitializeAsync()
    {
        RegisterTestContainers();

        var startupTasks = _testContainers.Select(container => container.StartAsync());
        await Task.WhenAll(startupTasks);

        var factory = new WebApplicationFactory<TStartup>();

        var modifications = new OptimisedServiceAndConfigModifications();
        ConfigureServicesAndConfiguration(modifications);

        var factoryBuilder = new OptimisedWebApplicationFactoryBuilder<TStartup>(factory);

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
        AfterFactoryConstructed(lookups);
    }

    protected virtual void RegisterTestContainers() { }

    protected virtual void ConfigureServicesAndConfiguration(
        OptimisedServiceAndConfigModifications serviceModifications
    ) { }

    protected virtual void AfterFactoryConstructed(OptimisedServiceCollectionLookups<TStartup> lookups) { }

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

    public void AddContainer(DockerContainer container)
    {
        _testContainers.Add(container);
    }

    public async Task DisposeAsync()
    {
        var shutdownTasks = _testContainers.Select(container => container.StopAsync());
        await Task.WhenAll(shutdownTasks);

        await DisposeResources();
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
