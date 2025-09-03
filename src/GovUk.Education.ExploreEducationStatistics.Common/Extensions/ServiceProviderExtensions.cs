using System.Text.Json;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using JsonOptions = Microsoft.AspNetCore.Http.Json.JsonOptions;

namespace GovUk.Education.ExploreEducationStatistics.Common.Extensions;

public static class ServiceProviderExtensions
{
    public static JsonOptions GetSystemTextJsonOptions(this IServiceProvider provider)
    {
        return provider.GetRequiredService<IOptions<JsonOptions>>().Value;
    }

    public static JsonSerializerOptions GetSystemTextJsonSerializerOptions(this IServiceProvider provider)
    {
        return provider.GetSystemTextJsonOptions().SerializerOptions;
    }
}
