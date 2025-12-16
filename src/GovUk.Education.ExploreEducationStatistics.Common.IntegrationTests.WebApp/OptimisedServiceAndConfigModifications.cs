using Microsoft.AspNetCore.Authentication;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Moq;

namespace GovUk.Education.ExploreEducationStatistics.Common.IntegrationTests.WebApp;

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

    /// <summary>
    /// Add a one-off Controller to the WebApplicationFactory.
    /// </summary>
    public OptimisedServiceAndConfigModifications AddController(Type controllerType)
    {
        ServiceModifications.Add(services =>
            services.AddControllers().AddApplicationPart(controllerType.Assembly).AddControllersAsServices()
        );
        return this;
    }

    /// <summary>
    /// Add all Controllers from the <see cref="TStartup"/> assembly to the WebApplicationFactory.
    /// </summary>
    public OptimisedServiceAndConfigModifications AddControllers<TStartup>()
        where TStartup : class
    {
        ServiceModifications.Add(services =>
            services.AddControllers().AddApplicationPart(typeof(TStartup).Assembly).AddControllersAsServices()
        );
        return this;
    }

    /// <summary>
    /// Add an in-memory DbContext to the WebApplicationFactory, removing any previously configured one first.
    /// </summary>
    public OptimisedServiceAndConfigModifications AddInMemoryDbContext<TDbContext>(string? databaseName = null)
        where TDbContext : DbContext
    {
        ServiceModifications.Add(services =>
        {
            // Remove the default DbContext descriptor that was provided by Startup.cs.
            var descriptor = services.SingleOrDefault(d => d.ServiceType == typeof(DbContextOptions<TDbContext>));

            if (descriptor == null)
            {
                throw new InvalidOperationException(
                    $"No DbContext of type {typeof(TDbContext).Name} can be replaced by an "
                        + $"in-memory version because no original has yet been registered."
                );
            }

            services.Remove(descriptor);

            // Add the new In-Memory replacement.
            services.AddDbContext<TDbContext>(options =>
                options.UseInMemoryDatabase(
                    databaseName ?? nameof(TDbContext),
                    builder => builder.EnableNullChecks(false)
                )
            );
        });
        return this;
    }

    /// <summary>
    /// Add a DbContext with options to the WebApplicationFactory.
    /// </summary>
    public OptimisedServiceAndConfigModifications AddDbContext<TDbContext>(Action<DbContextOptionsBuilder> options)
        where TDbContext : DbContext
    {
        ServiceModifications.Add(services => services.AddDbContext<TDbContext>(options));
        return this;
    }

    /// <summary>
    /// Add a one-off singleton service to the WebApplicationFactory. Use one of the "ReplaceService" methods
    /// instead if replacing a service that is already present in the WebApplicationFactory.
    /// </summary>
    public OptimisedServiceAndConfigModifications AddSingleton<TService>()
        where TService : class
    {
        ServiceModifications.Add(services => services.AddSingleton<TService>());
        return this;
    }

    /// <summary>
    /// Add a one-off singleton service implementation to the WebApplicationFactory. Use one of the "ReplaceService"
    /// methods instead if replacing a service that is already present in the WebApplicationFactory.
    /// </summary>
    public OptimisedServiceAndConfigModifications AddSingleton<TService>(TService service)
        where TService : class
    {
        ServiceModifications.Add(services => services.AddSingleton(service));
        return this;
    }

    /// <summary>
    /// Register an AuthenticationScheme with the WebApplicationFactory.
    /// </summary>
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

    /// <summary>
    /// Replace a previously-registered service in the WebApplicationFactory with a new implementation.
    /// </summary>
    public OptimisedServiceAndConfigModifications ReplaceService<TService>(
        TService service,
        ServiceLifetime? serviceLifetime = null,
        bool optional = false
    )
        where TService : class
    {
        ServiceModifications.Add(services =>
        {
            // Remove the default service descriptor that was provided by Startup.cs.
            var descriptors = services.Where(d => d.ServiceType == typeof(TService)).ToList();

            if (descriptors.Count > 1)
            {
                throw new InvalidOperationException(
                    $"More than one service of type {typeof(TService).Name} was found to replace."
                );
            }

            var descriptor = descriptors.SingleOrDefault();

            if (descriptor == null)
            {
                if (optional)
                {
                    return;
                }

                throw new ArgumentNullException($"No service of type {typeof(TService).Name} was found to replace.");
            }

            services.Remove(descriptor);

            // Add the replacement.
            switch (serviceLifetime ?? descriptor.Lifetime)
            {
                case ServiceLifetime.Singleton:
                    services.AddSingleton(service);
                    break;
                case ServiceLifetime.Scoped:
                    services.AddScoped(_ => service);
                    break;
                case ServiceLifetime.Transient:
                    services.AddTransient(_ => service);
                    break;
                default:
                    throw new ArgumentOutOfRangeException(
                        $"Cannot register test service with {nameof(ServiceLifetime)}.{descriptor.Lifetime}"
                    );
            }
        });
        return this;
    }

    /// <summary>
    /// Replace a previously-registered service in the WebApplicationFactory with a new registered type.
    /// </summary>
    public OptimisedServiceAndConfigModifications ReplaceService<TInterface, TImplementation>(
        ServiceLifetime serviceLifetime,
        bool optional = false
    )
        where TInterface : class
        where TImplementation : class, TInterface
    {
        ServiceModifications.Add(services =>
        {
            // Remove the default service descriptor that was provided by Startup.cs (or already altered afterward by
            // other test amendments to the WebApplicationFactory).
            var descriptors = services.Where(d => d.ServiceType == typeof(TInterface)).ToList();

            var descriptor = descriptors.SingleOrDefault();

            if (descriptor == null)
            {
                if (optional)
                {
                    return;
                }

                throw new ArgumentNullException($"No service of type {typeof(TInterface).Name} was found to replace.");
            }

            services.Remove(descriptor);

            // Add the replacement.
            switch (serviceLifetime)
            {
                case ServiceLifetime.Singleton:
                    services.AddSingleton<TInterface, TImplementation>();
                    break;
                case ServiceLifetime.Scoped:
                    services.AddScoped<TInterface, TImplementation>();
                    break;
                case ServiceLifetime.Transient:
                    services.AddTransient<TInterface, TImplementation>();
                    break;
                default:
                    throw new ArgumentOutOfRangeException(
                        $"Cannot register test service with {nameof(ServiceLifetime)}.{descriptor.Lifetime}"
                    );
            }
        });
        return this;
    }

    /// <summary>
    /// Replace a previously registered service type with a mocked version of that service in the
    /// WebApplicationFactory.
    /// </summary>
    public OptimisedServiceAndConfigModifications ReplaceServiceWithMock<TService>(
        MockBehavior mockBehavior = MockBehavior.Strict
    )
        where TService : class
    {
        ReplaceService(Mock.Of<TService>(mockBehavior), serviceLifetime: ServiceLifetime.Singleton);
        return this;
    }

    /// <summary>
    /// Replace an in-memory collection (e.g. appsettings configuration) with the WebApplicationFactory.
    /// </summary>
    public OptimisedServiceAndConfigModifications AddInMemoryCollection(
        IEnumerable<KeyValuePair<string, string?>> appsettings
    )
    {
        ConfigModifications.Add(config => config.AddInMemoryCollection(appsettings));
        return this;
    }

    /// <summary>
    /// Replace an in-memory collection (e.g. appsettings configuration) with the WebApplicationFactory.
    /// </summary>
    public OptimisedServiceAndConfigModifications ConfigureService<TService>(Action<TService> configurFn)
        where TService : class
    {
        ServiceModifications.Add(services => services.Configure(configurFn));
        return this;
    }
}
