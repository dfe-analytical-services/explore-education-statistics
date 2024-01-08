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
using GovUk.Education.ExploreEducationStatistics.Public.Data.Model.Database;
using Microsoft.AspNetCore.Rewrite;
using Microsoft.EntityFrameworkCore;
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

        services.AddProblemDetails();
        services.AddApiVersioning().AddMvc().AddApiExplorer();
        services.AddEndpointsApiExplorer();

        // Databases

        services.AddDbContext<PublicDataDbContext>(options =>
        {
            var dataSourceBuilder = new NpgsqlDataSourceBuilder(configuration.GetConnectionString("PublicDataDb"));
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

        // Services

        services.AddValidatorsFromAssemblyContaining<Startup>();
        services.AddFluentValidationAutoValidation();

        services.AddHttpClient("ContentApi", httpClient =>
        {
            var contentApiOptions = configuration
                .GetRequiredSection(ContentApiOptions.Section)
                .Get<ContentApiOptions>()!;

            httpClient.BaseAddress = new Uri(contentApiOptions.Url);
            httpClient.DefaultRequestHeaders.Add(HeaderNames.UserAgent, "EES Public Data API");
        });

        // TODO: Add more services
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
