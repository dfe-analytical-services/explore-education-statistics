#nullable enable
using GovUk.Education.ExploreEducationStatistics.Common.ModelBinding;
using GovUk.Education.ExploreEducationStatistics.Common.Tests.Fixtures;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json;

namespace GovUk.Education.ExploreEducationStatistics.Common.Tests;

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
            .AddNewtonsoftJson(
                options => { options.SerializerSettings.NullValueHandling = NullValueHandling.Ignore; }
            );

        services.AddControllers(
            options =>
            {
                options.ModelBinderProviders.Insert(0, new SeparatedQueryModelBinderProvider(","));
            }
        );
    }

    public void Configure(IApplicationBuilder app)
    {
        app.UseMvc();
    }
}
