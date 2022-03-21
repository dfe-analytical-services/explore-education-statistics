#nullable enable
using GovUk.Education.ExploreEducationStatistics.Common.ModelBinding;
using GovUk.Education.ExploreEducationStatistics.Common.Tests.Fixtures;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Database;
using GovUk.Education.ExploreEducationStatistics.Data.Model.Database;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json;
using static GovUk.Education.ExploreEducationStatistics.Common.Utils.StartupUtils;

namespace GovUk.Education.ExploreEducationStatistics.Content.Api.Tests
{
    /// <summary>
    /// Generic test application startup for use in integration tests.
    /// </summary>
    /// <remarks>
    /// Use in combination with <see cref="TestApplicationFactory{TStartup}"/>
    /// as a test class fixture.
    /// </remarks>
    public class TestStartup
    {
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddMvcCore(options => { options.EnableEndpointRouting = false; })
                .SetCompatibilityVersion(CompatibilityVersion.Version_3_0)
                .AddNewtonsoftJson(
                    options => { options.SerializerSettings.NullValueHandling = NullValueHandling.Ignore; }
                );

            services.AddControllers(
                options =>
                {
                    options.ModelBinderProviders.Insert(0, new SeparatedQueryModelBinderProvider(","));
                }
            )
                .AddApplicationPart(typeof(Startup).Assembly);

            services.AddDbContext<StatisticsDbContext>(
                options =>
                    options.UseInMemoryDatabase("TestStatisticsDb",
                        b => b.EnableNullChecks(false))
            );

            services.AddDbContext<ContentDbContext>(
                options =>
                    options.UseInMemoryDatabase("TestContentDb",
                        b => b.EnableNullChecks(false))
            );

            AddPersistenceHelper<ContentDbContext>(services);
            AddPersistenceHelper<StatisticsDbContext>(services);
        }

        public void Configure(IApplicationBuilder app)
        {
            app.UseMvc();
        }
    }
}
