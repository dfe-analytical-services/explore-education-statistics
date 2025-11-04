using GovUk.Education.ExploreEducationStatistics.Common.Model;
using GovUk.Education.ExploreEducationStatistics.Common.Utils;
using Microsoft.OpenApi.Any;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace GovUk.Education.ExploreEducationStatistics.Public.Data.Api.Swagger;

public class IndicatorUnitSchemaFilter : ISchemaFilter
{
    public void Apply(OpenApiSchema schema, SchemaFilterContext context)
    {
        if (context.MemberInfo == null && context.Type == typeof(IndicatorUnit))
        {
            schema.Type = "string";
            schema.Description = """
                The recommended unit to format an indicator with.

                The allowed values are:

                - `""` - No units with numeric formatting where possible e.g. `123,500,600`
                - `£` - Pound sterling with numeric formatting e.g. `£1,234,500`
                - `£m` - Pound sterling in millions with numeric formatting e.g. `£1,234m`
                - `%` - Percentage with numeric formatting e.g. `1,234.56%`
                - `pp` - Percentage points with numeric formatting e.g. `1,234pp`
                - `string` - String with no units and no numeric formatting e.g. `123500600`
                """;

            schema.Example = new OpenApiString("%");

            schema.Enum = EnumUtil
                .GetEnumLabels<IndicatorUnit>()
                .Select(unit => new OpenApiString(unit))
                .ToList<IOpenApiAny>();
        }
    }
}
