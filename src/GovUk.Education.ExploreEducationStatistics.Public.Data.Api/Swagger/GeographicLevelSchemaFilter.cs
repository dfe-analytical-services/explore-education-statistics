using System.Text.Json.Nodes;
using GovUk.Education.ExploreEducationStatistics.Common.Model.Data;
using GovUk.Education.ExploreEducationStatistics.Common.Utils;
using Microsoft.OpenApi;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace GovUk.Education.ExploreEducationStatistics.Public.Data.Api.Swagger;

public class GeographicLevelSchemaFilter : ISchemaFilter
{
    public void Apply(IOpenApiSchema schema, SchemaFilterContext context)
    {
        if (
            context.MemberInfo == null
            && context.Type == typeof(GeographicLevel)
            && schema is OpenApiSchema openApiSchema
        )
        {
            openApiSchema.Type = JsonSchemaType.String;
            openApiSchema.Format = null;

            openApiSchema.Description = """
                The code for a geographic level that locations are grouped by.

                The allowed values are:

                - `EDA` - English devolved area
                - `INST` - Institution
                - `LA` - Local authority
                - `LAD` - Local authority district
                - `LEP` - Local enterprise partnership
                - `LSIP` - Local skills improvement plan area
                - `MCA` - Mayoral combined authority
                - `MAT` - MAT
                - `NAT` - National
                - `OA` - Opportunity area
                - `PA` - Planning area
                - `PCON` - Parliamentary constituency
                - `PROV` - Provider
                - `REG` - Regional
                - `RSC` - RSC region
                - `SCH` - School
                - `SPON` - Sponsor
                - `WARD` - Ward
                - `PFA` - Police Force Area
                """;

            openApiSchema.Example = JsonValue.Create("NAT");

            openApiSchema.Enum = GeographicLevelUtils
                .OrderedCodes.Select(JsonNode (code) => JsonValue.Create(code))
                .ToList();
        }
    }
}
