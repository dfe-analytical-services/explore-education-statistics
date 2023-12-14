using System.Text.Json;
using Asp.Versioning.ApiExplorer;
using Microsoft.AspNetCore.Mvc.ApiExplorer;
using Microsoft.Extensions.Options;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace GovUk.Education.ExploreEducationStatistics.Public.Data.Api.Config;

public class SwaggerConfig(IApiVersionDescriptionProvider provider) : IConfigureOptions<SwaggerGenOptions>
{
    public void Configure(SwaggerGenOptions options)
    {
        options.OperationFilter<SwaggerDefaultValues>();

        var fileName = typeof(Program).Assembly.GetName().Name + ".xml";
        var filePath = Path.Combine(AppContext.BaseDirectory, fileName);

        options.IncludeXmlComments(filePath);
        options.UseAllOfForInheritance();
        options.UseOneOfForPolymorphism();
        options.CustomOperationIds(apiDesc =>
            {
                var actionDescriptor = apiDesc.ActionDescriptor;

                return actionDescriptor.AttributeRouteInfo?.Name
                       ?? actionDescriptor.EndpointMetadata
                               .OfType<IEndpointNameMetadata>()
                               .LastOrDefault()
                               ?.EndpointName
                       ?? (apiDesc.TryGetMethodInfo(out var methodInfo) ? methodInfo.Name : null);
            }
        );

        foreach (var description in provider.ApiVersionDescriptions)
        {
            options.SwaggerDoc(
                description.GroupName,
                new OpenApiInfo
                {
                    Title = "Explore education statistics - Public Data API",
                    Version = description.ApiVersion.ToString(),
                    Contact = new OpenApiContact
                    {
                        Name = "Explore education statistics",
                        Email = "explore.statistics@education.gov.uk",
                        Url = new Uri("https://explore-education-statistics.service.gov.uk")
                    },
                }
            );
        }
    }

    private class SwaggerDefaultValues : IOperationFilter
    {
        public void Apply(OpenApiOperation operation, OperationFilterContext context)
        {
            var apiDescription = context.ApiDescription;

            operation.Deprecated |= apiDescription.IsDeprecated();

            foreach (var responseType in context.ApiDescription.SupportedResponseTypes)
            {
                var responseKey = responseType.IsDefaultResponse
                    ? "default"
                    : responseType.StatusCode.ToString();
                var response = operation.Responses[responseKey];

                foreach (var contentType in response.Content.Keys)
                {
                    if (responseType.ApiResponseFormats.All(x => x.MediaType != contentType))
                    {
                        response.Content.Remove(contentType);
                    }
                }
            }

            if (operation.Parameters == null)
            {
                return;
            }

            foreach (var parameter in operation.Parameters)
            {
                var description = apiDescription.ParameterDescriptions
                    .First(p => p.Name == parameter.Name);

                parameter.Description ??= description.ModelMetadata?.Description;

                if (parameter.Schema.Default == null && description.DefaultValue != null)
                {
                    var json = JsonSerializer.Serialize(
                        description.DefaultValue,
                        description.ModelMetadata!.ModelType
                    );
                    parameter.Schema.Default = OpenApiAnyFactory.CreateFromJson(json);
                }

                parameter.Required |= description.IsRequired;
            }
        }
    }
}
