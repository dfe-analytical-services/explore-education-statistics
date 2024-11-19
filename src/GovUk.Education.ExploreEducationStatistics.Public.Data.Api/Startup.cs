using System.Diagnostics.CodeAnalysis;
using System.Net;
using System.Text.Json;
using System.Text.Json.Serialization;
using AngleSharp.Io;
using Dapper;
using FluentValidation;
using GovUk.Education.ExploreEducationStatistics.Common.Config;
using GovUk.Education.ExploreEducationStatistics.Common.Database;
using GovUk.Education.ExploreEducationStatistics.Common.Extensions;
using GovUk.Education.ExploreEducationStatistics.Common.ModelBinding;
using GovUk.Education.ExploreEducationStatistics.Common.Rules;
using GovUk.Education.ExploreEducationStatistics.Public.Data.Api.Extensions;
using GovUk.Education.ExploreEducationStatistics.Public.Data.Api.Migrations;
using GovUk.Education.ExploreEducationStatistics.Public.Data.Api.Options;
using GovUk.Education.ExploreEducationStatistics.Public.Data.Api.Repository;
using GovUk.Education.ExploreEducationStatistics.Public.Data.Api.Repository.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Public.Data.Api.Requests;
using GovUk.Education.ExploreEducationStatistics.Public.Data.Api.Services;
using GovUk.Education.ExploreEducationStatistics.Public.Data.Api.Services.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Public.Data.Api.Services.Interfaces.Security;
using GovUk.Education.ExploreEducationStatistics.Public.Data.Api.Services.Security;
using GovUk.Education.ExploreEducationStatistics.Public.Data.Model.Database;
using GovUk.Education.ExploreEducationStatistics.Public.Data.Model.DuckDb;
using GovUk.Education.ExploreEducationStatistics.Public.Data.Services;
using GovUk.Education.ExploreEducationStatistics.Public.Data.Services.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Public.Data.Services.Options;
using MicroElements.Swashbuckle.FluentValidation.AspNetCore;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Http.Timeouts;
using Microsoft.AspNetCore.Mvc.ModelBinding.Metadata;
using Microsoft.AspNetCore.Rewrite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using RequestTimeoutOptions = GovUk.Education.ExploreEducationStatistics.Public.Data.Api.Options.RequestTimeoutOptions;

namespace GovUk.Education.ExploreEducationStatistics.Public.Data.Api;

[ExcludeFromCodeCoverage]
public class Startup(IConfiguration configuration, IHostEnvironment hostEnvironment)
{
    private readonly MiniProfilerOptions _miniProfilerOptions = configuration
        .GetRequiredSection(MiniProfilerOptions.Section)
        .Get<MiniProfilerOptions>()!;

    private readonly OpenIdConnectOptions _openIdConnectOptions = configuration
        .GetRequiredSection(OpenIdConnectOptions.Section)
        .Get<OpenIdConnectOptions>()!;

    private readonly RequestTimeoutOptions _requestTimeoutOptions = configuration
        .GetSection(RequestTimeoutOptions.Section)
        .Get<RequestTimeoutOptions>()!;

    private readonly AppInsightsOptions _appInsightsOptions = configuration
        .GetSection(AppInsightsOptions.Section)
        .Get<AppInsightsOptions>()!;

    // This method gets called by the runtime. Use this method to add services to the container.
    public void ConfigureServices(IServiceCollection services)
    {
        // Health and telemetry

        services.AddHealthChecks();

        services
            .AddApplicationInsightsTelemetry(options =>
            {
                options.ConnectionString = _appInsightsOptions.ConnectionString;
            })
            .AddApplicationInsightsTelemetryProcessor<SensitiveDataTelemetryProcessor>();

        // Profiling

        if (_miniProfilerOptions.Enabled)
        {
            services
                .AddMiniProfiler(options =>
                {
                    options.RouteBasePath = "/profiler";
                    options.ShouldProfile = request => request.Path.StartsWithSegments("/api");
                })
                .AddEntityFramework();
        }

        // Authentication

        // Look for JWT Bearer tokens, and validate that they are issued from the correct tenant and are
        // issued for the Public API.  It is only necessary to enable this support in Azure environments
        // as it is using Azure Authentication to handle the access tokens.
        if (hostEnvironment.IsProduction())
        {
            services
                .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
                .AddJwtBearer(options =>
                {
                    var tenantId = _openIdConnectOptions.TenantId;
                    var clientId = _openIdConnectOptions.ClientId;
                    options.Authority = $"https://login.microsoftonline.com/{tenantId}";
                    options.Audience = $"api://{clientId}";
                });
        }

        // MVC

        services.AddRequestTimeouts(options =>
        {
            options.DefaultPolicy =
                new RequestTimeoutPolicy
                {
                    Timeout = TimeSpan.FromMilliseconds(_requestTimeoutOptions.TimeoutMilliseconds),
                    TimeoutStatusCode = (int)HttpStatusCode.GatewayTimeout
                };
        });

        services
            .AddControllers(options =>
            {
                options.RespectBrowserAcceptHeader = true;
                options.ReturnHttpNotAcceptable = true;
                options.EnableEndpointRouting = false;

                options.Filters.Add<ProblemDetailsResultFilter>();
                options.AddInvalidRequestInputResultFilter();
                options.AddCommaSeparatedQueryModelBinderProvider();
                options.AddTrimStringBinderProvider();

                // Stop empty query string parameters being converted to null
                options.ModelMetadataDetailsProviders.Add(new EmptyStringMetadataDetailsProvider());
                // Adds correct camelCased paths for model errors
                options.ModelMetadataDetailsProviders.Add(new SystemTextJsonValidationMetadataProvider());
            })
            .ConfigureApiBehaviorOptions(options =>
            {
                // Disables default model validation. Use FluentValidation instead.
                options.SuppressModelStateInvalidFilter = true;
            })
            .AddJsonOptions(options =>
            {
                options.JsonSerializerOptions.DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull;
                options.JsonSerializerOptions.UnmappedMemberHandling = JsonUnmappedMemberHandling.Disallow;
                // This allows comments to be left in JSON bodies so users can annotate
                // their data set queries for debugging - do not remove!
                options.JsonSerializerOptions.ReadCommentHandling = JsonCommentHandling.Skip;

                // This must be false to allow `JsonExceptionResultFilter` to work correctly,
                // otherwise, JsonExceptions can't be identified. Also, this prevents
                // error messages from accidentally leaking internals to users.
                options.AllowInputFormatterExceptionMessages = false;
            });

        services.AddProblemDetails();
        services.AddApiVersioning().AddMvc().AddApiExplorer();
        services.AddEndpointsApiExplorer();

        // Databases

        // Only set up the `PublicDataDbContext` in non-integration test
        // environments. Otherwise, the connection string will be null and
        // cause the data source builder to throw a host exception.
        if (!hostEnvironment.IsIntegrationTest())
        {
            var connectionString = configuration.GetConnectionString("PublicDataDb")!;
            services.AddPsqlDbContext<PublicDataDbContext>(connectionString, hostEnvironment);
        }

        // Configure Dapper to match CSV columns with underscores
        DefaultTypeMap.MatchNamesWithUnderscores = true;

        services.AddScoped<IDuckDbConnection>(_ => _miniProfilerOptions.Enabled
            ? new ProfiledDuckDbConnection()
            : new DuckDbConnection()
        );

        // Caching and compression

        services.AddResponseCaching();
        services.AddResponseCompression(options =>
        {
            options.EnableForHttps = true;
        });

        // Options - only need to add ones that will be used in services

        services.AddOptions<AppOptions>()
            .Bind(configuration.GetRequiredSection(AppOptions.Section));
        services.AddOptions<ContentApiOptions>()
            .Bind(configuration.GetRequiredSection(ContentApiOptions.Section));
        services.AddOptions<DataFilesOptions>()
            .Bind(configuration.GetRequiredSection(DataFilesOptions.Section));
        services.AddOptions<RequestTimeoutOptions>()
            .Bind(configuration.GetRequiredSection(RequestTimeoutOptions.Section));

        // Services

        services.AddFluentValidation();
        services.AddValidatorsFromAssembly(typeof(DataSetGetQueryLocations.Validator)
            .Assembly); // Adds *all* validators from Public.Data.Requests
        services.AddFluentValidationRulesToSwagger();

        services.AddHttpClient<IContentApiClient, ContentApiClient>((provider, httpClient) =>
        {
            var options = provider.GetRequiredService<IOptions<ContentApiOptions>>();
            httpClient.BaseAddress = new Uri(options.Value.Url);
            httpClient.DefaultRequestHeaders.Add(HeaderNames.UserAgent, "EES Public Data API");
        });

        services.AddSecurity();

        services.AddScoped<IAuthorizationHandlerService, AuthorizationHandlerService>();

        services.AddScoped<IPreviewTokenService, PreviewTokenService>();

        services.AddSingleton<IDataSetVersionPathResolver, DataSetVersionPathResolver>();
        services.AddScoped<IPublicationService, PublicationService>();
        services.AddScoped<IDataSetService, DataSetService>();
        services.AddScoped<IDataSetQueryService, DataSetQueryService>();
        services.AddScoped<IDataSetQueryParser, DataSetQueryParser>();
        services.AddScoped<IDataSetVersionChangeService, DataSetVersionChangeService>();

        services.AddScoped<IParquetDataRepository, ParquetDataRepository>();
        services.AddScoped<IParquetFilterRepository, ParquetFilterRepository>();
        services.AddScoped<IParquetIndicatorRepository, ParquetIndicatorRepository>();
        services.AddScoped<IParquetLocationRepository, ParquetLocationRepository>();
        services.AddScoped<IParquetTimePeriodRepository, ParquetTimePeriodRepository>();

        // TODO EES-5660 - remove this migration after it has been run against each Public API-enabled environment.
        services.AddScoped<ICustomMigration, EES5660_MigrateDraftDataSetVersionFolderNames>();
    }

    // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
    public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
    {
        UpdateDatabase(app);

        if (_miniProfilerOptions.Enabled)
        {
            app.UseMiniProfiler();
        }

        if (env.IsDevelopment() || env.IsIntegrationTest())
        {
            app.UseDeveloperExceptionPage();
        }

        // Rewrites

        app.UseRewriter(new RewriteOptions { Rules = { new LowercasePathRule() } });

        // Caching and compression

        app.UseResponseCaching();
        app.UseResponseCompression();

        // CORS

        app.UseCors(options => options
            .WithOrigins(
                "http://localhost:3000",
                "http://localhost:3001",
                "https://localhost:3000",
                "https://localhost:3001")
            .AllowAnyMethod()
            .AllowAnyHeader());

        // Routing / endpoints

        app.UseRouting();

        // Authentication and authorization

        app.UseAuthentication();
        app.UseAuthorization();

        app.UseRequestTimeouts();

        app.UseEndpoints(builder =>
        {
            builder
                .MapControllers()
                // Enable both anonymous public users and authenticated Admin user to securely access controllers.
                .RequireAuthorization()
                .AllowAnonymous();
        });

        app.UseHealthChecks("/health");
    }

    private static void UpdateDatabase(IApplicationBuilder app)
    {
        using var serviceScope = app.ApplicationServices
            .GetRequiredService<IServiceScopeFactory>()
            .CreateScope();

        using var context = serviceScope.ServiceProvider.GetRequiredService<PublicDataDbContext>();

        context.Database.SetCommandTimeout(300);
        context.Database.Migrate();

        ApplyCustomMigrations(app);
    }

    private static void ApplyCustomMigrations(IApplicationBuilder app)
    {
        using var serviceScope = app.ApplicationServices
            .GetRequiredService<IServiceScopeFactory>()
            .CreateScope();

        var migrations = serviceScope.ServiceProvider.GetServices<ICustomMigration>();

        migrations.ForEach(migration => migration.Apply());
    }
}
