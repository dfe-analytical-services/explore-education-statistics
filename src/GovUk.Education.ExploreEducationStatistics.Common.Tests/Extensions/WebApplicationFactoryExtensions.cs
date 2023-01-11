#nullable enable
using System;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace GovUk.Education.ExploreEducationStatistics.Common.Tests.Extensions;

public static class WebApplicationFactoryExtensions
{
    public static WebApplicationFactory<TEntrypoint> ConfigureServices<TEntrypoint>(
        this WebApplicationFactory<TEntrypoint> app,
        Action<IServiceCollection> configureServices) where TEntrypoint : class
    {
        return app.WithWebHostBuilder(builder => builder.ConfigureServices(configureServices));
    }

    public static WebApplicationFactory<TEntrypoint> ResetDbContext<TDbContext, TEntrypoint>(
        this WebApplicationFactory<TEntrypoint> app
    )
        where TDbContext : DbContext
        where TEntrypoint : class
    {
        return app.WithWebHostBuilder(builder => builder.ResetDbContext<TDbContext>());
    }

    public static WebApplicationFactory<TEntrypoint> AddTestData<TDbContext, TEntrypoint>(
        this WebApplicationFactory<TEntrypoint> app,
        Action<TDbContext> testData
    )
        where TDbContext : DbContext
        where TEntrypoint : class
    {
        return app.WithWebHostBuilder(builder => builder.AddTestData(testData));
    }
}
