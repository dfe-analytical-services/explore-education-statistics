#nullable enable
using System.Security.Claims;
using System.Text.Encodings.Web;
using System.Threading.Tasks;
using GovUk.Education.ExploreEducationStatistics.Admin.Areas.Identity.Data;
using GovUk.Education.ExploreEducationStatistics.Common.Services.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Common.Tests.Extensions;
using GovUk.Education.ExploreEducationStatistics.Common.Tests.Fixtures;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Database;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Tests.Utils;
using GovUk.Education.ExploreEducationStatistics.Data.Model.Database;
using GovUk.Education.ExploreEducationStatistics.Data.Model.Tests.Utils;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Tests;

/// <summary>
/// Generic test application startup for use in integration tests.
///
/// This startup inherits from the real production <see cref="Startup"/> process to make available as realistic a
/// set of services as possible, but mocks out services that interact with Azure services and registers in-memory
/// DbContexts in place of the real DbContexts. It also suppresses the applying of migrations to the DbContexts,
/// which is not compatible with in-memory databases.
///
/// Additionally this startup configuration does not attempt to start
/// up the SPA, which requires NPM and needs a more involved and lengthy startup process that is not useful for
/// integration tests.
/// </summary>
/// <remarks>
/// Use in combination with <see cref="TestApplicationFactory{TStartup}"/>
/// as a test class fixture.
/// </remarks>
// ReSharper disable once ClassNeverInstantiated.Global
public class TestStartup : Startup
{
    public TestStartup(
        IConfiguration configuration,
        IHostEnvironment hostEnvironment) : base(
        configuration, 
        hostEnvironment,
        applyDatabaseMigrations: false,
        configureSpa: false)
    {
    }
}

public static class TestStartupExtensions
{
    public static WebApplicationFactory<TestStartup> Initialise(
        this WebApplicationFactory<TestStartup> testApp)
    {
        return testApp
            .ReplaceExternalDependencies()
            .RegisterControllers<TestStartup, Startup>()
            .ResetDbContexts();
    }
    
    private static WebApplicationFactory<TestStartup> ReplaceExternalDependencies(
        this WebApplicationFactory<TestStartup> testApp)
    {
        return testApp.WithWebHostBuilder(builder => builder
            .ConfigureServices(services =>
            {
                services
                    .ReplaceDbContext<ContentDbContext>()
                    .ReplaceDbContext<StatisticsDbContext>()
                    .ReplaceDbContext<UsersAndRolesDbContext>()
                    .ReplaceService<IStorageQueueService>()
                    .ReplaceService<ITableStorageService>()
                    .ReplaceService<IPrivateBlobStorageService>()
                    .ReplaceService<IPublicBlobStorageService>();
            }));
    }
    
    private static WebApplicationFactory<TestStartup> ResetDbContexts(this WebApplicationFactory<TestStartup> testApp)
    {
        return testApp
            .ResetContentDbContext()
            .ResetStatisticsDbContext();
    }
    
    public static WebApplicationFactory<TestStartup> SetUser(
        this WebApplicationFactory<TestStartup> testApp, 
        ClaimsPrincipal? user = null)
    {
        return testApp.WithWebHostBuilder(builder => builder
            .ConfigureServices(services =>
            {
                services
                    .AddAuthentication(TestAuthHandler.AuthenticationScheme)
                    .AddScheme<AuthenticationSchemeOptions, TestAuthHandler>(TestAuthHandler.AuthenticationScheme, _ => { });

                if (user != null)
                {
                    services.AddScoped<ClaimsPrincipal>(_ => user);
                }
            }));
    }
}

/// <summary>
/// An AuthenticationHandler that allows the tests to make a ClaimsPrincipal available in the HttpContext
/// for authentication and authorization mechanisms to use. 
/// </summary>
internal class TestAuthHandler : AuthenticationHandler<AuthenticationSchemeOptions>
{
    public const string AuthenticationScheme = "TestAuthenticationScheme";

    private readonly ClaimsPrincipal? _claimsPrincipal;
        
    public TestAuthHandler(
        IOptionsMonitor<AuthenticationSchemeOptions> options, 
        ILoggerFactory logger, 
        UrlEncoder encoder, 
        ISystemClock clock, 
        ClaimsPrincipal? claimsPrincipal) : base(options, logger, encoder, clock)
    {
        _claimsPrincipal = claimsPrincipal;
    }
 
    protected override Task<AuthenticateResult> HandleAuthenticateAsync()
    {
        if (_claimsPrincipal == null)
        {
            return Task.FromResult(AuthenticateResult.NoResult());
        }
            
        var ticket = new AuthenticationTicket(_claimsPrincipal, AuthenticationScheme);
        var result = AuthenticateResult.Success(ticket);
        return Task.FromResult(result);
    }
}