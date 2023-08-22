#nullable enable
using System;
using System.Security.Claims;
using System.Text.Encodings.Web;
using System.Threading.Tasks;
using GovUk.Education.ExploreEducationStatistics.Common.Services.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Common.Tests.Fixtures;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Tests.Utils;
using GovUk.Education.ExploreEducationStatistics.Data.Model.Tests.Utils;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using static Moq.MockBehavior;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Tests;

/// <summary>
/// Generic test application startup for use in integration tests.
///
/// This startup inherits from the real production <see cref="Startup"/> process to make available as realistic a
/// set of services as possible, but mocks out services that interact with Azure services and registers in-memory
/// DbContexts in place of the real DbContexts. Additionally this startup configuration does not attempt to start
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
    public const string AuthenticationScheme = "TestAuthenticationScheme";

    public TestStartup(
        IConfiguration configuration,
        IHostEnvironment hostEnvironment) : base(configuration, hostEnvironment)
    {
    }

    protected override IStorageQueueService GetStorageQueueService()
    {
        return Mock.Of<IStorageQueueService>(Strict);
    }

    protected override ITableStorageService GetTableStorageService()
    {
        return Mock.Of<ITableStorageService>(Strict);
    }

    protected override IPrivateBlobStorageService GetPrivateBlobStorageService(IServiceProvider services)
    {
        return Mock.Of<IPrivateBlobStorageService>(Strict);
    }

    protected override IPublicBlobStorageService GetPublicBlobStorageService(IServiceProvider services)
    {
        return Mock.Of<IPublicBlobStorageService>(Strict);
    }

    protected override DbContextOptionsBuilder GetStatisticsDbContext(DbContextOptionsBuilder options)
    {
        return AddDbContext(options, "TestStatisticsDb");
    }

    protected override DbContextOptionsBuilder GetContentDbContext(DbContextOptionsBuilder options)
    {
        return AddDbContext(options, "TestContentDb");
    }

    protected override DbContextOptionsBuilder GetUsersAndRolesDbContext(DbContextOptionsBuilder options)
    {
        return AddDbContext(options, "TestUsersAndRolesDb");
    }

    protected override void UpdateDatabase(IApplicationBuilder app, IWebHostEnvironment env)
    {
        // Nothing to do here for in-memory database contexts.
    }

    protected override void ConfigureDevelopmentSpaServer(IApplicationBuilder app, IWebHostEnvironment env)
    {
        // Don't attempt to configure the SPA server during integration tests.
    }

    private static DbContextOptionsBuilder AddDbContext(DbContextOptionsBuilder options, string name)
    {
        return options.UseInMemoryDatabase(
            name, b => b.EnableNullChecks(false));
    }
}

public static class TestStartupExtensions
{
    public static WebApplicationFactory<TestStartup> ResetDbContexts(this WebApplicationFactory<TestStartup> testApp)
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
            .ConfigureTestServices(services =>
            {
                services
                    .AddAuthentication(TestStartup.AuthenticationScheme)
                    .AddScheme<AuthenticationSchemeOptions, TestAuthHandler>(TestStartup.AuthenticationScheme, _ => { });

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
            
        var ticket = new AuthenticationTicket(_claimsPrincipal, TestStartup.AuthenticationScheme);
        var result = AuthenticateResult.Success(ticket);
        return Task.FromResult(result);
    }
}