using System.Diagnostics.CodeAnalysis;
using System.Text.Json.Serialization;
using AngleSharp.Io;
using Dapper;
using FluentValidation;
using GovUk.Education.ExploreEducationStatistics.Common.Cancellation;
using GovUk.Education.ExploreEducationStatistics.Common.Config;
using GovUk.Education.ExploreEducationStatistics.Common.Extensions;
using GovUk.Education.ExploreEducationStatistics.Common.ModelBinding;
using GovUk.Education.ExploreEducationStatistics.Common.Requests;
using GovUk.Education.ExploreEducationStatistics.Common.Rules;
using GovUk.Education.ExploreEducationStatistics.Public.Data.Api.Extensions;
using GovUk.Education.ExploreEducationStatistics.Public.Data.Api.Options;
using GovUk.Education.ExploreEducationStatistics.Public.Data.Api.Repository;
using GovUk.Education.ExploreEducationStatistics.Public.Data.Api.Repository.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Public.Data.Api.Requests;
using GovUk.Education.ExploreEducationStatistics.Public.Data.Api.Services;
using GovUk.Education.ExploreEducationStatistics.Public.Data.Api.Services.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Public.Data.Model.Database;
using GovUk.Education.ExploreEducationStatistics.Public.Data.Model.DuckDb;
using GovUk.Education.ExploreEducationStatistics.Public.Data.Services;
using GovUk.Education.ExploreEducationStatistics.Public.Data.Services.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Public.Data.Services.Options;
using MicroElements.Swashbuckle.FluentValidation.AspNetCore;
using Microsoft.AspNetCore.Mvc.ModelBinding.Metadata;
using Microsoft.AspNetCore.Rewrite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

namespace GovUk.Education.ExploreEducationStatistics.Public.Data.Api;

[ExcludeFromCodeCoverage]
public class Startup(IConfiguration configuration, IHostEnvironment hostEnvironment)
{
    private readonly IConfiguration _miniProfilerConfig =
        configuration.GetRequiredSection(MiniProfilerOptions.Section);

    // This method gets called by the runtime. Use this method to add services to the container.
    public void ConfigureServices(IServiceCollection services)
    {
        // Health and telemetry

        services.AddHealthChecks();

        services
            .AddApplicationInsightsTelemetry()
            .AddApplicationInsightsTelemetryProcessor<SensitiveDataTelemetryProcessor>();

        // Profiling

        var isMiniProfilerEnabled = _miniProfilerConfig.GetValue<bool>(nameof(MiniProfilerOptions.Enabled));

        if (isMiniProfilerEnabled)
        {
            services
                .AddMiniProfiler(options =>
                {
                    options.RouteBasePath = "/profiler";
                    options.ShouldProfile = request => request.Path.StartsWithSegments("/api");
                })
                .AddEntityFramework();
        }

        // MVC

        services
            .AddControllers(options =>
            {
                options.RespectBrowserAcceptHeader = true;
                options.ReturnHttpNotAcceptable = true;
                options.EnableEndpointRouting = false;

                options.Filters.Add<OperationCancelledExceptionFilter>();
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

        services.AddScoped<IDuckDbConnection>(_ => isMiniProfilerEnabled
            ? new ProfiledDuckDbConnection()
            : new DuckDbConnection()
        );

        // Caching and compression

        services.AddResponseCaching();
        services.AddResponseCompression(options =>
        {
            options.EnableForHttps = true;
        });

        // Options

        services.AddOptions<ContentApiOptions>()
            .Bind(configuration.GetRequiredSection(ContentApiOptions.Section));
        services.AddOptions<DataFilesOptions>()
            .Bind(configuration.GetRequiredSection(DataFilesOptions.Section));
        services.AddOptions<MiniProfilerOptions>()
            .Bind(_miniProfilerConfig);

        // Services

        services.AddFluentValidation();
        services.AddValidatorsFromAssembly(typeof(DataSetGetQueryLocations.Validator).Assembly); // Adds *all* validators from Public.Data.Requests
        services.AddFluentValidationRulesToSwagger();

        services.AddHttpClient<IContentApiClient, ContentApiClient>((provider, httpClient) =>
        {
            var options = provider.GetRequiredService<IOptions<ContentApiOptions>>();
            httpClient.BaseAddress = new Uri(options.Value.Url);
            httpClient.DefaultRequestHeaders.Add(HeaderNames.UserAgent, "EES Public Data API");
        });

        services.AddSecurity();

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
    }

    // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
    public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
    {
        UpdateDatabase(app);

        if (_miniProfilerConfig.GetValue<bool>(nameof(MiniProfilerOptions.Enabled)))
        {
            app.UseMiniProfiler();
        }

        if (env.IsDevelopment() || env.IsIntegrationTest())
        {
            app.UseDeveloperExceptionPage();
        }

        // Rewrites

        app.UseRewriter(new RewriteOptions {Rules = {new LowercasePathRule()}});

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
