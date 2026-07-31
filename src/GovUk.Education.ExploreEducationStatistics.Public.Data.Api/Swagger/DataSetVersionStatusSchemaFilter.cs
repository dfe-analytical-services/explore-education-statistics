using System.Text.Json.Nodes;
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

    public void Apply(IOpenApiSchema schema, SchemaFilterContext context)
    {
        if (
            context.MemberInfo == null
            && context.Type == typeof(DataSetVersionStatus)
            && schema is OpenApiSchema openApiSchema
        )
        {
            openApiSchema.Type = JsonSchemaType.String;

            openApiSchema.Enum = EnumUtil
                .GetEnums<DataSetVersionStatus>()
                .Where(_publicStatuses.Contains)
                .Select(JsonNode (dataSetVersionStatus) => JsonValue.Create(dataSetVersionStatus.ToString()))
                .ToList();
        }
    }
}
