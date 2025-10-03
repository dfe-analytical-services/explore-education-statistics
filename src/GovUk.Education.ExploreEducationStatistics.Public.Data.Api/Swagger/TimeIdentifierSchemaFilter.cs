using GovUk.Education.ExploreEducationStatistics.Common.Extensions;
using GovUk.Education.ExploreEducationStatistics.Common.Model;
using GovUk.Education.ExploreEducationStatistics.Common.Utils;
using Microsoft.OpenApi.Any;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace GovUk.Education.ExploreEducationStatistics.Public.Data.Api.Swagger;

public class TimeIdentifierSchemaFilter : ISchemaFilter
{
    public void Apply(OpenApiSchema schema, SchemaFilterContext context)
    {
        if (context.MemberInfo == null && context.Type == typeof(TimeIdentifier))
        {
            schema.Type = "string";
            schema.Format = null;

            schema.Description = """
                The code identifying the time period's type.

                The allowed values are:

                - `AY` - academic year
                - `AYQ1 - AYQ4` - academic year quarter 1 to 4
                - `T1` - academic year's autumn term
                - `T2` - academic year's spring term
                - `T3` - academic year's summer term
                - `T1T2` - academic year's autumn and spring term
                - `CY` - calendar year
                - `CYQ1 - CYQ4` - calendar year quarter 1 to 4
                - `RY` - reporting year
                - `P1` - financial year part 1 (April to September)
                - `P2` - financial year part 2 (October to March)
                - `FY` - financial year
                - `FYQ1 - FYQ4` - financial year quarter 1 to 4
                - `TY` - tax year
                - `TYQ1 - FYQ4` - tax year quarter 1 to 4
                - `W1 - W52` - week 1 to 52
                - `M1 - M12` - month 1 to 12
                """;

            schema.Example = new OpenApiString("CY");

            schema.Enum = TimeIdentifierUtils
                .Codes.NaturalOrder()
                .Select(code => new OpenApiString(code))
                .ToList<IOpenApiAny>();
        }
    }
}
