using Asp.Versioning.ApiExplorer;
using Microsoft.Extensions.Options;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace GovUk.Education.ExploreEducationStatistics.Public.Data.Api.Swagger;

public class SwaggerConfig(IApiVersionDescriptionProvider provider) : IConfigureOptions<SwaggerGenOptions>
{
    public void Configure(SwaggerGenOptions options)
    {
        options.OperationFilter<DefaultValuesOperationFilter>();
        options.SchemaFilter<JsonConverterSchemaFilter>();
        options.SchemaFilter<SwaggerEnumSchemaFilter>();
        options.SchemaFilter<DataSetStatusSchemaFilter>();
        options.SchemaFilter<DataSetVersionStatusSchemaFilter>();
        options.SchemaFilter<GeographicLevelSchemaFilter>();
        options.SchemaFilter<TimeIdentifierSchemaFilter>();

        var fileName = typeof(Program).Assembly.GetName().Name + ".xml";
        var filePath = Path.Combine(AppContext.BaseDirectory, fileName);

        options.DescribeAllParametersInCamelCase();
        options.UseOneOfForPolymorphism();
        options.UseAllOfForInheritance();
        options.UseAllOfToExtendReferenceSchemas();
        options.SupportNonNullableReferenceTypes();
        options.IncludeXmlComments(filePath);
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
}
