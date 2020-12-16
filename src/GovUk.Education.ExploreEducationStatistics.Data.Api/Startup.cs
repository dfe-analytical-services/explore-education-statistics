﻿using System;
using System.Diagnostics.CodeAnalysis;
using AutoMapper;
using Azure.Storage.Blobs;
using GovUk.Education.ExploreEducationStatistics.Common.Model.Data;
using GovUk.Education.ExploreEducationStatistics.Common.Services;
using GovUk.Education.ExploreEducationStatistics.Common.Services.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Common.Services.Interfaces.Security;
using GovUk.Education.ExploreEducationStatistics.Common.Services.Security;
using GovUk.Education.ExploreEducationStatistics.Common.Utils;
using GovUk.Education.ExploreEducationStatistics.Data.Api.ModelBinding;
using GovUk.Education.ExploreEducationStatistics.Data.Api.Security;
using GovUk.Education.ExploreEducationStatistics.Data.Api.Services;
using GovUk.Education.ExploreEducationStatistics.Data.Api.Services.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Data.Model;
using GovUk.Education.ExploreEducationStatistics.Data.Model.Database;
using GovUk.Education.ExploreEducationStatistics.Data.Model.Services;
using GovUk.Education.ExploreEducationStatistics.Data.Model.Services.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Data.Services;
using GovUk.Education.ExploreEducationStatistics.Data.Services.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Data.Services.Security;
using GovUk.Education.ExploreEducationStatistics.Data.Services.Security.AuthorizationHandlers;
using GovUk.Education.ExploreEducationStatistics.Data.Services.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Rewrite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.OpenApi.Models;
using Newtonsoft.Json;

namespace GovUk.Education.ExploreEducationStatistics.Data.Api
{
    [ExcludeFromCodeCoverage]
    public class Startup
    {
        private IConfiguration Configuration { get; }
        private IHostEnvironment HostEnvironment { get; }

        public Startup(IConfiguration configuration, IHostEnvironment hostEnvironment)
        {
            Configuration = configuration;
            HostEnvironment = hostEnvironment;
        }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddApplicationInsightsTelemetry();
            services.AddMvc(options =>
            {
                options.RespectBrowserAcceptHeader = true;
                options.ReturnHttpNotAcceptable = true;
                options.Conventions.Add(new CommaSeparatedQueryStringConvention());
                options.EnableEndpointRouting = false;

            })
                .SetCompatibilityVersion(CompatibilityVersion.Version_3_0)
                .AddNewtonsoftJson(options => {
                    options.SerializerSettings.NullValueHandling = NullValueHandling.Ignore;
                });

            services.AddDbContext<StatisticsDbContext>(options =>
                options
                    .UseSqlServer(Configuration.GetConnectionString("StatisticsDb"),
                        builder => builder.MigrationsAssembly("GovUk.Education.ExploreEducationStatistics.Data.Model"))
                    .EnableSensitiveDataLogging(HostEnvironment.IsDevelopment())
            );

            // ReSharper disable once CommentTypo
            // Adds Brotli and Gzip compressing
            services.AddResponseCompression();

            // Register the Swagger generator, defining 1 or more Swagger documents
            services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1", new OpenApiInfo {Title = "Explore education statistics - Data API", Version = "v1"});
            });

            services.AddTransient<IResultBuilder<Observation, ObservationViewModel>, ResultBuilder>();
            services.AddTransient<IBoundaryLevelService, BoundaryLevelService>();
            services.AddTransient<ITableBuilderService, TableBuilderService>();
            services.AddTransient<IPublicationMetaService, PublicationMetaService>();
            services.AddTransient<IThemeMetaService, ThemeMetaService>();
            services.AddTransient<IResultSubjectMetaService, ResultSubjectMetaService>();
            services.AddTransient<ISubjectMetaService, SubjectMetaService>();
            services.AddSingleton<IBlobStorageService, BlobStorageService>(provider =>
                {
                    var connectionString = Configuration.GetValue<string>("PublicStorage");

                    return new BlobStorageService(
                        connectionString,
                        new BlobServiceClient(connectionString),
                        provider.GetRequiredService<ILogger<BlobStorageService>>()
                    );
                }
            );
            services.AddTransient<IFilterGroupService, FilterGroupService>();
            services.AddTransient<IFilterItemService, FilterItemService>();
            services.AddTransient<IFilterService, FilterService>();
            services.AddTransient<IFootnoteRepository, FootnoteRepository>();
            services.AddTransient<IGeoJsonService, GeoJsonService>();
            services.AddTransient<IIndicatorGroupService, IndicatorGroupService>();
            services.AddTransient<IIndicatorService, IndicatorService>();
            services.AddTransient<ILocationService, LocationService>();
            services.AddTransient<IObservationService, ObservationService>();
            services.AddTransient<IReleaseService, ReleaseService>();
            services.AddTransient<ISubjectService, SubjectService>();
            services.AddTransient<ITimePeriodService, TimePeriodService>();
            services.AddTransient<IPermalinkService, PermalinkService>();
            services.AddTransient<IPermalinkMigrationService, PermalinkMigrationService>();
            services.AddTransient<IFastTrackService, FastTrackService>();
            services.AddSingleton<DataServiceMemoryCache<BoundaryLevel>, DataServiceMemoryCache<BoundaryLevel>>();
            services.AddSingleton<DataServiceMemoryCache<GeoJson>, DataServiceMemoryCache<GeoJson>>();
            services.AddTransient<ITableStorageService, TableStorageService>(s => new TableStorageService(Configuration.GetValue<string>("PublicStorage")));
            services.AddTransient<IUserService, UserService>();

            services
                .AddAuthentication(options => {
                    options.DefaultAuthenticateScheme = "defaultScheme";
                    options.DefaultForbidScheme = "defaultScheme";
                    options.AddScheme<DefaultAuthenticationHandler>("defaultScheme", "Default Scheme");
                });

            services.AddAuthorization(options =>
            {
                // does this user have permission to view the subject data of a specific Release?
                options.AddPolicy(DataSecurityPolicies.CanViewSubjectData.ToString(), policy =>
                    policy.Requirements.Add(new ViewSubjectDataRequirement()));
            });

            services.AddTransient<IAuthorizationHandler, ViewSubjectDataForPublishedReleasesAuthorizationHandler>();

            services.AddCors();
            services.AddAutoMapper(AppDomain.CurrentDomain.GetAssemblies());

            AddPersistenceHelper<StatisticsDbContext>(services);
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

            if(env.IsDevelopment() || Configuration.GetValue<bool>("enableSwagger"))
            {
                app.UseSwagger();
                app.UseSwaggerUI(c =>
                {
                    c.SwaggerEndpoint("/swagger/v1/swagger.json", "Data API V1");
                    c.RoutePrefix = "docs";
                });

                var option = new RewriteOptions();
                option.AddRedirect("^$", "docs");
                app.UseRewriter(option);
            }

            // ReSharper disable once CommentTypo
            // Adds Brotli and Gzip compressing
            app.UseResponseCompression();

            app.UseCors(options => options.WithOrigins("http://localhost:3000","http://localhost:3001","https://localhost:3000","https://localhost:3001").AllowAnyMethod().AllowAnyHeader());
            app.UseMvc();
        }

        private static void AddPersistenceHelper<TDbContext>(IServiceCollection services)
            where TDbContext : DbContext
        {
            services.AddTransient<IPersistenceHelper<TDbContext>, PersistenceHelper<TDbContext>>(
                s =>
                {
                    var dbContext = s.GetService<TDbContext>();
                    return new PersistenceHelper<TDbContext>(dbContext);
                });
        }

        private static void UpdateDatabase(IApplicationBuilder app)
        {
            using (var serviceScope = app.ApplicationServices.GetRequiredService<IServiceScopeFactory>()
                .CreateScope())
            {
                using (var context = serviceScope.ServiceProvider.GetService<StatisticsDbContext>())
                {
                    context.Database.SetCommandTimeout(int.MaxValue);
                    context.Database.Migrate();
                }
            }
        }
    }
}