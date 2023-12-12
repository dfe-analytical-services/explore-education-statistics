using System.Diagnostics.CodeAnalysis;
using System.Text.Json.Serialization;
using AngleSharp.Io;
using FluentValidation;
using GovUk.Education.ExploreEducationStatistics.Common.Cancellation;
using GovUk.Education.ExploreEducationStatistics.Common.Config;
using GovUk.Education.ExploreEducationStatistics.Common.Extensions;
using GovUk.Education.ExploreEducationStatistics.Common.Model.Data;
using GovUk.Education.ExploreEducationStatistics.Common.Rules;
using GovUk.Education.ExploreEducationStatistics.Public.Data.Api.Options;
using GovUk.Education.ExploreEducationStatistics.Public.Data.Api.Services;
using GovUk.Education.ExploreEducationStatistics.Public.Data.Api.Services.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Public.Data.Model.Database;
using Microsoft.AspNetCore.Hosting.Server.Features;
using Microsoft.AspNetCore.Rewrite;
using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi.Models;
using Npgsql;
using SharpGrip.FluentValidation.AutoValidation.Mvc.Extensions;

namespace GovUk.Education.ExploreEducationStatistics.Public.Data.Api;

[ExcludeFromCodeCoverage]
public class Startup(IConfiguration configuration, IHostEnvironment hostEnvironment)
{
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

        services.ConfigureHttpJsonOptions(options =>
        {
            options.SerializerOptions.DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull;
        });

        services.AddControllers(options =>
        {
            options.AddCommaSeparatedQueryModelBinderProvider();
            options.AddTrimStringBinderProvider();
        });

        // Databases

        services.AddDbContext<PublicDataDbContext>(options =>
        {
            var dataSourceBuilder = new NpgsqlDataSourceBuilder(configuration.GetConnectionString("PublicDataDbContext"));
            dataSourceBuilder.MapEnum<GeographicLevel>();

            options
                .UseNpgsql(dataSourceBuilder.Build())
                .EnableSensitiveDataLogging(hostEnvironment.IsDevelopment());
        });

        // Caching and compression

        services.AddResponseCaching();
        services.AddResponseCompression(options =>
        {
            options.EnableForHttps = true;
        });

        // Docs

        services.AddEndpointsApiExplorer();
        services.AddSwaggerGen(options =>
        {
            options.UseAllOfForInheritance();
            options.UseOneOfForPolymorphism();

            options.SwaggerDoc("v1", new OpenApiInfo
            {
                Title = "Explore education statistics - Public Data API",
                Version = "v1",
                Contact = new OpenApiContact
                {
                    Name = "Explore education statistics",
                    Email = "explore.statistics@education.gov.uk",
                    Url = new Uri("https://explore-education-statistics.service.gov.uk")
                },
            });
        });

        // Services

        services.AddValidatorsFromAssemblyContaining<Startup>();
        services.AddFluentValidationAutoValidation();

        services.AddHttpClient(ContentApiOptions.Section, httpClient =>
        {
            var contentApiOptions = configuration
                .GetRequiredSection(ContentApiOptions.Section)
                .Get<ContentApiOptions>()!;

            httpClient.BaseAddress = new Uri(contentApiOptions.Url);
            httpClient.DefaultRequestHeaders.Add(HeaderNames.UserAgent, "EES Public Data API");
        });

        services.AddScoped<IContentApiClient, ContentApiClient>(sp =>
        {
            var httpClientFactory = sp.GetService<IHttpClientFactory>();
            var httpClient = httpClientFactory!.CreateClient(ContentApiOptions.Section);

            return new ContentApiClient(httpClient);
        });

        services.AddScoped<IPublicationService, PublicationService>();
    }

    // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
    public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
    {
        UpdateDatabase(app);

        if (env.IsDevelopment())
        {
            app.UseDeveloperExceptionPage();
        }
        else
        {
            app.UseHttpsRedirection();
            app.UseHsts(hsts => hsts.MaxAge(365).IncludeSubdomains());
        }

        // Swagger

        app.UseSwagger(options =>
        {
            options.RouteTemplate = "/docs/{documentName}/openapi.json";
        });
        app.UseSwaggerUI(options =>
        {
            options.RoutePrefix = "docs";
            options.SwaggerEndpoint("/docs/v1/openapi.json", "Public Data API v1");
        });

        // Rewrites

        app.UseRewriter(new RewriteOptions
        {
            Rules = { new LowercasePathRule() }
        });

        // Caching and compression

        app.UseResponseCaching();
        app.UseResponseCompression();

        // MVC

        app.UseMvc();
        app.UseHealthChecks("/api/health");

        app.ServerFeatures.Get<IServerAddressesFeature>()
            ?.Addresses
            .ForEach(address => Console.WriteLine($"Server listening on address: {address}"));
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
