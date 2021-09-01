#nullable enable
using System;
using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace GovUk.Education.ExploreEducationStatistics.Common.Tests.Extensions
{
    public static class WebApplicationFactoryExtensions
    {
        public static IWebHostBuilder AddTestData<TDbContext>(
            this IWebHostBuilder builder,
            Action<TDbContext> testDataSupplier) where TDbContext : DbContext
        {
            return builder.ConfigureServices(
                services =>
                {
                    var provider = services.BuildServiceProvider();

                    using var scope = provider.CreateScope();
                    using var db = scope.ServiceProvider.GetRequiredService<TDbContext>();

                    db.Database.EnsureCreated();

                    testDataSupplier(db);
                }
            );
        }
    }
}