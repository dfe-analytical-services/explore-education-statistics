using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using AutoMapper;
using GovUk.Education.ExploreEducationStatistics.Admin.Areas.Identity.Data;
using GovUk.Education.ExploreEducationStatistics.Admin.Models;
using GovUk.Education.ExploreEducationStatistics.Admin.Services;
using GovUk.Education.ExploreEducationStatistics.Admin.Services.Interfaces;
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
using Microsoft.AspNetCore.Routing;
using Microsoft.AspNetCore.SpaServices.ReactDevelopmentServer;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using Newtonsoft.Json;
using IReleaseService = GovUk.Education.ExploreEducationStatistics.Admin.Services.Interfaces.IReleaseService;
using ReleaseService = GovUk.Education.ExploreEducationStatistics.Admin.Services.ReleaseService;

namespace GovUk.Education.ExploreEducationStatistics.Admin
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.Configure<CookiePolicyOptions>(options =>
            {
                // This lambda determines whether user consent for non-essential cookies is needed for a given request.
                options.CheckConsentNeeded = context => true;
                options.MinimumSameSitePolicy = SameSiteMode.None;
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

            services.Configure<RouteOptions>(options => options.LowercaseUrls = true);

            services.AddAutoMapper(typeof(Startup).Assembly);
            services.AddMvc(options =>
                {
                    var policy = new AuthorizationPolicyBuilder()
                        .RequireAuthenticatedUser()
                        .Build();
                    options.Filters.Add(new AuthorizeFilter(policy));
                    options.EnableEndpointRouting = false;
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
            services.AddTransient<INotificationsService, NotificationsService>();
            services.AddTransient<IPublishingService, PublishingService>();
            services.AddTransient<IThemeService, ThemeService>();
            services.AddTransient<ITopicService, TopicService>();
            services.AddTransient<IPublicationService, PublicationService>();
            services.AddTransient<IMetaService, MetaService>();
            services.AddTransient<IContactService, ContactService>();
            services.AddTransient<IReleaseService, ReleaseService>();
            services.AddTransient<IMethodologyService, MethodologyService>();
            services.AddTransient<IDataBlockService, DataBlockService>();
            services.AddTransient<IPreReleaseService, PreReleaseService>();
            
            services.AddTransient<IBoundaryLevelService, BoundaryLevelService>();
            services.AddTransient<IDataService<ResultWithMetaViewModel>, DataService>();
            services.AddTransient<IDataService<TableBuilderResultViewModel>, TableBuilderDataService>();
            services.AddTransient<IFilterService, FilterService>();
            services.AddTransient<IFilterGroupService, FilterGroupService>();
            services.AddTransient<IFilterItemService, FilterItemService>();
            services.AddTransient<IFootnoteService, FootnoteService>();
            services.AddTransient<IGeoJsonService, GeoJsonService>();
            services.AddTransient<IIndicatorGroupService, IndicatorGroupService>();
            services.AddTransient<IIndicatorService, IndicatorService>();
            services.AddTransient<ILocationService, LocationService>();
            services.AddTransient<IObservationService, ObservationService>();
            services.AddTransient<IReleaseMetaService, ReleaseMetaService>();
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
            services.AddTransient<ITableStorageService, TableStorageService>(s => new TableStorageService(Configuration.GetConnectionString("CoreStorage")));

            services.AddTransient<IProfileService, ApplicationUserProfileService>();
            
            services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1", new OpenApiInfo { Title = "Explore education statistics - Admin API", Version = "v1" });
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
                .StyleSources(s => s.UnsafeInline())
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

            app.UseHttpsRedirection();
            app.UseStaticFiles();
            app.UseSpaStaticFiles();
            app.UseCookiePolicy();

            app.UseAuthentication();
            app.UseIdentityServer();
            app.UseAuthorization();

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
                c.SwaggerEndpoint("/swagger/v1/swagger.json", "Data API V1");
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