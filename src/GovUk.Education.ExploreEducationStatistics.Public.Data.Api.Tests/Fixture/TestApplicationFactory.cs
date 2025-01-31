using System.Security.Claims;
using System.Text.Encodings.Web;
using GovUk.Education.ExploreEducationStatistics.Common.Tests.Extensions;
using GovUk.Education.ExploreEducationStatistics.Common.Tests.Fixtures;
using GovUk.Education.ExploreEducationStatistics.Public.Data.Model.Database;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Testcontainers.PostgreSql;

namespace GovUk.Education.ExploreEducationStatistics.Public.Data.Api.Tests.Fixture;

public class TestApplicationFactory : TestApplicationFactory<Startup>
{
    private readonly PostgreSqlContainer _postgreSqlContainer = new PostgreSqlBuilder()
        .WithImage("postgres:16.1-alpine")
        .Build();

    public async Task Initialize()
    {
        await _postgreSqlContainer.StartAsync();
    }

    public override async ValueTask DisposeAsync()
    {
        await _postgreSqlContainer.DisposeAsync();
    }

    public async Task ClearTestData<TDbContext>() where TDbContext : DbContext
    {
        var context = this.GetDbContext<TDbContext, Startup>();
        await context.ClearTestData();
    }

    public async Task ClearAllTestData()
    {
        await ClearTestData<PublicDataDbContext>();
    }

    protected override IHostBuilder CreateHostBuilder()
    {
        return base
            .CreateHostBuilder()
            .ConfigureServices(services =>
            {
                services.AddDbContext<PublicDataDbContext>(
                    options => options
                        .UseNpgsql(
                            _postgreSqlContainer.GetConnectionString(),
                            psqlOptions => psqlOptions.EnableRetryOnFailure()));

                services
                    .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
                    .AddScheme<AuthenticationSchemeOptions, TestAuthHandler>(JwtBearerDefaults.AuthenticationScheme, null);
            });
    }
    
    /// <summary>
    /// This method adds an authenticated User in the form of a ClaimsPrincipal to the HttpContext.
    /// </summary>
    /// <param name="testApp"></param>
    /// <param name="user"></param>
    /// <returns></returns>
    public WebApplicationFactory<Startup> SetUser(ClaimsPrincipal? user)
    {
        return this.ConfigureServices(services =>
        {
            if (user != null)
            {
                services.AddScoped(_ => user);
            }
        });
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
}
