#nullable enable
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
        return app.WithWebHostBuilder(builder => builder
            .ResetDbContext<TDbContext>());
    }
    
    public static TDbContext GetDbContext<TDbContext, TEntrypoint>(this WebApplicationFactory<TEntrypoint> app) 
        where TDbContext : DbContext
        where TEntrypoint : class
    {
        var scope = app.Services.CreateScope();
        return scope.ServiceProvider.GetRequiredService<TDbContext>();
    }

    /// <summary>
    /// This method registers all Controllers found in the <see cref="TStartup"/> class's assembly.
    /// 
    /// Typically, <see cref="TEntrypoint"/> will be the TestStartup type, and <see cref="TStartup"/> will be the
    /// production Startup type.
    /// </summary>
    public static WebApplicationFactory<TEntrypoint> RegisterControllers<TEntrypoint, TStartup>(
        this WebApplicationFactory<TEntrypoint> app
    )
        where TEntrypoint : class
        where TStartup : class
    {
        return app.ConfigureServices(services => services.RegisterControllers<TStartup>());
    }
}
