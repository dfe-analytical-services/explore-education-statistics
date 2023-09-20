#nullable enable
using System.Security.Claims;
using System.Text.Encodings.Web;
using System.Threading.Tasks;
using GovUk.Education.ExploreEducationStatistics.Admin.Areas.Identity.Data;
using GovUk.Education.ExploreEducationStatistics.Common.Services.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Common.Tests.Extensions;
using GovUk.Education.ExploreEducationStatistics.Common.Tests.Fixtures;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Database;
using GovUk.Education.ExploreEducationStatistics.Data.Model.Database;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Tests;

/// <summary>
/// Test application Startup for use in Admin integration tests.
///
/// This startup inherits from the real production <see cref="Startup"/> process to make available as realistic a
/// set of services as possible, but mocks out services that interact with Azure services and registers in-memory
/// DbContexts in place of the real DbContexts. It also suppresses the applying of migrations to the DbContexts,
/// which is not compatible with in-memory databases.
///
/// Additionally this startup configuration does not attempt to start up the SPA, which requires NPM and needs a
/// more involved and lengthy startup process that is not useful for integration tests.
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
        hostEnvironment)
    {
    }

    public override void ConfigureServices(IServiceCollection services)
    {
        base.ConfigureServices(services);

        services
            .UseInMemoryDbContext<ContentDbContext>()
            .UseInMemoryDbContext<StatisticsDbContext>()
            .UseInMemoryDbContext<UsersAndRolesDbContext>()
            .MockService<IStorageQueueService>()
            .MockService<ICoreTableStorageService>()
            .MockService<IPublisherTableStorageService>()
            .MockService<IPrivateBlobStorageService>()
            .MockService<IPublicBlobStorageService>()
            .RegisterControllers<Startup>();

        services
            .AddAuthentication(TestAuthHandler.AuthenticationScheme)
            .AddScheme<AuthenticationSchemeOptions, TestAuthHandler>(TestAuthHandler.AuthenticationScheme, _ => { });
    }
}

public static class TestStartupExtensions
{
    /// <summary>
    /// This method adds an authenticated User in the form of a ClaimsPrincipal to the HttpContext.
    ///
    /// This User will subsequently be available for the Identity Framework as well as our own UserService, and any
    /// other production code that looks up the User from the current HttpContext.
    /// </summary>
    /// <param name="testApp"></param>
    /// <param name="user"></param>
    /// <returns></returns>
    public static WebApplicationFactory<TestStartup> SetUser(
        this WebApplicationFactory<TestStartup> testApp, 
        ClaimsPrincipal user)
    {
        return testApp.WithWebHostBuilder(builder => builder
            .ConfigureServices(services => services.AddScoped(_ => user)));
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
        ClaimsPrincipal? claimsPrincipal = null) : base(options, logger, encoder, clock)
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