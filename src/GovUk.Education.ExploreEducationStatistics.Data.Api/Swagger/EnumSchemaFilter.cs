#nullable enable
using System.Text.Json.Nodes;
using Microsoft.OpenApi;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace GovUk.Education.ExploreEducationStatistics.Data.Api.Swagger;

/// <summary>
/// Emit the Enum string names in the Swagger Documentation instead of their int values.
/// </summary>
public class EnumSchemaFilter : ISchemaFilter
{
    public void Apply(IOpenApiSchema schema, SchemaFilterContext context)
    {
        if (context.Type.IsEnum && schema is OpenApiSchema openApiSchema)
        {
            openApiSchema.Enum = Enum.GetNames(context.Type).Select(JsonNode (name) => JsonValue.Create(name)).ToList();
        }
    }
}
