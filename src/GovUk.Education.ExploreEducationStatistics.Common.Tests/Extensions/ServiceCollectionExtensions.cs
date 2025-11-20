using GovUk.Education.ExploreEducationStatistics.Common.Extensions;
using GovUk.Education.ExploreEducationStatistics.Common.ModelBinding;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using static Moq.MockBehavior;

namespace GovUk.Education.ExploreEducationStatistics.Common.Tests.Extensions;

/// <summary>
/// This class contains a number of extension methods for IServiceCollection that are useful when setting up
/// integration tests that require a customised Startup class.
/// </summary>
public static class ServiceCollectionExtensions
{
    /// <summary>
    /// This method replaces a DcContext that has been registered in Startup with an in-memory equivalent.
    /// </summary>
    public static IServiceCollection UseInMemoryDbContext<TDbContext>(
        this IServiceCollection services,
        string databaseName = null
    )
        where TDbContext : DbContext
    {
        // Remove the default DbContext descriptor that was provided by Startup.cs.
        var descriptor = services.Single(d => d.ServiceType == typeof(DbContextOptions<TDbContext>));

        services.Remove(descriptor);

        // Add the new In-Memory replacement.
        return services.AddDbContext<TDbContext>(options =>
            options.UseInMemoryDatabase(databaseName ?? nameof(TDbContext), builder => builder.EnableNullChecks(false))
        );
    }

    /// <summary>
    /// Replace a service that has been registered in Startup with a new implementation. The same
    /// lifecycle that was registered in Startup will be used to register the new service.
    /// </summary>
    public static IServiceCollection ReplaceService<TService>(
        this IServiceCollection services,
        TService replacement,
        bool optional = false
    )
        where TService : class
    {
        return services.ReplaceService(_ => replacement, optional);
    }

    /// <summary>
    /// Replace a service that has been registered in Startup with a new implementation. The same
    /// lifecycle that was registered in Startup will be used to register the new service.
    /// </summary>
    public static IServiceCollection ReplaceService<TService>(
        this IServiceCollection services,
        Func<IServiceProvider, TService> replacement,
        bool optional = false,
        ServiceLifetime? serviceLifetime = null
    )
        where TService : class
    {
        // Remove the default service descriptor that was provided by Startup.cs.
        var descriptor = services.SingleOrDefault(d => d.ServiceType == typeof(TService));

        if (descriptor == null)
        {
            if (optional)
            {
                return services;
            }

            throw new ArgumentNullException($"{nameof(TService)} service was not found to replace.");
        }

        services.Remove(descriptor);

        // Add the replacement.
        return (serviceLifetime ?? descriptor.Lifetime) switch
        {
            ServiceLifetime.Singleton => services.AddSingleton(replacement),
            ServiceLifetime.Scoped => services.AddScoped(replacement),
            ServiceLifetime.Transient => services.AddTransient(replacement),
            _ => throw new ArgumentOutOfRangeException(
                $"Cannot register test service with {nameof(ServiceLifetime)}.{descriptor.Lifetime}"
            ),
        };
    }

    /// <summary>
    /// Replace a service that has been registered in Startup with a Mock. The same
    /// lifecycle that was registered in Startup will be used to register the Mock.
    /// </summary>
    public static IServiceCollection ReplaceService<TService>(
        this IServiceCollection services,
        Mock<TService> replacement
    )
        where TService : class
    {
        return services.ReplaceService(replacement.Object);
    }

    /// <summary>
    /// This method replaces a service that has been registered in Startup with a Mock. The Mock is registered as a
    /// singleton so that we can be assured that a mock that is looked up in order to set setups and perform
    /// verifications on is the same instance as the mocked service that the production code will be using during test
    /// execution.
    ///
    /// The Mock will be setup with Strict behavior by default.
    /// </summary>
    public static IServiceCollection MockService<TService>(
        this IServiceCollection services,
        MockBehavior behavior = Strict
    )
        where TService : class
    {
        return services.ReplaceService(_ => Mock.Of<TService>(behavior), serviceLifetime: ServiceLifetime.Singleton);
    }

    /// <summary>
    /// This method registers all Controllers found in the <see cref="TStartup"/> class's assembly.
    /// </summary>
    public static IServiceCollection RegisterControllers<TStartup>(
        this IServiceCollection services,
        Type[] additionalControllers = default
    )
        where TStartup : class
    {
        var servicesWithMainControllers = services
            .AddControllers(options =>
                options.ModelBinderProviders.Insert(0, new SeparatedQueryModelBinderProvider(","))
            )
            .AddApplicationPart(typeof(TStartup).Assembly);

        additionalControllers?.ForEach(controllerType =>
            servicesWithMainControllers = servicesWithMainControllers.AddApplicationPart(controllerType.Assembly)
        );

        servicesWithMainControllers.AddControllersAsServices();
        return services;
    }
}
