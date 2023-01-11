#nullable enable
using System;
using Microsoft.AspNetCore.Hosting;
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
}
