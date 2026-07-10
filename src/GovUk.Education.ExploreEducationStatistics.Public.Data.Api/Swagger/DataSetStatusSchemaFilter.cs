using System.Text.Json.Nodes;
using GovUk.Education.ExploreEducationStatistics.Common.Utils;
using GovUk.Education.ExploreEducationStatistics.Public.Data.Model;
using Microsoft.OpenApi;
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

    public void Apply(IOpenApiSchema schema, SchemaFilterContext context)
    {
        if (
            context.MemberInfo == null
            && context.Type == typeof(DataSetStatus)
            && schema is OpenApiSchema openApiSchema
        )
        {
            openApiSchema.Type = JsonSchemaType.String;

            openApiSchema.Enum = EnumUtil
                .GetEnums<DataSetStatus>()
                .Where(_publicStatuses.Contains)
                .Select(JsonNode (dataSetStatus) => JsonValue.Create(dataSetStatus.ToString()))
                .ToList();
        }
    }
}
