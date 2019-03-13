using System;
using AutoMapper;
using GovUk.Education.ExploreEducationStatistics.Data.Api.Data;
using GovUk.Education.ExploreEducationStatistics.Data.Api.Importer;
using GovUk.Education.ExploreEducationStatistics.Data.Api.ModelBinding;
using GovUk.Education.ExploreEducationStatistics.Data.Api.Models.Configuration;
using GovUk.Education.ExploreEducationStatistics.Data.Api.Services;
using GovUk.Education.ExploreEducationStatistics.Data.Api.Services.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Data.Api.Services.Meta;
using GovUk.Education.ExploreEducationStatistics.Data.Api.Services.TableBuilder;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Rewrite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Swashbuckle.AspNetCore.Swagger;

namespace GovUk.Education.ExploreEducationStatistics.Data.Api
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
            services.AddMvc(config =>
            {
                config.RespectBrowserAcceptHeader = true;
                config.ReturnHttpNotAcceptable = true;
                config.Conventions.Add(new CommaSeparatedQueryStringConvention());
            }).SetCompatibilityVersion(CompatibilityVersion.Version_2_1);

            services.AddDbContext<ApplicationDbContext>(options =>
                options.UseSqlServer(Configuration.GetConnectionString("StatisticsDb")));

            // Adds Brotli and Gzip compressing
            services.AddResponseCompression();

            // Register the Swagger generator, defining 1 or more Swagger documents
            services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1", new Info {Title = "Explore education statistics - Data API", Version = "v1"});
            });

            services.AddScoped<TidyDataReleaseCounter>();

            services.AddScoped<GeographicResultBuilder>();
            services.AddScoped<CharacteristicResultBuilder>();
            services.AddScoped<LaCharacteristicResultBuilder>();

            services.AddTransient<ISeedService, SeedService>();
            services.AddTransient<ITableBuilderService, TableBuilderService>();

            services.AddTransient<IAttributeMetaService, AttributeMetaService>();
            services.AddTransient<ICharacteristicMetaService, CharacteristicMetaService>();

            services.AddScoped<IGeographicDataService, GeographicDataService>();
            services.AddScoped<ILaCharacteristicDataService, LaCharacteristicDataService>();
            services.AddScoped<INationalCharacteristicDataService, NationalCharacteristicDataService>();

            services.AddCors();
            services.AddAutoMapper();

            services.AddOptions();
            services.Configure<SeedConfigurationOptions>(Configuration.GetSection("SeedConfig"));
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env)
        {
            UpdateDatabase(app);

            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                app.UseHsts();
            }

            // Adds Brotli and Gzip compressing
            app.UseResponseCompression();

            // Enable middleware to serve generated Swagger as a JSON endpoint.
            app.UseSwagger();

            // Enable middleware to serve swagger-ui (HTML, JS, CSS, etc.), 
            // specifying the Swagger JSON endpoint.
            app.UseSwaggerUI(c =>
            {
                c.SwaggerEndpoint("/swagger/v1/swagger.json", "Data API V1");
                c.RoutePrefix = "docs";
            });

            app.UseHttpsRedirection();
            app.UseCors(options => options.WithOrigins("http://localhost:3000").AllowAnyMethod().AllowAnyHeader());
            app.UseMvc();

            var option = new RewriteOptions();
            option.AddRedirect("^$", "docs");
            app.UseRewriter(option);
        }

        private static void UpdateDatabase(IApplicationBuilder app)
        {
            using (var serviceScope = app.ApplicationServices.GetRequiredService<IServiceScopeFactory>()
                .CreateScope())
            {
                using (var context = serviceScope.ServiceProvider.GetService<ApplicationDbContext>())
                {
                    context.Database.SetCommandTimeout(int.MaxValue);
                    context.Database.Migrate();
                }
            }
        }
    }
}