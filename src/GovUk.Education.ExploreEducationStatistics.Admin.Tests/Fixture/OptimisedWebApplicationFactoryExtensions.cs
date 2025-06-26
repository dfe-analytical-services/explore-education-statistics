#nullable enable
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Text.Encodings.Web;
using System.Threading.Tasks;
using GovUk.Education.ExploreEducationStatistics.Admin.Database;
using GovUk.Education.ExploreEducationStatistics.Admin.Services.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Common.Extensions;
using GovUk.Education.ExploreEducationStatistics.Common.Services;
using GovUk.Education.ExploreEducationStatistics.Common.Services.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Common.Tests.Extensions;
using GovUk.Education.ExploreEducationStatistics.Common.Tests.Fixtures;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Database;
using GovUk.Education.ExploreEducationStatistics.Data.Model.Database;
using GovUk.Education.ExploreEducationStatistics.Data.Processor.Model;
using GovUk.Education.ExploreEducationStatistics.Public.Data.Model.Database;
using GovUk.Education.ExploreEducationStatistics.Publisher.Model;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using Testcontainers.PostgreSql;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Tests.Fixture;

public static class OptimisedWebApplicationFactoryExtensions
{
    public static async Task ResetDatabases<TStartup>(
        this WebApplicationFactory<TStartup> testApp) where TStartup : class
    {
        await testApp.EnsureDatabaseDeleted<ContentDbContext, TStartup>();
        await testApp.EnsureDatabaseDeleted<StatisticsDbContext, TStartup>();
        await testApp.EnsureDatabaseDeleted<UsersAndRolesDbContext, TStartup>();
        await testApp.ClearTestData<PublicDataDbContext, TStartup>();
        testApp.Services.GetRequiredService<PublicDataDbContext>().ChangeTracker.Clear();
        // await _azuriteContainer.DisposeAsync();
    }

    public static async Task ClearTestData<TDbContext, TStartup>(
        this WebApplicationFactory<TStartup> testApp)
        where TDbContext : DbContext
        where TStartup : class
    {
        var context = testApp.GetDbContext<TDbContext, TStartup>();
        await context.ClearTestData();
    }

    // public static TDbContext GetDbContext<TDbContext, TStartup>(
    //     this WebApplicationFactory<TStartup> testApp)
    //     where TDbContext : DbContext
    //     where TStartup : class
    // {
    //     return testApp.GetDbContext<TDbContext, TStartup>();
    // }

    public static async Task EnsureDatabaseDeleted<TDbContext, TStartup>(
        this WebApplicationFactory<TStartup> testApp)
        where TDbContext : DbContext
        where TStartup : class
    {
        await using var context = testApp.GetDbContext<TDbContext, TStartup>();
        await context.Database.EnsureDeletedAsync();
    }

    // public static async Task AddTestData<TDbContext, TStartup>(
    //     this WebApplicationFactory<TStartup> testApp,
    //     Action<TDbContext> supplier)
    //     where TDbContext : DbContext
    //     where TStartup : class
    // {
    //     await using var context = testApp.GetDbContext<TDbContext, TStartup>();
    //
    //     supplier.Invoke(context);
    //     await context.SaveChangesAsync();
    // }
    
    public static OptimisedWebApplicationFactoryBuilder<Startup> ConfigureAdmin(
        this WebApplicationFactory<Startup> testApp)
    {
        return new OptimisedWebApplicationFactoryBuilder<Startup>(testApp)
            .WithAdmin();
    }
}
