#nullable enable
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using Azure.Storage.Blobs;
using GovUk.Education.ExploreEducationStatistics.Common.Cache;
using GovUk.Education.ExploreEducationStatistics.Common.Cancellation;
using GovUk.Education.ExploreEducationStatistics.Common.Config;
using GovUk.Education.ExploreEducationStatistics.Common.Database;
using GovUk.Education.ExploreEducationStatistics.Common.Extensions;
using GovUk.Education.ExploreEducationStatistics.Common.Model.Data;
using GovUk.Education.ExploreEducationStatistics.Common.ModelBinding;
using GovUk.Education.ExploreEducationStatistics.Common.Services;
using GovUk.Education.ExploreEducationStatistics.Common.Services.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Common.Services.Interfaces.Security;
using GovUk.Education.ExploreEducationStatistics.Common.Services.Security;
using GovUk.Education.ExploreEducationStatistics.Common.Tests.Fixtures;
using GovUk.Education.ExploreEducationStatistics.Common.Utils;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Hosting.Server.Features;
using Microsoft.AspNetCore.Mvc.Authorization;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Rewrite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
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
        
        services.AddTransient<IBlobCacheService, BlobCacheService>();
    }

    public void Configure(IApplicationBuilder app)
    {
        app.UseMvc();
    }
}