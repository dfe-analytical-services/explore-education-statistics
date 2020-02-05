using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using AutoMapper;
using GovUk.Education.ExploreEducationStatistics.Admin.Areas.Identity.Data;
using GovUk.Education.ExploreEducationStatistics.Admin.Controllers.Utils;
using GovUk.Education.ExploreEducationStatistics.Admin.Mappings;
using GovUk.Education.ExploreEducationStatistics.Admin.Mappings.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Admin.Models;
using GovUk.Education.ExploreEducationStatistics.Admin.Security;
using GovUk.Education.ExploreEducationStatistics.Admin.Security.AuthorizationHandlers;
using GovUk.Education.ExploreEducationStatistics.Admin.Services;
using GovUk.Education.ExploreEducationStatistics.Admin.Services.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Admin.Services.Interfaces.ManageContent;
using GovUk.Education.ExploreEducationStatistics.Admin.Services.Interfaces.Security;
using GovUk.Education.ExploreEducationStatistics.Admin.Services.ManageContent;
using GovUk.Education.ExploreEducationStatistics.Admin.Services.Security;
using GovUk.Education.ExploreEducationStatistics.Common.Model;
using GovUk.Education.ExploreEducationStatistics.Common.Model.Data;
using GovUk.Education.ExploreEducationStatistics.Common.Services;
using GovUk.Education.ExploreEducationStatistics.Common.Services.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Database;
using GovUk.Education.ExploreEducationStatistics.Data.Model;
using GovUk.Education.ExploreEducationStatistics.Data.Model.Database;
using GovUk.Education.ExploreEducationStatistics.Data.Model.Services;
using GovUk.Education.ExploreEducationStatistics.Data.Model.Services.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Data.Services;
using GovUk.Education.ExploreEducationStatistics.Data.Services.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Data.Services.ViewModels;
using IdentityServer4.Services;
using Microsoft.AspNetCore.ApiAuthorization.IdentityServer;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
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
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using Newtonsoft.Json;
using Notify.Client;
using Notify.Interfaces;
using FootnoteService = GovUk.Education.ExploreEducationStatistics.Admin.Services.FootnoteService;
using IFootnoteService = GovUk.Education.ExploreEducationStatistics.Admin.Services.Interfaces.IFootnoteService;
using IHostingEnvironment = Microsoft.Extensions.Hosting.IHostingEnvironment;
using IReleaseService = GovUk.Education.ExploreEducationStatistics.Admin.Services.Interfaces.IReleaseService;
using ReleaseService = GovUk.Education.ExploreEducationStatistics.Admin.Services.ReleaseService;

namespace GovUk.Education.ExploreEducationStatistics.Admin
{
    public class Startup
    {
        public Startup(IConfiguration configuration, IHostingEnvironment hostingEnvironment)
        {
            Configuration = configuration;
            HostingEnvironment = hostingEnvironment;
        }

        public IConfiguration Configuration { get; }
        public IHostingEnvironment HostingEnvironment { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.Configure<CookiePolicyOptions>(options =>
            {
                // This lambda determines whether user consent for non-essential cookies is needed for a given request.
                options.CheckConsentNeeded = context => true;
                options.MinimumSameSitePolicy = SameSiteMode.None;
                options.Secure = CookieSecurePolicy.Always;
            });

            services.AddDbContext<UsersAndRolesDbContext>(options =>
                options
                    .UseSqlServer(Configuration.GetConnectionString("ContentDb"),
                        builder => builder.MigrationsAssembly(typeof(Startup).Assembly.FullName))
                    .EnableSensitiveDataLogging()
            );

            services.AddDbContext<ContentDbContext>(options =>
                options
                    .UseSqlServer(Configuration.GetConnectionString("ContentDb"),
                        builder => builder.MigrationsAssembly(typeof(Startup).Assembly.FullName))
                    .EnableSensitiveDataLogging()
            );

            services.AddDbContext<StatisticsDbContext>(options =>
                options
                    .UseSqlServer(Configuration.GetConnectionString("StatisticsDb"),
                        builder => builder.MigrationsAssembly("GovUk.Education.ExploreEducationStatistics.Data.Model"))
                    .EnableSensitiveDataLogging()
            );

            // remove default Microsoft remapping of the name of the OpenID "roles" claim mapping
            JwtSecurityTokenHandler.DefaultInboundClaimTypeMap.Remove("roles");
            JwtSecurityTokenHandler.DefaultInboundClaimTypeMap.Remove("role");

            services
                .AddDefaultIdentity<ApplicationUser>()
                .AddRoles<IdentityRole>()
                .AddEntityFrameworkStores<UsersAndRolesDbContext>()
                .AddDefaultTokenProviders();

            if (HostingEnvironment.IsDevelopment())
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
            // This config tells UserManager to expect the logged in user's id to be in a Claim called
            // "http://schemas.xmlsoap.org/ws/2005/05/identity/claims/nameidentifier" rather than "sub" (because
            // this Claim is renamed via the DefaultInboundClaimTypeMap earlier in the login process).
            //
            // It doesn't seem to be possible to remove the renaming (via
            // JwtSecurityTokenHandler.DefaultInboundClaimTypeMap.Remove("sub")) because some mechanism earlier in the 
            // authentication process requires it to be in the Claim named
            // "http://schemas.xmlsoap.org/ws/2005/05/identity/claims/nameidentifier" rather than "sub".
            services.Configure<IdentityOptions>(options =>
            {
                options.ClaimsIdentity.UserIdClaimType = ClaimTypes.NameIdentifier;
            });

            services.AddAuthorization(options =>
            {
                // does this user have minimal permissions to access any admin APIs?
                options.AddPolicy(SecurityPolicies.CanAccessSystem.ToString(), policy => 
                    policy.RequireClaim(SecurityClaimTypes.ApplicationAccessGranted.ToString()));
                
                // does this user have permissions to access analyst pages?
                options.AddPolicy(SecurityPolicies.CanAccessAnalystPages.ToString(), policy => 
                    policy.RequireClaim(SecurityClaimTypes.AnalystPagesAccessGranted.ToString()));

                // does this user have permissions to access prerelease pages?
                options.AddPolicy(SecurityPolicies.CanAccessPrereleasePages.ToString(), policy => 
                    policy.RequireClaim(SecurityClaimTypes.PrereleasePagesAccessGranted.ToString()));

                // does this user have permissions to view the prerelease contacts list?
                options.AddPolicy(SecurityPolicies.CanViewPrereleaseContacts.ToString(), policy => 
                    policy.RequireClaim(SecurityClaimTypes.CanViewPrereleaseContacts.ToString()));

                // does this user have permissions to invite and manage all users on the system?
                options.AddPolicy(SecurityPolicies.CanManageUsersOnSystem.ToString(), policy => 
                    policy.RequireClaim(SecurityClaimTypes.ManageAnyUser.ToString()));
                
                // does this user have permissions to manage all methodologies on the system?
                options.AddPolicy(SecurityPolicies.CanManageMethodologiesOnSystem.ToString(), policy => 
                    policy.RequireClaim(SecurityClaimTypes.ManageAnyMethodology.ToString()));
                
                // does this user have permission to view all Topics across the application?
                options.AddPolicy(SecurityPolicies.CanViewAllTopics.ToString(), policy => 
                    policy.RequireClaim(SecurityClaimTypes.AccessAllTopics.ToString()));
                
                // does this user have permission to view a specific Theme?
                options.AddPolicy(SecurityPolicies.CanViewSpecificTheme.ToString(), policy =>
                    policy.Requirements.Add(new ViewSpecificThemeRequirement()));
                
                // does this user have permission to view all Releases across the application?
                options.AddPolicy(SecurityPolicies.CanViewAllReleases.ToString(), policy => 
                    policy.RequireClaim(SecurityClaimTypes.AccessAllReleases.ToString()));
                
                // does this user have permission to create a publication under a specific topic?
                options.AddPolicy(SecurityPolicies.CanCreatePublicationForSpecificTopic.ToString(), policy => 
                    policy.Requirements.Add(new CreatePublicationForSpecificTopicRequirement()));
                
                // does this user have permission to create a release under a specific publication?
                options.AddPolicy(SecurityPolicies.CanCreateReleaseForSpecificPublication.ToString(), policy => 
                    policy.Requirements.Add(new CreateReleaseForSpecificPublicationRequirement()));
                
                // does this user have permission to view a specific Publication?
                options.AddPolicy(SecurityPolicies.CanViewSpecificPublication.ToString(), policy =>
                    policy.Requirements.Add(new ViewSpecificPublicationRequirement()));

                // does this user have permission to view a specific Release?
                options.AddPolicy(SecurityPolicies.CanViewSpecificRelease.ToString(), policy =>
                    policy.Requirements.Add(new ViewSpecificReleaseRequirement()));

                // does this user have permission to update a specific Release?
                options.AddPolicy(SecurityPolicies.CanUpdateSpecificRelease.ToString(), policy =>
                    policy.Requirements.Add(new UpdateSpecificReleaseRequirement()));

                // does this user have permission to mark a specific Release as a Draft?
                options.AddPolicy(SecurityPolicies.CanMarkSpecificReleaseAsDraft.ToString(), policy =>
                    policy.Requirements.Add(new MarkSpecificReleaseAsDraftRequirement()));

                // does this user have permission to submit a specific Release to Higher Review?
                options.AddPolicy(SecurityPolicies.CanSubmitSpecificReleaseToHigherReview.ToString(), policy =>
                    policy.Requirements.Add(new SubmitSpecificReleaseToHigherReviewRequirement()));

                // does this user have permission to approve a specific Release?
                options.AddPolicy(SecurityPolicies.CanApproveSpecificRelease.ToString(), policy =>
                    policy.Requirements.Add(new ApproveSpecificReleaseRequirement()));

                // does this user have permission to assign prerelease contacts to a specific Release?
                options.AddPolicy(SecurityPolicies.CanAssignPrereleaseContactsToSpecificRelease.ToString(), policy =>
                    policy.Requirements.Add(new AssignPrereleaseContactsToSpecificReleaseRequirement()));
            });

            services.Configure<RouteOptions>(options => options.LowercaseUrls = true);

            services.AddAutoMapper(typeof(Startup).Assembly);
            services.AddTransient<IMyReleasePermissionSetPropertyResolver, MyReleasePermissionSetPropertyResolver>();
            services.AddTransient<IMyPublicationPermissionSetPropertyResolver, MyPublicationPermissionSetPropertyResolver>();

            services.AddAutoMapper(typeof(Startup).Assembly);
            
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

            services.AddApplicationInsightsTelemetry();

            services.AddTransient<IFileStorageService, FileStorageService>();
            services.AddTransient<IImportService, ImportService>();
            services.AddTransient<IPublishingService, PublishingService>();
            services.AddTransient<IReleaseStatusService, ReleaseStatusService>(s => 
                new ReleaseStatusService(
                    s.GetService<IMapper>(),
                    s.GetService<IUserService>(),
                    s.GetService<IPersistenceHelper<ContentDbContext>>(),
                    new TableStorageService(Configuration.GetConnectionString("PublisherStorage"))));
            services.AddTransient<IThemeService, ThemeService>();
            services.AddTransient<IThemeRepository, ThemeRepository>();
            services.AddTransient<ITopicService, TopicService>();
            services.AddTransient<IPublicationService, PublicationService>();
            services.AddTransient<IPublicationRepository, PublicationRepository>();
            services.AddTransient<IMetaService, MetaService>();
            services.AddTransient<IContactService, ContactService>();
            services.AddTransient<IReleaseService, ReleaseService>();
            services.AddTransient<IReleaseRepository, ReleaseRepository>();
            services.AddTransient<IMethodologyService, MethodologyService>();
            services.AddTransient<IDataBlockService, DataBlockService>();
            services.AddTransient<IPreReleaseService, PreReleaseService>();

            services.AddTransient<IManageContentPageService, ManageContentPageService>();
            services.AddTransient<IContentService, ContentService>();
            services.AddTransient<IRelatedInformationService, RelatedInformationService>();

            services.AddTransient<INotificationClient>(s =>
            {
                var notifyApiKey = Configuration.GetValue<string>("NotifyApiKey");
                
                if (!HostingEnvironment.IsDevelopment())
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
            
            services.AddTransient<IBoundaryLevelService, BoundaryLevelService>();
            services.AddTransient<IDataService<ResultWithMetaViewModel>, DataService>();
            services.AddTransient<IDataService<TableBuilderResultViewModel>, TableBuilderDataService>();
            services.AddTransient<IFilterService, FilterService>();
            services.AddTransient<IFilterGroupService, FilterGroupService>();
            services.AddTransient<IFilterItemService, FilterItemService>();
            services.AddTransient<IFootnoteService, FootnoteService>();
            services.AddTransient<Data.Model.Services.Interfaces.IFootnoteService, Data.Model.Services.FootnoteService>();
            services.AddTransient<IGeoJsonService, GeoJsonService>();
            services.AddTransient<IIndicatorGroupService, IndicatorGroupService>();
            services.AddTransient<IIndicatorService, IndicatorService>();
            services.AddTransient<ILocationService, LocationService>();
            services.AddTransient<IObservationService, ObservationService>();
            services.AddTransient<IReleaseMetaService, ReleaseMetaService>();
            services.AddTransient<IReleaseNoteService, ReleaseNoteService>();
            services.AddTransient<IResultBuilder<Observation, ObservationViewModel>, ResultBuilder>();
            services.AddTransient<Data.Model.Services.Interfaces.IReleaseService, Data.Model.Services.ReleaseService>();
            services.AddTransient<ISubjectService, SubjectService>();
            services.AddTransient<ISubjectMetaService, SubjectMetaService>();
            services.AddTransient<ITimePeriodService, TimePeriodService>();
            services.AddTransient<ITableBuilderSubjectMetaService, TableBuilderSubjectMetaService>();
            services.AddTransient<ITableBuilderResultSubjectMetaService, TableBuilderResultSubjectMetaService>();
            services.AddTransient<IImportStatusService, ImportStatusService>();
            services.AddSingleton<DataServiceMemoryCache<BoundaryLevel>, DataServiceMemoryCache<BoundaryLevel>>();
            services.AddSingleton<DataServiceMemoryCache<GeoJson>, DataServiceMemoryCache<GeoJson>>();
            services.AddTransient<IUserManagementService, UserManagementService>();
            services.AddTransient<ITableStorageService, TableStorageService>(s =>
                new TableStorageService(Configuration.GetConnectionString("CoreStorage")));
            AddPersistenceHelper<ContentDbContext>(services);
            AddPersistenceHelper<StatisticsDbContext>(services);

            // This service handles the generation of the JWTs for users after they log in
            services.AddTransient<IProfileService, ApplicationUserProfileService>();
            
            // These services allow us to check our Policies within Controllers and Services
            services.AddTransient<IAuthorizationService, DefaultAuthorizationService>();
            services.AddTransient<IUserService, UserService>();
            
            // These handlers enforce Resource-based access control
            services.AddTransient<IAuthorizationHandler, CreatePublicationForSpecificTopicCanCreateForAnyTopicAuthorizationHandler>();
            services.AddTransient<IAuthorizationHandler, CreateReleaseForSpecificPublicationCanCreateForAnyPublicationAuthorizationHandler>();
            services.AddTransient<IAuthorizationHandler, ViewSpecificThemeAuthorizationHandler>();
            services.AddTransient<IAuthorizationHandler, ViewSpecificPublicationAuthorizationHandler>();
            services.AddTransient<IAuthorizationHandler, ViewSpecificReleaseAuthorizationHandler>();
            services.AddTransient<IAuthorizationHandler, UpdateSpecificReleaseAuthorizationHandler>();
            services.AddTransient<IAuthorizationHandler, MarkSpecificReleaseAsDraftAuthorizationHandler>();
            services.AddTransient<IAuthorizationHandler, SubmitSpecificReleaseToHigherReviewAuthorizationHandler>();
            services.AddTransient<IAuthorizationHandler, ApproveSpecificReleaseAuthorizationHandler>();
            services.AddTransient<IAuthorizationHandler, AssignPrereleaseContactsToSpecificReleaseAuthorizationHandler>();

            services.AddSingleton(HostingEnvironment.ContentRootFileProvider);
            services.AddTransient<IFileTypeService, FileTypeService>();

            services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1",
                    new OpenApiInfo {Title = "Explore education statistics - Admin API", Version = "v1"});
                // c.SwaggerDoc("v1", new Info {Title = "Explore education statistics - Admin API", Version = "v1"});
                c.CustomSchemaIds((type) => type.FullName);
                c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme()
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

        private static void AddPersistenceHelper<TDbContext>(IServiceCollection services)
            where TDbContext : DbContext
        {
            services.AddTransient<
                IPersistenceHelper<TDbContext>,
                PersistenceHelper<TDbContext>>(
                s =>
                {
                    var dbContext = s.GetService<TDbContext>();
                    return new PersistenceHelper<TDbContext>(dbContext);
                });
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
                app.UseExceptionHandler("/Error");
                app.UseHsts(hsts => hsts.MaxAge(365).IncludeSubdomains());
            }

            // Security Headers
            app.UseXContentTypeOptions();
            app.UseXXssProtection(options => options.EnabledWithBlockMode());
            app.UseXfo(options => options.SameOrigin());
            app.UseReferrerPolicy(options => options.NoReferrerWhenDowngrade());
            app.UseCsp(opts => opts
                .BlockAllMixedContent()
                .StyleSources(s => s.Self())
                .StyleSources(s => s
                    .CustomSources(" https://cdnjs.cloudflare.com")
                    .UnsafeInline())
                .FontSources(s => s.Self())
                .FormActions(s => s
                    .CustomSources("https://login.microsoftonline.com")
                    .Self()
                )
                .FrameAncestors(s => s.Self())
                .ImageSources(s => s.Self())
                .ImageSources(s => s.CustomSources("data:"))
                .ScriptSources(s => s.Self())
                .ScriptSources(s => s.UnsafeInline())
            );
            app.UseHsts(options =>
            {
                options.MaxAge(365);
                options.IncludeSubdomains();
                options.Preload();
            });

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
            
            app.UseSwagger();
            app.UseSwaggerUI(c =>
            {
                c.SwaggerEndpoint("/swagger/v1/swagger.json", "Admin API V1");
                c.RoutePrefix = "docs";
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

                using (var context = serviceScope.ServiceProvider.GetService<ContentDbContext>())
                {
                    context.Database.SetCommandTimeout(int.MaxValue);
                    context.Database.Migrate();
                }

                using (var context = serviceScope.ServiceProvider.GetService<UsersAndRolesDbContext>())
                {
                    context.Database.SetCommandTimeout(int.MaxValue);
                    context.Database.Migrate();
                }
            }
        }
    }
}
