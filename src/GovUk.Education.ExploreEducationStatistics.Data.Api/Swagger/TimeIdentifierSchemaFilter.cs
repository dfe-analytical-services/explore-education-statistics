#nullable enable
using System.Text.Json.Nodes;
using GovUk.Education.ExploreEducationStatistics.Common.Extensions;
using GovUk.Education.ExploreEducationStatistics.Common.Model;
using Microsoft.OpenApi;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace GovUk.Education.ExploreEducationStatistics.Data.Api.Swagger;

/// <summary>
/// TimeIdentifier is special. Each enum value has an attached attribute that contains a different API value.
/// </summary>
public class TimeIdentifierSchemaFilter : ISchemaFilter
{
    public void Apply(IOpenApiSchema schema, SchemaFilterContext context)
    {
        if (context.Type == typeof(TimeIdentifier) && schema is OpenApiSchema openApiSchema)
        {
            openApiSchema.Enum = Enum.GetValues<TimeIdentifier>()
                .Select(JsonNode (timeIdentifier) => JsonValue.Create(timeIdentifier.GetEnumValue()))
                .ToList();
        }
    }
}
