using System;
using System.Linq;
using GovUk.Education.ExploreEducationStatistics.Common.ModelBinding;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using static Moq.MockBehavior;

namespace GovUk.Education.ExploreEducationStatistics.Common.Tests.Extensions;

public static class ServiceCollectionExtensions
{
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
    
    public static IServiceCollection ReplaceService<TService>(
        this IServiceCollection services)
        where TService : class
    {
        return services.ReplaceService(Mock.Of<TService>(Strict));
    }
    
    public static IServiceCollection RegisterControllers<TStartup>(
        this IServiceCollection services)
        where TStartup : class
    {
        services.AddControllers(
                options => { options.ModelBinderProviders.Insert(0, new SeparatedQueryModelBinderProvider(",")); }
            )
            .AddApplicationPart(typeof(TStartup).Assembly)
            .AddControllersAsServices();

        return services;
    }
}