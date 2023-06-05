using System;
using System.Diagnostics.CodeAnalysis;
using System.Net.Http.Headers;
using System.Text;
using AutoMapper;
using Azure.Storage.Blobs;
using GovUk.Education.ExploreEducationStatistics.Common.Cache;
using GovUk.Education.ExploreEducationStatistics.Common.Model.Data;
using GovUk.Education.ExploreEducationStatistics.Common.ModelBinding;
using GovUk.Education.ExploreEducationStatistics.Common.Services;
using GovUk.Education.ExploreEducationStatistics.Common.Services.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Common.Services.Interfaces.Security;
using GovUk.Education.ExploreEducationStatistics.Common.Services.Security;
using GovUk.Education.ExploreEducationStatistics.Common.Cancellation;
using GovUk.Education.ExploreEducationStatistics.Common.Config;
using GovUk.Education.ExploreEducationStatistics.Common.Database;
using GovUk.Education.ExploreEducationStatistics.Common.Utils;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Database;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Repository;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Repository.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Services;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Services.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Content.Security;
using GovUk.Education.ExploreEducationStatistics.Content.Security.AuthorizationHandlers;
using GovUk.Education.ExploreEducationStatistics.Data.Api.Security;
using GovUk.Education.ExploreEducationStatistics.Data.Api.Services;
using GovUk.Education.ExploreEducationStatistics.Data.Api.Services.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Data.Model;
using GovUk.Education.ExploreEducationStatistics.Data.Model.Database;
using GovUk.Education.ExploreEducationStatistics.Data.Model.Repository;
using GovUk.Education.ExploreEducationStatistics.Data.Model.Repository.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Data.Services;
using GovUk.Education.ExploreEducationStatistics.Data.Services.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Data.Services.Security;
using GovUk.Education.ExploreEducationStatistics.Data.Services.Security.AuthorizationHandlers;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Hosting.Server.Features;
using Microsoft.AspNetCore.Rewrite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Net.Http.Headers;
using Microsoft.OpenApi.Models;
using Newtonsoft.Json;
using Thinktecture;
using static GovUk.Education.ExploreEducationStatistics.Common.Utils.StartupUtils;

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
            services.AddHealthChecks();

            services.AddApplicationInsightsTelemetry()
                .AddApplicationInsightsTelemetryProcessor<SensitiveDataTelemetryProcessor>();
            
            services.AddMvc(options =>
                {
                    options.Filters.Add(new OperationCancelledExceptionFilter());
                    options.RespectBrowserAcceptHeader = true;
                    options.ReturnHttpNotAcceptable = true;
                    options.EnableEndpointRouting = false;

                })
                .AddNewtonsoftJson(options => {
                    options.SerializerSettings.NullValueHandling = NullValueHandling.Ignore;
                });

            services.AddControllers(
                options =>
                {
                    options.ModelBinderProviders.Insert(0, new SeparatedQueryModelBinderProvider(","));
                }
            );

            services.AddDbContext<StatisticsDbContext>(options =>
                options
                    .UseSqlServer(Configuration.GetConnectionString("StatisticsDb"),
                        providerOptions =>
                            providerOptions
                                .MigrationsAssembly("GovUk.Education.ExploreEducationStatistics.Data.Model")
                                .AddBulkOperationSupport()
                                .EnableCustomRetryOnFailure()
                    )
                    .EnableSensitiveDataLogging(HostEnvironment.IsDevelopment())
            );

            services.AddDbContext<ContentDbContext>(options =>
                options
                    .UseSqlServer(Configuration.GetConnectionString("ContentDb"),
                        providerOptions => 
                            providerOptions
                                .MigrationsAssembly(typeof(Startup).Assembly.FullName)
                                .EnableCustomRetryOnFailure()
                    )
                    .EnableSensitiveDataLogging(HostEnvironment.IsDevelopment())
            );

            // Adds Brotli and Gzip compressing
            services.AddResponseCompression(options =>
            {
                options.EnableForHttps = true;
            });

            // Register the Swagger generator, defining 1 or more Swagger documents
            services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1", new OpenApiInfo {Title = "Explore education statistics - Data API", Version = "v1"});
            });

            //
            // Services
            //

            services.Configure<LocationsOptions>(Configuration.GetSection(LocationsOptions.Locations));
            services.Configure<TableBuilderOptions>(Configuration.GetSection(TableBuilderOptions.TableBuilder));

            services.AddSingleton<IPublicBlobStorageService, PublicBlobStorageService>();
            services.AddTransient<IBlobCacheService, BlobCacheService>(provider => new BlobCacheService(
                provider.GetRequiredService<IPublicBlobStorageService>(),
                provider.GetRequiredService<ILogger<BlobCacheService>>()
            ));
            services.AddTransient<IBoundaryLevelRepository, BoundaryLevelRepository>();
            services.AddTransient<ITableBuilderService, TableBuilderService>();
            services.AddTransient<IDataBlockService, DataBlockService>();
            services.AddTransient<IReleaseService, ReleaseService>();
            services.AddTransient<IReleaseSubjectService, ReleaseSubjectService>();
            services.AddTransient<ISubjectResultMetaService, SubjectResultMetaService>();
            services.AddTransient<ISubjectCsvMetaService, SubjectCsvMetaService>();
            services.AddTransient<ISubjectMetaService, SubjectMetaService>();
            services.AddTransient<IReleaseFileBlobService, PublicReleaseFileBlobService>();
            services.AddTransient<IFilterItemRepository, FilterItemRepository>();
            services.AddTransient<IFilterRepository, FilterRepository>();
            services.AddTransient<IFootnoteRepository, FootnoteRepository>();
            services.AddTransient<IFrontendService, FrontendService>();
            services.AddTransient<IGeoJsonRepository, GeoJsonRepository>();
            services.AddTransient<IIndicatorGroupRepository, IndicatorGroupRepository>();
            services.AddTransient<IIndicatorRepository, IndicatorRepository>();
            services.AddTransient<ILocationRepository, LocationRepository>();
            services.AddTransient<IObservationService, ObservationService>();
            services.AddTransient<IReleaseRepository, ReleaseRepository>();
            services.AddTransient<IReleaseDataFileRepository, ReleaseDataFileRepository>();
            services.AddTransient<IReleaseSubjectRepository, ReleaseSubjectRepository>();
            services.AddTransient<ISubjectRepository, SubjectRepository>();
            services.AddTransient<IDataGuidanceSubjectService, DataGuidanceSubjectService>();
            services.AddTransient<ITimePeriodService, TimePeriodService>();
            services.AddTransient<IPermalinkService, PermalinkService>();
            services.AddTransient<IPermalinkCsvMetaService, PermalinkCsvMetaService>();
            services.AddTransient<IPublicationRepository, PublicationRepository>();
            services.AddSingleton<DataServiceMemoryCache<BoundaryLevel>, DataServiceMemoryCache<BoundaryLevel>>();
            services.AddSingleton<DataServiceMemoryCache<GeoJson>, DataServiceMemoryCache<GeoJson>>();
            services.AddTransient<IUserService, UserService>();
            services.AddTransient<ICacheKeyService, CacheKeyService>();

            services
                .AddAuthentication(options => {
                    options.DefaultAuthenticateScheme = "defaultScheme";
                    options.DefaultForbidScheme = "defaultScheme";
                    options.AddScheme<DefaultAuthenticationHandler>("defaultScheme", "Default Scheme");
                });

            services.AddAuthorization(options =>
            {
                // does this use have permission to view a specific Release?
                options.AddPolicy(ContentSecurityPolicies.CanViewSpecificRelease.ToString(), policy =>
                    policy.Requirements.Add(new ViewReleaseRequirement()));

                // does this user have permission to view the subject data of a specific Release?
                options.AddPolicy(DataSecurityPolicies.CanViewSubjectData.ToString(), policy =>
                    policy.Requirements.Add(new ViewSubjectDataRequirement()));
            });

            services.AddHttpClient("PublicApp", httpClient =>
            {
                var publicAppOptions = new PublicAppOptions();
                Configuration.GetSection("PublicApp").Bind(publicAppOptions);

                httpClient.BaseAddress = new Uri(publicAppOptions.Url);

                if (publicAppOptions.BasicAuth)
                {
                    httpClient.DefaultRequestHeaders.Authorization =
                        new AuthenticationHeaderValue("Basic",
                            Convert.ToBase64String(Encoding.UTF8.GetBytes(
                                $"{publicAppOptions.BasicAuthUsername}:{publicAppOptions.BasicAuthPassword}")));
                }

                httpClient.DefaultRequestHeaders.Add(HeaderNames.UserAgent, "DataApi");
            });

            services.AddTransient<IAuthorizationHandler, ViewReleaseAuthorizationHandler>();
            services.AddTransient<IAuthorizationHandler, ViewSubjectDataForPublishedReleasesAuthorizationHandler>();

            services.AddCors();
            services.AddAutoMapper(AppDomain.CurrentDomain.GetAssemblies());

            AddPersistenceHelper<StatisticsDbContext>(services);
            AddPersistenceHelper<ContentDbContext>(services);
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            // Enable caching and register any caching services.
            CacheAspect.Enabled = true;
            BlobCacheAttribute.AddService("default", app.ApplicationServices.GetRequiredService<IBlobCacheService>());
            // Enable cancellation aspects and register request timeout configuration.
            CancellationTokenTimeoutAspect.Enabled = true;
            CancellationTokenTimeoutAttribute.SetTimeoutConfiguration(Configuration.GetSection("RequestTimeouts"));

            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                app.UseHttpsRedirection();
                app.UseHsts(hsts => hsts.MaxAge(365).IncludeSubdomains());
            }

            if(Configuration.GetValue<bool>("enableSwagger"))
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

            app.UseCors(options => options
                .WithOrigins(
                    "http://localhost:3000",
                    "http://localhost:3001",
                    "https://localhost:3000",
                    "https://localhost:3001")
                .AllowAnyMethod()
                .AllowAnyHeader());

            app.UseMvc();
            app.UseHealthChecks("/api/health");

            var serverAddressesFeature = app.ServerFeatures.Get<IServerAddressesFeature>();
            foreach (var address in serverAddressesFeature.Addresses)
            {
                Console.WriteLine($"Server listening on address: {address}");
            }
        }

        private record PublicAppOptions
        {
            public string Url { get; init; } = string.Empty;

            public bool BasicAuth { get; init; }

            public string BasicAuthUsername { get; init; } = string.Empty;

            public string BasicAuthPassword { get; init; } = string.Empty;
        }
    }
}
