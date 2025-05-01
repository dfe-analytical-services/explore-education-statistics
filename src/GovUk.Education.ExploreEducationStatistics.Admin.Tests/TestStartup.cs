#nullable enable
using System.Security.Claims;
using System.Text.Encodings.Web;
using System.Threading.Tasks;
using GovUk.Education.ExploreEducationStatistics.Admin.Database;
using GovUk.Education.ExploreEducationStatistics.Admin.Services.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Common.Services.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Common.Tests.Extensions;
using GovUk.Education.ExploreEducationStatistics.Common.Tests.Fixtures;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Database;
using GovUk.Education.ExploreEducationStatistics.Data.Model.Database;
using GovUk.Education.ExploreEducationStatistics.Data.Processor.Model;
using GovUk.Education.ExploreEducationStatistics.Publisher.Model;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;

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
            .MockService<IDataProcessorClient>()
            .MockService<IPublisherClient>()
            .MockService<IPublisherTableStorageService>()
            .MockService<IPrivateBlobStorageService>()
            .MockService<IPublicBlobStorageService>()
            .MockService<IAdminEventRaiser>(MockBehavior.Loose) // Ignore calls to publish events
            .RegisterControllers<Startup>();

        services
            .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
            .AddScheme<AuthenticationSchemeOptions, TestAuthHandler>(JwtBearerDefaults.AuthenticationScheme, null);
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
        return testApp.ConfigureServices(services =>
        {
            services.AddScoped(_ => user);
        });
    }
}

/// <summary>
/// An AuthenticationHandler that allows the tests to make a ClaimsPrincipal available in the HttpContext
/// for authentication and authorization mechanisms to use.
/// </summary>
internal class TestAuthHandler(
    IOptionsMonitor<AuthenticationSchemeOptions> options,
    ILoggerFactory logger,
    UrlEncoder encoder,
    ClaimsPrincipal? claimsPrincipal = null)
    : AuthenticationHandler<AuthenticationSchemeOptions>(options, logger, encoder)
{
    protected override Task<AuthenticateResult> HandleAuthenticateAsync()
    {
        if (claimsPrincipal == null)
        {
            return Task.FromResult(AuthenticateResult.NoResult());
        }

        var ticket = new AuthenticationTicket(claimsPrincipal, JwtBearerDefaults.AuthenticationScheme);
        var result = AuthenticateResult.Success(ticket);
        return Task.FromResult(result);
    }
}
