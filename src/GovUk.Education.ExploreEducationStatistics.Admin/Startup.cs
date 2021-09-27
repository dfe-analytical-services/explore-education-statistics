#nullable enable
using System;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using AutoMapper;
using Azure.Storage.Blobs;
using GovUk.Education.ExploreEducationStatistics.Admin.Areas.Identity.Data;
using GovUk.Education.ExploreEducationStatistics.Admin.Mappings;
using GovUk.Education.ExploreEducationStatistics.Admin.Mappings.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Admin.Migrations.Custom;
using GovUk.Education.ExploreEducationStatistics.Admin.Models;
using GovUk.Education.ExploreEducationStatistics.Admin.Security;
using GovUk.Education.ExploreEducationStatistics.Admin.Services;
using GovUk.Education.ExploreEducationStatistics.Admin.Services.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Admin.Services.Interfaces.ManageContent;
using GovUk.Education.ExploreEducationStatistics.Admin.Services.Interfaces.Methodologies;
using GovUk.Education.ExploreEducationStatistics.Admin.Services.ManageContent;
using GovUk.Education.ExploreEducationStatistics.Admin.Services.Methodologies;
using GovUk.Education.ExploreEducationStatistics.Common.Model.Data;
using GovUk.Education.ExploreEducationStatistics.Common.ModelBinding;
using GovUk.Education.ExploreEducationStatistics.Common.Services;
using GovUk.Education.ExploreEducationStatistics.Common.Services.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Common.Services.Interfaces.Security;
using GovUk.Education.ExploreEducationStatistics.Common.Utils;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Database;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Repository;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Repository.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Data.Model;
using GovUk.Education.ExploreEducationStatistics.Data.Model.Database;
using GovUk.Education.ExploreEducationStatistics.Data.Model.Repository;
using GovUk.Education.ExploreEducationStatistics.Data.Model.Repository.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Data.Services;
using GovUk.Education.ExploreEducationStatistics.Data.Services.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Data.Services.ViewModels;
using IdentityServer4.Services;
using Microsoft.AspNetCore.ApiAuthorization.IdentityServer;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Authorization;
using Microsoft.AspNetCore.Rewrite;
using Microsoft.AspNetCore.Routing;
using Microsoft.AspNetCore.SpaServices.ReactDevelopmentServer;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using Newtonsoft.Json;
using Notify.Client;
using Notify.Interfaces;
using static GovUk.Education.ExploreEducationStatistics.Common.Utils.StartupUtils;
using FootnoteService = GovUk.Education.ExploreEducationStatistics.Admin.Services.FootnoteService;
using IFootnoteService = GovUk.Education.ExploreEducationStatistics.Admin.Services.Interfaces.IFootnoteService;
using IPublicationService = GovUk.Education.ExploreEducationStatistics.Admin.Services.Interfaces.IPublicationService;
using IReleaseRepository = GovUk.Education.ExploreEducationStatistics.Admin.Services.Interfaces.IReleaseRepository;
using IReleaseService = GovUk.Education.ExploreEducationStatistics.Admin.Services.Interfaces.IReleaseService;
using IThemeService = GovUk.Education.ExploreEducationStatistics.Admin.Services.Interfaces.IThemeService;
using PublicationService = GovUk.Education.ExploreEducationStatistics.Admin.Services.PublicationService;
using ReleaseRepository = GovUk.Education.ExploreEducationStatistics.Admin.Services.ReleaseRepository;
using ReleaseService = GovUk.Education.ExploreEducationStatistics.Admin.Services.ReleaseService;
using ThemeService = GovUk.Education.ExploreEducationStatistics.Admin.Services.ThemeService;

namespace GovUk.Education.ExploreEducationStatistics.Admin
{
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

            services.Configure<CookiePolicyOptions>(options =>
            {
                // This lambda determines whether user consent for non-essential cookies is needed for a given request.
                options.CheckConsentNeeded = context => true;
                options.MinimumSameSitePolicy = SameSiteMode.None;
                options.Secure = CookieSecurePolicy.Always;
            });

            services.AddControllers(
                options =>
                {
                    options.ModelBinderProviders.Insert(0, new SeparatedQueryModelBinderProvider(","));
                }
            );

            services.AddDbContext<UsersAndRolesDbContext>(options =>
                options
                    .UseSqlServer(Configuration.GetConnectionString("ContentDb"),
                        builder => builder.MigrationsAssembly(typeof(Startup).Assembly.FullName))
                    .EnableSensitiveDataLogging(HostEnvironment.IsDevelopment())
            );

            services.AddDbContext<ContentDbContext>(options =>
                options
                    .UseSqlServer(Configuration.GetConnectionString("ContentDb"),
                        builder => builder.MigrationsAssembly(typeof(Startup).Assembly.FullName))
                    .EnableSensitiveDataLogging(HostEnvironment.IsDevelopment())
            );

            services.AddDbContext<StatisticsDbContext>(options =>
                options
                    .UseSqlServer(Configuration.GetConnectionString("StatisticsDb"),
                        builder => builder.MigrationsAssembly("GovUk.Education.ExploreEducationStatistics.Data.Model"))
                    .EnableSensitiveDataLogging(HostEnvironment.IsDevelopment())
            );

            // remove default Microsoft remapping of the name of the OpenID "roles" claim mapping
            JwtSecurityTokenHandler.DefaultInboundClaimTypeMap.Remove("roles");
            JwtSecurityTokenHandler.DefaultInboundClaimTypeMap.Remove("role");

            services
                .AddDefaultIdentity<ApplicationUser>()
                .AddRoles<IdentityRole>()
                .AddEntityFrameworkStores<UsersAndRolesDbContext>()
                .AddDefaultTokenProviders();

            if (HostEnvironment.IsDevelopment())
            {
                services
                    .AddIdentityServer(options =>
                    {
                        options.Events.RaiseErrorEvents = true;
                        options.Events.RaiseInformationEvents = true;
                        options.Events.RaiseFailureEvents = true;
                        options.Events.RaiseSuccessEvents = true;
                    })
                    .AddApiAuthorization<ApplicationUser, UsersAndRolesDbContext>()
                    .AddProfileService<ApplicationUserProfileService>()
                    .AddDeveloperSigningCredential();
            }
            else
            {
                services
                    .AddIdentityServer(options =>
                    {
                        options.Events.RaiseErrorEvents = true;
                        options.Events.RaiseInformationEvents = true;
                        options.Events.RaiseFailureEvents = true;
                        options.Events.RaiseSuccessEvents = true;
                    })
                    .AddApiAuthorization<ApplicationUser, UsersAndRolesDbContext>()
                    .AddProfileService<ApplicationUserProfileService>()
                    // TODO DW - this should be conditional based upon whether or not we're in dev mode
                    .AddSigningCredentials();

                services.Configure<JwtBearerOptions>(
                    IdentityServerJwtConstants.IdentityServerJwtBearerScheme,
                    options =>
                    {
                        options.TokenValidationParameters = new TokenValidationParameters
                        {
                            ValidateIssuerSigningKey = false,
                            ValidateIssuer = false,
                        };
                    });
            }

            services
                .AddAuthentication()
                .AddOpenIdConnect(options => Configuration.GetSection("OpenIdConnect").Bind(options))
                .AddIdentityServerJwt();

            // This configuration has to occur after the AddAuthentication() block as it is otherwise overridden.
            services.Configure<IdentityOptions>(options =>
            {
                // This config tells UserManager to expect the logged in user's id to be in a Claim called
                // "http://schemas.xmlsoap.org/ws/2005/05/identity/claims/nameidentifier" rather than "sub" (because
                // this Claim is renamed via the DefaultInboundClaimTypeMap earlier in the login process).
                //
                // It doesn't seem to be possible to remove the renaming (via
                // JwtSecurityTokenHandler.DefaultInboundClaimTypeMap.Remove("sub")) because some mechanism earlier in the
                // authentication process requires it to be in the Claim named
                // "http://schemas.xmlsoap.org/ws/2005/05/identity/claims/nameidentifier" rather than "sub".
                options.ClaimsIdentity.UserIdClaimType = ClaimTypes.NameIdentifier;

                // Default User settings
                options.User.AllowedUserNameCharacters =
                    "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789-._@+'";
            });

            services.Configure<PreReleaseOptions>(Configuration);

            // here we configure our security policies
            StartupSecurityConfiguration.ConfigureAuthorizationPolicies(services);

            services.Configure<RouteOptions>(options => options.LowercaseUrls = true);

            services.AddAutoMapper(AppDomain.CurrentDomain.GetAssemblies());
            services.AddTransient<IMyReleasePermissionSetPropertyResolver,
                MyReleasePermissionSetPropertyResolver>();
            services.AddTransient<IMyPublicationPermissionSetPropertyResolver,
                MyPublicationPermissionSetPropertyResolver>();
            services.AddTransient<IMyPublicationMethodologyPermissionsPropertyResolver,
                MyPublicationMethodologyPermissionsPropertyResolver>();
            services.AddTransient<IMyMethodologyPermissionSetPropertyResolver,
                MyMethodologyPermissionSetPropertyResolver>();

            services.AddMvc(options =>
                {
                    options.Filters.Add(new AuthorizeFilter(SecurityPolicies.CanAccessSystem.ToString()));
                    options.EnableEndpointRouting = false;
                    options.AllowEmptyInputInBodyModelBinding = true;
                })
                .SetCompatibilityVersion(CompatibilityVersion.Version_3_0)
                .AddNewtonsoftJson(options =>
                {
                    options.SerializerSettings.NullValueHandling = NullValueHandling.Ignore;
                });

            // In production, the React files will be served from this directory
            services.AddSpaStaticFiles(configuration => { configuration.RootPath = "wwwroot"; });

            services.AddTransient<IFileRepository, FileRepository>();
            services.AddTransient<IDataImportRepository, DataImportRepository>();
            services.AddTransient<IReleaseFileRepository, ReleaseFileRepository>();
            services.AddTransient<IReleaseDataFileRepository, ReleaseDataFileRepository>();

            services.AddTransient<IReleaseDataFileService, ReleaseDataFileService>();
            services.AddTransient<IReleaseFileService, ReleaseFileService>();
            services.AddTransient<IReleaseImageService, ReleaseImageService>();
            services.AddTransient<IDataImportService, DataImportService>();
            services.AddTransient<IImportStatusBauService, ImportStatusBauService>();

            services.AddTransient<IPublishingService, PublishingService>(provider =>
                new PublishingService(
                    provider.GetService<IPersistenceHelper<ContentDbContext>>(),
                    new StorageQueueService(Configuration.GetValue<string>("PublisherStorage")),
                    provider.GetService<IUserService>(),
                    provider.GetRequiredService<ILogger<PublishingService>>()));
            services.AddTransient<IReleasePublishingStatusService, ReleasePublishingStatusService>(s =>
                new ReleasePublishingStatusService(
                    s.GetService<IMapper>(),
                    s.GetService<IUserService>(),
                    s.GetService<IPersistenceHelper<ContentDbContext>>(),
                    new TableStorageService(Configuration.GetValue<string>("PublisherStorage"))));
            services.AddTransient<IReleasePublishingStatusRepository, ReleasePublishingStatusRepository>(s =>
                new ReleasePublishingStatusRepository(
                    new TableStorageService(Configuration.GetValue<string>("PublisherStorage"))
                )
            );
            services.AddTransient<IThemeService, ThemeService>();
            services.AddTransient<ITopicService, TopicService>();
            services.AddTransient<IPublicationService, PublicationService>();
            services.AddTransient<IPublicationRepository, PublicationRepository>();
            services.AddTransient<IMetaService, MetaService>();
            services.AddTransient<ILegacyReleaseService, LegacyReleaseService>();
            services.AddTransient<IReleaseService, ReleaseService>();
            services.AddTransient<ReleaseSubjectRepository.SubjectDeleter, ReleaseSubjectRepository.SubjectDeleter>();
            services.AddTransient<IReleaseSubjectRepository, ReleaseSubjectRepository>();
            services.AddTransient<IReleaseChecklistService, ReleaseChecklistService>();
            services.AddTransient<IReleaseRepository, ReleaseRepository>();
            services.AddTransient<IMethodologyService, MethodologyService>();
            services.AddTransient<IMethodologyNoteService, MethodologyNoteService>();
            services.AddTransient<IMethodologyNoteRepository, MethodologyNoteRepository>();
            services.AddTransient<IMethodologyVersionRepository, MethodologyVersionRepository>();
            services.AddTransient<IMethodologyRepository, MethodologyRepository>();
            services.AddTransient<IMethodologyContentService, MethodologyContentService>();
            services.AddTransient<IMethodologyFileRepository, MethodologyFileRepository>();
            services.AddTransient<IMethodologyImageService, MethodologyImageService>();
            services.AddTransient<IMethodologyAmendmentService, MethodologyAmendmentService>();
            services.AddTransient<IDataBlockService, DataBlockService>();
            services.AddTransient<IPreReleaseUserService, PreReleaseUserService>();
            services.AddTransient<IPreReleaseService, PreReleaseService>();
            services.AddTransient<IPreReleaseSummaryService, PreReleaseSummaryService>();

            services.AddTransient<IManageContentPageService, ManageContentPageService>();
            services.AddTransient<IContentService, ContentService>();
            services.AddTransient<ICommentService, CommentService>();
            services.AddTransient<IRelatedInformationService, RelatedInformationService>();
            services.AddTransient<IReplacementService, ReplacementService>();
            services.AddTransient<IUserRoleService, UserRoleService>();
            services.AddTransient<IUserPublicationRoleRepository, UserPublicationRoleRepository>();
            services.AddTransient<IUserReleaseRoleRepository, UserReleaseRoleRepository>();

            services.AddTransient<INotificationClient>(s =>
            {
                var notifyApiKey = Configuration.GetValue<string>("NotifyApiKey");

                if (!HostEnvironment.IsDevelopment())
                {
                    return new NotificationClient(notifyApiKey);
                }

                if (notifyApiKey != null && notifyApiKey != "change-me")
                {
                    return new NotificationClient(notifyApiKey);
                }

                var logger = s.GetRequiredService<ILogger<LoggingNotificationClient>>();
                return new LoggingNotificationClient(logger);
            });
            services.AddTransient<IEmailService, EmailService>();

            services.AddTransient<IBoundaryLevelRepository, BoundaryLevelRepository>();
            services.AddTransient<IBlobCacheService, BlobCacheService>();
            services.AddTransient<IEmailTemplateService, EmailTemplateService>();
            services.AddTransient<ITableBuilderService, TableBuilderService>();
            services.AddTransient<IFilterRepository, FilterRepository>();
            services.AddTransient<IFilterItemRepository, FilterItemRepository>();
            services.AddTransient<IFootnoteService, FootnoteService>();
            services.AddTransient<IFootnoteRepository, FootnoteRepository>();
            services.AddTransient<IGeoJsonRepository, GeoJsonRepository>();
            services.AddTransient<IIndicatorGroupRepository, IndicatorGroupRepository>();
            services.AddTransient<IIndicatorRepository, IndicatorRepository>();
            services.AddTransient<ILocationRepository, LocationRepository>();
            services.AddTransient<IMetaGuidanceService, MetaGuidanceService>();
            services.AddTransient<IMetaGuidanceSubjectService, MetaGuidanceSubjectService>();
            services.AddTransient<IObservationService, ObservationService>();
            services.AddTransient<Data.Services.Interfaces.IReleaseService, Data.Services.ReleaseService>();
            // TODO: EES-2343 Remove when file sizes are stored in database
            services.AddTransient<Data.Services.Interfaces.IReleaseService.IBlobInfoGetter, ReleaseBlobInfoGetter>();
            services.AddTransient<IReleaseContentBlockRepository, ReleaseContentBlockRepository>();
            services.AddTransient<IReleaseContentSectionRepository, ReleaseContentSectionRepository>();
            services.AddTransient<IReleaseNoteService, ReleaseNoteService>();
            services.AddTransient<IResultBuilder<Observation, ObservationViewModel>, ResultBuilder>();
            services.AddTransient<Data.Model.Repository.Interfaces.IReleaseRepository, Data.Model.Repository.ReleaseRepository>();
            services.AddTransient<ISubjectRepository, SubjectRepository>();
            services.AddTransient<ITimePeriodService, TimePeriodService>();
            services.AddTransient<ISubjectMetaService, SubjectMetaService>();
            services.AddTransient<IResultSubjectMetaService, ResultSubjectMetaService>();
            services.AddSingleton<DataServiceMemoryCache<BoundaryLevel>, DataServiceMemoryCache<BoundaryLevel>>();
            services.AddSingleton<DataServiceMemoryCache<GeoJson>, DataServiceMemoryCache<GeoJson>>();
            services.AddTransient<IUserManagementService, UserManagementService>();
            services.AddTransient<IFileUploadsValidatorService, FileUploadsValidatorService>();
            services.AddTransient<IBlobStorageService, BlobStorageService>(provider =>
                {
                    var connectionString = Configuration.GetValue<string>("CoreStorage");

                    return new BlobStorageService(
                        connectionString,
                        new BlobServiceClient(connectionString),
                        provider.GetRequiredService<ILogger<BlobStorageService>>()
                    );
                }
            );
            services.AddTransient<ITableStorageService, TableStorageService>(s =>
                new TableStorageService(Configuration.GetValue<string>("CoreStorage")));
            services.AddTransient<IStorageQueueService, StorageQueueService>(s =>
                new StorageQueueService(Configuration.GetValue<string>("CoreStorage")));
            services.AddSingleton<IGuidGenerator, SequentialGuidGenerator>();
            AddPersistenceHelper<ContentDbContext>(services);
            AddPersistenceHelper<StatisticsDbContext>(services);
            AddPersistenceHelper<UsersAndRolesDbContext>(services);

            // This service handles the generation of the JWTs for users after they log in
            services.AddTransient<IProfileService, ApplicationUserProfileService>();

            // This service allows a set of users to be pre-invited to the service on startup.
            if (HostEnvironment.IsDevelopment())
            {
                services.AddTransient<BootstrapUsersService>();
            }

            // These services allow us to check our Policies within Controllers and Services
            StartupSecurityConfiguration.ConfigureResourceBasedAuthorization(services);

            services.AddSingleton<IFileTypeService, FileTypeService>();
            services.AddTransient<IDataArchiveValidationService, DataArchiveValidationService>();

            services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1",
                    new OpenApiInfo {Title = "Explore education statistics - Admin API", Version = "v1"});
                c.CustomSchemaIds((type) => type.FullName);
                c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
                {
                    Description = "Please enter into field the word 'Bearer' followed by a space and the JWT contents",
                    Name = "Authorization",
                    In = ParameterLocation.Header,
                    Type = SecuritySchemeType.ApiKey
                });
                c.AddSecurityRequirement(new OpenApiSecurityRequirement
                {
                    {
                        new OpenApiSecurityScheme
                        {
                            Reference = new OpenApiReference { Type = ReferenceType.SecurityScheme, Id = "Bearer" }
                        },
                        new[] {string.Empty}
                    }
                });
            });
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            UpdateDatabase(app, env);

            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                app.UseExceptionHandler("/Error");
                app.UseHsts(opts =>
                {
                    opts.MaxAge(365);
                    opts.IncludeSubdomains();
                    opts.Preload();
                });
            }

            if(Configuration.GetValue<bool>("enableSwagger"))
            {
                app.UseSwagger();
                app.UseSwaggerUI(c =>
                {
                    c.SwaggerEndpoint("/swagger/v1/swagger.json", "Admin API V1");
                    c.RoutePrefix = "docs";
                });
            }

            // Security Headers
            app.UseXContentTypeOptions();
            app.UseXXssProtection(opts => opts.EnabledWithBlockMode());
            app.UseXfo(opts => opts.SameOrigin());
            app.UseReferrerPolicy(opts => opts.NoReferrerWhenDowngrade());
            app.UseCsp(opts => opts
                .BlockAllMixedContent()
                .StyleSources(s => s.Self())
                .StyleSources(s => s
                    .CustomSources(" https://cdnjs.cloudflare.com")
                    .UnsafeInline())
                .FontSources(s => s.Self())
                .FormActions(s =>
                {
                    var loginAuthorityUrl = Configuration.GetSection("OpenIdConnect").GetValue<string>("Authority");
                    var loginAuthorityUri = new Uri(loginAuthorityUrl);
                    s
                        .CustomSources(loginAuthorityUri.GetLeftPart(UriPartial.Authority))
                        .Self();
                })
                .FrameAncestors(s => s.Self())
                .ImageSources(s => s.Self())
                .ImageSources(s => s.CustomSources("data:"))
                .ScriptSources(s => s.Self())
                .ScriptSources(s => s.UnsafeInline())
            );

            app.UseHttpsRedirection();
            app.UseStaticFiles();
            app.UseSpaStaticFiles();
            app.UseCookiePolicy();

            app.UseAuthentication();
            app.UseIdentityServer();
            app.UseAuthorization();

            // deny access to all Identity routes other than /Identity/Account/Login and
            // /Identity/Account/ExternalLogin
            var options = new RewriteOptions()
                .AddRewrite(@"^(?i)identity/(?!account/(?:external)*login$)", "/", skipRemainingRules: true);
            app.UseRewriter(options);

            app.UseMvc(routes =>
            {
                routes.MapRoute(
                    name: "Areas",
                    template: "{area:exists}/{controller=Home}/{action=Index}/{id?}");

                routes.MapRoute(
                    name: "default",
                    template: "{controller}/{action=Index}/{id?}");
            });

            app.UseSpa(spa =>
            {
                if (env.IsDevelopment())
                {
                    spa.Options.SourcePath = "../explore-education-statistics-admin";
                    spa.UseReactDevelopmentServer("start");
                }
            });
        }

        private void UpdateDatabase(IApplicationBuilder app, IWebHostEnvironment env)
        {
            using (var serviceScope = app.ApplicationServices.GetRequiredService<IServiceScopeFactory>()
                .CreateScope())
            {
                using (var context = serviceScope.ServiceProvider.GetService<StatisticsDbContext>())
                {
                    context.Database.SetCommandTimeout(int.MaxValue);
                    context.Database.Migrate();
                }

                using (var context = serviceScope.ServiceProvider.GetService<UsersAndRolesDbContext>())
                {
                    context.Database.SetCommandTimeout(int.MaxValue);
                    context.Database.Migrate();
                }

                using (var context = serviceScope.ServiceProvider.GetService<ContentDbContext>())
                {
                    context.Database.SetCommandTimeout(int.MaxValue);
                    context.Database.Migrate();

                    ApplyCustomMigrations();
                }
            }

            if (env.IsDevelopment())
            {
                using var serviceScope = app.ApplicationServices.GetRequiredService<IServiceScopeFactory>()
                    .CreateScope();

                serviceScope.ServiceProvider
                    .GetService<BootstrapUsersService>()
                    .AddBootstrapUsers();
            }
        }

        private static void ApplyCustomMigrations(params ICustomMigration[] migrations)
        {
            foreach (var migration in migrations)
            {
                migration.Apply();
            }
        }
    }
}
