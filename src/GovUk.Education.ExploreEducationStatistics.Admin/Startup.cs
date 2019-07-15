using AutoMapper;
using GovUk.Education.ExploreEducationStatistics.Admin.Services;
using GovUk.Education.ExploreEducationStatistics.Admin.Services.Interfaces;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.AzureAD.UI;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Authorization;
using Microsoft.AspNetCore.Routing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.AspNetCore.SpaServices.ReactDevelopmentServer;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Database;
using Swashbuckle.AspNetCore.Swagger;

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

            services.AddAuthentication(AzureADDefaults.AuthenticationScheme)
                .AddAzureAD(options => Configuration.Bind("AzureAd", options));

            services.Configure<RouteOptions>(options => options.LowercaseUrls = true);

            services.AddAutoMapper();
            services.AddMvc(options =>
                {
                    var policy = new AuthorizationPolicyBuilder()
                        .RequireAuthenticatedUser()
                        .Build();
                    options.Filters.Add(new AuthorizeFilter(policy));
                })
                .SetCompatibilityVersion(CompatibilityVersion.Version_2_2);

            services.AddDbContext<ApplicationDbContext>(options =>
                options
                    .UseSqlServer(Configuration.GetConnectionString("ContentDb"),
                        builder => builder.MigrationsAssembly(typeof(Startup).Assembly.FullName))
                    .EnableSensitiveDataLogging()
            );

            // In production, the React files will be served from this directory
            services.AddSpaStaticFiles(configuration => { configuration.RootPath = "wwwroot"; });

            services.AddTransient<IFileStorageService, FileStorageService>();
            services.AddTransient<IImportService, ImportService>();
            services.AddTransient<INotificationsService, NotificationsService>();
            services.AddTransient<IPublishingService, PublishingService>();
            services.AddTransient<IThemeService, ThemeService>();
            services.AddTransient<IPublicationService, PublicationService>();

            services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1", new Info {Title = "Explore education statistics - Admin API", Version = "v1"});
            });
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                app.UseExceptionHandler("/Home/Error");

                app.UseHsts(hsts => hsts.MaxAge(365).IncludeSubdomains());
            }

            app.UseHttpsRedirection();

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
                .FormActions(s => s.Self())
                .FrameAncestors(s => s.Self())
                .ImageSources(s => s.Self())
                .ImageSources(s => s.CustomSources("data:"))
                .ScriptSources(s => s.Self())
                .ScriptSources(s => s.UnsafeInline())
            );

            app.UseStaticFiles();
            app.UseSpaStaticFiles();
            app.UseCookiePolicy();

            app.UseAuthentication();

            // adds a application root specific for the serverside admin tools built to aid development
            app.Map("/tools", adminApp =>
                adminApp.UseMvc(routes =>
                {
                    routes.MapRoute(
                        name: "default",
                        template: "{controller=Home}/{action=Index}/{id?}");
                })
            );

            // TODO Is this the best solution to the routing - controllers can be accessed on both "/tools" and "/api"
            // TODO We could perhaps move everything to the toplevel "/" and have routes specified in the controllers
            // TODO themselves but this will have an effect on the react app we serve up.
            app.Map("/api", apiApp => apiApp.UseMvc());
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
    }
}