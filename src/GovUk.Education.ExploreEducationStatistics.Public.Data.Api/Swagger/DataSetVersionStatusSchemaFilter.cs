using GovUk.Education.ExploreEducationStatistics.Common.Utils;
using GovUk.Education.ExploreEducationStatistics.Public.Data.Model;
using Microsoft.OpenApi;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace GovUk.Education.ExploreEducationStatistics.Public.Data.Api.Swagger;

public class DataSetVersionStatusSchemaFilter : ISchemaFilter
{
    private readonly HashSet<DataSetVersionStatus> _publicStatuses =
    [
        DataSetVersionStatus.Published,
        DataSetVersionStatus.Deprecated,
        DataSetVersionStatus.Withdrawn,
    ];

    public void Apply(OpenApiSchema schema, SchemaFilterContext context)
    {
        if (context.MemberInfo == null && context.Type == typeof(DataSetVersionStatus))
        {
            schema.Type = JsonSchemaType.String;

            schema.Enum = EnumUtil
                .GetEnums<DataSetVersionStatus>()
                .Where(_publicStatuses.Contains)
                .Select(e => new OpenApiString(e.ToString()))
                .ToList<IOpenApiAny>();
        }
    }
}
