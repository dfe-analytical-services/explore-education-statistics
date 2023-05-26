#nullable enable
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using GovUk.Education.ExploreEducationStatistics.Common.Extensions;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.ApplicationParts;
using Microsoft.AspNetCore.Mvc.Controllers;
using Microsoft.AspNetCore.TestHost;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace GovUk.Education.ExploreEducationStatistics.Common.Tests.Extensions;

public static class WebHostBuilderExtensions
{
    public static IWebHostBuilder ResetDbContext<TDbContext>(this IWebHostBuilder builder) where TDbContext : DbContext
    {
        return builder.ConfigureServices(
            services =>
            {
                var provider = services.BuildServiceProvider();

                using var scope = provider.CreateScope();
                using var db = scope.ServiceProvider.GetRequiredService<TDbContext>();

                db.Database.EnsureDeleted();
                db.Database.EnsureCreated();
            }
        );
    }

    public static IWebHostBuilder AddTestData<TDbContext>(
        this IWebHostBuilder builder,
        Action<TDbContext> testData) where TDbContext : DbContext
    {
        return builder.ConfigureServices(
            services =>
            {
                var provider = services.BuildServiceProvider();

                using var scope = provider.CreateScope();
                using var db = scope.ServiceProvider.GetRequiredService<TDbContext>();

                db.Database.EnsureCreated();

                testData(db);

                db.SaveChanges();
            }
        );
    }
    
    /// <summary>
    /// Register controllers directly within integration tests, allowing them to be non-top-level and non-public
    /// classes. Taken from
    /// https://tpodolak.com/blog/2020/06/22/asp-net-core-adding-controllers-directly-integration-tests/
    /// </summary>
    public static IWebHostBuilder WithAdditionalControllers(this IWebHostBuilder builder, params Type[] controllers)
    {
        return builder.ConfigureTestServices(
            services =>
            {
                var partManager = GetApplicationPartManager(services);

                partManager.FeatureProviders.Add(new ExternalControllersFeatureProvider(controllers));
            });
    }

    private static ApplicationPartManager GetApplicationPartManager(IServiceCollection services)
    {
        return (ApplicationPartManager)services
            .Last(descriptor => descriptor.ServiceType == typeof(ApplicationPartManager))
            .ImplementationInstance!;
    }

    private class ExternalControllersFeatureProvider : IApplicationFeatureProvider<ControllerFeature>
    {
        private readonly Type[] _controllers;

        public ExternalControllersFeatureProvider(params Type[] controllers)
        {
            _controllers = controllers;
        }

        public void PopulateFeature(IEnumerable<ApplicationPart> parts, ControllerFeature feature)
        {
            feature.Controllers.AddRange(_controllers.Select(controller => controller.GetTypeInfo()));
        }
    }
}
