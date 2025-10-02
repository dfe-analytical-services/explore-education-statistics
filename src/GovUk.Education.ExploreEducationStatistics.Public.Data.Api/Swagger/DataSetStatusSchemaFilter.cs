using GovUk.Education.ExploreEducationStatistics.Common.Utils;
using GovUk.Education.ExploreEducationStatistics.Public.Data.Model;
using Microsoft.OpenApi.Any;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace GovUk.Education.ExploreEducationStatistics.Public.Data.Api.Swagger;

public class DataSetStatusSchemaFilter : ISchemaFilter
{
    private readonly HashSet<DataSetStatus> _publicStatuses =
    [
        DataSetStatus.Published,
        DataSetStatus.Deprecated,
        DataSetStatus.Withdrawn,
    ];

    public void Apply(OpenApiSchema schema, SchemaFilterContext context)
    {
        if (context.MemberInfo == null && context.Type == typeof(DataSetStatus))
        {
            schema.Type = "string";

            schema.Enum = EnumUtil
                .GetEnums<DataSetStatus>()
                .Where(_publicStatuses.Contains)
                .Select(e => new OpenApiString(e.ToString()))
                .ToList<IOpenApiAny>();
        }
    }
}
