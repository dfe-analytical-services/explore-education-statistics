using System;
using System.Linq;
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
    public static IServiceCollection ReplaceDbContext<TDbContext>(this IServiceCollection services)
        where TDbContext : DbContext
    {
        // Remove the default DbContext descriptor that was provided by Startup.cs.
        var descriptor = services
            .Single(d => d.ServiceType == typeof(DbContextOptions<TDbContext>));

        services.Remove(descriptor);
            
        // Add the new In-Memory replacement.
        return services.AddDbContext<TDbContext>(
            options => options
                .UseInMemoryDatabase(nameof(TDbContext), builder => builder.EnableNullChecks(false)));
    }
    
    /// <summary>
    /// This method replaces a service that has been registered in Startup with a new implementation. The same
    /// lifecycle that was registered in Startup will be used to register the new service.
    /// </summary>
    public static IServiceCollection ReplaceService<TService>(
        this IServiceCollection services, 
        TService replacement)
        where TService : class 
    {
        // Remove the default service descriptor that was provided by Startup.cs.
        var descriptor = services
            .Single(d => d.ServiceType == typeof(TService));
    
        services.Remove(descriptor);
            
        // Add the replacement.
        return descriptor.Lifetime switch
        {
            ServiceLifetime.Singleton => services.AddSingleton(_ => replacement),
            ServiceLifetime.Scoped => services.AddScoped(_ => replacement),
            ServiceLifetime.Transient => services.AddTransient(_ => replacement),
            _ => throw new ArgumentOutOfRangeException(
                $"Cannot register test service with ${nameof(ServiceLifetime)} {descriptor.Lifetime}")
        };
    }
    
    /// <summary>
    /// This method replaces a service that has been registered in Startup with a Strict Mock. The same
    /// lifecycle that was registered in Startup will be used to register the new service.
    /// </summary>
    public static IServiceCollection ReplaceService<TService>(
        this IServiceCollection services)
        where TService : class
    {
        return services.ReplaceService(Mock.Of<TService>(Strict));
    }
    
    /// <summary>
    /// This method registers all Controllers found in the <see cref="TStartup"/> class's assembly.
    /// </summary>
    public static IServiceCollection RegisterControllers<TStartup>(
        this IServiceCollection services)
        where TStartup : class
    {
        services
            .AddControllers(options => 
                options.ModelBinderProviders.Insert(0, new SeparatedQueryModelBinderProvider(",")))
            .AddApplicationPart(typeof(TStartup).Assembly)
            .AddControllersAsServices();

        return services;
    }
}