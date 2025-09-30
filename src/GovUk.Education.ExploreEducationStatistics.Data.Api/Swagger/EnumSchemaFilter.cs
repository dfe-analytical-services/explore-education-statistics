#nullable enable
using GovUk.Education.ExploreEducationStatistics.Common.Extensions;
using Microsoft.OpenApi.Any;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace GovUk.Education.ExploreEducationStatistics.Data.Api.Swagger;

/// <summary>
/// Emit the Enum string names in the Swagger Documentation instead of their int values.
/// </summary>
public class EnumSchemaFilter : ISchemaFilter
{
    public void Apply(OpenApiSchema schema, SchemaFilterContext context)
    {
        if (context.Type.IsEnum)
        {
            schema.Enum.Clear();
            schema.Enum.AddRange(
                Enum.GetNames(context.Type).Select(name => new OpenApiString($"{name}"))
            );
        }
    }
}
