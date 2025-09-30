using Asp.Versioning.ApiExplorer;
using GovUk.Education.ExploreEducationStatistics.Common.Extensions;
using GovUk.Education.ExploreEducationStatistics.Common.Model;
using GovUk.Education.ExploreEducationStatistics.Common.Model.Data;
using GovUk.Education.ExploreEducationStatistics.Public.Data.Api.Options;
using Microsoft.Extensions.Options;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace GovUk.Education.ExploreEducationStatistics.Public.Data.Api.Swagger;

public class SwaggerConfig(
    IApiVersionDescriptionProvider provider,
    IOptions<AppOptions> appOptions)
    : IConfigureOptions<SwaggerGenOptions>
{
    public void Configure(SwaggerGenOptions options)
    {
        options.DocumentFilter<VersionedPathsDocumentFilter>();

        options.OperationFilter<DefaultValuesOperationFilter>();
        options.OperationFilter<GeneralResponseOperationFilter>();

        options.SchemaFilter<JsonConverterSchemaFilter>();
        options.SchemaFilter<RequiredPropertySchemaFilter>();
        options.SchemaFilter<SwaggerEnumSchemaFilter>();
        options.SchemaFilter<DataSetStatusSchemaFilter>();
        options.SchemaFilter<DataSetVersionStatusSchemaFilter>();
        options.SchemaFilter<ErrorViewModelSchemaFilter>();
        options.SchemaFilter<GeographicLevelSchemaFilter>();
        options.SchemaFilter<IndicatorUnitSchemaFilter>();
        options.SchemaFilter<TimeIdentifierSchemaFilter>();

        Directory
            .GetFiles(AppContext.BaseDirectory, "*.xml", SearchOption.TopDirectoryOnly)
            .ForEach(xmlFile => options.IncludeXmlComments(xmlFile));

        options.EnableAnnotations(enableAnnotationsForInheritance: true, enableAnnotationsForPolymorphism: true);
        options.DescribeAllParametersInCamelCase();
        options.UseAllOfToExtendReferenceSchemas();
        options.SupportNonNullableReferenceTypes();

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

        options.CustomSchemaIds(SchemaIdSelector);

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

        options.AddServer(new OpenApiServer
        {
            Description = "API server",
            Url = appOptions.Value.Url
        });
    }

    private static string SchemaIdSelector(Type type)
    {
        if (type == typeof(TimeIdentifier))
        {
            return "TimePeriodCode";
        }

        if (type == typeof(GeographicLevel))
        {
            return "GeographicLevelCode";
        }

        return type.Name;
    }
}
