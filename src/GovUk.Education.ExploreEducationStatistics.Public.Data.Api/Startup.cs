using System.Diagnostics.CodeAnalysis;
using System.Text.Json.Serialization;
using AngleSharp.Io;
using Azure.Core;
using Azure.Identity;
using GovUk.Education.ExploreEducationStatistics.Common.Cancellation;
using GovUk.Education.ExploreEducationStatistics.Common.Config;
using GovUk.Education.ExploreEducationStatistics.Common.Extensions;
using GovUk.Education.ExploreEducationStatistics.Common.Rules;
using GovUk.Education.ExploreEducationStatistics.Public.Data.Api.Options;
using GovUk.Education.ExploreEducationStatistics.Public.Data.Model.Database;
using GovUk.Education.ExploreEducationStatistics.Public.Data.Api.Services;
using GovUk.Education.ExploreEducationStatistics.Public.Data.Api.Services.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Public.Data.Services;
using GovUk.Education.ExploreEducationStatistics.Public.Data.Services.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Public.Data.Services.Options;
using MicroElements.Swashbuckle.FluentValidation.AspNetCore;
using Microsoft.AspNetCore.Rewrite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Npgsql;

namespace GovUk.Education.ExploreEducationStatistics.Public.Data.Api;

[ExcludeFromCodeCoverage]
public class Startup(IConfiguration configuration, IHostEnvironment hostEnvironment)
{
    private static readonly string[] ManagedIdentityTokenScopes = new [] { "https://ossrdbms-aad.database.windows.net/.default" };

    // This method gets called by the runtime. Use this method to add services to the container.
    public void ConfigureServices(IServiceCollection services)
    {
        // Health and telemetry

        services.AddHealthChecks();

        services
            .AddApplicationInsightsTelemetry()
            .AddApplicationInsightsTelemetryProcessor<SensitiveDataTelemetryProcessor>();

        // MVC

        services.AddMvcCore(options =>
        {
            options.Filters.Add(new OperationCancelledExceptionFilter());
            options.RespectBrowserAcceptHeader = true;
            options.ReturnHttpNotAcceptable = true;
            options.EnableEndpointRouting = false;
        });

        services
            .AddControllers(options =>
            {
                options.Filters.Add(new ProblemDetailsResultFilter());
                options.AddCommaSeparatedQueryModelBinderProvider();
                options.AddTrimStringBinderProvider();
            })
            .ConfigureApiBehaviorOptions(options =>
            {
                // Disables default model validation. Use FluentValidation instead.
                options.SuppressModelStateInvalidFilter = true;
            })
            .AddJsonOptions(options =>
            {
                options.JsonSerializerOptions.DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull;
            });

        services.AddProblemDetails();
        services.AddApiVersioning().AddMvc().AddApiExplorer();
        services.AddEndpointsApiExplorer();

        // Databases

        // Only set up the `PublicDataDbContext` in non-integration test
        // environments. Otherwise, the connection string will be null and
        // cause the data source builder to throw a host exception.
        if (!_hostEnvironment.IsIntegrationTest())
        {
            var connectionString = _configuration.GetConnectionString("PublicDataDb")!;

            if (_hostEnvironment.IsDevelopment())
            {
                var dataSourceBuilder = new NpgsqlDataSourceBuilder(
                    _configuration.GetConnectionString("PublicDataDb"));

                // Set up the data source outside the `AddDbContext` action as this
                // prevents `ManyServiceProvidersCreatedWarning` warnings due to EF
                // creating over 20 `IServiceProvider` instances.
                var dbDataSource = dataSourceBuilder.Build();

                services.AddDbContext<PublicDataDbContext>(options =>
                {
                    options
                        .UseNpgsql(dbDataSource)
                        .EnableSensitiveDataLogging();
                });
            }
            else
            {
                services.AddDbContext<PublicDataDbContext>(options =>
                {
                    var sqlServerTokenProvider = new DefaultAzureCredential();
                    var accessToken = sqlServerTokenProvider.GetToken(
                        new TokenRequestContext(scopes: ManagedIdentityTokenScopes)).Token;

                    var connectionStringWithAccessToken =
                        connectionString.Replace("[access_token]", accessToken);

                    var dbDataSource = new NpgsqlDataSourceBuilder(connectionStringWithAccessToken).Build();

                    options.UseNpgsql(dbDataSource);
                });
            }
        }

        // Caching and compression

        services.AddResponseCaching();
        services.AddResponseCompression(options =>
        {
            options.EnableForHttps = true;
        });

        // Options

        services.AddOptions<ContentApiOptions>()
            .Bind(configuration.GetRequiredSection(ContentApiOptions.Section));
        services.AddOptions<ParquetFilesOptions>()
            .Bind(configuration.GetRequiredSection(ParquetFilesOptions.Section));

        // Services

        services.AddFluentValidation();
        services.AddFluentValidationRulesToSwagger();

        services.AddHttpClient<IContentApiClient, ContentApiClient>((provider, httpClient) =>
        {
            var options = provider.GetRequiredService<IOptions<ContentApiOptions>>();
            httpClient.BaseAddress = new Uri(options.Value.Url);
            httpClient.DefaultRequestHeaders.Add(HeaderNames.UserAgent, "EES Public Data API");
        });

        services.AddSecurity();

        services.AddSingleton<IParquetPathResolver, ParquetPathResolver>();
        services.AddScoped<IPublicationService, PublicationService>();
        services.AddScoped<IDataSetService, DataSetService>();
    }

    // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
    public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
    {
        UpdateDatabase(app);

        if (env.IsDevelopment() || env.IsIntegrationTest())
        {
            app.UseDeveloperExceptionPage();
        }

        // Rewrites

        app.UseRewriter(new RewriteOptions
        {
            Rules = { new LowercasePathRule() }
        });

        // Caching and compression

        app.UseResponseCaching();
        app.UseResponseCompression();

        // Routing / endpoints

        app.UseRouting();
        app.UseEndpoints(builder =>
        {
            builder.MapControllers();
        });
        app.UseHealthChecks("/api/health");
    }

    private static void UpdateDatabase(IApplicationBuilder app)
    {
        using var serviceScope = app.ApplicationServices
            .GetRequiredService<IServiceScopeFactory>()
            .CreateScope();

        using var context = serviceScope.ServiceProvider.GetRequiredService<PublicDataDbContext>();

        context.Database.SetCommandTimeout(300);
        context.Database.Migrate();
    }
}
