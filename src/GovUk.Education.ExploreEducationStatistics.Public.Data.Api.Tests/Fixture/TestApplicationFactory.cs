using System.Reflection;
using System.Security.Claims;
using System.Text.Encodings.Web;
using GovUk.Education.ExploreEducationStatistics.Common.Extensions;
using GovUk.Education.ExploreEducationStatistics.Common.Tests.Extensions;
using GovUk.Education.ExploreEducationStatistics.Common.Tests.Fixtures;
using GovUk.Education.ExploreEducationStatistics.Public.Data.Api.Services.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Public.Data.Api.Tests.Services;
using GovUk.Education.ExploreEducationStatistics.Public.Data.Model.Database;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
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
    
    private readonly HashSet<string> _additionalAppsettingsFiles = new();

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

    public TestApplicationFactory AddAppSettings(string filename)
    {
        _additionalAppsettingsFiles.Add(filename);
        return this;
    }

    protected override IHostBuilder CreateHostBuilder()
    {
        return base
            .CreateHostBuilder()
            .ConfigureAppConfiguration((_, builder) =>
            {
                var configuration = new ConfigurationBuilder();
                
                _additionalAppsettingsFiles.ForEach(settingsFile =>
                {
                    configuration.AddJsonFile(
                        Path.Combine(Assembly.GetExecutingAssembly().GetDirectoryPath(),
                            settingsFile), optional: false);
                });
                
                builder.AddConfiguration(configuration.Build());
            })
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

                services.AddSingleton<TestAuthHandlerUserProvider>();
                
                services
                    .ReplaceService<IAnalyticsPathResolver>(new TestAnalyticsPathResolver(), optional: true);
            });
    }

    internal class TestAuthHandlerUserProvider
    {
        public ClaimsPrincipal? User { get; set; }
    }
    
    /// <summary>
    /// An AuthenticationHandler that allows the tests to make a ClaimsPrincipal available in the HttpContext
    /// for authentication and authorization mechanisms to use.
    /// </summary>
    internal class TestAuthHandler(
        IOptionsMonitor<AuthenticationSchemeOptions> options,
        ILoggerFactory logger,
        UrlEncoder encoder,
        TestAuthHandlerUserProvider userProvider)
        : AuthenticationHandler<AuthenticationSchemeOptions>(options, logger, encoder)
    {
        protected override Task<AuthenticateResult> HandleAuthenticateAsync()
        {
            var user = userProvider.User;

            if (user == null)
            {
                return Task.FromResult(AuthenticateResult.NoResult());
            }

            var ticket = new AuthenticationTicket(user, JwtBearerDefaults.AuthenticationScheme);
            var result = AuthenticateResult.Success(ticket);
            return Task.FromResult(result);
        }
    }
}

public static class TestWebApplicationFactoryExtensions
{
    public static WebApplicationFactory<Startup> WithUser(this WebApplicationFactory<Startup> factory,
        ClaimsPrincipal? user)
    {
        var authHandler = factory.Services.GetRequiredService<TestApplicationFactory.TestAuthHandlerUserProvider>();
        authHandler.User = user;
        return factory;
    }
}
